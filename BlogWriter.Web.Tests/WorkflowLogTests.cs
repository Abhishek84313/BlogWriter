using Bunit;
using BlogWriter.Web.Components;
using BlogWriter.Web.Services;

namespace BlogWriter.Web.Tests;

public sealed class WorkflowLogTests : BunitContext
{
    [Fact]
    public void WorkflowLog_RendersEmptyStateAndAccessibleLiveRegion()
    {
        IRenderedComponent<WorkflowLog> cut = Render<WorkflowLog>();

        Assert.Equal("Workflow log", cut.Find("section").GetAttribute("aria-label"));
        Assert.Equal("polite", cut.Find("section").GetAttribute("aria-live"));
        Assert.Equal("0", cut.Find("section").GetAttribute("tabindex"));
        Assert.Contains("No workflow activity yet.", cut.Markup);
    }

    [Fact]
    public void WorkflowLog_RendersOrderedTextEntries()
    {
        IRenderedComponent<WorkflowLog> cut = Render<WorkflowLog>(parameters => parameters
            .Add(component => component.Entries, [
                new WorkflowLogEntry("started", WorkflowOutputOutcome.Progress),
                new WorkflowLogEntry("<failed>", WorkflowOutputOutcome.Failure),
            ]));

        Assert.Equal(["Progress: started", "Failure: <failed>"], cut.FindAll("li").Select(item => item.TextContent).ToArray());
        Assert.Contains("&lt;failed&gt;", cut.Markup);
    }

    [Fact]
    public void WorkflowLog_RetainsOlderEntriesForScrolling()
    {
        IReadOnlyList<WorkflowLogEntry> entries = Enumerable.Range(1, 5)
            .Select(index => new WorkflowLogEntry($"entry {index}", WorkflowOutputOutcome.Progress))
            .ToList();

        IRenderedComponent<WorkflowLog> cut = Render<WorkflowLog>(parameters => parameters
            .Add(component => component.Entries, entries));

        Assert.Equal(5, cut.FindAll("li").Count);
        Assert.Contains("workflow-log", cut.Find("section").ClassList);
    }
}