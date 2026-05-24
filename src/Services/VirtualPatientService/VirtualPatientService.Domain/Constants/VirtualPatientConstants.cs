namespace VirtualPatientService.Domain.Constants;

public static class VirtualPatientConstants
{
    public const int MaxAttemptsAllowed = 3;
    public const int DefaultPageSize = 20;
    public const int DefaultDiscoveryPageSize = 9;
    public const int MaxPageSize = 100;

    public static class SortOptions
    {
        public const string Newest = "newest";
        public const string Oldest = "oldest";
        public const string LevelAsc = "level_asc";
        public const string LevelDesc = "level_desc";
        public const string ScoreDesc = "score_desc";
        public const string ScoreAsc = "score_asc";
        public const string MostPracticed = "most_practiced";
        public const string HighestFeedback = "highest_feedback";
        public const string ExpertAsc = "expert_asc";
        public const string ExpertDesc = "expert_desc";

        public static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
        {
            Newest,
            Oldest,
            LevelAsc,
            LevelDesc,
            ScoreDesc,
            ScoreAsc,
            MostPracticed,
            HighestFeedback,
            ExpertAsc,
            ExpertDesc,
        };
    }

    public static class Status
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Draft = "draft";
        public const string Archived = "archived";
        public const string Published = "published";

        public static readonly HashSet<string> ExpertStatuses = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            Active,
            Inactive,
            Draft,
            Archived,
            Published,
        };
    }

    public static class PracticeStatus
    {
        public const string Practicing = "Practicing";
        public const string VpCompleted = "VpCompleted";
        public const string ReasoningStarted = "ReasoningStarted";
        public const string Submitted = "Submitted";
        public const string Completed = "Completed";
        public const string Abandoned = "Abandoned";
    }
}
