using BlogWriter.PromptLibrary;

namespace BlogWriter;

/// <summary>
/// Compatibility layer for the shared prompt catalog.
/// </summary>
public static class Prompts
{
    public const string BloggerInstructions = PromptCatalog.BloggerInstructions;
    public const string ResearcherInstructions = PromptCatalog.ResearcherInstructions;
    public const string AuthorInstructions = PromptCatalog.AuthorInstructions;
    public const string ReviewerInstructions = PromptCatalog.ReviewerInstructions;
}
