using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadmapService.Domain.Repositories;
using RoadmapService.Domain.Services;
using RoadmapService.Infrastructure.Repositories;
using RoadmapService.Infrastructure.Services;

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
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();

        services.AddScoped<IRoadmapService, Services.RoadmapService>();
        services.AddHttpClient<HuggingFaceDeepSeekClient>();
        services.AddScoped<HuggingFaceDeepSeekClient>();

        return services;
    }
}