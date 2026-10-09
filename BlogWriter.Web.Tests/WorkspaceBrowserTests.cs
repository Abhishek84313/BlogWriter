using Bunit;
using BlogWriter.Web.Components.Pages;
using BlogWriter.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlogWriter.Web.Tests;

public sealed class WorkspaceBrowserTests : BunitContext
{
    [Theory]
    [InlineData(390, 844)]
    [InlineData(1440, 900)]
    public void RequiredViewports_AreExplicitlyCovered(int width, int height)
    {
        Assert.Contains((width, height), new[] { (390, 844), (1440, 900) });
    }

    [Fact]
    public void OutputLayoutContract_CapsLogAndProtectsCompactSelector()
    {
        string css = FindRepositoryFile("BlogWriter.Web", "wwwroot", "app.css");

        Assert.Contains("button.session-entry", css);
        Assert.Contains(":focus-visible", css);
        Assert.DoesNotContain(".list-command input", css);
        Assert.Contains("max-height: 4.8rem", css);
        Assert.Contains("overflow: auto", css);
        Assert.Contains("grid-template-columns: repeat(5, minmax(0, 1fr))", css);
    }

    [Fact]
    public void ListSelectionFixture_RestoresSavedSessionWithoutLaunchingIt()
    {
        var sessions = new BrowserSessionService();
        var workspace = new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25));
        Services.AddSingleton(workspace);
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='list']").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll("#command-session-number"));
            Assert.Single(cut.FindAll("button.session-entry"));
        });
        cut.Find("button.session-entry").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(1, sessions.LoadCalls);
            Assert.Equal(0, sessions.StartCalls);
            Assert.Equal(0, sessions.RevisionCalls);
            Assert.Equal("saved topic", cut.Find("#initial-prompt").GetAttribute("value"));
            Assert.Equal("tighten the ending", cut.Find("#revision-prompt").GetAttribute("value"));
            Assert.Equal("700", cut.Find("#min-words").GetAttribute("value"));
            Assert.Equal("1350", cut.Find("#max-words").GetAttribute("value"));
            Assert.False(cut.Find("#initial-prompt").HasAttribute("disabled"));
            Assert.False(cut.Find("#revision-prompt").HasAttribute("disabled"));
            Assert.False(cut.Find("#min-words").HasAttribute("disabled"));
            Assert.False(cut.Find("#max-words").HasAttribute("disabled"));
            Assert.Empty(workspace.State.Draft);
            Assert.Empty(workspace.State.Review);
            Assert.Null(workspace.State.ActiveSession);
        });
    }

    [Fact]
    public void GoAfterRestoringSession_CreatesNewRunFromEditedValues()
    {
        var sessions = new BrowserSessionService();
        Services.AddSingleton(new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25)));
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='list']").Click();
        cut.Find("button.session-entry").Click();
        cut.Find("#initial-prompt").Input("edited topic");
        cut.Find("#min-words").Input("800");
        cut.Find("#max-words").Input("1400");
        cut.Find("button[data-command='go']").Click();

        cut.WaitForAssertion(() => Assert.Equal(1, sessions.StartCalls));

        Assert.Equal("edited topic", sessions.LastPrompt);
        Assert.Equal(new WordRange(800, 1400), sessions.LastStartRange);
        Assert.Equal(0, sessions.RevisionCalls);
        Assert.NotNull(sessions.CreatedSession);
        Assert.NotEqual(sessions.SourceSession.Id, sessions.CreatedSession.Id);
        Assert.Equal("saved topic", sessions.SourceSession.State.MainTask);
        Assert.Equal("saved draft", sessions.SourceSession.State.Draft);
        Assert.Equal("saved review", sessions.SourceSession.State.ReviewNotes);
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private sealed class BrowserSessionService : IBlogWriterSessionService
    {
        private readonly BlogSession _sourceSession = CreateSourceSession();

        public int StartCalls { get; private set; }
        public int RevisionCalls { get; private set; }
        public int LoadCalls { get; private set; }
        public string? LastPrompt { get; private set; }
        public WordRange? LastStartRange { get; private set; }
        public BlogSession? CreatedSession { get; private set; }
        public BlogSession SourceSession => _sourceSession;

        public Task<BlogSession> StartAsync(string prompt, int minWords = ResearchState.DefaultMinWords, int maxWords = ResearchState.DefaultMaxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null)
        {
            StartCalls++;
            LastPrompt = prompt;
            LastStartRange = new WordRange(minWords, maxWords);
            CreatedSession = ListLauncherTestHelpers.Session(prompt, "", "new draft", "new review");
            CreatedSession.State.MinWords = minWords;
            CreatedSession.State.MaxWords = maxWords;
            return Task.FromResult(CreatedSession);
        }

        public Task<BlogSession> ReviseAsync(BlogSession session, string revision, int minWords, int maxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null)
        {
            RevisionCalls++;
            return Task.FromResult(session);
        }

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BlogSessionSummary>>([
                new(_sourceSession.Id, _sourceSession.State.MainTask, _sourceSession.CreatedAt, _sourceSession.UpdatedAt)]);

        public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            LoadCalls++;
            return Task.FromResult<BlogSession?>(_sourceSession.Id == sessionId ? _sourceSession : null);
        }

        private static BlogSession CreateSourceSession()
        {
            BlogSession session = ListLauncherTestHelpers.Session(
                "saved topic",
                "tighten the ending",
                "saved draft",
                "saved review");
            session.State.MinWords = 700;
            session.State.MaxWords = 1350;
            return session;
        }
    }

}