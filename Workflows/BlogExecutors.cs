using Microsoft.Agents.AI.Workflows;

namespace BlogWriter;

// MAF workflow executors. Each wraps one of the existing "node" chains so the
// tested business logic is reused unchanged. A [MessageHandler] returning
// ValueTask<ResearchState> auto-routes the returned state to downstream edges.

/// <summary>Entry executor: lets the blogger plan the task and seed the sub-task.</summary>
internal sealed partial class BloggerExecutor(IBloggerAgent blogger) : Executor("Blogger")
{
    [MessageHandler]
    private async ValueTask<ResearchState> HandleAsync(ResearchState state, IWorkflowContext context, CancellationToken cancellationToken)
        => await blogger.BloggerNodeAsync(state, cancellationToken);
}

/// <summary>Gathers research findings.</summary>
internal sealed partial class ResearcherExecutor(IResearcherAgent researcher) : Executor("Researcher")
{
    [MessageHandler]
    private async ValueTask<ResearchState> HandleAsync(ResearchState state, IWorkflowContext context, CancellationToken cancellationToken)
        => await researcher.ResearchNodeAsync(state, cancellationToken);
}

/// <summary>Writes or revises the draft (increments the revision counter).</summary>
internal sealed partial class AuthorExecutor(IAuthorAgent author) : Executor("Author")
{
    [MessageHandler]
    private async ValueTask<ResearchState> HandleAsync(ResearchState state, IWorkflowContext context, CancellationToken cancellationToken)
    {
        state = await author.AuthorNodeAsync(state, cancellationToken);

        if (state.RevisionNumber >= ResearchState.MaxRevisions)
        {
            await context.YieldOutputAsync(state);
        }

        return state;
    }
}

/// <summary>
/// Reviews the initial draft and records approval / revision notes. It yields
/// approved output; a rejected draft routes to the author for one revision.
/// </summary>
internal sealed partial class ReviewerExecutor(
    IReviewerAgent reviewer,
    WorkflowOutputPublisher? publisher) : Executor("Reviewer")
{
    [MessageHandler]
    private async ValueTask<ResearchState> HandleAsync(ResearchState state, IWorkflowContext context, CancellationToken cancellationToken)
    {
        state = await reviewer.ReviewerNodeAsync(state, cancellationToken);

        if (!string.IsNullOrWhiteSpace(state.ReviewNotes))
        {
            publisher?.PublishReviewer(state.ReviewNotes, state.RevisionNumber);
        }

        if (!state.NeedsRevision)
        {
            // Approval is terminal here. A capped revision is emitted by the author.
            await context.YieldOutputAsync(state);
        }

        // Rejected initial state is routed back to the author only when the loop
        // edge condition (NeedsRevision) is satisfied.
        return state;
    }
}
