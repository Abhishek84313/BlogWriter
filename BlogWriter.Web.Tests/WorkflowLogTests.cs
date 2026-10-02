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
    public void WorkflowLog_RendersLatestStatusAsSafeText()
    {
        IRenderedComponent<WorkflowLog> cut = Render<WorkflowLog>(parameters => parameters
            .Add(component => component.CurrentStatus, "<failed>")
            .Add(component => component.CurrentStatusOutcome, WorkflowOutputOutcome.Failure));

        Assert.Contains("<failed>", cut.Find(".workflow-status").TextContent);
        Assert.Empty(cut.FindAll("li"));
        Assert.Contains("&lt;failed&gt;", cut.Markup);
    }

    [Fact]
    public void WorkflowLog_ReplacesPriorStatusWithNewestValue()
    {
        IRenderedComponent<WorkflowLog> cut = Render<WorkflowLog>(parameters => parameters
            .Add(component => component.CurrentStatus, "latest"));

        Assert.Equal("latest", cut.Find(".workflow-status").TextContent);
        Assert.Empty(cut.FindAll("li"));
        Assert.Contains("workflow-log", cut.Find("section").ClassList);
    }
}