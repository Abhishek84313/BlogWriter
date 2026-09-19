# Quickstart: Blog Writer Web Interface

## Prerequisites

- .NET 10 SDK
- Existing Foundry hosted agents deployed and reachable
- Existing Cosmos session store provisioned
- Microsoft Entra app registration configured for the local HTTPS callback, with its local client secret stored in user secrets
- For production validation, an OIDC certificate in Azure Key Vault and managed-identity access to retrieve it
- Azure CLI sign-in for local Azure resource access
- Browser dependencies installed for the web test project

Configure the existing Foundry, Cosmos, tenant, and agent-name settings through user secrets or the documented local configuration path. Do not place credentials in source or committed settings files.

## Build and unit/component tests

From the repository root:

```powershell
dotnet restore
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
```

Expected outcome: application-service, ownership, workspace-state, Razor component, and accessibility-unit checks pass without requiring live model calls.

## Start the web app

```powershell
dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj
```

Open the HTTPS URL printed by the application and sign in with Microsoft Entra ID.

## Primary browser validation

1. Verify unauthenticated navigation requires sign-in and does not reveal workspace or session data.
2. Enter an initial prompt and press Shift+Enter; confirm a line break is inserted without submission.
3. Press Enter; confirm one operation starts and processing status is announced.
4. Confirm the completed draft and reviewer feedback appear in separate scrollable regions.
5. Enter a revision and submit it; confirm both regions update together after completion.
6. Enter unsaved text and choose New; decline confirmation and verify all state remains unchanged.
7. Repeat and accept; verify active work is cancelled before the workspace clears.
8. Choose List; confirm at most 20 owner-only sessions appear newest first with `[1]`, `[2]`, and so on.
9. Confirm the number input and Revise button appear only for a non-empty list.
10. Enter a valid number and choose Revise; confirm the matching saved draft and review load.
11. Try empty, malformed, zero, negative, and out-of-range selections; confirm stable content remains.
12. Choose Quit; confirm the circuit enters an ended state and rejects further writing actions.

## Accessibility and responsive validation

Run the browser suite after installing its browser binaries as documented by the chosen Playwright test package:

```powershell
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "Category=Browser|Category=Accessibility"
```

Validate at 390 × 844 (mobile) and 1440 × 900 (desktop):

- No overlap or clipped labels; draft, review, and list content scroll independently.
- Logical Tab order, visible focus, keyboard-only operation, and restored focus after dialogs.
- Programmatic labels for both prompt inputs and the session-number input.
- Live announcements for processing, cancellation, validation, errors, and completion.
- Automated accessibility scan reports no WCAG 2.2 Level A or AA violations in primary journeys.

## Cancellation and stale-result validation

Use a controllable fake workflow that waits for cancellation:

1. Start a prompt or revision.
2. Choose New, List, or Quit.
3. Accept discard confirmation when shown.
4. Verify the active operation receives cancellation and the selected action waits no more than 10 seconds.
5. Test both cancellation confirmation and timeout; in both cases verify the selected action proceeds.
6. Complete the old fake operation after cancellation and verify its result does not replace current workspace state.

## Expected architecture boundaries

- Razor components call the circuit-scoped workspace service, not hosted-agent endpoints or Cosmos directly.
- The existing BlogWriter application service owns workflow and persistence sequencing.
- Signed-in user `oid` determines session ownership; Azure CLI or managed identity separately authorizes Azure resource access.
- Existing hosted-agent projects and MAF workflow topology remain unchanged.

## MAF Doctor baseline

The post-implementation scan remains grade F with the same four pre-existing
hosted-agent credential errors and two observability warnings. Two additional
heuristic `COST-001` matches point to `IBlogWorkflow.RunAsync` calls in
`BlogWriterSessionService`; these are not direct model calls and remain protected by
the existing shared `TokenCapChatClient` factory. No new MAF error, warning, prompt,
or topology finding was introduced by the web host.
