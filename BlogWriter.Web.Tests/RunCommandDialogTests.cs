using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class RunCommandDialogTests : BunitContext
{
    public RunCommandDialogTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Dialog_ShowsAboutTextAndAccessibleModal()
    {
        HelpDialogTestHelpers.AllowDialogOpen(this);
        IRenderedComponent<RunCommandDialog> cut = Render<RunCommandDialog>();

        Assert.Contains("About BlogWriter", cut.Markup);
        Assert.Contains("BlogWriter", cut.Markup);
        Assert.Contains("An open source program", cut.Markup);
        Assert.Contains("© Copyright 2026 Jesse Liberty", cut.Markup);
        Assert.Contains("See License", cut.Markup);
        Assert.Equal("dialog", cut.Find("dialog").GetAttribute("role"));
        Assert.Equal("true", cut.Find("dialog").GetAttribute("aria-modal"));
        Assert.DoesNotContain("Copy", cut.FindAll(".dialog-actions button").Select(button => button.TextContent));
        Assert.DoesNotContain("HTTPS port", cut.Markup);
        Assert.Equal("Close", cut.Find(".dialog-actions button").TextContent.Trim());
    }
}
