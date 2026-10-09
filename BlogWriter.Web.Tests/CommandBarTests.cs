using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class CommandBarTests : BunitContext
{
    public CommandBarTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void CommandBar_ShowsCommandsWithoutNumericSessionSelector()
    {
        IRenderedComponent<CommandBar> cut = Render<CommandBar>();

        Assert.Empty(cut.FindAll("#command-session-number"));
        Assert.NotNull(cut.Find("button[data-command='list']"));
        Assert.NotNull(cut.Find("button[data-command='go']"));
        Assert.Equal("Help", cut.Find("button[data-command='help']").GetAttribute("aria-label"));
    }
}
