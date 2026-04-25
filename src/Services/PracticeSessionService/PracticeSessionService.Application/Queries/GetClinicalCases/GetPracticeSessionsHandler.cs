// using PracticeSessionService.Domain.Repositories;
// using PracticeSessionService.Application.Queries.GetClinicalCases;
// using MediatR;
//
// namespace PracticeSessionService.Application.Queries.GetClinicalCases;
//
// public class GetPracticeSessionsHandler : IRequestHandler<GetPracticeSessionsQuery, PagedResult<PracticeSessionDto>>
// {
//     private readonly IPracticeSessionRepository _repo;
//
//     public GetPracticeSessionsHandler(IPracticeSessionRepository repo)
//     {
//         _repo = repo;
//     }
//
//     public async Task<PagedResult<PracticeSessionDto>> Handle(GetPracticeSessionsQuery q, CancellationToken cancellationToken)
//     {
//         if (q.Page < 1) q = q with { Page = 1 };
//         if (q.PageSize <= 0 || q.PageSize > 100)
//             q = q with { PageSize = 20 };
//
//         var (items, total) =
//             await _repo.GetPagedAsync(q.Status, q.Page, q.PageSize);
//
//         return new PagedResult<PracticeSessionDto>
//         {
//             Items = items.Select(x => new PracticeSessionDto
//             {
//                 Id = x.ClinicalCaseId,
//                 Title = x.Title,
//                 Type = x.CaseType
//             }).ToList(),
//             Total = total,
//             Page = q.Page,
//             PageSize = q.PageSize
//         };
//     }
// }
//
