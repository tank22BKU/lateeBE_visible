using AssessmentService.Domain.Repositories;
using AssessmentService.Infrastructure.Persistance;
using AssessmentService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssessmentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        
        services.AddHttpClient<IGeminiAiRepository, GeminiAiRepository>();

        return services;
    }
}