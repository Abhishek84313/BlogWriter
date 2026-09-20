using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlogWriter.Tests;

public sealed class BlogWorkflowTests
{
    [Fact]
    public async Task RunAsync_EmitsLifecycleAndReviewerUpdatesWithoutChangingFinalState()
    {
        var workflow = new BlogWorkflow(
            new TestBlogger(),
            new TestResearcher(),
            new TestAuthor(),
            new TestReviewer(),
            NullLogger<BlogWorkflow>.Instance);
        var output = new WorkflowOutputCollector();

        var service = new BlogWriterSessionService(workflow, new RecordingStore());
        BlogSession session = await service.StartAsync("topic", output: output);
        ResearchState result = session.State;

        Assert.Equal("draft", result.Draft);
        Assert.Equal("APPROVED", result.ReviewNotes);
        Assert.Contains(output.Updates, update =>
            update.Kind == WorkflowOutputKind.Lifecycle &&
            update.Outcome == WorkflowOutputOutcome.Progress);
        Assert.Contains(output.Updates, update =>
            update.Kind == WorkflowOutputKind.ReviewerFeedback &&
            update.Message == "APPROVED");
        Assert.Contains(output.Updates, update =>
            update.Kind == WorkflowOutputKind.Lifecycle &&
            update.Outcome == WorkflowOutputOutcome.Success);
        Assert.Equal(output.Updates.Count, output.Updates.Select(update => update.Sequence).Distinct().Count());
    }

    private sealed class TestBlogger : IBloggerAgent
    {
        public Task<BloggerDecision> InvokeAsync(ResearchState state, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BloggerDecision("research", state.MainTask));

        public Task<ResearchState> BloggerNodeAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            state.NextStep = "research";
            state.CurrentSubTask = state.MainTask;
            return Task.FromResult(state);
        }
    }

    private sealed class TestResearcher : IResearcherAgent
    {
        public Task<string> InvokeAsync(string query, CancellationToken cancellationToken = default) =>
            Task.FromResult("finding");

        public Task<ResearchState> ResearchNodeAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            state.ResearchFindings.Add("finding");
            return Task.FromResult(state);
        }
    }

    private sealed class TestAuthor : IAuthorAgent
    {
        public Task<string?> InvokeAsync(ResearchState state, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>("draft");

        public Task<ResearchState> AuthorNodeAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            state.Draft = "draft";
            return Task.FromResult(state);
        }
    }

    private sealed class TestReviewer : IReviewerAgent
    {
        public Task<string> InvokeAsync(ResearchState state, CancellationToken cancellationToken = default) =>
            Task.FromResult("APPROVED");

        public Task<ResearchState> ReviewerNodeAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            state.ReviewNotes = "APPROVED";
            return Task.FromResult(state);
        }
    }

    private sealed class RecordingStore : IBlogSessionStore
    {
        public Task<BlogSession> CreateAsync(ResearchState state, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BlogSession
            {
                Id = Guid.NewGuid().ToString("N"),
                OwnerId = "owner",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                State = state,
            });

        public Task<BlogSession?> GetAsync(string sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<BlogSession?>(null);

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BlogSessionSummary>>([]);

        public Task SaveAsync(BlogSession session, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteOwnerSessionsAsync(string ownerId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
