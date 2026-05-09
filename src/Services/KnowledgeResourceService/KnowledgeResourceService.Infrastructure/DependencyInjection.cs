using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeResourceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register infrastructure services, repositories, http clients etc.
        // Example: services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();

        return services;
    }
}
