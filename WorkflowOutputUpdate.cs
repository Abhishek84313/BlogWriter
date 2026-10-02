namespace BlogWriter;

public enum WorkflowOutputKind
{
    Lifecycle,
    ReviewerFeedback,
}

public enum WorkflowOutputOutcome
{
    Progress,
    Success,
    Cancellation,
    Validation,
    Conflict,
    Failure,
    Review,
}

public sealed record WorkflowOutputUpdate
{
    private WorkflowOutputUpdate(
        WorkflowOutputKind kind,
        WorkflowOutputOutcome outcome,
        string message,
        long operationVersion,
        long sequence,
        string updateKey,
        int? revisionNumber)
    {
        Kind = kind;
        Outcome = outcome;
        Message = message;
        OperationVersion = operationVersion;
        Sequence = sequence;
        UpdateKey = updateKey;
        RevisionNumber = revisionNumber;
    }

    public WorkflowOutputKind Kind { get; }
    public WorkflowOutputOutcome Outcome { get; }
    public string Message { get; }
    public long OperationVersion { get; }
    public long Sequence { get; }
    public string UpdateKey { get; }
    public int? RevisionNumber { get; }

    public static WorkflowOutputUpdate Create(
        WorkflowOutputKind kind,
        WorkflowOutputOutcome outcome,
        string message,
        long operationVersion,
        long sequence,
        string updateKey,
        int? revisionNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(updateKey);
        ArgumentOutOfRangeException.ThrowIfNegative(operationVersion);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);

        return new WorkflowOutputUpdate(
            kind,
            outcome,
            message,
            operationVersion,
            sequence,
            updateKey,
            revisionNumber);
    }
}