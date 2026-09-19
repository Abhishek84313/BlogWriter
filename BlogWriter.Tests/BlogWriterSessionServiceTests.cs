using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public sealed class BlogWriterSessionServiceTests
{
    [Fact]
    public async Task StartAsync_CreatesRunsAndSavesSession()
    {
        var store = new RecordingStore();
        var workflow = new StubWorkflow(state =>
        {
            state.Draft = "draft";
            state.ReviewNotes = "review";
            return state;
        });
        var service = new BlogWriterSessionService(workflow, store);

        BlogSession session = await service.StartAsync("topic", 500, 900);

        Assert.Equal("topic", session.State.MainTask);
        Assert.Equal("draft", session.State.Draft);
        Assert.Equal("review", session.State.ReviewNotes);
        Assert.Equal(1, store.CreateCalls);
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public async Task ReviseAsync_DoesNotMutateStableSessionWhenWorkflowFails()
    {
        var store = new RecordingStore();
        var service = new BlogWriterSessionService(
            new StubWorkflow(_ => throw new InvalidOperationException("failed")),
            store);
        BlogSession original = CreateSession("original draft", "original review");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReviseAsync(original, "change it"));

        Assert.Equal("original draft", original.State.Draft);
        Assert.Equal("original review", original.State.ReviewNotes);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public async Task ReviseAsync_PublishesAndSavesCompletedCopy()
    {
        var store = new RecordingStore();
        var service = new BlogWriterSessionService(
            new StubWorkflow(state =>
            {
                state.Draft = "revised";
                state.ReviewNotes = "approved";
                return state;
            }),
            store);

        BlogSession revised = await service.ReviseAsync(CreateSession("old", "old review"), "change it");

        Assert.Equal("revised", revised.State.Draft);
        Assert.Equal("approved", revised.State.ReviewNotes);
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public async Task ListAndLoadAsync_DelegateToOwnerScopedStore()
    {
        var store = new RecordingStore();
        BlogSession existing = CreateSession("draft", "review");
        store.Session = existing;
        var service = new BlogWriterSessionService(new StubWorkflow(state => state), store);

        IReadOnlyList<BlogSessionSummary> summaries = await service.ListAsync();
        BlogSession? loaded = await service.LoadAsync(existing.Id);

        Assert.Single(summaries);
        Assert.Same(existing, loaded);
    }

    private static BlogSession CreateSession(string draft, string review) => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        OwnerId = "owner",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        ETag = "etag",
        State = new ResearchState { MainTask = "topic", Draft = draft, ReviewNotes = review },
    };

    private sealed class StubWorkflow(Func<ResearchState, ResearchState> run) : IBlogWorkflow
    {
        public Task<ResearchState> RunAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(run(state));
        }
    }

    private sealed class RecordingStore : IBlogSessionStore
    {
        public int CreateCalls { get; private set; }
        public int SaveCalls { get; private set; }
        public BlogSession? Session { get; set; }

        public Task<BlogSession> CreateAsync(ResearchState state, CancellationToken cancellationToken = default)
        {
            CreateCalls++;
            Session = new BlogSession
            {
                Id = Guid.NewGuid().ToString("N"),
                OwnerId = "owner",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                State = state,
            };
            return Task.FromResult(Session);
        }

        public Task<BlogSession?> GetAsync(string sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Session?.Id == sessionId ? Session : null);

        public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BlogSessionSummary>>(Session is null
                ? []
                : [new(Session.Id, Session.State.MainTask, Session.CreatedAt, Session.UpdatedAt)]);

        public Task SaveAsync(BlogSession session, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            Session = session;
            return Task.CompletedTask;
        }

        public Task DeleteOwnerSessionsAsync(string ownerId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
