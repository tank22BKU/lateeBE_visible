using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KnowledgeResourceService.Domain.Repositories;
using KnowledgeResourceService.Infrastructure.Repositories;

namespace KnowledgeResourceService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IKnowledgeResourceRepository, KnowledgeResourceRepository>();

        return services;
    }
}
