namespace BlogWriter.Web.Tests;

internal static class ListLauncherTestHelpers
{
    public static BlogSessionSummary Summary(string task) =>
        new(Guid.NewGuid().ToString("N"), task, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    public static BlogSession Session(
        string mainTask,
        string currentSubTask = "",
        string draft = "draft",
        string review = "review") => new()
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnerId = "owner",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            State = new ResearchState
            {
                MainTask = mainTask,
                CurrentSubTask = currentSubTask,
                Draft = draft,
                ReviewNotes = review,
            },
        };
}
