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
            message = "Tạo Assessment thành công.", 
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
            return NotFound(new { message = $"Không tìm thấy Assessment với ID: {id}" });

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAssessmentCommand command)
    {
        if (id != command.AssessmentId)
            return BadRequest(new { message = "ID trên URL và trong Body payload không khớp." });

        var result = await _mediator.Send(command);
        
        if (!result) 
            return NotFound(new { message = $"Không tìm thấy Assessment với ID: {id}" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteAssessmentCommand(id));
        
        if (!result) 
            return NotFound(new { message = $"Không tìm thấy Assessment với ID: {id}" });

        return NoContent();
    }

    [HttpPost("{id}/generate-questions")]
    public async Task<IActionResult> GenerateQuestionsWithAI(string id, [FromBody] GenerateQuestionsRequest req)
    {
        var result = await _mediator.Send(new GenerateAssessmentQuestionsCommand(id, req.CustomPrompt));
        
        if (!result) 
            return BadRequest(new { message = "Không thể tạo câu hỏi hoặc ID bài test không tồn tại." });

        return Ok(new { message = "Đã sinh và lưu thành công ngân hàng câu hỏi vào Database." });
    }

    [HttpPost("{id}/questions")]
    public async Task<IActionResult> CreateQuestion(string id, [FromBody] CreateQuestionCommand command)
    {
        if (id != command.AssessmentId)
            return BadRequest(new { message = "AssessmentId trên URL và Body không khớp." });

        try
        {
            var questionId = await _mediator.Send(command);
            return Ok(new { message = "Tạo câu hỏi thành công.", questionId = questionId });
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
            return BadRequest(new { message = "QuestionId trên URL và Body không khớp." });

        var result = await _mediator.Send(command);
        
        if (!result) 
            return NotFound(new { message = $"Không tìm thấy câu hỏi với ID: {questionId}" });

        return NoContent();
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> DeleteQuestion(string questionId)
    {
        var result = await _mediator.Send(new DeleteQuestionCommand(questionId));
        
        if (!result) 
            return NotFound(new { message = $"Không tìm thấy câu hỏi với ID: {questionId}" });

        return NoContent();
    }
}

public class GenerateQuestionsRequest
{
    public string CustomPrompt { get; set; } = string.Empty;
}