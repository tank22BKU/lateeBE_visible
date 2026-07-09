using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace ApiGateway.Auth;

public sealed class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;

        _connectionString =
            _configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("Missing connection string 'AuthDb'.");
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var connection = CreateConnection();

            var user = await GetUserByIdentityAsync(connection, request.Email, cancellationToken);

            if (
                user is null
                || !string.Equals(user.IsActive, "active", StringComparison.OrdinalIgnoreCase)
                || user.IsDeleted
            )
            {
                return null;
            }

            var passwordMatched = VerifyPassword(request.Password, user.PasswordHash);

            if (!passwordMatched)
            {
                return null;
            }

            if (!IsBcryptHash(user.PasswordHash))
            {
                user = await UpgradePasswordHashAsync(
                    connection,
                    user,
                    request.Password,
                    cancellationToken
                );
            }

            return await IssueTokensAsync(
                connection,
                user,
                ipAddress,
                userAgent,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to login user.", ex);
        }
    }

    public async Task<AuthResponse?> RefreshAsync(
        string refreshToken,
        string ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var connection = CreateConnection();

            var tokenHash = ComputeSha256(refreshToken);

            var tokenRecord = await GetRefreshTokenRecordAsync(
                connection,
                tokenHash,
                cancellationToken
            );

            if (
                tokenRecord is null
                || tokenRecord.IsRevoked
                || tokenRecord.ExpiresAt <= DateTime.UtcNow
                || !string.Equals(
                    tokenRecord.IsActive,
                    "active",
                    StringComparison.OrdinalIgnoreCase
                )
                || tokenRecord.IsDeleted
            )
            {
                return null;
            }

            await RevokeRefreshTokenAsync(
                connection,
                tokenRecord.TokenId,
                "REFRESH_ROTATION",
                cancellationToken
            );

            var user = new UserRecord(
                tokenRecord.UserId,
                tokenRecord.Username,
                tokenRecord.Email,
                tokenRecord.PasswordHash,
                tokenRecord.Role,
                tokenRecord.IsActive,
                tokenRecord.AvatarUrl,
                tokenRecord.IsDeleted
            );

            return await IssueTokensAsync(
                connection,
                user,
                ipAddress,
                userAgent,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to refresh token.", ex);
        }
    }

    public async Task LogoutAsync(
        string? refreshToken,
        string? accessTokenJti,
        DateTime? accessTokenExpiresAt,
        string? userId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var connection = CreateConnection();

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var tokenHash = ComputeSha256(refreshToken);

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        UPDATE user_refresh_tokens
                        SET is_revoked = 1,
                            revoked_at = UTC_TIMESTAMP(),
                            revoked_reason = 'LOGOUT'
                        WHERE token_hash = @TokenHash
                          AND is_revoked = 0;
                        """,
                        new { TokenHash = tokenHash },
                        cancellationToken: cancellationToken
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(accessTokenJti) && accessTokenExpiresAt.HasValue)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO revoked_access_tokens
                        (
                            jti,
                            user_id,
                            expires_at,
                            revoked_at,
                            reason
                        )
                        VALUES
                        (
                            @Jti,
                            @UserId,
                            @ExpiresAt,
                            UTC_TIMESTAMP(),
                            'LOGOUT'
                        )
                        ON DUPLICATE KEY UPDATE
                            revoked_at = VALUES(revoked_at),
                            reason = VALUES(reason);
                        """,
                        new
                        {
                            Jti = accessTokenJti,
                            UserId = userId,
                            ExpiresAt = accessTokenExpiresAt.Value,
                        },
                        cancellationToken: cancellationToken
                    )
                );
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to logout user.", ex);
        }
    }

    public async Task<bool> IsAccessTokenRevokedAsync(
        string jti,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var connection = CreateConnection();

            var revoked = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    SELECT COUNT(1)
                    FROM revoked_access_tokens
                    WHERE jti = @Jti
                      AND expires_at > UTC_TIMESTAMP();
                    """,
                    new { Jti = jti },
                    cancellationToken: cancellationToken
                )
            );

            return revoked > 0;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to validate revoked token.", ex);
        }
    }

    private async Task<UserRecord?> GetUserByIdentityAsync(
        MySqlConnection connection,
        string email,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT
                userid AS UserId,
                name AS Username,
                email AS Email,
                password AS PasswordHash,
                role AS Role,
                status AS IsActive,
                avatar_url AS AvatarUrl,
                is_deleted AS IsDeleted
            FROM users
            WHERE email = @Email
            LIMIT 1;
            """;

        return await connection.QueryFirstOrDefaultAsync<UserRecord>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken)
        );
    }

    private async Task<RefreshTokenUserRecord?> GetRefreshTokenRecordAsync(
        MySqlConnection connection,
        string tokenHash,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT
                rt.token_id AS TokenId,
                rt.user_id AS UserId,
                rt.expires_at AS ExpiresAt,
                rt.is_revoked AS IsRevoked,

                u.name AS Username,
                u.email AS Email,
                u.password AS PasswordHash,
                u.role AS Role,

                u.status AS IsActive,
                u.avatar_url AS AvatarUrl,
                u.is_deleted AS IsDeleted

            FROM user_refresh_tokens rt

            INNER JOIN users u
                ON u.userid = rt.user_id

            WHERE rt.token_hash = @TokenHash

            LIMIT 1;
            """;

        return await connection.QueryFirstOrDefaultAsync<RefreshTokenUserRecord>(
            new CommandDefinition(
                sql,
                new { TokenHash = tokenHash },
                cancellationToken: cancellationToken
            )
        );
    }

    private async Task<UserRecord> UpgradePasswordHashAsync(
        MySqlConnection connection,
        UserRecord user,
        string password,
        CancellationToken cancellationToken
    )
    {
        var upgradedHash = BCrypt.Net.BCrypt.HashPassword(password);

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE users
                SET password = @PasswordHash
                WHERE userid = @UserId;
                """,
                new { PasswordHash = upgradedHash, user.UserId },
                cancellationToken: cancellationToken
            )
        );

        return user with
        {
            PasswordHash = upgradedHash,
        };
    }

    private async Task RevokeRefreshTokenAsync(
        MySqlConnection connection,
        string tokenId,
        string reason,
        CancellationToken cancellationToken
    )
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE user_refresh_tokens
                SET is_revoked = 1,
                    revoked_at = UTC_TIMESTAMP(),
                    revoked_reason = @Reason
                WHERE token_id = @TokenId;
                """,
                new { TokenId = tokenId, Reason = reason },
                cancellationToken: cancellationToken
            )
        );
    }

    private async Task<AuthResponse> IssueTokensAsync(
        MySqlConnection connection,
        UserRecord user,
        string ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        var now = DateTime.UtcNow;

        var accessTokenMinutes = _configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 1440;

        var refreshTokenDays = _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;

        var accessExpiresAt = now.AddMinutes(accessTokenMinutes);
        var refreshExpiresAt = now.AddDays(refreshTokenDays);

        var jti = Guid.NewGuid().ToString("N");

        var accessToken = GenerateAccessToken(user, jti, now, accessExpiresAt);

        var refreshToken = GenerateRefreshToken();

        await SaveRefreshTokenAsync(
            connection,
            user.UserId,
            refreshToken,
            refreshExpiresAt,
            ipAddress,
            userAgent,
            cancellationToken
        );

        return new AuthResponse(
            accessToken,
            accessExpiresAt,
            refreshToken,
            refreshExpiresAt,
            "Bearer",
            user.UserId,
            user.Username,
            user.Role,
            user.AvatarUrl
        );
    }

    private string GenerateAccessToken(
        UserRecord user,
        string jti,
        DateTime now,
        DateTime expiresAt
    )
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "latee-auth";

        var audience = _configuration["Jwt:Audience"] ?? "latee-clients";

        var signingKey =
            _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Missing JWT signing key.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, jti),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SaveRefreshTokenAsync(
        MySqlConnection connection,
        string userId,
        string refreshToken,
        DateTime expiresAt,
        string ipAddress,
        string? userAgent,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            INSERT INTO user_refresh_tokens
            (
                token_id,
                user_id,
                token_hash,
                expires_at,
                created_at,
                created_by_ip,
                user_agent,
                is_revoked
            )
            VALUES
            (
                @TokenId,
                @UserId,
                @TokenHash,
                @ExpiresAt,
                UTC_TIMESTAMP(),
                @IpAddress,
                @UserAgent,
                0
            );
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    TokenId = Guid.NewGuid().ToString("N"),
                    UserId = userId,
                    TokenHash = ComputeSha256(refreshToken),
                    ExpiresAt = expiresAt,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                },
                cancellationToken: cancellationToken
            )
        );
    }

    private MySqlConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    private static bool VerifyPassword(string inputPassword, string storedPassword)
    {
        if (IsBcryptHash(storedPassword))
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }

        return string.Equals(inputPassword, storedPassword, StringComparison.Ordinal);
    }

    private static bool IsBcryptHash(string hash)
    {
        return hash.StartsWith("$2", StringComparison.Ordinal);
    }

    private static string GenerateRefreshToken()
    {
        Span<byte> bytes = stackalloc byte[64];

        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes);
    }

    private sealed record UserRecord(
        string UserId,
        string Username,
        string Email,
        string PasswordHash,
        string Role,
        string IsActive,
        string? AvatarUrl,
        bool IsDeleted
    );

    private sealed record RefreshTokenUserRecord(
        string TokenId,
        string UserId,
        DateTime ExpiresAt,
        bool IsRevoked,
        string Username,
        string Email,
        string PasswordHash,
        string Role,
        string IsActive,
        string? AvatarUrl,
        bool IsDeleted
    );
}
