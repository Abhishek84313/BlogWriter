using BlogWriter.Web.Services;

namespace BlogWriter.Web.Tests;

internal static class RevisionStatusTestHelpers
{
    public static BlogSession Session(string draft = "draft") => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        OwnerId = "owner",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        State = new ResearchState { MainTask = "topic", Draft = draft, ReviewNotes = "review" },
    };

    public static WorkflowOutputUpdate Lifecycle(string message, WorkflowOutputOutcome outcome = WorkflowOutputOutcome.Progress) =>
        WorkflowOutputUpdate.Create(WorkflowOutputKind.Lifecycle, outcome, message, 1, 1, $"status-{Guid.NewGuid():N}");
}
