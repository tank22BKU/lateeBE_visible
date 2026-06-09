using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualPatientService.Application.Dtos;
using VirtualPatientService.Domain.Constants;
using VirtualPatientService.Domain.Entities;
using VirtualPatientService.Infrastructure.Persistance;

namespace VirtualPatientService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/expert/virtual-patients")]
public class VirtualPatientExpertController : ControllerBase
{
    private readonly VirtualPatientDbContext _db;

    public VirtualPatientExpertController(VirtualPatientDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] VirtualPatientExpertListQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize =
            request.PageSize <= 0 || request.PageSize > VirtualPatientConstants.MaxPageSize
                ? 8
                : request.PageSize;

        var query = BuildListQuery(request);
        var total = await query.CountAsync(cancellationToken);
        var rows = await ApplySort(query, request.SortBy, request.SortDir)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExpertListRow
            {
                PatientId = x.PatientId,
                CaseId = x.CaseId,
                Name = x.Name,
                Age = x.Age,
                Gender = x.Gender,
                Occupation = x.Occupation,
                ChiefConcern = x.ChiefConcern,
                Level = x.Level,
                Status = x.Status,
                AvatarImage = x.AvatarImage,
                TimeSetting = x.TimeSetting,
                ArgumentTime = x.ArgumentTime,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var patientIds = rows.Select(x => x.PatientId).ToList();
        var statsByPatient = await GetStatsByPatientIdsAsync(patientIds, cancellationToken);

        var response = new VirtualPatientExpertListResponseDto
        {
            Items = rows.Select(x =>
                {
                    statsByPatient.TryGetValue(x.PatientId, out var stats);
                    return new VirtualPatientExpertListItemDto
                    {
                        PatientId = x.PatientId,
                        CaseId = x.CaseId,
                        Name = x.Name,
                        Age = x.Age,
                        Gender = x.Gender,
                        Occupation = x.Occupation,
                        ChiefConcern = x.ChiefConcern,
                        Level = x.Level,
                        Status = NormalizeStatusForResponse(x.Status),
                        AvatarImage = x.AvatarImage,
                        TimeSetting = x.TimeSetting,
                        ArgumentTime = x.ArgumentTime,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        AttemptCount = stats?.TotalAttempts ?? 0,
                        AvgScore = stats?.AvgScore,
                        ExpertCount = stats?.ExpertCount ?? 0,
                    };
                })
                .ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Filters = await BuildFiltersAsync(cancellationToken),
        };
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var detail = await BuildDetailAsync(id, cancellationToken);
        if (detail is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        return Ok(detail);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] VirtualPatientExpertUpsertRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationError = ValidateUpsertRequest(request);
        if (validationError is not null)
            return BadRequest(new { message = validationError });

        var caseExists = await _db
            .ClinicalCases.AsNoTracking()
            .AnyAsync(x => x.CaseId == request.CaseId, cancellationToken);
        if (!caseExists)
            return BadRequest(
                new { message = $"Không tìm thấy clinical case với ID: {request.CaseId}" }
            );

        var ownerExpertId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(ownerExpertId))
            return Unauthorized(new { message = "Không xác định được owner expert từ token." });

        ownerExpertId = ownerExpertId.Trim();

        var ownerExists = await _db
            .Experts.AsNoTracking()
            .AnyAsync(x => x.ExpertId == ownerExpertId, cancellationToken);
        if (!ownerExists)
            return BadRequest(new { message = $"ownerExpertId không hợp lệ: {ownerExpertId}" });

        var patientId = string.IsNullOrWhiteSpace(request.PatientId)
            ? await GenerateUniquePatientIdAsync(cancellationToken)
            : request.PatientId.Trim();

        var duplicate = await _db
            .VirtualPatients.AsNoTracking()
            .AnyAsync(x => x.PatientId == patientId, cancellationToken);
        if (duplicate)
            return Conflict(new { message = $"Virtual patient ID đã tồn tại: {patientId}" });

