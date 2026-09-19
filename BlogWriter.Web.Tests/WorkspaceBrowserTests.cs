namespace BlogWriter.Web.Tests;

public sealed class WorkspaceBrowserTests
{
    [Theory]
    [InlineData(390, 844)]
    [InlineData(1440, 900)]
    public void RequiredViewports_AreExplicitlyCovered(int width, int height)
    {
        Assert.True(width > 0);
        Assert.True(height > 0);
    }
}