using Microsoft.AspNetCore.Mvc;
using MediatR;
using EvaluationService.Application.Commands.DeleteEvaluation;
using EvaluationService.Application.Commands.GeneratePracticeFeedback;
using EvaluationService.Application.Queries.GetReport;
using EvaluationService.Application.Queries.GetHistory;
using EvaluationService.Application.Commands.SubmitEvaluation;

namespace EvaluationService.API.Controllers;

[ApiController]
[Route("api/evaluation")]
public class EvaluationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EvaluationController(IMediator mediator) => _mediator = mediator;

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitEvaluationCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return Ok(new { message = "Evaluation saved successfully.", data = result });
    }

    [HttpGet("{userId}/history")]
    public async Task<IActionResult> GetHistory(string userId)
        => Ok(await _mediator.Send(new GetUserHistoryQuery(userId)));

    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetReport(string id)
    {
        var res = await _mediator.Send(new GetEvaluationReportQuery(id));
        return res != null ? Ok(res) : NotFound(new { message = $"Evaluation '{id}' not found." });
    }

    [HttpPost("practice-feedback/{practiceSessionId}")]
    public async Task<IActionResult> GeneratePracticeFeedback(string practiceSessionId)
    {
        var result = await _mediator.Send(new GeneratePracticeFeedbackCommand(practiceSessionId));
        return Ok(new
        {
            message = result.WasCached
                ? "Feedback retrieved from cache."
                : "Feedback generated successfully.",
            data = result
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _mediator.Send(new DeleteEvaluationCommand(id));
        return deleted
            ? NoContent()
            : NotFound(new { message = $"Evaluation '{id}' not found." });
    }
}