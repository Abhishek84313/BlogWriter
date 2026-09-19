# Implementation Plan: Blog Writer Web Interface

**Branch**: `003-blazor-blog-ui` | **Date**: 2026-09-19 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-blazor-blog-ui/spec.md`

## Summary

Add a .NET 10 Blazor Web App with Interactive Server rendering as a separate web host
that references the existing BlogWriter application assembly. Keep MAF workflow creation,
token limiting, and Cosmos persistence in the existing application layer; expose them to a
circuit-scoped workspace coordinator that owns UI state and cancellation. Authenticate users
with Microsoft Entra ID, derive session ownership from the signed-in user's `oid` claim, and
use separate explicit credentials for Azure resource access.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components, HTML and CSS

**Primary Dependencies**: ASP.NET Core Blazor Web App with Interactive Server rendering; Microsoft.Identity.Web for Microsoft Entra OpenID Connect; existing Microsoft Agent Framework, Azure.Identity, and Microsoft.Azure.Cosmos packages; bUnit and Playwright-compatible accessibility tooling for tests

**Storage**: Existing Azure Cosmos DB for NoSQL session container through `IBlogSessionStore`; no document or partition-key change

**Testing**: Existing xUnit suite; bUnit component tests; browser-level Playwright tests for responsive layout, keyboard behavior, focus, cancellation, and WCAG 2.2 AA checks

**Target Platform**: Server-hosted responsive web application on modern desktop and mobile browsers

**Project Type**: Existing console/application assembly plus a new server-side Blazor web host and web test project

**Performance Goals**: Deterministic local state transitions for New/List/selection controls; display up to 20 saved summaries without layout shift; one workflow submission per accepted Enter action; wait no more than 10 seconds for cancellation confirmation before suppressing stale results and completing New, List, or Quit

**Constraints**: Microsoft Entra sign-in required; owner identity comes from the authenticated user's `oid` claim and never from the app's Azure resource credential; use a client secret stored in user secrets for local OIDC development and a certificate stored in Azure Key Vault for production OIDC; use `AzureCliCredential` locally and `ManagedIdentityCredential` in production for Azure resource access and Key Vault certificate retrieval; no credentials in committed configuration, raw agent HTTP, client-side secrets, singleton user state, MAF topology changes, or session schema changes; validate responsive behavior at 390 × 844 and 1440 × 900; WCAG 2.2 AA; preserve token cap, cancellation, tracing, and optimistic concurrency

**Scale/Scope**: One primary authenticated workspace page per browser circuit, up to 20 listed sessions per user, one active workflow operation per circuit, four hosted agents reused unchanged; deployment infrastructure changes are outside this feature unless required for local web startup

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-design gate: PASS. Post-design gate: PASS.**

| Principle | Plan response | Status |
| --- | --- | --- |
| Hosted-Agent Boundaries | The web project calls a BlogWriter application service; existing MAF agents and hosted endpoints remain independently deployed and no component issues raw agent HTTP. | Pass |
| MAF-Native Workflow Composition | `BlogWorkflow` and `ResearchState` remain the sole workflow topology and state model; the UI adds cancellation and presentation only. | Pass |
| Identity, Secrets, and Budget Control | Entra authenticates users; `oid` scopes session data; local OIDC secrets remain in user secrets; production OIDC uses a Key Vault certificate retrieved by managed identity; explicit Azure CLI/deployed managed-identity credentials access Azure resources; existing shared token limiting remains in the application layer. | Pass |
| Testable and Observable Behavior | Workspace coordination is interface-based and circuit-scoped; component and browser tests cover state, cancellation, auth, and accessibility; existing logs and activities remain server-side. | Pass |
| Simple, Compatible Evolution | Add one web host and a narrow shared application service; preserve console behavior, session documents, agent contracts, and persistence interfaces except for owner-provider abstraction. | Pass |

MAF Doctor reports an existing F baseline from four hosted-agent `DefaultAzureCredential`
findings and uncapped call-site warnings. This feature does not modify hosted-agent code or
workflow message topology and must introduce no additional findings. MAF hard constraints
applied here include no production `DefaultAzureCredential`, no user state in singleton or
MAF provider fields, no deprecated message attributes, and no bypass of the shared token cap.

## Project Structure

### Documentation (this feature)

```text
specs/003-blazor-blog-ui/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
./
├── BlogWriter.csproj                         # Existing console/application assembly; exclude web project globs
├── Program.cs                                # Existing console host, updated to use shared registration/service
├── IBlogWriterSessionService.cs              # Host-neutral application operations
├── BlogWriterSessionService.cs               # Create, run, revise, list, load, save, and cancellation-safe coordination
├── ISessionOwnerProvider.cs                  # Owner identity abstraction
├── EntraSessionOwnerProvider.cs              # Existing console token-claim adapter
├── CosmosBlogSessionStore.cs                 # Reuse persistence through owner abstraction
├── BlogWriter.Web/
│   ├── BlogWriter.Web.csproj
│   ├── Program.cs                            # Blazor, Entra OIDC, explicit Azure credentials, DI
│   ├── ClaimsSessionOwnerProvider.cs         # Circuit/request user `oid` adapter
│   ├── Services/BlogWorkspaceService.cs      # Circuit-scoped UI state and serialized operations
│   ├── Components/App.razor
│   ├── Components/Routes.razor
│   ├── Components/Layout/MainLayout.razor
│   ├── Components/Pages/Home.razor           # Primary workspace
│   ├── Components/ConfirmDiscardDialog.razor
│   └── wwwroot/app.css
├── BlogWriter.Web.Tests/
│   ├── BlogWriter.Web.Tests.csproj
│   ├── BlogWorkspaceServiceTests.cs
│   ├── HomePageTests.cs
│   └── AccessibilityTests.cs
└── docs/
  ├── configuration.md
  └── deployment.md
```

## Structure Decision

Use a separate Interactive Server Blazor host that references
the existing BlogWriter assembly. Extract host-neutral workflow/session coordination
from the console entry point instead of duplicating it in Razor components. Keep all
per-user mutable workspace state in a scoped service whose lifetime is the Blazor circuit;
keep only thread-safe Azure clients and immutable agent configuration at longer lifetimes.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| --- | --- | --- |
| Separate web host and web test project | Blazor hosting/authentication and component tests require web SDK boundaries while the existing console must remain runnable. | Converting the console project in place would mix entry points and risk breaking the established CLI. |
| Owner-provider abstraction | Web user ownership must come from the authenticated `oid` claim, while Cosmos/Foundry access uses an application credential. | Reusing the current token-decoding provider with managed identity would partition sessions by the app identity and violate owner isolation. |
