namespace BlogWriter.Web.Services;

public sealed record WorkflowLogEntry(string Message, WorkflowOutputOutcome Outcome);

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
    public string? SelectionError { get; internal set; }
    public bool IsProcessing { get; internal set; }
    public string? StatusMessage { get; internal set; }
    public string? ValidationMessage { get; internal set; }
    public IReadOnlyList<WorkflowLogEntry> WorkflowLog { get; internal set; } = [];
    public string? CurrentStatus { get; internal set; }
    public WorkflowOutputOutcome? CurrentStatusOutcome { get; internal set; }

    internal HashSet<string> ReviewerUpdateKeys { get; } = new(StringComparer.Ordinal);

    internal void AppendLog(string message, WorkflowOutputOutcome outcome)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        WorkflowLog = [.. WorkflowLog, new WorkflowLogEntry(message, outcome)];
        CurrentStatus = message;
        CurrentStatusOutcome = outcome;
    }

    internal void ClearOutput()
    {
        WorkflowLog = [];
        ReviewerUpdateKeys.Clear();
    }

    internal void AppendReviewerFeedback(string message, string updateKey)
    {
        if (string.IsNullOrWhiteSpace(message) || !ReviewerUpdateKeys.Add(updateKey))
        {
            return;
        }

        if (string.Equals(Review, message, StringComparison.Ordinal) ||
            Review.EndsWith($"\n\n{message}", StringComparison.Ordinal))
        {
            return;
        }

        Review = string.IsNullOrWhiteSpace(Review)
            ? message
            : $"{Review}\n\n{message}";
    }

    public bool IsSelectionVisible => Mode == WorkspaceMode.List;
    public bool HasDraft => !string.IsNullOrWhiteSpace(Draft);
    public bool IsRevisionInputEnabled => !IsProcessing && HasDraft;
    public bool IsReviseEnabled => !IsProcessing && HasDraft;
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
