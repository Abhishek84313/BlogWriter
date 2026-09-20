namespace BlogWriter;

public interface IBlogWriterSessionService
{
    Task<BlogSession> StartAsync(
        string prompt,
        int minWords = ResearchState.DefaultMinWords,
        int maxWords = ResearchState.DefaultMaxWords,
        CancellationToken cancellationToken = default);

    Task<BlogSession> ReviseAsync(
        BlogSession session,
        string revision,
        int minWords,
        int maxWords,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlogSessionSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<BlogSession?> LoadAsync(string sessionId, CancellationToken cancellationToken = default);
}
