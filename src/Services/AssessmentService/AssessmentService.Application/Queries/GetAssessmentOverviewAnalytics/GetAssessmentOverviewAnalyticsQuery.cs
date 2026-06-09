using MediatR;
using AssessmentService.Domain.Repositories;
using AssessmentService.Domain.Entities;

namespace AssessmentService.Application.Queries.GetAssessmentOverviewAnalytics;

public record GetAssessmentOverviewAnalyticsQuery(string LearnerId) : IRequest<AssessmentOverviewAnalyticsDto>;

public class GetAssessmentOverviewAnalyticsHandler : IRequestHandler<GetAssessmentOverviewAnalyticsQuery, AssessmentOverviewAnalyticsDto>
{
    private readonly IAssessmentRepository _repo;

    public GetAssessmentOverviewAnalyticsHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<AssessmentOverviewAnalyticsDto> Handle(GetAssessmentOverviewAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var learnerId = request.LearnerId?.Trim();
        var empty = new AssessmentOverviewAnalyticsDto();
        if (string.IsNullOrWhiteSpace(learnerId))
            return empty;

        var sessions = await _repo.GetAllAttemptsOverviewOfLearner(learnerId);
        if (sessions == null || sessions.Count == 0)
            return empty;

        // load assessments to compute max scores and titles
        var allAssessments = await _repo.GetAllAsync();
        var assessmentMap = allAssessments.ToDictionary(a => a.AssessmentId, a => a);

        // convert to local times where needed
        var sessionsWithEnd = sessions.Where(s => s.EndTime.HasValue)
            .Select(s => new
            {
                Session = s,
                EndLocal = s.EndTime!.Value.ToLocalTime(),
                StartLocal = s.StartTime.ToLocalTime()
            })
            .ToList();

        var nowLocal = DateTime.Now;
        var currentMonth = new DateTime(nowLocal.Year, nowLocal.Month, 1);
        var prevMonth = currentMonth.AddMonths(-1);

        // Monthly counts
        int currentMonthCount = sessionsWithEnd.Count(x => x.EndLocal.Year == nowLocal.Year && x.EndLocal.Month == nowLocal.Month);
        int prevMonthCount = sessionsWithEnd.Count(x => x.EndLocal.Year == prevMonth.Year && x.EndLocal.Month == prevMonth.Month);

        decimal assessmentMonthlyGrowth;
        if (prevMonthCount == 0)
        {
            assessmentMonthlyGrowth = currentMonthCount == 0 ? 0m : 100m;
        }
        else
        {
            var growth = (currentMonthCount - prevMonthCount) / (decimal)prevMonthCount * 100m;
            assessmentMonthlyGrowth = Math.Round(growth, 1);
        }

        // Completion monthly rate: completed / started in current month
        int startedThisMonth = sessions.Count(s => s.StartTime.ToLocalTime().Year == nowLocal.Year && s.StartTime.ToLocalTime().Month == nowLocal.Month);
        int completedThisMonth = sessionsWithEnd.Count(x => x.EndLocal.Year == nowLocal.Year && x.EndLocal.Month == nowLocal.Month);
        decimal completionMonthlyRate = startedThisMonth == 0 ? 0m : Math.Round((completedThisMonth / (decimal)startedThisMonth * 100m), 1);

        // Average monthly score: sum(achieved) / sum(max) * 10 => return only the number
        var sessionsInMonth = sessionsWithEnd.Where(x => x.EndLocal.Year == nowLocal.Year && x.EndLocal.Month == nowLocal.Month).ToList();
        decimal totalAchieved = 0m;
        decimal totalMax = 0m;
        foreach (var item in sessionsInMonth)
        {
            totalAchieved += item.Session.OverallScore;
            if (assessmentMap.TryGetValue(item.Session.AssessmentId, out var a))
            {
                decimal maxScore = a.Questions != null && a.Questions.Count > 0 ? a.Questions.Sum(q => q.Points) : a.NumQuestions;
                totalMax += maxScore;
            }
        }

        decimal averageMonthlyScore;
        if (totalMax <= 0)
            averageMonthlyScore = 0m;
        else
        {
            var value = (totalAchieved / totalMax * 10m);
            averageMonthlyScore = Math.Round(value, 1);
        }

        // todayAssessments: EndTime local is today
        var today = nowLocal.Date;
        var todayItems = sessionsWithEnd
            .Where(x => x.EndLocal.Date == today)
            .OrderByDescending(x => x.EndLocal)
            .Select(x => new AssessmentActivityItemDto
            {
                Time = x.EndLocal,
                Title = assessmentMap.TryGetValue(x.Session.AssessmentId, out var a) ? a.Title : string.Empty,
                AttemptNo = x.Session.AttemptNo
            })
            .ToList();

        // recentActivities: 3 most recent by EndTime
        var recent = sessionsWithEnd
            .OrderByDescending(x => x.EndLocal)
            .Take(3)
            .Select(x => new AssessmentActivityItemDto
            {
                Time = x.EndLocal,
                Title = assessmentMap.TryGetValue(x.Session.AssessmentId, out var a) ? a.Title : string.Empty,
                AttemptNo = x.Session.AttemptNo
            })
            .ToList();

        return new AssessmentOverviewAnalyticsDto
        {
            AssessmentMonthlyGrowth = assessmentMonthlyGrowth,
            CompletionMonthlyRate = completionMonthlyRate,
            AverageMonthlyScore = averageMonthlyScore,
            TodayAssessments = todayItems,
            RecentActivities = recent
        };
    }
}

public class AssessmentOverviewAnalyticsDto
{
    public decimal AssessmentMonthlyGrowth { get; set; }
    public decimal CompletionMonthlyRate { get; set; }
    public decimal AverageMonthlyScore { get; set; }
    public List<AssessmentActivityItemDto> TodayAssessments { get; set; } = new List<AssessmentActivityItemDto>();
    public List<AssessmentActivityItemDto> RecentActivities { get; set; } = new List<AssessmentActivityItemDto>();
}

public class AssessmentActivityItemDto
{
    public DateTime Time { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AttemptNo { get; set; }
}
