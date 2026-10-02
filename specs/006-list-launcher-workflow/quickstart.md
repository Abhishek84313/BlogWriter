# Quickstart: List Launcher Workflow

## Prerequisites

- Existing BlogWriter solution and web test project
- .NET 10 SDK
- No live Foundry, Cosmos, or Entra dependency for focused tests
- Read [list-launcher-ui.md](contracts/list-launcher-ui.md) for the interaction contract

## Focused automated validation

From the repository root:

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj --filter "FullyQualifiedName~SessionListSelectionTests"
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~BlogWorkspaceServiceTests|FullyQualifiedName~CommandBarTests|FullyQualifiedName~RunCommandDialogTests|FullyQualifiedName~HomePageTests"
```

Expected outcome: valid selection launches exactly one operation, invalid selection
starts none, MainTask/CurrentSubTask restoration is correct, the bottom selector is
absent, Help copy behavior is covered, Revision request follows Revise availability,
and the log viewport is bounded to three visible lines.

Automated checkpoint: 88 core tests and 58 web tests pass; both projects build cleanly.

Browser checkpoint: Help, exact command text, disabled Revision request, and responsive
containment passed at 390 × 844 and 1440 × 900. The Testing environment's placeholder
Cosmos endpoint did not return saved sessions, so inline selection was validated through
the focused workspace/service tests rather than a live browser session list.

## Manual workspace validation

1. Start the web application with `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https` and sign in.
2. Click List and confirm a compact numeric input appears beside List; confirm the selector is not present at the bottom of the form.
3. Confirm New, List, Revise, Quit, and `?` remain visible and the commands to the right of List remain operable.
4. Enter an invalid selector such as empty text, `0`, `-1`, `1.5`, text, or a number outside the displayed list; confirm no session loads or workflow starts and a log validation entry appears.
5. Enter a valid displayed number; confirm Draft, Reviewer notes, New writing prompt, and Revision request clear before the selected prompt data is restored.
6. Confirm the selected session's MainTask fills New writing prompt and a non-empty CurrentSubTask fills Revision request.
7. Confirm processing starts exactly once from the restored New writing prompt.
8. Open Help, verify the HTTPS run command, copy it, and confirm success feedback. Test clipboard denial/failure if available and confirm the command remains visible.
9. Disable Revise by leaving no eligible selected session and confirm Revision request is disabled; make Revise eligible and confirm it becomes enabled when not processing.
10. Produce more than three log entries and confirm the visible log viewport holds three lines while older entries remain scrollable.

## Responsive and accessibility validation

At 390 × 844 and 1440 × 900:

- List selector is beside List, is wide enough for three digits, and does not overlap commands.
- The Help button has an accessible name and the dialog has modal labeling and keyboard dismissal.
- Revision request disabled state matches Revise disabled state.
- Workflow log shows three lines without horizontal overflow and retains scrollability.
- Prompt and restored session values are readable and text-safe.
- Existing labels, focus order, and WCAG 2.2 Level A/AA behavior remain intact.

## Full regression

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
git diff --check
```

Existing authentication, session ownership, Cosmos persistence, cancellation, word-count
controls, workflow termination, Reviewer notes routing, and MAF topology must remain unchanged.

## MAF Doctor comparison

Run MAF Doctor before and after implementation. The post-implementation result must match
the baseline: grade F, 4 errors, 3 warnings, 0 silent-starvation risks, and 6 heuristic
uncapped-call matches. No new workflow, prompt, credential, or model-call finding is expected.
