using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class RunCommandDialogTests : BunitContext
{
    public RunCommandDialogTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Dialog_ShowsExactRunCommandAndAccessibleModal()
    {
        HelpDialogTestHelpers.AllowDialogOpen(this);
        IRenderedComponent<RunCommandDialog> cut = Render<RunCommandDialog>();

        Assert.Equal(RunCommandDialog.CommandText, cut.Find(".run-command-text").TextContent);
        Assert.Equal("dialog", cut.Find("dialog").GetAttribute("role"));
        Assert.Equal("true", cut.Find("dialog").GetAttribute("aria-modal"));
        Assert.Equal("Copy", cut.Find(".dialog-actions button").TextContent.Trim());
    }

    [Fact]
    public void Dialog_CopyReportsSuccess()
    {
        HelpDialogTestHelpers.AllowDialogOpen(this);
        IRenderedComponent<RunCommandDialog> cut = Render<RunCommandDialog>();

        cut.Find(".dialog-actions button").Click();

        Assert.Contains("Command copied", cut.Markup);
    }

    [Fact]
    public void Dialog_CopyFailureKeepsCommandVisibleAndReportsFailure()
    {
        HelpDialogTestHelpers.AllowDialogOpen(this);
        HelpDialogTestHelpers.FailClipboardCopy(this);
        IRenderedComponent<RunCommandDialog> cut = Render<RunCommandDialog>();

        cut.Find(".dialog-actions button").Click();

        Assert.Contains(RunCommandDialog.CommandText, cut.Markup);
        Assert.Contains("Copying was not available", cut.Markup);
    }
}
