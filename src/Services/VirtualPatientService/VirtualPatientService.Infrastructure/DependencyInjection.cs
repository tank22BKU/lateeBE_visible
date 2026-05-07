using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Infrastructure.Persistance;
using VirtualPatientService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VirtualPatientService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext (nếu chưa khai báo ở Program.cs thì để ở đây)
        // services.AddDbContext<VirtualPatientDbContext>(...);

        // Repository (cách đơn giản, rõ ràng)
        services.AddScoped<IVirtualPatientRepository, VirtualPatientRepository>();
        services.AddScoped<IClinicalCaseRepository, ClinicalCaseRepository>();

        return services;
    }
}
