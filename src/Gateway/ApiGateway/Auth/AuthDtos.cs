namespace ApiGateway.Auth;

public sealed record LoginRequest(string Identity, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string? RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    string TokenType,
    string Username,
    string Role
);
