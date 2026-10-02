using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public sealed class ConsoleHostRegressionTests
{
    [Fact]
    public void ExistingCommandsRemainRecognizable()
    {
        Assert.IsType<ListSessionsCommand>(SessionCommandParser.Parse("list"));
        Assert.Equal("2", Assert.IsType<ResumeSessionCommand>(SessionCommandParser.Parse("resume 2")).SessionId);
        Assert.Equal("new topic", Assert.IsType<NewTopicCommand>(SessionCommandParser.Parse("new topic")).Topic);
    }

    [Fact]
    public void SessionListFormattingRemainsNumberedAndOrdered()
    {
        IReadOnlyList<BlogSessionSummary> summaries =
        [
            new("1".PadLeft(32, '0'), "newest", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new("2".PadLeft(32, '0'), "older", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(-1)),
        ];

        IReadOnlyList<string> lines = SessionListSelection.Format(summaries);

        Assert.StartsWith("[1] newest", lines[0]);
        Assert.StartsWith("[2] older", lines[1]);
    }
}
