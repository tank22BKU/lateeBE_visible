using VirtualPatientService.Application;
using VirtualPatientService.Infrastructure;
using VirtualPatientService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
var connectionString = builder.Configuration.GetConnectionString("VirtualPatientDb");
builder.Services.AddDbContext<VirtualPatientDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Virtual Patient API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
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

app.UseAuthorization();
app.MapControllers();
app.Run();