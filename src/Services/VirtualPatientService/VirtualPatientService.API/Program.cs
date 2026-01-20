using VirtualPatientService.Application;
using VirtualPatientService.Infrastructure;
using VirtualPatientService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database
// =======================
var connectionString =
    builder.Configuration.GetConnectionString("VirtualPatientDb");

builder.Services.AddDbContext<VirtualPatientDbContext>(options =>
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
        Title = "Virtual Patient API",
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Virtual Patient API v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
