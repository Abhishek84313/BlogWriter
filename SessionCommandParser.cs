namespace BlogWriter;

public static class SessionCommandParser
{
    public static SessionCommand Parse(string input)
    {
        string trimmed = input.Trim();
        if (string.Equals(trimmed, "list", StringComparison.OrdinalIgnoreCase))
        {
            return new ListSessionsCommand();
        }

        const string resumePrefix = "resume ";
        if (trimmed.StartsWith(resumePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return new ResumeSessionCommand(trimmed[resumePrefix.Length..].Trim());
        }

        return new NewTopicCommand(input);
    }
}

public abstract record SessionCommand;
public sealed record ListSessionsCommand : SessionCommand;
public sealed record ResumeSessionCommand(string SessionId) : SessionCommand;
public sealed record NewTopicCommand(string Topic) : SessionCommand;