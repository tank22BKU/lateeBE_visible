using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EvaluationService.API.Controllers;
using EvaluationService.Application.Commands.SubmitEvaluation;
using EvaluationService.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EvaluationService.API.Tests;

public class EvaluationControllerTests
{
	[Fact]
	public async Task Submit_ReturnsOk_WithExpectedMessageAndData()
	{
		var mediator = new Mock<IMediator>();
		var expected = new SubmitEvaluationResultDto
		{
			EvaluationId = "EVAL-001",
			PracticeSessionId = "SESS_1715050000000",
			Score = 85.5m,
			EntrustmentLevel = 4,
			FeedbackDetail = "Good clinical reasoning.",
			FinalDiagnosis = "Appendicitis",
			DiagnosisMatchType = "Exact",
			DiscussionType = "Message Type",
			Duration = 30,
			PracticeFeedbackAvailable = true
		};

		mediator
			.Setup(x => x.Send(It.IsAny<SubmitEvaluationCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		var controller = new EvaluationController(mediator.Object);
		var command = new SubmitEvaluationCommand(
			"SESS_1715050000000",
			"USR-LRN-01",
			"Appendicitis",
			"{...}",
			"{...}",
			"Message Type",
			"EPA_STANDARD_V1",
			[
				new WarningDto
				{
					WarningId = "W-001",
					PracticeSessionId = "SESS_1715050000000",
					LearnerId = "USR-LRN-01",
					Label = "Incomplete HPI",
					Description = "Missing onset details.",
					CreatedAt = DateTime.Parse("2026-05-14T10:29:26.412Z")
				}
			]);

		var result = await controller.Submit(command);

		var ok = Assert.IsType<OkObjectResult>(result);
		var response = ok.Value!;

		var message = (string?)response.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance)
			?.GetValue(response);
		var data = response.GetType().GetProperty("data", BindingFlags.Public | BindingFlags.Instance)
			?.GetValue(response) as SubmitEvaluationResultDto;

		Assert.Equal("Evaluation saved successfully.", message);
		Assert.NotNull(data);
		Assert.Equal(expected.EvaluationId, data!.EvaluationId);
		Assert.Equal(expected.PracticeSessionId, data.PracticeSessionId);
		Assert.Equal(expected.Score, data.Score);
		Assert.Equal(expected.FinalDiagnosis, data.FinalDiagnosis);
	}
}