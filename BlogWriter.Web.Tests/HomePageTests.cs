using Bunit;
using BlogWriter.Web.Components.Pages;
using BlogWriter.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace BlogWriter.Web.Tests;

public sealed class HomePageTests : BunitContext
{
    public HomePageTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Home_RendersWritingWorkspaceAndFourCommands()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();

        IRenderedComponent<Home> cut = Render<Home>();

        Assert.NotNull(cut.Find("#initial-prompt"));
        Assert.NotNull(cut.Find("#revision-prompt"));
        Assert.NotNull(cut.Find("[aria-labelledby='draft-heading']"));
        Assert.NotNull(cut.Find("[aria-labelledby='review-heading']"));
        Assert.Equal(["New", "List", "Quit", "?"],
            cut.FindAll(".command-bar button").Select(button => button.TextContent.Trim()).ToArray());
        Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));
        Assert.False(workspace.State.IsSelectionVisible);
    }

    [Fact]
    public void Home_PlacesDefaultWordRangeBetweenPromptsAndContentPanes()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        string markup = cut.Markup;

        Assert.True(markup.IndexOf("prompt-strip", StringComparison.Ordinal) <
            markup.IndexOf("word-range-row", StringComparison.Ordinal));
        Assert.True(markup.IndexOf("word-range-row", StringComparison.Ordinal) <
            markup.IndexOf("work-grid", StringComparison.Ordinal));
        Assert.Equal("1000", cut.Find("#min-words").GetAttribute("value"));
        Assert.Equal("2000", cut.Find("#max-words").GetAttribute("value"));
        Assert.NotNull(cut.Find("button[data-command='go']"));
    }

    [Fact]
    public void Home_ShowsNumberInputAndKeepsRevisionDisabledForUnselectedList()
    {
        BlogWorkspaceService workspace = RegisterWorkspace([
            new BlogSessionSummary(Guid.NewGuid().ToString("N"), "first topic", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        ]);
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='list']").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("#command-session-number"));
            Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));
            Assert.Contains("[1]", cut.Find(".session-list").TextContent);
        });
    }

    [Fact]
    public void Home_NewClearsWorkspace()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        workspace.State.InitialPrompt = "unsaved";
        workspace.State.Draft = "draft";
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='new']").Click();
        cut.WaitForAssertion(() => Assert.NotNull(cut.Find("[role='dialog']")));
        cut.Find("button[data-confirm='discard']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(workspace.State.InitialPrompt);
            Assert.Empty(workspace.State.Draft);
        });
    }

    [Fact]
    public void Home_KeepsNewListAndQuitEnabledWhileRevisionIsConditional()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();

        Assert.False(cut.Find("button[data-command='new']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[data-command='list']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[data-command='quit']").HasAttribute("disabled"));
        Assert.Empty(cut.FindAll("button[data-command='revise']"));
        Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));
    }

    [Fact]
    public void Home_UsesAccessibleRegionsLabelsAndLiveStatus()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();

        Assert.Equal("New writing prompt", cut.Find("label[for='initial-prompt']").TextContent.Trim());
        Assert.Equal("Revision request", cut.Find("label[for='revision-prompt']").TextContent.Trim());
        Assert.NotNull(cut.Find("nav[aria-label='Workspace commands']"));
        Assert.NotNull(cut.Find("[aria-live='polite']"));
        Assert.NotNull(cut.Find("[aria-labelledby='draft-heading']"));
        Assert.NotNull(cut.Find("[aria-labelledby='review-heading']"));
    }

    [Fact]
    public void Home_PlacesWorkflowLogDirectlyBelowCommandBar()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        string markup = cut.Markup;

        Assert.True(markup.IndexOf("command-bar", StringComparison.Ordinal) <
            markup.IndexOf("workflow-log", StringComparison.Ordinal));
        Assert.DoesNotContain("status-stack", markup);
    }

    [Fact]
    public void Home_PreservesKeyboardOrderFromCommandsToLogToContent()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        string markup = cut.Markup;

        Assert.True(markup.IndexOf("command-bar", StringComparison.Ordinal) <
            markup.IndexOf("workflow-log", StringComparison.Ordinal));
        Assert.Equal("0", cut.Find(".workflow-log").GetAttribute("tabindex"));
    }

    [Fact]
    public void Home_OnlyGoSubmitsPromptText()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        var prompt = cut.Find("#initial-prompt");
        prompt.Input("topic");

        Assert.False(prompt.HasAttribute("onkeydown"));
        Assert.Equal(WorkspaceMode.New, workspace.State.Mode);

        cut.Find("button[data-command='go']").Click();
        cut.WaitForAssertion(() => Assert.Equal(WorkspaceMode.Draft, workspace.State.Mode));
    }

    [Fact]
    public void Home_ValidSelectionLoadsSavedSession()
    {
        BlogWorkspaceService workspace = RegisterWorkspace([
            new BlogSessionSummary(Guid.NewGuid().ToString("N"), "first topic", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        ]);
        IRenderedComponent<Home> cut = Render<Home>();
        cut.Find("button[data-command='list']").Click();
        cut.WaitForElement("#command-session-number").Change("1");

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("draft", workspace.State.Draft);
            Assert.False(cut.Find("#revision-prompt").HasAttribute("disabled"));
        });
    }

    [Fact]
    public void Home_DisplaysCompletedDraftAndReviewerNotes()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        workspace.State.InitialPrompt = "topic";

        cut.Find("#initial-prompt").Input("topic");
        cut.Find("button[data-command='go']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("draft", cut.Find("[aria-labelledby='draft-heading']").TextContent);
            Assert.Contains("review", cut.Find("[aria-labelledby='review-heading']").TextContent);
        });
    }

    [Fact]
    public void Home_RevisionFieldLatchesOnDraftUntilNew()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();

        Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));

        workspace.State.Draft = "draft";
        cut.Render();

        Assert.False(cut.Find("#revision-prompt").HasAttribute("disabled"));

        workspace.State.Draft = "  ";
        cut.Render();

        Assert.False(cut.Find("#revision-prompt").HasAttribute("disabled"));

        cut.Find("button[data-command='new']").Click();

        Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));
    }

    [Fact]
    public void Home_WritingPromptKeepsTextAndLocksWhileRevisingUntilNew()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        Assert.False(cut.Find("#initial-prompt").HasAttribute("disabled"));

        cut.Find("#initial-prompt").Input("topic");
        cut.Find("button[data-command='go']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("topic", cut.Find("#initial-prompt").GetAttribute("value"));
            Assert.True(cut.Find("#initial-prompt").HasAttribute("disabled"));
            Assert.False(cut.Find("#revision-prompt").HasAttribute("disabled"));
        });

        cut.Find("button[data-command='new']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.False(cut.Find("#initial-prompt").HasAttribute("disabled"));
            Assert.True(cut.Find("#revision-prompt").HasAttribute("disabled"));
        });
    }

    private BlogWorkspaceService RegisterWorkspace(IReadOnlyList<BlogSessionSummary>? summaries = null)
    {
        var sessions = new StubSessionService { Summaries = summaries ?? [] };
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(10));
        Services.AddSingleton(workspace);
        return workspace;
    }

    private sealed class StubSessionService : IBlogWriterSessionService
    {
        public IReadOnlyList<BlogSessionSummary> Summaries { get; init; } = [];

        public Task<BlogSession> StartAsync(string prompt, int minWords = ResearchState.DefaultMinWords, int maxWords = ResearchState.DefaultMaxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null) =>
            Task.FromResult(CreateSession(prompt));

        public Task<BlogSession> ReviseAsync(
            BlogSession session,
            string revision,
            int minWords,
            int maxWords,
            CancellationToken cancellationToken = default,
            IProgress<WorkflowOutputUpdate>? output = null) =>
            Task.FromResult(session);

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Summaries);

        public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<BlogSession?>(new BlogSession
            {
                Id = sessionId,
                OwnerId = "owner",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                State = new ResearchState { MainTask = "loaded", Draft = "loaded", ReviewNotes = "loaded review" },
            });

        private static BlogSession CreateSession(string prompt) => new()
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnerId = "owner",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            State = new ResearchState { MainTask = prompt, Draft = "draft", ReviewNotes = "review" },
        };
    }
}
