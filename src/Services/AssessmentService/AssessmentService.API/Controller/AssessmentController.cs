using Microsoft.AspNetCore.Mvc;
using MediatR;
using AssessmentService.Application.Commands.CreateAssessment;
using AssessmentService.Application.Commands.UpdateAssessment;
using AssessmentService.Application.Commands.DeleteAssessment;
using AssessmentService.Application.Commands.GenerateQuestions;
using AssessmentService.Application.Commands.Questions.CreateQuestion;
using AssessmentService.Application.Commands.Questions.UpdateQuestion;
using AssessmentService.Application.Commands.Questions.DeleteQuestion;
using AssessmentService.Application.Queries.GetPagedAssessments;
using AssessmentService.Application.Queries.GetAllAssessments;
using AssessmentService.Application.Queries.GetAssessmentById;
using AssessmentService.Application.Commands.CreateFullAssessment;
using AssessmentService.Application.Commands.SubmitAssessment;
using AssessmentService.Application.Queries.GetAttemptDetails;
using AssessmentService.Application.Queries.GetAllAttempts;
using AssessmentService.Application.Queries.GetAllAssessmentsOverviewOfLearner;
using AssessmentService.Application.Queries.GetAssessmentOverviewAnalytics;
using AssessmentService.Application.Queries.GetAssessmentByUserId;

namespace AssessmentService.API.Controllers;

[ApiController]
[Route("api/assessments")]
public class AssessmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssessmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentCommand command)
    {
        var assessmentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = assessmentId }, new
        {
            message = "Assessment created successfully.",
            assessmentId = assessmentId
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? specialty,
        [FromQuery] string? difficultyLevel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetPagedAssessmentsQuery(specialty, difficultyLevel, page, pageSize));
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllAssessmentsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetAssessmentByIdQuery(id));

        if (result == null)
            return NotFound(new { message = $"Don't find assessment with ID: {id}" });

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAssessmentCommand command)
    {
        if (id != command.AssessmentId)
            return BadRequest(new { message = "ID on URL and in Body payload do not match." });

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = $"Don't find assessment with ID: {id}" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteAssessmentCommand(id));

        if (!result)
            return NotFound(new { message = $"Don't find assessment with ID: {id}" });

        return NoContent();
    }
    [HttpPost("full-generation")]
    public async Task<IActionResult> CreateFullAssessment([FromBody] CreateFullAssessmentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                message = "Generating full assessment successful.",
                data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/generate-questions")]
    public async Task<IActionResult> GenerateQuestionsWithAI(string id, [FromBody] GenerateQuestionsRequest req)
    {
        var result = await _mediator.Send(new GenerateAssessmentQuestionsCommand(id, req.AdditionalPrompt));

        if (!result)
            return BadRequest(new { message = "Cannot generate questions or assessment ID does not exist." });

        return Ok(new { message = "Successfully generated and saved questions to the database." });
    }

    [HttpPost("{id}/questions")]
    public async Task<IActionResult> CreateQuestion(string id, [FromBody] CreateQuestionCommand command)
    {
        if (id != command.AssessmentId)
            return BadRequest(new { message = "AssessmentId on URL and Body do not match." });

        try
        {
            var questionId = await _mediator.Send(command);
            return Ok(new { message = "Generated question successfully.", questionId = questionId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("questions/{questionId}")]
    public async Task<IActionResult> UpdateQuestion(string questionId, [FromBody] UpdateQuestionCommand command)
    {
        if (questionId != command.QuestionId)
            return BadRequest(new { message = "QuestionId on URL and Body do not match." });

        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = $"Don't find question with ID: {questionId}" });

        return NoContent();
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(string questionId)
    {
        var result = await _mediator.Send(new DeleteQuestionCommand(questionId));

        if (!result)
            return NotFound(new { message = $"Don't find question with ID: {questionId}" });

        return NoContent();
    }


    [HttpPost("api/attempts/submit")]
    public async Task<IActionResult> SubmitAssessment([FromBody] SubmitAssessmentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                message = "Submit assessment successful.",
                data = result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    [HttpGet("attempts/{attemptId}")]
    public async Task<IActionResult> GetAttemptDetail(string attemptId)
    {
        var result = await _mediator.Send(new GetAttemptDetailQuery(attemptId));

        if (result == null)
            return NotFound(new { message = "Không tìm thấy kết quả lượt thi." });

        return Ok(new { data = result });
    }

    [HttpGet("{assessmentId}/learner/{learnerId}/attempts")]
    public async Task<IActionResult> GetAllAttempts(string assessmentId, string learnerId)
    {
        var result = await _mediator.Send(new GetAllAttemptsQuery(assessmentId, learnerId));

        if (result == null || result.Count == 0)
            return NotFound(new { message = "Không tìm thấy lượt thi nào cho learner này." });

        return Ok(new { data = result });
    }

    [HttpGet("learner/{learnerId}")]
    public async Task<IActionResult> GetAllAssessmentsOverviewOfLearner([FromRoute] string learnerId)
    {
        var result = await _mediator.Send(new GetAllAssessmentsOverviewOfLearnerQuery(learnerId));
        return Ok(result);
    }
    
    [HttpGet("learner/{learnerId}/analytics")]
    public async Task<IActionResult> GetAssessmentOverviewAnalytics([FromRoute] string learnerId)
    {
        var result = await _mediator.Send(new GetAssessmentOverviewAnalyticsQuery(learnerId));
        return Ok(result);
    }
    
    [HttpGet("{assessmentId}/learner/{learnerId}")]
    public async Task<IActionResult> GetAssesmentDetailByUserId(string assessmentId, string learnerId)
    {
        var result = await _mediator.Send(new GetAssessmentByUserIdQuery(assessmentId, learnerId));

        return Ok(result);
    }
}

public class GenerateQuestionsRequest
{
    public string AdditionalPrompt { get; set; } = string.Empty;
}