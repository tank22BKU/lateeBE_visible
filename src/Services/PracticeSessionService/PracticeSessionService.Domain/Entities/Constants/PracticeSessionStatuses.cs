namespace PracticeSessionService.Domain.Entities.Constants;

public static class PracticeSessionStatuses
{
	public const string Practicing = "Practicing";
	public const string VpCompleted = "VpCompleted";
	public const string ReasoningStarted = "ReasoningStarted";
	public const string Completed = "Completed";
	public const string Abandoned = "Abandoned";

	public static readonly string[] ActiveStatuses =
	[
		Practicing,
		VpCompleted,
		ReasoningStarted
	];

	public static readonly string[] AttemptStatuses =
	[
		Practicing,
		Completed
	];

	public static readonly string[] AllStatuses =
	[
		Practicing,
		VpCompleted,
		ReasoningStarted,
		Completed,
		Abandoned
	];
}