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

        Assert.Contains(".list-command input", css);
        Assert.Contains("width: 3.5rem", css);
        Assert.Contains("max-height: 4.8rem", css);
        Assert.Contains("overflow: auto", css);
        Assert.Contains("grid-template-columns: repeat(5, minmax(0, 1fr))", css);
    }

    [Fact]
    public void ListSelectionFixture_LaunchesSavedSessionWithRestoredPromptContext()
    {
        var sessions = new BrowserSessionService();
        Services.AddSingleton(new BlogWorkspaceService(sessions, TimeSpan.FromMilliseconds(25)));
        IRenderedComponent<Home> cut = Render<Home>();

        cut.Find("button[data-command='list']").Click();
        cut.WaitForElement("#command-session-number").Change("1");

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(1, sessions.StartCalls);
            Assert.Equal("saved topic", sessions.LastPrompt);
            Assert.Equal("tighten the ending", cut.Find("#revision-prompt").GetAttribute("value"));
        });
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
        public int StartCalls { get; private set; }
        public string? LastPrompt { get; private set; }

        public Task<BlogSession> StartAsync(string prompt, int minWords = ResearchState.DefaultMinWords, int maxWords = ResearchState.DefaultMaxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null)
        {
            StartCalls++;
            LastPrompt = prompt;
            return Task.FromResult(ListLauncherTestHelpers.Session(prompt, "tighten the ending", "new draft", "new review"));
        }

        public Task<BlogSession> ReviseAsync(BlogSession session, string revision, int minWords, int maxWords, CancellationToken cancellationToken = default, IProgress<WorkflowOutputUpdate>? output = null) =>
            Task.FromResult(session);

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BlogSessionSummary>>([ListLauncherTestHelpers.Summary("saved topic")]);

        public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<BlogSession?>(ListLauncherTestHelpers.Session("saved topic", "tighten the ending"));
    }

}