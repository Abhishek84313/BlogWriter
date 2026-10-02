using System.Globalization;

namespace BlogWriter;

public readonly record struct WordRange(int Min, int Max)
{
    public static WordRange Default => new(ResearchState.DefaultMinWords, ResearchState.DefaultMaxWords);

    public static WordRangeValidation Parse(string? minInput, string? maxInput)
    {
        string? minError = ParsePositiveWholeNumber(minInput, "Min", out int min);
        string? maxError = ParsePositiveWholeNumber(maxInput, "Max", out int max);

        if (minError is null && maxError is null && max < min)
        {
            maxError = "Max must be at least Min.";
        }

        return minError is null && maxError is null
            ? new WordRangeValidation(new WordRange(min, max), null, null)
            : new WordRangeValidation(null, minError, maxError);
    }

    public static WordRange Create(int min, int max)
    {
        WordRangeValidation validation = Parse(
            min.ToString(CultureInfo.InvariantCulture),
            max.ToString(CultureInfo.InvariantCulture));
        return validation.Range ?? throw new ArgumentOutOfRangeException(
            nameof(max),
            validation.MaxError ?? validation.MinError);
    }

    private static string? ParsePositiveWholeNumber(string? input, string label, out int value)
    {
        if (!int.TryParse(input?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value) || value <= 0)
        {
            return $"{label} must be a positive whole number.";
        }

        return null;
    }
}

public sealed record WordRangeValidation(
    WordRange? Range,
    string? MinError,
    string? MaxError)
{
    public bool IsValid => Range.HasValue;
}
