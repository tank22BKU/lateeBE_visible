using ClinicalCaseService.Application;
using ClinicalCaseService.Infrastructure;
using ClinicalCaseService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database
// =======================
var connectionString =
    builder.Configuration.GetConnectionString("ClinicalCaseDb");

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
