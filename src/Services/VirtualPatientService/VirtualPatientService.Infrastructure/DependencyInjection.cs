using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;
using VirtualPatientService.Infrastructure.Repositories;

namespace VirtualPatientService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<IVirtualPatientRepository, VirtualPatientRepository>();
        services.AddScoped<IVirtualPatientFetchRepository, VirtualPatientFetchRepository>();
        services.AddScoped<IClinicalCaseRepository, ClinicalCaseRepository>();
        services.AddScoped<ILearnerDiscoveryStateRepository, LearnerDiscoveryStateRepository>();
        services.AddScoped<IPracticeAttemptRepository, PracticeAttemptRepository>();

        return services;
    }
}
