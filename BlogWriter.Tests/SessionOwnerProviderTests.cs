using System.Text;
using Azure.Core;
using BlogWriter;
using Xunit;

namespace BlogWriter.Tests;

public sealed class SessionOwnerProviderTests
{
    [Fact]
    public async Task GetOwnerIdAsync_ReturnsObjectIdClaim()
    {
        var provider = new EntraSessionOwnerProvider(new JwtCredential("owner-123"));

        Assert.Equal("owner-123", await provider.GetOwnerIdAsync());
    }

    [Fact]
    public async Task GetOwnerIdAsync_RejectsTokenWithoutObjectId()
    {
        var provider = new EntraSessionOwnerProvider(new JwtCredential(null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetOwnerIdAsync());
    }

    private sealed class JwtCredential(string? objectId) : TokenCredential
    {
        private readonly AccessToken _token = new(CreateToken(objectId), DateTimeOffset.UtcNow.AddMinutes(5));

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => _token;

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            ValueTask.FromResult(_token);

        private static string CreateToken(string? objectId)
        {
            string payload = objectId is null ? "{}" : $"{{\"oid\":\"{objectId}\"}}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return $"e30.{encoded}.signature";
        }
    }
}
