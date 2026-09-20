using Xunit;

namespace BlogWriter.Tests;

public sealed class WorkflowOutputUpdateTests
{
    [Fact]
    public void Create_RequiresUserVisibleMessage()
    {
        Assert.Throws<ArgumentException>(() => WorkflowOutputUpdate.Create(
            WorkflowOutputKind.Lifecycle,
            WorkflowOutputOutcome.Progress,
            "",
            operationVersion: 1,
            sequence: 1,
            updateKey: "op-1"));
    }

    [Fact]
    public void Create_PreservesRoutingAndOrderingMetadata()
    {
        WorkflowOutputUpdate update = WorkflowOutputUpdate.Create(
            WorkflowOutputKind.ReviewerFeedback,
            WorkflowOutputOutcome.Review,
            "Needs a stronger conclusion.",
            operationVersion: 4,
            sequence: 7,
            updateKey: "op-4-review-1",
            revisionNumber: 1);

        Assert.Equal(WorkflowOutputKind.ReviewerFeedback, update.Kind);
        Assert.Equal(WorkflowOutputOutcome.Review, update.Outcome);
        Assert.Equal("Needs a stronger conclusion.", update.Message);
        Assert.Equal(4, update.OperationVersion);
        Assert.Equal(7, update.Sequence);
        Assert.Equal("op-4-review-1", update.UpdateKey);
        Assert.Equal(1, update.RevisionNumber);
    }

    [Fact]
    public void Create_RequiresStableUpdateKey()
    {
        Assert.Throws<ArgumentException>(() => WorkflowOutputUpdate.Create(
            WorkflowOutputKind.Lifecycle,
            WorkflowOutputOutcome.Success,
            "Complete",
            operationVersion: 1,
            sequence: 1,
            updateKey: " "));
    }
}
