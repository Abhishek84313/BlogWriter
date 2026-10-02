using System.Security.Claims;
using BlogWriter.Web;

namespace BlogWriter.Web.Tests;

public sealed class ClaimsSessionOwnerProviderTests
{
    [Fact]
    public async Task GetOwnerIdAsync_ReturnsAuthenticatedObjectId()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("oid", "owner-123")],
            authenticationType: "test"));
        var provider = new ClaimsSessionOwnerProvider(() => principal);

        Assert.Equal("owner-123", await provider.GetOwnerIdAsync());
    }

    [Fact]
    public async Task GetOwnerIdAsync_RejectsUnauthenticatedUser()
    {
        var provider = new ClaimsSessionOwnerProvider(() => new ClaimsPrincipal(new ClaimsIdentity()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetOwnerIdAsync());
    }

    [Fact]
    public async Task GetOwnerIdAsync_RejectsMissingObjectId()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([], authenticationType: "test"));
        var provider = new ClaimsSessionOwnerProvider(() => principal);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetOwnerIdAsync());
    }
}
