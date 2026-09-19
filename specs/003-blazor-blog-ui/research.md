# Research: Blog Writer Web Interface

## Use a Blazor Web App with Interactive Server rendering

**Decision**: Add a .NET 10 Blazor Web App with Interactive Server rendering as a separate host project.

**Rationale**: Workflow execution, Cosmos access, and Foundry credentials must remain on the server. Interactive Server provides responsive component events, circuit-scoped state, cancellation, and direct use of the existing .NET application services without sending secrets or session documents to WebAssembly.

**Alternatives considered**: Blazor WebAssembly would require a separate authenticated API and browser/server state synchronization; converting the console project into the web host would mix entry points and increase regression risk.

## Keep orchestration in the existing BlogWriter application assembly

**Decision**: Extract a host-neutral `IBlogWriterSessionService` from console coordination and have both the console and Blazor host call it.

**Rationale**: The constitution assigns workflow orchestration and session handling to the existing application boundary. A shared service avoids duplicating MAF workflow calls, token-cap behavior, persistence sequencing, and conflict handling in Razor components.

**Alternatives considered**: Calling hosted agents directly from components violates the hosted-agent boundary; duplicating console logic in the web project creates divergent behavior; introducing an HTTP API between two server-side projects is unnecessary for the requested scope.

## Separate user identity from Azure resource credentials

**Decision**: Introduce `ISessionOwnerProvider`. The web implementation reads the signed-in user's immutable Entra `oid` claim; the console adapter retains its current user-token behavior. Cosmos and Foundry clients use explicit resource credentials independently.

**Rationale**: A deployed web app's managed identity identifies the application, not the signed-in user. Using it as the Cosmos partition owner would merge users' data. Claims-based ownership preserves owner isolation while managed identity securely authorizes the app to Azure resources.

**Alternatives considered**: Decoding the managed-identity Cosmos token for ownership is incorrect; passing owner IDs from browser input is insecure; storing owner identity in singleton state risks cross-circuit disclosure.

## Use explicit credentials by environment

**Decision**: Use `AzureCliCredential` for local Azure resource access and `ManagedIdentityCredential` for deployed production resource access. Use Microsoft Entra OpenID Connect through `Microsoft.Identity.Web`; keep the local OIDC client secret in user secrets and use a certificate stored in Azure Key Vault for production OIDC, retrieved with the managed identity.

**Rationale**: This satisfies repository and MAF constraints against production `DefaultAzureCredential`, avoids committed credentials, separates user sign-in from resource authorization, and makes credential selection deterministic.

**Alternatives considered**: `DefaultAzureCredential` in production is prohibited by repository guidance; a production client secret has greater rotation and leakage risk than a Key Vault certificate; workload-identity federation for the OIDC confidential client is deferred because it adds configuration complexity beyond this feature.

## Keep mutable workspace state circuit-scoped

**Decision**: Register `BlogWorkspaceService` as scoped and treat it as the serialized state machine for one authenticated Blazor circuit. Azure clients may be singleton when thread-safe, while user state, active session, displayed list, and cancellation ownership remain scoped.

**Rationale**: Microsoft guidance states that scoped services in server-side Blazor live for the circuit and warns against singleton user state. A single coordinator prevents overlapping submissions and provides one place to suppress late results after cancellation.

**Alternatives considered**: Component-local orchestration complicates navigation and testing; singleton workspace state can leak data between users; browser storage would expose sensitive drafts and complicate concurrency.

## Confirm discard before cancellation and action

**Decision**: For New, List, or Quit, first confirm discard when unsaved text exists. If accepted, cancel any active operation and wait up to 10 seconds for confirmation. In all cases increment the operation version, suppress late results, and perform the requested transition.

**Rationale**: This ordering prevents unnecessary cancellation when the user declines, bounds UI waiting when downstream cancellation stalls, and guarantees that an older workflow cannot overwrite the state produced by the selected action.

**Alternatives considered**: Waiting indefinitely can freeze always-enabled controls; acting before cancellation permits stale updates; cancelling before confirmation disrupts work even when the user chooses to remain; queueing actions makes state transitions less predictable.

## Validate two representative viewport sizes

**Decision**: Use 390 × 844 for mobile browser checks and 1440 × 900 for desktop browser checks.

**Rationale**: These dimensions make responsive acceptance deterministic while exercising a narrow portrait layout and a typical wide desktop workspace.

**Alternatives considered**: The phrase "supported viewports" was not reproducible; testing every browser dimension is impractical; adding a tablet breakpoint is optional after the two required layouts pass.

## Test components and browser behavior at separate levels

**Decision**: Use xUnit for application services, bUnit for Razor state/render tests, and Playwright browser tests with an axe-core-compatible accessibility scan plus manual keyboard/focus verification.

**Rationale**: Unit/component tests efficiently cover state transitions and disabled states, while real browser tests are needed for responsive layout, scrolling, keyboard events, focus, dialogs, and WCAG 2.2 AA behavior.

**Alternatives considered**: Unit tests alone cannot verify browser layout or accessibility tree behavior; manual testing alone is not repeatable.

## References

- [ASP.NET Core Blazor authentication and authorization](https://learn.microsoft.com/aspnet/core/blazor/security/?view=aspnetcore-10.0)
- [Secure a Blazor Web App with Microsoft Entra ID](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-entra?view=aspnetcore-10.0)
- [Managed identities for Azure resources](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview)
