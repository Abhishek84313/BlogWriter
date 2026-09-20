namespace BlogWriter.Web.Tests;

public sealed class WorkspaceBrowserTests
{
    [Theory]
    [InlineData(390, 844)]
    [InlineData(1440, 900)]
    public void RequiredViewports_AreExplicitlyCovered(int width, int height)
    {
        Assert.Contains((width, height), new[] { (390, 844), (1440, 900) });
    }

}