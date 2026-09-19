using BlogWriter.Web.Services;

namespace BlogWriter.Web.Tests;

public sealed class BlogWorkspaceServiceTests
{
    [Fact]
    public async Task SubmitInitialAsync_PublishesCompletedDraftAndReview()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "write about testing";

        await workspace.SubmitInitialAsync();

        Assert.Equal(WorkspaceMode.Draft, workspace.State.Mode);
        Assert.Equal("draft: write about testing", workspace.State.Draft);
        Assert.Equal("review", workspace.State.Review);
        Assert.False(workspace.State.IsProcessing);
        Assert.Equal(1, sessions.StartCalls);
    }

    [Fact]
    public async Task ListAsync_EnablesSelectionOnlyForNonEmptyCurrentList()
    {
        var sessions = new StubSessionService { Summaries = [CreateSummary("one"), CreateSummary("two")] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));

        Assert.Equal(WorkspaceTransitionResult.Completed, await workspace.ListAsync(discardConfirmed: false));

        Assert.Equal(WorkspaceMode.List, workspace.State.Mode);
        Assert.True(workspace.State.IsSelectionVisible);
        Assert.True(workspace.State.IsReviseEnabled);
        Assert.Equal(2, workspace.State.DisplayedSessions.Count);
    }

    [Fact]
    public async Task ListAsync_CapsDisplayedSessionsAtTwenty()
    {
        var sessions = new StubSessionService
        {
            Summaries = Enumerable.Range(1, 21).Select(index => CreateSummary($"topic {index}")).ToList(),
        };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));

        await workspace.ListAsync(discardConfirmed: false);

        Assert.Equal(20, workspace.State.DisplayedSessions.Count);
    }

    [Fact]
    public async Task LoadSelectionAsync_RejectsInvalidValueWithoutReplacingStableOutput()
    {
        var sessions = new StubSessionService { Summaries = [CreateSummary("one")] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.Draft = "stable";
        workspace.State.Review = "stable review";
        await workspace.ListAsync(discardConfirmed: false);
        workspace.State.SelectionInput = "0";

        await workspace.LoadSelectionAsync();

        Assert.Equal("stable", workspace.State.Draft);
        Assert.Equal("stable review", workspace.State.Review);
        Assert.NotNull(workspace.State.ValidationMessage);
        Assert.Equal(0, sessions.LoadCalls);
    }

    [Fact]
    public async Task NewAsync_RequiresConfirmationForUnsavedText()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "unsaved";

        WorkspaceTransitionResult result = await workspace.NewAsync(discardConfirmed: false);

        Assert.Equal(WorkspaceTransitionResult.RequiresConfirmation, result);
        Assert.Equal("unsaved", workspace.State.InitialPrompt);
    }

    [Fact]
    public async Task NewAsync_TimesOutCancellationAndSuppressesLateResult()
    {
        var sessions = new StubSessionService { PendingStart = new TaskCompletionSource<BlogSession>() };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        Task submit = workspace.SubmitInitialAsync();
        await sessions.Started.Task;

        Assert.Equal(WorkspaceTransitionResult.Completed, await workspace.NewAsync(discardConfirmed: true));
        Assert.Equal(WorkspaceMode.New, workspace.State.Mode);

        sessions.PendingStart.SetResult(CreateSession("late draft"));
        await submit;

        Assert.Equal(WorkspaceMode.New, workspace.State.Mode);
        Assert.Empty(workspace.State.Draft);
    }

    [Fact]
    public async Task SubmitInitialAsync_RejectsDuplicateWhileOperationIsActive()
    {
        var sessions = new StubSessionService { PendingStart = new TaskCompletionSource<BlogSession>() };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        Task first = workspace.SubmitInitialAsync();
        await sessions.Started.Task;

        await workspace.SubmitInitialAsync();

        Assert.Equal(1, sessions.StartCalls);
        Assert.Contains("already in progress", workspace.State.ValidationMessage);
        sessions.PendingStart.SetCanceled();
        await first;
    }

    [Fact]
    public async Task QuitAsync_EndsWorkspaceAndRejectsFurtherSubmission()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));

        await workspace.QuitAsync(discardConfirmed: true);
        workspace.State.InitialPrompt = "ignored";
        await workspace.SubmitInitialAsync();

        Assert.Equal(WorkspaceMode.Ended, workspace.State.Mode);
        Assert.NotNull(workspace.State.ValidationMessage);
    }

    [Fact]
    public async Task SubmitRevisionAsync_PublishesCompletedRevision()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        await workspace.SubmitInitialAsync();
        workspace.State.RevisionPrompt = "make it shorter";

        await workspace.SubmitRevisionAsync();

        Assert.Equal("revised: make it shorter", workspace.State.Draft);
        Assert.Equal("revision review", workspace.State.Review);
        Assert.Empty(workspace.State.RevisionPrompt);
    }

    private static BlogSessionSummary CreateSummary(string task) =>
        new(Guid.NewGuid().ToString("N"), task, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    private static BlogSession CreateSession(string draft) => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        OwnerId = "owner",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        State = new ResearchState { MainTask = "topic", Draft = draft, ReviewNotes = "review" },
    };

    private sealed class StubSessionService : IBlogWriterSessionService
    {
        public int StartCalls { get; private set; }
        public int LoadCalls { get; private set; }
        public IReadOnlyList<BlogSessionSummary> Summaries { get; set; } = [];
        public TaskCompletionSource<BlogSession>? PendingStart { get; init; }
        public TaskCompletionSource Started { get; } = new();

        public Task<BlogSession> StartAsync(string prompt, int minWords = ResearchState.DefaultMinWords, int maxWords = ResearchState.DefaultMaxWords, CancellationToken cancellationToken = default)
        {
            StartCalls++;
            Started.TrySetResult();
            return PendingStart?.Task ?? Task.FromResult(CreateSession($"draft: {prompt}"));
        }

        public Task<BlogSession> ReviseAsync(BlogSession session, string revision, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BlogSession
            {
                Id = session.Id,
                OwnerId = session.OwnerId,
                CreatedAt = session.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                State = new ResearchState
                {
                    MainTask = session.State.MainTask,
                    Draft = $"revised: {revision}",
                    ReviewNotes = "revision review",
                },
            });

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Summaries);

        public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            LoadCalls++;
            return Task.FromResult<BlogSession?>(CreateSession("loaded"));
        }
    }
}
