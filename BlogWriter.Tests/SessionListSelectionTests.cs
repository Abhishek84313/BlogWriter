using Xunit;

namespace BlogWriter.Tests;

public sealed class SessionListSelectionTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData(" 2 ", 2)]
    public void TryResolve_UsesOneBasedDisplayedPosition(string input, int expectedIndex)
    {
        IReadOnlyList<BlogSessionSummary> sessions = [CreateSummary("one"), CreateSummary("two")];

        Assert.True(SessionListSelection.TryResolve(input, sessions, out BlogSessionSummary? selected));
        Assert.Equal(sessions[expectedIndex - 1].Id, selected!.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("text")]
    [InlineData("3")]
    public void TryResolve_RejectsInvalidOrOutOfRangeInput(string input)
    {
        IReadOnlyList<BlogSessionSummary> sessions = [CreateSummary("one"), CreateSummary("two")];

        Assert.False(SessionListSelection.TryResolve(input, sessions, out BlogSessionSummary? selected));
        Assert.Null(selected);
    }

    private static BlogSessionSummary CreateSummary(string task) =>
        new(Guid.NewGuid().ToString("N"), task, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
