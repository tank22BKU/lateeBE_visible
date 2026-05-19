using System.Reflection;
using EvaluationService.Application.Orchestrators;
using EvaluationService.Application.Services;
using EvaluationService.Domain.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EvaluationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        services.AddScoped<EvaluationOrchestrator>();
        services.AddScoped<IEpaScoreAggregator, EpaScoreAggregator>();
        services.AddScoped<IFeedbackComposer, FeedbackComposer>();
        services.AddScoped<IEvaluationPersistenceService, EvaluationPersistenceService>();

        return services;
    }
}
