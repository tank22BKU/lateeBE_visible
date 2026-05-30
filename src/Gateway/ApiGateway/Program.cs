using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using ApiGateway.Auth;

var builder = WebApplication.CreateBuilder(args);

var authorizationEnabled = builder.Configuration.GetValue<bool>("Security:AuthorizationEnabled");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "latee-auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "latee-clients";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "latee_super_secret_signing_key_2026_change_me";

var ocelotConfigFile = authorizationEnabled ? "ocelot.auth.json" : "ocelot.noauth.json";

// Add Ocelot configuration
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);

// Add services
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("Missing token identifier.");
                    return;
                }

                var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
                // var isRevoked = await authService.IsAccessTokenRevokedAsync(jti, context.HttpContext.RequestAborted);
                // if (isRevoked)
                // {
                //     context.Fail("Access token has been revoked.");
                // }
                try
                {
                    var isRevoked = await authService.IsAccessTokenRevokedAsync(jti, context.HttpContext.RequestAborted);
                    if (isRevoked)
                        context.Fail("revoked");
                }
                catch
                {
                    context.Fail("revocation check failed");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    if (authorizationEnabled)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api Gateway Auth API",
        Version = "v1",
        Description = "Authentication endpoints: login, refresh, logout"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseRouting();

app.Use(async (context, next) =>
{
    var origin = context.Request.Headers.Origin.ToString();

    if (!string.IsNullOrWhiteSpace(origin))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Vary"] = "Origin";
            return Task.CompletedTask;
        });
    }

    await next();
});

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

// ===== CRITICAL: Swagger middleware MUST run before authentication/authorization
// to serve static files without JWT token requirement =====
app.UseSwagger(options =>
{
    options.RouteTemplate = "gateway-auth/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/gateway-auth/swagger/v1/swagger.json", "Api Gateway Auth API v1");
    options.RoutePrefix = "swagger/auth";
});

// Configure Swagger for Ocelot (MUST be before routing/auth to serve static files)
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot();

app.Run();