using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public class FileBlogSessionStoreTests
{
    [Fact]
    public async Task CreateAndGetAsync_RoundTripsCompletedWorkflowState()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"BlogWriterTests-{Guid.NewGuid():N}");

        try
        {
            var store = new FileBlogSessionStore(directory);
            var state = new ResearchState
            {
                MainTask = "session memory",
                ResearchFindings = ["finding"],
                Draft = "draft",
                ReviewNotes = ResearchState.ApprovedMarker,
            };

            BlogSession created = await store.CreateAsync(state);
            BlogSession? loaded = await store.GetAsync(created.Id);

            Assert.NotNull(loaded);
            Assert.Equal(created.Id, loaded.Id);
            Assert.Equal("session memory", loaded.State.MainTask);
            Assert.Equal(["finding"], loaded.State.ResearchFindings);
            Assert.Equal("draft", loaded.State.Draft);
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
    public async Task GetAsync_ReturnsNullForUnknownOrInvalidSessionId()
    {
        var store = new FileBlogSessionStore(Path.Combine(Path.GetTempPath(), $"BlogWriterTests-{Guid.NewGuid():N}"));

        Assert.Null(await store.GetAsync("not-a-session-id"));
        Assert.Null(await store.GetAsync(Guid.NewGuid().ToString("N")));
    }

    [Fact]
    public async Task ListAsync_ReturnsTheTwentyMostRecentlyUpdatedSessionsFirst()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"BlogWriterTests-{Guid.NewGuid():N}");

        try
        {
            var store = new FileBlogSessionStore(directory);
            for (int index = 0; index < 21; index++)
            {
                await store.CreateAsync(new ResearchState { MainTask = $"topic {index}" });
            }

            IReadOnlyList<BlogSessionSummary> sessions = await store.ListAsync();

            Assert.Equal(20, sessions.Count);
            Assert.Equal("topic 20", sessions[0].MainTask);
            Assert.DoesNotContain(sessions, session => session.MainTask == "topic 0");
            Assert.True(sessions.Zip(sessions.Skip(1), (first, second) => first.UpdatedAt >= second.UpdatedAt).All(result => result));
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