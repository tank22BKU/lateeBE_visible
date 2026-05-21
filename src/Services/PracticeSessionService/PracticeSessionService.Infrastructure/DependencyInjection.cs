using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PracticeSessionService.Domain.Repositories;
using PracticeSessionService.Infrastructure.Persistance;
using PracticeSessionService.Infrastructure.Repositories;

namespace PracticeSessionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // DbContext (nếu chưa khai báo ở Program.cs thì để ở đây)
        // services.AddDbContext<ClinicalCaseDbContext>(...);

        // Repository (cách đơn giản, rõ ràng)
        services.AddScoped<IPracticeSessionRepository, PracticeSessionRepository>();
        services.AddScoped<IClinicalCaseRepository, ClinicalCaseRepository>();

        return services;
    }
}
