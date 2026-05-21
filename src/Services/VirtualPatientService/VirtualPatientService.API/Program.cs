using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VirtualPatientService.Application;
using VirtualPatientService.Infrastructure;
using VirtualPatientService.Infrastructure.Persistance;

var builder = WebApplication.CreateBuilder(args);

var authorizationEnabled = builder.Configuration.GetValue<bool>("Security:AuthorizationEnabled");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "latee-auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "latee-clients";
var jwtSigningKey =
    builder.Configuration["Jwt:SigningKey"] ?? "latee_super_secret_signing_key_2026_change_me";

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("VirtualPatientDb");
builder.Services.AddDbContext<VirtualPatientDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Dependency Injection
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = authorizationEnabled
        ? new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
        : new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Virtual Patient API", Version = "v1" }
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "SwaggerCors",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();
app.UseCors("SwaggerCors");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Virtual Patient API v1");
        c.RoutePrefix = "swagger";
    });
}

if (authorizationEnabled)
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
