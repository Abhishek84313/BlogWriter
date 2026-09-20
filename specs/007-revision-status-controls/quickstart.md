# Quickstart: Revision Status Controls

## Prerequisites

- Existing BlogWriter solution and web test project
- .NET 10 SDK
- No live Foundry, Cosmos, or Entra dependency for focused tests
- Read [revision-status-ui.md](contracts/revision-status-ui.md) for the UI contract

## Focused automated validation

From the repository root:

```powershell
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~HomePageTests|FullyQualifiedName~BlogWorkspaceServiceTests|FullyQualifiedName~WorkflowLogTests|FullyQualifiedName~CommandBarTests"
```

Expected outcome: New enables Revision request but not Revise, drafts enable both,
processing disables submissions, the latest status replaces earlier messages, Reviewer
notes remain separate, and command controls retain compact responsive layout.

Focused checkpoint: feature 007 web tests pass with the clarified New/draft/processing
availability matrix and latest-status presentation.

Final automated checkpoint: 88 core tests and 64 web tests pass; both projects build
cleanly.

## Manual workspace validation

1. Start the web application with `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https` and sign in.
2. In an empty workspace, confirm Revision request and Revise are disabled.
3. Click New; confirm Revision request becomes editable while Revise remains disabled until a draft exists.
4. Submit a prompt and confirm both Revision request and Revise become available after the draft appears.
5. Start a revision and confirm both controls are protected during processing.
6. Trigger progress, success, validation, cancellation, and failure updates; confirm one status line shows only the newest message.
7. Confirm the scrolling workflow-log list is gone and Reviewer notes remain in their own pane.
8. Click List and confirm the List selector appears while Revise, Quit, and Help move right without overlap.
9. Confirm command buttons are smaller but readable and operable.

## Responsive and accessibility validation

At 390 × 844 and 1440 × 900:

- Command buttons and conditional List selector do not overlap or create horizontal scrolling.
- Status line is labeled, polite, text-safe, and readable.
- Revision request disabled state is announced correctly.
- Reviewer notes remain separately labeled and visible.
- Keyboard focus and Enter/Shift+Enter prompt behavior remain intact.

## Full regression

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
git diff --check
```

Existing authentication, session ownership, word-count controls, cancellation, workflow
termination, List selection, Help copy behavior, and Reviewer notes routing must remain unchanged.
