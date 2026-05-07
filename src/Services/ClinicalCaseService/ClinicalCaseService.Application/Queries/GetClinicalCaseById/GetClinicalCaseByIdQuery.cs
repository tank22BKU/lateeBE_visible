using ClinicalCaseService.Application.Queries.GetClinicalCases;
using MediatR;

namespace ClinicalCaseService.Application.Queries.GetClinicalCaseById;

public record GetClinicalCaseByIdQuery(string CaseId) : IRequest<ClinicalCaseDto?>;