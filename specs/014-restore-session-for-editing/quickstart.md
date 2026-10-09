# Quickstart: Restore Saved Session for Editing

## Prerequisites

- .NET 10 SDK.
- Repository dependencies restored.
- For automated tests, no live Foundry or Cosmos connection is required; tests use session-service doubles.
- For the manual browser check, a signed-in test/development identity and configured session store with at least one saved session are required.

## Focused Automated Validation

Run the workspace service, page, and browser interaction tests:

```powershell
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~BlogWorkspaceServiceTests|FullyQualifiedName~HomePageTests|FullyQualifiedName~WorkspaceBrowserTests"
```

Run the complete web test suite and build the web project:

```powershell
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
```

## Manual Browser Scenario

1. Sign in and open the Blog Writer workspace.
2. Choose List and wait for saved sessions to appear.
3. Verify there is no numeric selector next to List; select a saved row with the mouse or keyboard.
4. Verify the query and saved Min/Max values appear in their fields, the applicable revision request is restored, and the input controls remain editable.
5. Verify Draft and Reviewer output are cleared and no workflow progress or new history entry appears before Go.
6. Change the query or word range and activate Go once.
7. Verify exactly one new writing run uses the displayed query and range; its result appears as a separate history entry, while the selected source entry remains unchanged.
8. Repeat with invalid Min/Max and confirm existing validation prevents submission.

## Expected Outcome

Selection only restores editable state. Go is the sole workflow trigger and creates a distinct saved run through the existing session service. Existing owner isolation, cancellation, and word-range validation continue to pass.
