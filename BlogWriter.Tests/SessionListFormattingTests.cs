using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public sealed class SessionListFormattingTests
{
    [Fact]
    public void Format_AssignsOneBasedLabelsInDisplayOrder()
    {
        IReadOnlyList<BlogSessionSummary> sessions = CreateSummaries("first", "second");

        IReadOnlyList<string> lines = SessionListSelection.Format(sessions);

        Assert.StartsWith("[1]", lines[0]);
        Assert.Contains("first", lines[0]);
        Assert.StartsWith("[2]", lines[1]);
        Assert.Contains("second", lines[1]);
    }

    [Fact]
    public void Format_UsesCompleteMultiDigitLabels()
    {
        IReadOnlyList<BlogSessionSummary> sessions = Enumerable.Range(1, 100)
            .Select(index => CreateSummary($"topic {index}", index.ToString("D32")))
            .ToList();

        IReadOnlyList<string> lines = SessionListSelection.Format(sessions);

        Assert.StartsWith("[10]", lines[9]);
        Assert.StartsWith("[100]", lines[99]);
    }

    [Fact]
    public void Format_ReturnsNoLinesForEmptyList()
    {
        Assert.Empty(SessionListSelection.Format([]));
    }

    [Theory]
    [InlineData("1", "first")]
    [InlineData(" 2 ", "second")]
    public void TryResolve_ReturnsSummaryForValidNumber(string input, string expectedTask)
    {
        IReadOnlyList<BlogSessionSummary> sessions = CreateSummaries("first", "second");

        bool resolved = SessionListSelection.TryResolve(input, sessions, out BlogSessionSummary? selected);

        Assert.True(resolved);
        Assert.NotNull(selected);
        Assert.Equal(expectedTask, selected.MainTask);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("3")]
    [InlineData("00000000000000000000000000000000")]
    public void TryResolve_RejectsInvalidOrRawIdentifierInput(string input)
    {
        IReadOnlyList<BlogSessionSummary> sessions = CreateSummaries("first", "second");

        bool resolved = SessionListSelection.TryResolve(input, sessions, out BlogSessionSummary? selected);

        Assert.False(resolved);
        Assert.Null(selected);
    }

    private static IReadOnlyList<BlogSessionSummary> CreateSummaries(params string[] tasks) =>
        tasks.Select((task, index) => CreateSummary(task, (index + 1).ToString("D32"))).ToList();

    private static BlogSessionSummary CreateSummary(string task, string id) =>
        new(id, task, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
