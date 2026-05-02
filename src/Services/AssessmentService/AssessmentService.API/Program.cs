using AssessmentService.Application;
using AssessmentService.Infrastructure;
using AssessmentService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database
// =======================
var connectionString = builder.Configuration.GetConnectionString("AssessmentDb");

builder.Services.AddDbContext<AssessmentDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

// =======================
// Dependency Injection
// =======================
builder.Services.AddApplication();      
builder.Services.AddInfrastructure(builder.Configuration); 

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Assessment API",
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assessment API v1");
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();