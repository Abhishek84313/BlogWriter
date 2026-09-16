using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public class BlogSessionStoreOwnershipTests
{
    [Fact]
    public async Task GetAndListAsync_OnlyReturnTheCurrentOwnerSessions()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"BlogWriterTests-{Guid.NewGuid():N}");

        try
        {
            var firstOwnerStore = new FileBlogSessionStore(directory, "owner-one");
            var secondOwnerStore = new FileBlogSessionStore(directory, "owner-two");
            BlogSession firstOwnerSession = await firstOwnerStore.CreateAsync(new ResearchState { MainTask = "first" });
            await secondOwnerStore.CreateAsync(new ResearchState { MainTask = "second" });

            Assert.NotNull(await firstOwnerStore.GetAsync(firstOwnerSession.Id));
            Assert.Single(await firstOwnerStore.ListAsync());
            Assert.Equal("first", (await firstOwnerStore.ListAsync())[0].MainTask);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DeleteOwnerSessionsAsync_RemovesOnlyRequestedOwnerSessions()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"BlogWriterTests-{Guid.NewGuid():N}");

        try
        {
            var firstOwnerStore = new FileBlogSessionStore(directory, "owner-one");
            var secondOwnerStore = new FileBlogSessionStore(directory, "owner-two");
            await firstOwnerStore.CreateAsync(new ResearchState { MainTask = "first" });
            await secondOwnerStore.CreateAsync(new ResearchState { MainTask = "second" });

            await firstOwnerStore.DeleteOwnerSessionsAsync("owner-one");

            Assert.Empty(await firstOwnerStore.ListAsync());
            Assert.Single(await secondOwnerStore.ListAsync());
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}