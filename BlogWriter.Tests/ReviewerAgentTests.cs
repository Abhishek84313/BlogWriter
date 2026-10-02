using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlogWriter.Tests;

public sealed class ReviewerAgentTests
{
    [Fact]
    public async Task InvokeAsync_PropagatesInfrastructureFailure()
    {
        var agent = new ReviewerAgent(
            new ThrowingChatClient(),
            new ChatOptions { Temperature = 0, MaxOutputTokens = 100 },
            NullLogger<ReviewerAgent>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            agent.InvokeAsync(new ResearchState { MainTask = "topic", Draft = "draft" }));
    }
}