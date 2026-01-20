using VirtualPatientService.Domain.Repositories;
using VirtualPatientService.Application.Queries.GetVirtualPatients;
using MediatR;

namespace VirtualPatientService.Application.Queries.GetVirtualPatients;

public class GetVirtualPatientsHandler : IRequestHandler<GetVirtualPatientQuery, PagedResult<VirtualPatientDto>>
{
    private readonly IVirtualPatientRepository _repo;

    public GetVirtualPatientsHandler(IVirtualPatientRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<VirtualPatientDto>> Handle(GetVirtualPatientQuery q, CancellationToken cancellationToken)
    {
        if (q.Page < 1) q = q with { Page = 1 };
        if (q.PageSize <= 0 || q.PageSize > 100)
            q = q with { PageSize = 20 };

        var (items, total) =
            await _repo.GetPagedAsync(q.Gender, q.Page, q.PageSize);

        return new PagedResult<VirtualPatientDto>
        {
            Items = items.Select(x => new VirtualPatientDto
            {
                Id = x.PatientId,
                Description = x.Description,
                Behaviors = x.Behaviors
            }).ToList(),
            Total = total,
            Page = q.Page,
            PageSize = q.PageSize
        };
    }
}

