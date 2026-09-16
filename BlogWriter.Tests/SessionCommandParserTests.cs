using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public class SessionCommandParserTests
{
    [Theory]
    [InlineData("list")]
    [InlineData(" LIST ")]
    public void Parse_RecognizesListCommand(string input)
    {
        Assert.IsType<ListSessionsCommand>(SessionCommandParser.Parse(input));
    }

    [Fact]
    public void Parse_RecognizesResumeCommand()
    {
        var command = Assert.IsType<ResumeSessionCommand>(SessionCommandParser.Parse("resume abc"));

        Assert.Equal("abc", command.SessionId);
    }

    [Fact]
    public void Parse_TreatsOtherInputAsNewTopic()
    {
        var command = Assert.IsType<NewTopicCommand>(SessionCommandParser.Parse("list a topic"));

        Assert.Equal("list a topic", command.Topic);
    }
}