        var now = DateTime.UtcNow;
        var entity = new VirtualPatient
        {
            PatientId = patientId,
            CaseId = request.CaseId.Trim(),
            OwnerExpertId = ownerExpertId,
            Name = request.Name.Trim(),
            Age = request.Age,
            Gender = NormalizeNullableText(request.Gender),
            Pronouns = NormalizeNullableText(request.Pronouns),
            Ethnicity = NormalizeNullableText(request.Ethnicity),
            Occupation = NormalizeNullableText(request.Occupation),
            ChiefConcern = NormalizeNullableText(request.ChiefConcern),
            Persona = SerializeJson(request.Persona),
            VitalSigns = SerializeJson(request.VitalSigns),
            Instructions = SerializeJson(request.Instructions),
            Behaviors = SerializeJson(request.Behaviors),
            TimeSetting = request.TimeSetting,
            ArgumentTime = request.ArgumentTime,
            LearningObjectives = SerializeJson(request.LearningObjectives),
            Level = NormalizeNullableText(request.Level),
            AvatarImage = NormalizeNullableText(request.AvatarImage),
            CaseRule = SerializeJson(request.CaseRule),
            Status = VirtualPatientConstants.Status.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        _db.VirtualPatients.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await ReplaceExpertsAsync(entity.PatientId, request.ExpertIds ?? [], cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(new { message = ex.Message });
        }

        var expertIds = await GetExpertIdsByPatientIdAsync(entity.PatientId, cancellationToken);
        var experts = await GetExpertsByPatientIdAsync(entity.PatientId, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.PatientId },
            new
            {
                patientId = entity.PatientId,
                ownerExpertId = entity.OwnerExpertId,
                name = entity.Name,
                status = NormalizeStatusForResponse(entity.Status),
                createdAt = entity.CreatedAt,
                expertIds = expertIds,
                experts = experts,
                stats = new
                {
                    totalAttempts = 0,
                    avgScore = (decimal?)null,
                    completionRate = 0m,
                    expertCount = expertIds.Count,
                },
            }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] VirtualPatientExpertUpsertRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationError = ValidateUpsertRequest(request);
        if (validationError is not null)
            return BadRequest(new { message = validationError });

        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        var caseExists = await _db
            .ClinicalCases.AsNoTracking()
            .AnyAsync(x => x.CaseId == request.CaseId, cancellationToken);
        if (!caseExists)
            return BadRequest(
                new { message = $"Không tìm thấy clinical case với ID: {request.CaseId}" }
            );

