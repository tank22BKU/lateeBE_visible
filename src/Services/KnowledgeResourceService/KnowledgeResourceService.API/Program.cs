using KnowledgeResourceService.Application;
using KnowledgeResourceService.Infrastructure;
using KnowledgeResourceService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

var authorizationEnabled = builder.Configuration.GetValue<bool>("Security:AuthorizationEnabled");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "latee-auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "latee-clients";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? "latee_super_secret_signing_key_2026_change_me";

var geminiModel = Environment.GetEnvironmentVariable("GEMINI_MODEL");
if (!string.IsNullOrEmpty(geminiModel))
{
    builder.Configuration["Gemini:Model"] = geminiModel;
}

// =======================
// Database
// =======================
var connectionString =
    builder.Configuration.GetConnectionString("KnowledgeResourceDb");

builder.Services.AddDbContext<KnowledgeDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

// =======================
// Dependency Injection
// =======================
builder.Services.AddApplication();      // MediatR, Validators, AutoMapper
builder.Services.AddInfrastructure(builder.Configuration); // Repositories, DbContext

// =======================
// API & Swagger
// =======================
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
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = authorizationEnabled
        ? new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
        : new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
});

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Knowledge Resource API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseCors("SwaggerCors");

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Resource API v1");
    });
}

if (authorizationEnabled)
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
