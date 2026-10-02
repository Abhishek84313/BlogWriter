using BlogWriter.Web.Services;

namespace BlogWriter.Web.Tests;

internal static class BlogWorkspaceOutputTestHelpers
{
    public static WorkflowOutputUpdate Lifecycle(
        string message,
        long operationVersion = 1,
        long sequence = 1,
        WorkflowOutputOutcome outcome = WorkflowOutputOutcome.Progress) =>
        WorkflowOutputUpdate.Create(
            WorkflowOutputKind.Lifecycle,
            outcome,
            message,
            operationVersion,
            sequence,
            $"lifecycle-{operationVersion}-{sequence}");

    public static WorkflowOutputUpdate Review(
        string message,
        string updateKey,
        long operationVersion = 1,
        long sequence = 1,
        int revisionNumber = 0) =>
        WorkflowOutputUpdate.Create(
            WorkflowOutputKind.ReviewerFeedback,
            WorkflowOutputOutcome.Review,
            message,
            operationVersion,
            sequence,
            updateKey,
            revisionNumber);
}
