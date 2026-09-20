using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class CommandBarTests : BunitContext
{
    public CommandBarTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void CommandBar_ShowsInlineThreeDigitSelectorAndHelp()
    {
        IRenderedComponent<CommandBar> cut = Render<CommandBar>(parameters => parameters
            .Add(component => component.SelectionVisible, true)
            .Add(component => component.SelectionInput, "12")
            .Add(component => component.ReviseEnabled, true));

        Assert.Equal("12", cut.Find("#command-session-number").GetAttribute("value"));
        Assert.Equal("3", cut.Find("#command-session-number").GetAttribute("maxlength"));
        Assert.Equal("Help", cut.Find("button[data-command='help']").GetAttribute("aria-label"));
        Assert.DoesNotContain("session-selector", cut.Markup);
    }

    [Fact]
    public void CommandBar_SelectionChangeInvokesCallback()
    {
        string? value = null;
        IRenderedComponent<CommandBar> cut = Render<CommandBar>(parameters => parameters
            .Add(component => component.SelectionVisible, true)
            .Add(component => component.SelectionChanged, (string selected) => value = selected));

        cut.Find("#command-session-number").Change("2");

        Assert.Equal("2", value);
    }
}
