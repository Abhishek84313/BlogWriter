using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlogWriter.Web;

public sealed class ClaimsSessionOwnerProvider : ISessionOwnerProvider
{
    private const string ObjectIdClaim = "oid";
    private const string ObjectIdClaimUri = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private readonly Func<Task<ClaimsPrincipal>> _principalAccessor;

    public ClaimsSessionOwnerProvider(AuthenticationStateProvider authenticationStateProvider)
        : this(async () => (await authenticationStateProvider.GetAuthenticationStateAsync()).User)
    {
    }

    public ClaimsSessionOwnerProvider(Func<ClaimsPrincipal> principalAccessor)
        : this(() => Task.FromResult(principalAccessor()))
    {
    }

    private ClaimsSessionOwnerProvider(Func<Task<ClaimsPrincipal>> principalAccessor) =>
        _principalAccessor = principalAccessor;

    public async Task<string> GetOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        ClaimsPrincipal principal = await _principalAccessor();
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException("Microsoft Entra sign-in is required.");
        }

        string? objectId = principal.FindFirstValue(ObjectIdClaim) ?? principal.FindFirstValue(ObjectIdClaimUri);
        if (string.IsNullOrWhiteSpace(objectId))
        {
            throw new InvalidOperationException("The signed-in identity does not contain an object ID claim.");
        }

        return objectId;
    }
}
