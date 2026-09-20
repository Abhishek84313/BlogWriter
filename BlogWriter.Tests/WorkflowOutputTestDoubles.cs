namespace BlogWriter.Tests;

internal sealed class WorkflowOutputCollector : IProgress<WorkflowOutputUpdate>
{
    public List<WorkflowOutputUpdate> Updates { get; } = [];

    public void Report(WorkflowOutputUpdate value) => Updates.Add(value);
}
