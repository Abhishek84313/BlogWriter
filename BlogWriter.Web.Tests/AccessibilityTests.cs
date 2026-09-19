using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class AccessibilityTests : BunitContext
{
    public AccessibilityTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void ConfirmDialog_ExposesModalSemanticsAndNamedActions()
    {
        IRenderedComponent<ConfirmDiscardDialog> cut = Render<ConfirmDiscardDialog>();

        Assert.Equal("true", cut.Find("[role='dialog']").GetAttribute("aria-modal"));
        Assert.NotNull(cut.Find("#discard-title"));
        Assert.Equal("Keep editing", cut.Find("button[data-confirm='keep']").TextContent.Trim());
        Assert.Equal("Discard", cut.Find("button[data-confirm='discard']").TextContent.Trim());
    }
}