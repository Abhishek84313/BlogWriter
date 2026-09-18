namespace BlogWriter;

public static class SessionListSelection
{
    public static IReadOnlyList<string> Format(IReadOnlyList<BlogSessionSummary> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);

        return sessions
            .Select((session, index) => FormatEntry(index + 1, session))
            .ToList();
    }

    public static bool TryResolve(
        string input,
        IReadOnlyList<BlogSessionSummary> sessions,
        out BlogSessionSummary? selected)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(sessions);

        selected = null;
        if (!int.TryParse(input.Trim(), out int displayNumber) ||
            displayNumber < 1 ||
            displayNumber > sessions.Count)
        {
            return false;
        }

        selected = sessions[displayNumber - 1];
        return true;
    }

    private static string FormatEntry(int displayNumber, BlogSessionSummary session) =>
        $"[{displayNumber}] {session.MainTask} | Updated {session.UpdatedAt:u} | Created {session.CreatedAt:u}";
}
