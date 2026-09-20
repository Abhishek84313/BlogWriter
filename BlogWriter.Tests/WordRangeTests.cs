using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public sealed class WordRangeTests
{
    [Theory]
    [InlineData("1000", "2000", 1000, 2000)]
    [InlineData(" 500 ", " 500 ", 500, 500)]
    public void Parse_ValidPositiveWholeNumbers_ReturnsRange(
        string minInput,
        string maxInput,
        int expectedMin,
        int expectedMax)
    {
        WordRangeValidation result = WordRange.Parse(minInput, maxInput);

        Assert.True(result.IsValid);
        Assert.Equal(new WordRange(expectedMin, expectedMax), result.Range);
        Assert.Null(result.MinError);
        Assert.Null(result.MaxError);
    }

    [Theory]
    [InlineData("", "2000")]
    [InlineData("words", "2000")]
    [InlineData("1.5", "2000")]
    [InlineData("0", "2000")]
    [InlineData("-1", "2000")]
    [InlineData("999999999999999999999", "2000")]
    public void Parse_InvalidMin_ReturnsMinError(string minInput, string maxInput)
    {
        WordRangeValidation result = WordRange.Parse(minInput, maxInput);

        Assert.False(result.IsValid);
        Assert.NotNull(result.MinError);
    }

    [Theory]
    [InlineData("1000", "")]
    [InlineData("1000", "words")]
    [InlineData("1000", "1.5")]
    [InlineData("1000", "0")]
    [InlineData("1000", "-1")]
    [InlineData("1000", "999999999999999999999")]
    public void Parse_InvalidMax_ReturnsMaxError(string minInput, string maxInput)
    {
        WordRangeValidation result = WordRange.Parse(minInput, maxInput);

        Assert.False(result.IsValid);
        Assert.NotNull(result.MaxError);
    }

    [Fact]
    public void Parse_MaxBelowMin_ReturnsOrderedRangeError()
    {
        WordRangeValidation result = WordRange.Parse("2000", "1000");

        Assert.False(result.IsValid);
        Assert.Equal("Max must be at least Min.", result.MaxError);
    }
}
