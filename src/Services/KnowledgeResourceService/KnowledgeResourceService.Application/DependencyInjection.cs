using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeResourceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper, MediatR, Validators here if needed
        return services;
    }
}
