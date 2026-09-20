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
        Assert.Equal(["New", "List", "Revise", "Quit"],
            cut.FindAll(".command-bar button").Select(button => button.TextContent.Trim()).ToArray());
        Assert.True(cut.Find("button[data-command='revise']").HasAttribute("disabled"));
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
    }

    [Fact]
    public void Home_ShowsNumberInputAndEnablesReviseForNonEmptyList()
    {
        BlogWorkspaceService workspace = RegisterWorkspace([
            new BlogSessionSummary(Guid.NewGuid().ToString("N"), "first topic", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        ]);
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='list']").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("#session-number"));
            Assert.False(cut.Find("button[data-command='revise']").HasAttribute("disabled"));
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
    public void Home_KeepsNewListAndQuitEnabledWhileReviseIsConditional()
    {
        RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();

        Assert.False(cut.Find("button[data-command='new']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[data-command='list']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[data-command='quit']").HasAttribute("disabled"));
        Assert.True(cut.Find("button[data-command='revise']").HasAttribute("disabled"));
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
    public void Home_EnterSubmitsAndShiftEnterDoesNotSubmit()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        var prompt = cut.Find("#initial-prompt");
        prompt.Input("topic");

        prompt.KeyDown(new KeyboardEventArgs { Key = "Enter", ShiftKey = true });
        Assert.Equal(WorkspaceMode.New, workspace.State.Mode);

        prompt.KeyDown(new KeyboardEventArgs { Key = "Enter" });
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
        cut.WaitForElement("#session-number").Input("1");

        cut.Find("button[data-command='revise']").Click();

        cut.WaitForAssertion(() => Assert.Equal("loaded", workspace.State.Draft));
    }

    [Fact]
    public void Home_DisplaysCompletedDraftAndReviewerNotes()
    {
        BlogWorkspaceService workspace = RegisterWorkspace();
        IRenderedComponent<Home> cut = Render<Home>();
        workspace.State.InitialPrompt = "topic";

        cut.Find("#initial-prompt").Input("topic");
        cut.Find("#initial-prompt").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("draft", cut.Find("[aria-labelledby='draft-heading']").TextContent);
            Assert.Contains("review", cut.Find("[aria-labelledby='review-heading']").TextContent);
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
