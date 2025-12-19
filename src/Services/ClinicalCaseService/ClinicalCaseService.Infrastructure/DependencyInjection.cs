using ClinicalCaseService.Domain.Repositories;
using ClinicalCaseService.Infrastructure.Persistence;
using ClinicalCaseService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicalCaseService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext (nếu chưa khai báo ở Program.cs thì để ở đây)
        // services.AddDbContext<ClinicalCaseDbContext>(...);

        // Repository (cách đơn giản, rõ ràng)
        services.AddScoped<IClinicalCaseRepository, ClinicalCaseRepository>();

        return services;
    }
}
