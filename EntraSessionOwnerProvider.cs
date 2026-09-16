using System.Text;
using System.Text.Json;
using Azure.Core;

namespace BlogWriter;

public sealed class EntraSessionOwnerProvider(TokenCredential credential)
{
    private static readonly TokenRequestContext s_cosmosScope = new(["https://cosmos.azure.com/.default"]);
    private readonly TokenCredential _credential = credential;

    public async Task<string> GetOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        AccessToken token = await _credential.GetTokenAsync(s_cosmosScope, cancellationToken);
        string[] segments = token.Token.Split('.');
        if (segments.Length < 2)
        {
            throw new InvalidOperationException("The Microsoft Entra access token does not contain an identity payload.");
        }

        string payload = segments[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using JsonDocument document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
        if (!document.RootElement.TryGetProperty("oid", out JsonElement objectId) ||
            string.IsNullOrWhiteSpace(objectId.GetString()))
        {
            throw new InvalidOperationException("The Microsoft Entra access token does not identify a user object.");
        }

        return objectId.GetString()!;
    }
}