        entity.CaseId = request.CaseId.Trim();
        entity.Name = request.Name.Trim();
        entity.Age = request.Age;
        entity.Gender = NormalizeNullableText(request.Gender);
        entity.Pronouns = NormalizeNullableText(request.Pronouns);
        entity.Ethnicity = NormalizeNullableText(request.Ethnicity);
        entity.Occupation = NormalizeNullableText(request.Occupation);
        entity.ChiefConcern = NormalizeNullableText(request.ChiefConcern);
        entity.Persona = SerializeJson(request.Persona);
        entity.VitalSigns = SerializeJson(request.VitalSigns);
        entity.Instructions = SerializeJson(request.Instructions);
        entity.Behaviors = SerializeJson(request.Behaviors);
        entity.TimeSetting = request.TimeSetting;
        entity.ArgumentTime = request.ArgumentTime;
        entity.LearningObjectives = SerializeJson(request.LearningObjectives);
        entity.Level = NormalizeNullableText(request.Level);
        entity.AvatarImage = NormalizeNullableText(request.AvatarImage);
        entity.CaseRule = SerializeJson(request.CaseRule);
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            if (request.ExpertIds is not null)
                await ReplaceExpertsAsync(entity.PatientId, request.ExpertIds, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { patientId = entity.PatientId, updatedAt = entity.UpdatedAt });
    }

    [HttpPut("{id}/experts")]
    public async Task<IActionResult> ReplaceExperts(
        string id,
        [FromBody] VirtualPatientExpertManagementRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        try
        {
            await ReplaceExpertsAsync(id, request.ExpertIds, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(
            new
            {
                patientId = entity.PatientId,
                expertIds = await GetExpertIdsByPatientIdAsync(id, cancellationToken),
                updatedAt = entity.UpdatedAt,
            }
        );
    }

    [HttpPost("{id}/experts")]
    public async Task<IActionResult> AddExperts(
        string id,
        [FromBody] VirtualPatientExpertManagementRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var patient = await _db
            .VirtualPatients.AsNoTracking()
            .Where(x => x.PatientId == id)
            .Select(x => new { x.PatientId, x.UpdatedAt })
            .FirstOrDefaultAsync(cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        var ids = NormalizeExpertIds(request.ExpertIds);
        if (ids.Count == 0)
        {
            return Ok(
                new
                {
                    patientId = patient.PatientId,
                    expertIds = await GetExpertIdsByPatientIdAsync(id, cancellationToken),
                    updatedAt = patient.UpdatedAt,
                }
            );
        }

        await ValidateExpertIdsExistAsync(ids, cancellationToken);

        var existingIds = await GetExpertIdsByPatientIdAsync(id, cancellationToken);
        var newIds = ids.Except(existingIds, StringComparer.OrdinalIgnoreCase).ToList();

        if (newIds.Count > 0)
        {
            _db.ExpertVirtualPatientManagements.AddRange(
                newIds.Select(x => new ExpertVirtualPatientManagement
                {
                    ExpertId = x,
                    VirtualId = id,
                })
            );
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(
            new
            {
                patientId = patient.PatientId,
                expertIds = await GetExpertIdsByPatientIdAsync(id, cancellationToken),
                updatedAt = patient.UpdatedAt,
            }
        );
    }

    [HttpDelete("{id}/experts/{expertId}")]
    public async Task<IActionResult> RemoveExpert(
        string id,
        string expertId,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        var normalizedExpertId = expertId.Trim();
        var link = await _db.ExpertVirtualPatientManagements.FirstOrDefaultAsync(
            x => x.VirtualId == id && x.ExpertId == normalizedExpertId,
            cancellationToken
        );
        if (link is null)
            return NotFound(
                new { message = $"Không tìm thấy expert {normalizedExpertId} cho case {id}" }
            );

        _db.ExpertVirtualPatientManagements.Remove(link);
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(
            new
            {
                patientId = entity.PatientId,
                expertIds = await GetExpertIdsByPatientIdAsync(id, cancellationToken),
                updatedAt = entity.UpdatedAt,
            }
        );
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] VirtualPatientExpertStatusRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var status = NormalizeStatusInput(request.Status);
        if (status is null)
            return BadRequest(
                new
                {
                    message = "status must be one of active, draft, archived, published, inactive",
                }
            );

        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(
            new
            {
                patientId = entity.PatientId,
                status = NormalizeStatusForResponse(entity.Status),
                updatedAt = entity.UpdatedAt,
            }
        );
    }

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(
        string id,
        [FromQuery] bool confirm = false,
        CancellationToken cancellationToken = default
    ) => DeleteInternalAsync(id, confirm, cancellationToken);

    [HttpPost("{id}")]
    [HttpPost("{id}/delete")]
    public Task<IActionResult> DeleteViaPost(
        string id,
        CancellationToken cancellationToken = default
    ) => DeleteInternalAsync(id, true, cancellationToken);

    private async Task<IActionResult> DeleteInternalAsync(
        string id,
        bool confirm,
        CancellationToken cancellationToken
    )
    {
        if (!confirm)
            return BadRequest(new { message = "Confirmation is required. Pass confirm=true." });

        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        var poolRows = await _db
            .LearnerDiscoveryPools.Where(x => x.PatientId == id)
            .ToListAsync(cancellationToken);
        var expertLinks = await _db
            .ExpertVirtualPatientManagements.Where(x => x.VirtualId == id)
            .ToListAsync(cancellationToken);

        if (poolRows.Count > 0)
            _db.LearnerDiscoveryPools.RemoveRange(poolRows);

        if (expertLinks.Count > 0)
            _db.ExpertVirtualPatientManagements.RemoveRange(expertLinks);

        _db.VirtualPatients.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { success = true, patientId = id });
    }

    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var source = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (source is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        var newPatientId = await GenerateUniquePatientIdAsync(cancellationToken);
        var now = DateTime.UtcNow;

        var duplicate = new VirtualPatient
        {
            PatientId = newPatientId,
            CaseId = source.CaseId,
            Name = source.Name,
            Age = source.Age,
            Gender = source.Gender,
            Pronouns = source.Pronouns,
            Ethnicity = source.Ethnicity,
            Occupation = source.Occupation,
            ChiefConcern = source.ChiefConcern,
            Persona = source.Persona,
            VitalSigns = source.VitalSigns,
            Instructions = source.Instructions,
            Behaviors = source.Behaviors,
            TimeSetting = source.TimeSetting,
            ArgumentTime = source.ArgumentTime,
            LearningObjectives = source.LearningObjectives,
            Level = source.Level,
            AvatarImage = source.AvatarImage,
            CaseRule = source.CaseRule,
            Status = VirtualPatientConstants.Status.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        _db.VirtualPatients.Add(duplicate);
        await _db.SaveChangesAsync(cancellationToken);

        var expertIds = await _db
            .ExpertVirtualPatientManagements.AsNoTracking()
            .Where(x => x.VirtualId == id)
            .Select(x => x.ExpertId)
            .ToListAsync(cancellationToken);
        await ReplaceExpertsAsync(duplicate.PatientId, expertIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = duplicate.PatientId },
            new
            {
                patientId = duplicate.PatientId,
                name = duplicate.Name,
                status = NormalizeStatusForResponse(duplicate.Status),
                createdAt = duplicate.CreatedAt,
            }
        );
    }

    [HttpPatch("{id}/publish")]
    public async Task<IActionResult> PublishToggle(
        string id,
        [FromBody] VirtualPatientExpertPublishRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await _db.VirtualPatients.FirstOrDefaultAsync(
            x => x.PatientId == id,
            cancellationToken
        );
        if (entity is null)
            return NotFound(new { message = $"Không tìm thấy virtual patient với ID: {id}" });

        entity.Status = request.Publish
            ? VirtualPatientConstants.Status.Published
            : VirtualPatientConstants.Status.Active;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(
            new
            {
                patientId = entity.PatientId,
                status = NormalizeStatusForResponse(entity.Status),
                updatedAt = entity.UpdatedAt,
            }
        );
    }

    private IQueryable<VirtualPatient> BuildListQuery(VirtualPatientExpertListQuery request)
    {
        var query = _db.VirtualPatients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.PatientId, pattern)
                || EF.Functions.Like(x.Name, pattern)
                || EF.Functions.Like(x.CaseId, pattern)
                || EF.Functions.Like(x.ChiefConcern, pattern)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = NormalizeStatusInput(request.Status);
            if (status is null)
            {
                query = query.Where(_ => false);
            }
            else if (
                string.Equals(
                    status,
                    VirtualPatientConstants.Status.Archived,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                query = query.Where(x =>
                    x.Status == VirtualPatientConstants.Status.Archived
                    || x.Status == VirtualPatientConstants.Status.Inactive
                );
            }
            else
            {
                query = query.Where(x => x.Status == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Level))
            query = query.Where(x => x.Level == request.Level);

        if (!string.IsNullOrWhiteSpace(request.Gender))
            query = query.Where(x => x.Gender == request.Gender);

        if (!string.IsNullOrWhiteSpace(request.CaseId))
            query = query.Where(x => x.CaseId == request.CaseId);

        return query;
    }

    private static IQueryable<VirtualPatient> ApplySort(
        IQueryable<VirtualPatient> query,
        string? sortBy,
        string? sortDir
    )
    {
        var descending = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        var sortKey = (sortBy ?? string.Empty).Trim().ToLowerInvariant();

        return sortKey switch
        {
            "updatedat" => descending
                ? query.OrderByDescending(x => x.UpdatedAt)
                : query.OrderBy(x => x.UpdatedAt),
            "name" => descending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            "level" => descending
                ? query.OrderByDescending(x => x.Level)
                : query.OrderBy(x => x.Level),
            _ => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
        };
    }

    private async Task<VirtualPatientExpertDetailDto?> BuildDetailAsync(
        string patientId,
        CancellationToken cancellationToken
    )
    {
        var patient = await _db
            .VirtualPatients.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PatientId == patientId, cancellationToken);
        if (patient is null)
            return null;

        var clinicalCase = await _db
            .ClinicalCases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CaseId == patient.CaseId, cancellationToken);

        var ownerExpertId = patient.OwnerExpertId;

        var experts = await GetExpertsByPatientIdAsync(patientId, cancellationToken);
        var statsByPatient = await GetStatsByPatientIdsAsync(
            new[] { patientId },
            cancellationToken
        );
        statsByPatient.TryGetValue(patientId, out var stats);

        return new VirtualPatientExpertDetailDto
        {
            PatientId = patient.PatientId,
            OwnerExpertId = ownerExpertId,
            CaseId = patient.CaseId,
            Name = patient.Name,
            Age = patient.Age,
            Gender = patient.Gender,
            Pronouns = patient.Pronouns,
            Ethnicity = patient.Ethnicity,
            Occupation = patient.Occupation,
            ChiefConcern = patient.ChiefConcern,
            MedicalHistory = clinicalCase?.MedicalHistory,
            Symptom = clinicalCase?.Symptom,
            Persona = ParseJson(patient.Persona),
            VitalSigns = ParseJson(patient.VitalSigns),
            Instructions = ParseJson(patient.Instructions),
            Behaviors = ParseJson(patient.Behaviors),
            TimeSetting = patient.TimeSetting,
            ArgumentTime = patient.ArgumentTime,
            LearningObjectives = ParseJson(patient.LearningObjectives),
            Level = patient.Level,
            AvatarImage = patient.AvatarImage,
            CaseRule = ParseJson(patient.CaseRule),
            Status = NormalizeStatusForResponse(patient.Status),
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt,
            Experts = experts,
            Stats = new VirtualPatientExpertStatsDto
            {
                TotalAttempts = stats?.TotalAttempts ?? 0,
                AvgScore = stats?.AvgScore,
                CompletionRate = stats?.CompletionRate ?? 0m,
            },
        };
    }

    private async Task<VirtualPatientExpertFiltersDto> BuildFiltersAsync(
        CancellationToken cancellationToken
    )
    {
        var levels = await _db
            .VirtualPatients.AsNoTracking()
            .Where(x => x.Level != null)
            .Select(x => x.Level!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var genders = await _db
            .VirtualPatients.AsNoTracking()
            .Where(x => x.Gender != null)
            .Select(x => x.Gender!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var caseIds = await _db
            .VirtualPatients.AsNoTracking()
            .Select(x => x.CaseId)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new VirtualPatientExpertFiltersDto
        {
            AvailableStatuses =
            [
                VirtualPatientConstants.Status.Active,
                VirtualPatientConstants.Status.Draft,
                VirtualPatientConstants.Status.Archived,
                VirtualPatientConstants.Status.Published,
            ],
            AvailableLevels = levels,
            AvailableGenders = genders,
            AvailableCaseIds = caseIds,
        };
    }

    private async Task<Dictionary<string, ExpertPatientStatsSnapshot>> GetStatsByPatientIdsAsync(
        IReadOnlyCollection<string> patientIds,
        CancellationToken cancellationToken
    )
    {
        var ids = patientIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (ids.Count == 0)
            return new Dictionary<string, ExpertPatientStatsSnapshot>();

        var attemptRows = await (
            from session in _db.PracticeSessionRefs.AsNoTracking()
            where ids.Contains(session.PatientId)
            join evaluation in _db.EvaluationRefs.AsNoTracking()
                on session.Id equals evaluation.PracticeSessionId
                into evaluations
            from evaluation in evaluations.DefaultIfEmpty()
            select new
            {
                session.PatientId,
                session.Status,
                Score = (decimal?)evaluation.Score,
            }
        ).ToListAsync(cancellationToken);

        var expertCounts = await _db
            .ExpertVirtualPatientManagements.AsNoTracking()
            .Where(x => ids.Contains(x.VirtualId))
            .GroupBy(x => x.VirtualId)
            .Select(g => new
            {
                PatientId = g.Key,
                Count = g.Select(x => x.ExpertId).Distinct().Count(),
            })
            .ToListAsync(cancellationToken);

        var expertCountMap = expertCounts.ToDictionary(
            x => x.PatientId,
            x => x.Count,
            StringComparer.OrdinalIgnoreCase
        );

        return attemptRows
            .GroupBy(x => x.PatientId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var rows = g.ToList();
                    var scores = rows.Where(x => x.Score.HasValue)
                        .Select(x => x.Score!.Value)
                        .ToList();
                    var totalAttempts = rows.Count;
                    var completedAttempts = rows.Count(x =>
                        string.Equals(
                            x.Status,
                            VirtualPatientConstants.PracticeStatus.Completed,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                    return new ExpertPatientStatsSnapshot
                    {
                        TotalAttempts = totalAttempts,
                        AvgScore = scores.Count == 0 ? null : scores.Average(),
                        CompletionRate =
                            totalAttempts == 0 ? 0m : (decimal)completedAttempts / totalAttempts,
                        ExpertCount = expertCountMap.TryGetValue(g.Key, out var expertCount)
                            ? expertCount
                            : 0,
                    };
                },
                StringComparer.OrdinalIgnoreCase
            );
    }

    private async Task<List<ExpertDto>> GetExpertsByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken
    )
    {
        return await (
            from mapping in _db.ExpertVirtualPatientManagements.AsNoTracking()
            where mapping.VirtualId == patientId
            join expert in _db.Experts.AsNoTracking() on mapping.ExpertId equals expert.ExpertId
            join user in _db.UserRefs.AsNoTracking() on expert.ExpertId equals user.UserId
            select new ExpertDto
            {
                ExpertId = expert.ExpertId,
                Name = user.Name,
                Role = expert.TitlePosition,
                AvatarUrl = user.AvatarUrl,
                BioQuote = expert.BioQuote,
                EducationDetail = expert.EducationDetail,
                ExpertiseSkill = expert.ExpertiseSkill,
                Phone = user.Phone,
                Email = user.Email,
                Location = user.Address,
            }
        ).ToListAsync(cancellationToken);
    }

    private async Task ReplaceExpertsAsync(
        string patientId,
        IReadOnlyCollection<string>? expertIds,
        CancellationToken cancellationToken
    )
    {
        var ids = NormalizeExpertIds(expertIds);

        if (ids.Count > 0)
        {
            await ValidateExpertIdsExistAsync(ids, cancellationToken);
        }

        var existingLinks = await _db
            .ExpertVirtualPatientManagements.Where(x => x.VirtualId == patientId)
            .ToListAsync(cancellationToken);
        if (existingLinks.Count > 0)
            _db.ExpertVirtualPatientManagements.RemoveRange(existingLinks);

        if (ids.Count == 0)
            return;

        _db.ExpertVirtualPatientManagements.AddRange(
            ids.Select(x => new ExpertVirtualPatientManagement
            {
                ExpertId = x,
                VirtualId = patientId,
            })
        );
    }

    private async Task<List<string>> GetExpertIdsByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .ExpertVirtualPatientManagements.AsNoTracking()
            .Where(x => x.VirtualId == patientId)
            .Select(x => x.ExpertId)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private async Task ValidateExpertIdsExistAsync(
        IReadOnlyCollection<string> expertIds,
        CancellationToken cancellationToken
    )
    {
        var validExpertIds = await _db
            .Experts.AsNoTracking()
            .Where(x => expertIds.Contains(x.ExpertId))
            .Select(x => x.ExpertId)
            .ToListAsync(cancellationToken);

        var missingExpertIds = expertIds
            .Except(validExpertIds, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (missingExpertIds.Count > 0)
            throw new InvalidOperationException(
                $"Không tìm thấy expert: {string.Join(", ", missingExpertIds)}"
            );
    }

    private static List<string> NormalizeExpertIds(IReadOnlyCollection<string>? expertIds)
    {
        return expertIds is null
            ? new List<string>()
            : expertIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    private async Task<string> GenerateUniquePatientIdAsync(CancellationToken cancellationToken)
    {
        // First try up to 5 times generating 8-digit IDs
        const int eightDigitTries = 5;
        for (var i = 0; i < eightDigitTries; i++)
        {
            var candidate = GeneratePatientId(8);
            var exists = await _db
                .VirtualPatients.AsNoTracking()
                .AnyAsync(x => x.PatientId == candidate, cancellationToken);
            if (!exists)
                return candidate;
        }

        // If 8-digit candidates keep colliding, fall back to 9-digit IDs
        const int nineDigitTries = 50;
        for (var i = 0; i < nineDigitTries; i++)
        {
            var candidate = GeneratePatientId(9);
            var exists = await _db
                .VirtualPatients.AsNoTracking()
                .AnyAsync(x => x.PatientId == candidate, cancellationToken);
            if (!exists)
                return candidate;
        }

        throw new InvalidOperationException("Không thể tạo patientId duy nhất.");
    }

    private static string GeneratePatientId(int length = 8)
    {
        if (length < 1 || length > 9)
            throw new ArgumentOutOfRangeException(nameof(length), "length must be between 1 and 9");

        // Upper bound is exclusive
        var maxExclusive = (int)Math.Pow(10, length);
        var value = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, maxExclusive);
        return value.ToString($"D{length}");
    }

    private static string? ValidateUpsertRequest(VirtualPatientExpertUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CaseId))
            return "caseId is required";

        if (string.IsNullOrWhiteSpace(request.Name))
            return "name is required";

        return null;
    }

    private static string? NormalizeStatusInput(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        var normalized = status.Trim().ToLowerInvariant();
        return normalized switch
        {
            VirtualPatientConstants.Status.Active => VirtualPatientConstants.Status.Active,
            VirtualPatientConstants.Status.Inactive => VirtualPatientConstants.Status.Inactive,
            VirtualPatientConstants.Status.Draft => VirtualPatientConstants.Status.Draft,
            VirtualPatientConstants.Status.Archived => VirtualPatientConstants.Status.Archived,
            VirtualPatientConstants.Status.Published => VirtualPatientConstants.Status.Published,
            _ => null,
        };
    }

    private static string? NormalizeStatusForResponse(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return status;

        return string.Equals(
            status,
            VirtualPatientConstants.Status.Inactive,
            StringComparison.OrdinalIgnoreCase
        )
            ? VirtualPatientConstants.Status.Archived
            : status.ToLowerInvariant();
    }

    private static string? NormalizeNullableText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("userid")
            ?? User.FindFirstValue("userId")
            ?? User.FindFirstValue("uid");
    }

    private static string? SerializeJson(JsonElement? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? null
            : value.Value.GetRawText();
    }

    private static object? ParseJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(value);
        }
        catch
        {
            return value;
        }
    }

    private sealed class ExpertListRow
    {
        public string PatientId { get; set; } = string.Empty;
        public string CaseId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? ChiefConcern { get; set; }
        public string? Level { get; set; }
        public string? Status { get; set; }
        public string? AvatarImage { get; set; }
        public int? TimeSetting { get; set; }
        public int? ArgumentTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private sealed class ExpertPatientStatsSnapshot
    {
        public int TotalAttempts { get; set; }
        public decimal? AvgScore { get; set; }
        public decimal CompletionRate { get; set; }
        public int ExpertCount { get; set; }
    }
}
