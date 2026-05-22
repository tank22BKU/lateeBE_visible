using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Infrastructure.Repositories;
using EvaluationService.Infrastructure.Rubrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EvaluationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();

        services.AddHttpClient<GeminiEvaluationRepository>();
        services.AddScoped<IAiEvaluationProvider, GeminiEvaluationRepository>();

        services.AddMemoryCache();
        services.AddScoped<IRubricProvider, RubricProvider>();
        services.AddScoped<IEvaluationPromptBuilder, EvaluationPromptBuilder>();
        services.AddScoped<IFeedbackPromptBuilder, FeedbackPromptBuilder>();
        services.AddHttpClient();

        return services;
    }
}
