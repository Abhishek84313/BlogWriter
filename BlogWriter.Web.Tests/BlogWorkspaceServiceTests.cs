using BlogWriter.Web.Services;

namespace BlogWriter.Web.Tests;

public sealed class BlogWorkspaceServiceTests
{
    [Fact]
    public async Task NewState_DisablesRevisionInputUntilDraftExists()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));

        Assert.False(workspace.State.IsRevisionInputEnabled);

        await workspace.NewAsync(true);

        Assert.False(workspace.State.IsRevisionInputEnabled);
    }

    [Fact]
    public void RevisionInput_EnablesOnceDraftHasText()
    {
        var state = new BlogWorkspaceState();

        state.Draft = "  ";
        Assert.False(state.IsRevisionInputEnabled);

        state.Draft = "draft";
        Assert.True(state.IsRevisionInputEnabled);
    }

    [Fact]
    public void RevisionInput_StaysEnabledWhenDraftIsCleared()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));
        workspace.State.Draft = "draft";

        workspace.State.Draft = "";

        Assert.True(workspace.State.IsRevisionInputEnabled);
    }

    [Fact]
    public async Task RevisionInput_EnablesWhenListItemIsSelected()
    {
        var sessions = new StubSessionService { Summaries = [CreateSummary("one")] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        await workspace.ListAsync(discardConfirmed: false);

        await workspace.LaunchSelectionAsync("1");

        Assert.True(workspace.State.IsRevisionInputEnabled);
    }

    [Fact]
    public async Task NewAsync_DisablesRevisionInputAgain()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));
        workspace.State.Draft = "draft";

        await workspace.NewAsync(discardConfirmed: true);

        Assert.False(workspace.State.IsRevisionInputEnabled);
    }

    [Fact]
    public async Task DraftState_EnablesRevisionInput()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";

        await workspace.SubmitInitialAsync();

        Assert.True(workspace.State.IsRevisionInputEnabled);
    }

    [Fact]
    public async Task ListUpdates_ReplaceCurrentStatus()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService { Summaries = [CreateSummary("saved")] }, TimeSpan.FromMilliseconds(25));
        await workspace.ListAsync(true);

        Assert.Equal("1 saved sessions loaded.", workspace.State.CurrentStatus);
        Assert.Equal(WorkflowOutputOutcome.Success, workspace.State.CurrentStatusOutcome);
    }
    [Fact]
    public void NewWorkspace_UsesDefaultWordRange()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));

        Assert.Equal("1000", workspace.State.MinWordsInput);
        Assert.Equal("2000", workspace.State.MaxWordsInput);
        Assert.Equal(WordRange.Default, workspace.State.AcceptedRange);
        Assert.False(workspace.State.HasUnsavedRange);
    }

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
    public async Task SubmitAsync_StartsDraftWhenOnlyInitialPromptExists()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "write about testing";

        await workspace.SubmitAsync();

        Assert.Equal(1, sessions.StartCalls);
        Assert.Equal(0, sessions.RevisionCalls);
        Assert.Equal(WorkspaceMode.Draft, workspace.State.Mode);
    }

    [Fact]
    public async Task SubmitAsync_PrioritizesRevisionWhenBothPromptsExist()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        await workspace.SubmitInitialAsync();
        workspace.State.InitialPrompt = "new draft";
        workspace.State.RevisionPrompt = "make it shorter";

        await workspace.SubmitAsync();

        Assert.Equal(1, sessions.StartCalls);
        Assert.Equal(1, sessions.RevisionCalls);
        Assert.Equal("new draft", workspace.State.InitialPrompt);
        Assert.Empty(workspace.State.RevisionPrompt);
    }

    [Fact]
    public async Task SubmitAsync_DoesNotStartWithoutEligibleInput()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));

        await workspace.SubmitAsync();

        Assert.Equal(0, sessions.StartCalls);
        Assert.Equal(0, sessions.RevisionCalls);
    }

    [Fact]
    public async Task LaunchSelectionAsync_RestoresPromptContextAndStartsExactlyOnce()
    {
        var sessions = new StubSessionService
        {
            Summaries = [CreateSummary("saved")],
            SessionToLoad = ListLauncherTestHelpers.Session("saved topic", "tighten the ending"),
        };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.Draft = "old draft";
        workspace.State.Review = "old review";
        workspace.State.InitialPrompt = "old prompt";
        workspace.State.RevisionPrompt = "old revision";

        await workspace.ListAsync(true);
        await workspace.LaunchSelectionAsync("1");

        Assert.Equal(1, sessions.StartCalls);
        Assert.Equal("saved topic", sessions.LastStartPrompt);
        Assert.Equal("tighten the ending", workspace.State.RevisionPrompt);
        Assert.Equal(WorkspaceMode.Draft, workspace.State.Mode);
        Assert.Empty(workspace.State.SelectionError ?? "");
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("9")]
    public async Task LaunchSelectionAsync_InvalidInputDoesNotLoadOrStart(string input)
    {
        var sessions = new StubSessionService { Summaries = [CreateSummary("saved")] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        await workspace.ListAsync(false);

        await workspace.LaunchSelectionAsync(input);

        Assert.Equal(0, sessions.LoadCalls);
        Assert.Equal(0, sessions.StartCalls);
        Assert.NotNull(workspace.State.SelectionError);
    }

    [Fact]
    public async Task SubmitInitialAsync_AppendsLifecycleAndReviewerOutput()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";

        await workspace.SubmitInitialAsync();

        Assert.Contains(workspace.State.WorkflowLog, entry => entry.Message == "Writing in progress...");
        Assert.Contains("review", workspace.State.Review);
        Assert.DoesNotContain("review", workspace.State.WorkflowLog.Select(entry => entry.Message));
    }

    [Fact]
    public async Task ReviewerOutput_AccumulatesAcrossRevisionsAndResetsForNewSession()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        await workspace.SubmitInitialAsync();
        workspace.State.RevisionPrompt = "revise";

        await workspace.SubmitRevisionAsync();

        Assert.Contains("review", workspace.State.Review);
        Assert.Contains("revision review", workspace.State.Review);
        Assert.Equal(2, workspace.State.Review.Split("\n\n", StringSplitOptions.None).Length);

        await workspace.NewAsync(discardConfirmed: true);

        Assert.Empty(workspace.State.Review);
        Assert.Empty(workspace.State.WorkflowLog);
    }

    [Fact]
    public async Task LateOutputFromSupersededOperationDoesNotChangeWorkspace()
    {
        var sessions = new StubSessionService { PendingStart = new TaskCompletionSource<BlogSession>() };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        Task submission = workspace.SubmitInitialAsync();
        await sessions.Started.Task;
        IProgress<WorkflowOutputUpdate> output = sessions.LastOutput!;

        await workspace.NewAsync(discardConfirmed: true);
        output.Report(WorkflowOutputUpdate.Create(
            WorkflowOutputKind.ReviewerFeedback,
            WorkflowOutputOutcome.Review,
            "late review",
            operationVersion: 1,
            sequence: 99,
            updateKey: "late"));
        sessions.PendingStart.SetResult(CreateSession("late"));
        await submission;

        Assert.Empty(workspace.State.Review);
        Assert.DoesNotContain(workspace.State.WorkflowLog, entry => entry.Message == "late review");
    }

    [Fact]
    public async Task DuplicateReviewerOutputIsRenderedOnce()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";

        await workspace.SubmitInitialAsync();
        sessions.LastOutput!.Report(BlogWorkspaceOutputTestHelpers.Review("review", "initial-review"));

        Assert.Single(workspace.State.Review.Split("\n\n", StringSplitOptions.None));
    }

    [Fact]
    public async Task LoadingSessionSeedsReviewerNotesAndClearsTransientLog()
    {
        var sessions = new StubSessionService
        {
            Summaries = [CreateSummary("saved")],
            SessionToLoad = CreateSession("saved draft", "stored review"),
        };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        await workspace.SubmitInitialAsync();
        await workspace.ListAsync(discardConfirmed: true);
        workspace.State.SelectionInput = "1";

        await workspace.LoadSelectionAsync();

        Assert.Equal("stored review", workspace.State.Review);
        Assert.Empty(workspace.State.WorkflowLog);
    }

    [Fact]
    public async Task SubmitInitialAsync_UsesVisibleWordRangeAndAcceptsIt()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        workspace.UpdateMinWords("600");
        workspace.UpdateMaxWords("850");

        await workspace.SubmitInitialAsync();

        Assert.Equal(new WordRange(600, 850), sessions.LastStartRange);
        Assert.Equal(new WordRange(600, 850), workspace.State.AcceptedRange);
        Assert.False(workspace.State.HasUnsavedRange);
    }

    [Theory]
    [InlineData("", "2000")]
    [InlineData("words", "2000")]
    [InlineData("1.5", "2000")]
    [InlineData("0", "2000")]
    [InlineData("-1", "2000")]
    [InlineData("2000", "1000")]
    public async Task SubmitInitialAsync_InvalidRangeDoesNotStartWorkflow(string min, string max)
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        workspace.UpdateMinWords(min);
        workspace.UpdateMaxWords(max);

        await workspace.SubmitInitialAsync();

        Assert.Equal(0, sessions.StartCalls);
        Assert.True(workspace.State.MinWordsError is not null || workspace.State.MaxWordsError is not null);
    }

    [Fact]
    public void CorrectingRange_ClearsErrors()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));

        workspace.UpdateMinWords("0");
        Assert.NotNull(workspace.State.MinWordsError);
        workspace.UpdateMinWords("500");

        Assert.Null(workspace.State.MinWordsError);
        Assert.Null(workspace.State.MaxWordsError);
    }

    [Fact]
    public async Task ListAsync_LeavesRevisionDisabledWithoutDraft()
    {
        var sessions = new StubSessionService { Summaries = [CreateSummary("one"), CreateSummary("two")] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));

        Assert.Equal(WorkspaceTransitionResult.Completed, await workspace.ListAsync(discardConfirmed: false));

        Assert.Equal(WorkspaceMode.List, workspace.State.Mode);
        Assert.True(workspace.State.IsSelectionVisible);
        Assert.False(workspace.State.IsRevisionInputEnabled);
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
    public async Task NewAsync_RequiresConfirmationForUnsavedRangeAndResetsAfterAcceptance()
    {
        var workspace = new BlogWorkspaceService(new StubSessionService(), TimeSpan.FromMilliseconds(25));
        workspace.UpdateMinWords("700");

        Assert.Equal(WorkspaceTransitionResult.RequiresConfirmation, await workspace.NewAsync(false));
        Assert.Equal("700", workspace.State.MinWordsInput);

        Assert.Equal(WorkspaceTransitionResult.Completed, await workspace.NewAsync(true));
        Assert.Equal("1000", workspace.State.MinWordsInput);
        Assert.Equal("2000", workspace.State.MaxWordsInput);
        Assert.False(workspace.State.HasUnsavedRange);
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
        Assert.Contains("review", workspace.State.Review);
        Assert.Contains("revision review", workspace.State.Review);
        Assert.Empty(workspace.State.RevisionPrompt);
    }

    [Fact]
    public async Task LoadSelectionAsync_PopulatesStoredWordRange()
    {
        var sessions = new StubSessionService
        {
            Summaries = [CreateSummary("one")],
            SessionToLoad = CreateSession("loaded", 700, 900),
        };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        await workspace.ListAsync(false);
        workspace.State.SelectionInput = "1";

        await workspace.LoadSelectionAsync();

        Assert.Equal("700", workspace.State.MinWordsInput);
        Assert.Equal("900", workspace.State.MaxWordsInput);
        Assert.Equal(new WordRange(700, 900), workspace.State.AcceptedRange);
    }

    [Fact]
    public async Task SubmitRevisionAsync_UsesChangedRange()
    {
        var sessions = new StubSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        await workspace.SubmitInitialAsync();
        workspace.UpdateMinWords("650");
        workspace.UpdateMaxWords("750");
        workspace.State.RevisionPrompt = "shorten it";

        await workspace.SubmitRevisionAsync();

        Assert.Equal(new WordRange(650, 750), sessions.LastRevisionRange);
        Assert.Equal(new WordRange(650, 750), workspace.State.AcceptedRange);
    }

    [Fact]
    public async Task ProcessingEditsRemainVisibleAndUnsavedAfterSubmittedRangeCompletes()
    {
        var sessions = new StubSessionService { PendingStart = new TaskCompletionSource<BlogSession>() };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        workspace.State.InitialPrompt = "topic";
        workspace.UpdateMinWords("500");
        workspace.UpdateMaxWords("900");
        Task submission = workspace.SubmitInitialAsync();
        await sessions.Started.Task;

        workspace.UpdateMinWords("600");
        sessions.PendingStart.SetResult(CreateSession("draft", 500, 900));
        await submission;

        Assert.Equal(new WordRange(500, 900), workspace.State.AcceptedRange);
        Assert.Equal("600", workspace.State.MinWordsInput);
        Assert.True(workspace.State.HasUnsavedRange);
    }

    private static BlogSessionSummary CreateSummary(string task) =>
        new(Guid.NewGuid().ToString("N"), task, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    private static BlogSession CreateSession(string draft, string review) =>
        CreateSession(draft, ResearchState.DefaultMinWords, ResearchState.DefaultMaxWords, review);

    private static BlogSession CreateSession(
        string draft,
        int minWords = ResearchState.DefaultMinWords,
        int maxWords = ResearchState.DefaultMaxWords,
        string review = "review") => new()
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnerId = "owner",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            State = new ResearchState
            {
                MainTask = "topic",
                MinWords = minWords,
                MaxWords = maxWords,
                Draft = draft,
                ReviewNotes = review,
            },
        };

    private sealed class StubSessionService : IBlogWriterSessionService
    {
        public int StartCalls { get; private set; }
        public int RevisionCalls { get; private set; }
        public string? LastStartPrompt { get; private set; }
        public int LoadCalls { get; private set; }
        public WordRange? LastStartRange { get; private set; }
        public WordRange? LastRevisionRange { get; private set; }
        public IReadOnlyList<BlogSessionSummary> Summaries { get; set; } = [];
        public BlogSession? SessionToLoad { get; init; }
        public TaskCompletionSource<BlogSession>? PendingStart { get; init; }
        public TaskCompletionSource Started { get; } = new();
        public IProgress<WorkflowOutputUpdate>? LastOutput { get; private set; }

        public Task<BlogSession> StartAsync(string prompt, int minWords = ResearchState.DefaultMinWords, int maxWords = ResearchState.DefaultMaxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null)
        {
            StartCalls++;
            LastStartPrompt = prompt;
            LastOutput = output;
            output?.Report(WorkflowOutputUpdate.Create(
                WorkflowOutputKind.ReviewerFeedback,
                WorkflowOutputOutcome.Review,
                "review",
                operationVersion: 1,
                sequence: 1,
                updateKey: "initial-review"));
            LastStartRange = new WordRange(minWords, maxWords);
            Started.TrySetResult();
            return PendingStart?.Task ?? Task.FromResult(CreateSession($"draft: {prompt}", minWords, maxWords));
        }

        public Task<BlogSession> ReviseAsync(
            BlogSession session,
            string revision,
            int minWords,
            int maxWords,
            CancellationToken cancellationToken = default,
            IProgress<WorkflowOutputUpdate>? output = null)
        {
            RevisionCalls++;
            LastOutput = output;
            output?.Report(WorkflowOutputUpdate.Create(
                WorkflowOutputKind.ReviewerFeedback,
                WorkflowOutputOutcome.Review,
                "revision review",
                operationVersion: 2,
                sequence: 1,
                updateKey: "revision-review"));
            LastRevisionRange = new WordRange(minWords, maxWords);
            return Task.FromResult(new BlogSession
            {
                Id = session.Id,
                OwnerId = session.OwnerId,
                CreatedAt = session.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                State = new ResearchState
                {
                    MainTask = session.State.MainTask,
                    MinWords = minWords,
                    MaxWords = maxWords,
                    Draft = $"revised: {revision}",
                    ReviewNotes = "revision review",
                },
            });
        }

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Summaries);

        public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            LoadCalls++;
            return Task.FromResult<BlogSession?>(SessionToLoad ?? CreateSession("loaded"));
        }
    }
}
