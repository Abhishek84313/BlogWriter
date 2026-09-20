namespace BlogWriter.Web.Services;

public enum WorkspaceMode
{
    New,
    Draft,
    List,
    Ended,
}

public enum WorkspaceTransitionResult
{
    Completed,
    RequiresConfirmation,
}

public sealed class BlogWorkspaceState
{
    public WorkspaceMode Mode { get; internal set; } = WorkspaceMode.New;
    public string InitialPrompt { get; set; } = "";
    public string RevisionPrompt { get; set; } = "";
    public string MinWordsInput { get; internal set; } = ResearchState.DefaultMinWords.ToString();
    public string MaxWordsInput { get; internal set; } = ResearchState.DefaultMaxWords.ToString();
    public WordRange AcceptedRange { get; internal set; } = WordRange.Default;
    public string? MinWordsError { get; internal set; }
    public string? MaxWordsError { get; internal set; }
    public string Draft { get; set; } = "";
    public string Review { get; set; } = "";
    public BlogSession? ActiveSession { get; internal set; }
    public IReadOnlyList<BlogSessionSummary> DisplayedSessions { get; internal set; } = [];
    public string SelectionInput { get; set; } = "";
    public bool IsProcessing { get; internal set; }
    public string? StatusMessage { get; internal set; }
    public string? ValidationMessage { get; internal set; }

    public bool IsSelectionVisible => Mode == WorkspaceMode.List && DisplayedSessions.Count > 0;
    public bool IsReviseEnabled => IsSelectionVisible && !IsProcessing;
    public bool HasUnsavedRange
    {
        get
        {
            WordRangeValidation validation = WordRange.Parse(MinWordsInput, MaxWordsInput);
            return !validation.IsValid || validation.Range != AcceptedRange;
        }
    }

    public bool HasUnsavedText =>
        !string.IsNullOrWhiteSpace(InitialPrompt) ||
        !string.IsNullOrWhiteSpace(RevisionPrompt) ||
        HasUnsavedRange;
}
