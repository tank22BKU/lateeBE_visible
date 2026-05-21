using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Repositories;

namespace UserService.Application.Users.Queries.GetDashboardStatistics;

public sealed class GetDashboardStatisticsQuery : IRequest<DashboardStatsDto>
{
}

public sealed class GetDashboardStatisticsHandler : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatsDto>
{
    private readonly IUserRepository _repository;

    public GetDashboardStatisticsHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetDashboardStatisticsAsync();

        return new DashboardStatsDto
        {
            IncreaseUser = stats.IncreaseUser,
            TotalLearners = stats.TotalLearners,
            IncreaseLearners = stats.IncreaseLearners,
            TotalExperts = stats.TotalExperts,
            TotalAdmins = stats.TotalAdmins,
            TotalActiveUsers = stats.TotalActiveUsers
        };
    }
}