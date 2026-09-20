using Bunit;
using BlogWriter.Web.Components;

namespace BlogWriter.Web.Tests;

public sealed class ReviewPaneTests : BunitContext
{
    [Fact]
    public void ReviewPane_RendersAccumulatedFeedbackAsText()
    {
        IRenderedComponent<ReviewPane> cut = Render<ReviewPane>(parameters => parameters
            .Add(component => component.Content, "first review\n\n<second review>"));

        Assert.Contains("first review", cut.Find(".review-copy").TextContent);
        Assert.Contains("<second review>", cut.Find(".review-copy").TextContent);
        Assert.Equal("polite", cut.Find(".pane-content").GetAttribute("aria-live"));
    }
}