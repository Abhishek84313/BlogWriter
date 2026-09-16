namespace BlogWriter;

public sealed class SessionConflictException(string sessionId)
    : InvalidOperationException($"Session '{sessionId}' was updated elsewhere. Reload it before trying again.");