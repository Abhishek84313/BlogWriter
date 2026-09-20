namespace BlogWriter;

public sealed class BlogWriterSessionService(
    IBlogWorkflow workflow,
    IBlogSessionStore sessionStore) : IBlogWriterSessionService
{
    private readonly IBlogWorkflow _workflow = workflow;
    private readonly IBlogSessionStore _sessionStore = sessionStore;

    public async Task<BlogSession> StartAsync(
        string prompt,
        int minWords = ResearchState.DefaultMinWords,
        int maxWords = ResearchState.DefaultMaxWords,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        WordRange range = WordRange.Create(minWords, maxWords);

        BlogSession session = await _sessionStore.CreateAsync(new ResearchState
        {
            MainTask = prompt.Trim(),
            MinWords = range.Min,
            MaxWords = range.Max,
        }, cancellationToken);

        session.State = await _workflow.RunAsync(session.State, cancellationToken);
        await _sessionStore.SaveAsync(session, cancellationToken);
        return session;
    }

    public async Task<BlogSession> ReviseAsync(
        BlogSession session,
        string revision,
        int minWords,
        int maxWords,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(revision);
        WordRange range = WordRange.Create(minWords, maxWords);

        var candidate = new BlogSession
        {
            Id = session.Id,
            OwnerId = session.OwnerId,
            ETag = session.ETag,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            State = Clone(session.State),
        };

        candidate.State.MinWords = range.Min;
        candidate.State.MaxWords = range.Max;
        candidate.State.StartFollowUp(revision);
        candidate.State = await _workflow.RunAsync(candidate.State, cancellationToken);
        await _sessionStore.SaveAsync(candidate, cancellationToken);
        return candidate;
    }

    public Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default) =>
        _sessionStore.ListAsync(cancellationToken);

    public Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default) =>
        _sessionStore.GetAsync(sessionId, cancellationToken);

    private static ResearchState Clone(ResearchState source) => new()
    {
        MainTask = source.MainTask,
        MinWords = source.MinWords,
        MaxWords = source.MaxWords,
        ResearchFindings = [.. source.ResearchFindings],
        SearchRefinements = [.. source.SearchRefinements],
        Draft = source.Draft,
        ReviewNotes = source.ReviewNotes,
        RevisionNumber = source.RevisionNumber,
        NextStep = source.NextStep,
        CurrentSubTask = source.CurrentSubTask,
    };
}
