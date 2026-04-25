using EvaluationService.Domain.Repositories;
using EvaluationService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EvaluationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddHttpClient<IGeminiAiRepository, GeminiAiRepository>();

        return services;
    }
}