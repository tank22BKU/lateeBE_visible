using RoadmapService.Domain.Repositories;
using RoadmapService.Domain.Services;
using RoadmapService.Infrastructure.Persistence;
using RoadmapService.Infrastructure.Repositories;
using RoadmapService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RoadmapService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext (nếu chưa khai báo ở Program.cs thì để ở đây)
        // services.AddDbContext<ClinicalCaseDbContext>(...);

        // Repository (cách đơn giản, rõ ràng)
        services.AddScoped<IClinicalCaseRepository, ClinicalCaseRepository>();

        services.AddScoped<IGeminiService, GeminiService>();

        return services;
    }
}
