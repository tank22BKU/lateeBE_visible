using RoadmapService.Application;
using RoadmapService.Infrastructure;
using RoadmapService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

var geminiModel = Environment.GetEnvironmentVariable("GEMINI_MODEL");
if (!string.IsNullOrEmpty(geminiModel))
{
    builder.Configuration["Gemini:Model"] = geminiModel;
}

// =======================
// Database
// =======================
var connectionString =
    builder.Configuration.GetConnectionString("RoadmapDb");

builder.Services.AddDbContext<ClinicalCaseDbContext>(options =>
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
        Title = "Roadmap API",
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Roadmap API v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
