# Quickstart: Workflow Log and Reviewer Notes

## Prerequisites

- Existing BlogWriter solution and web test project
- .NET 10 SDK
- No live Foundry, Cosmos, or Entra dependency for focused tests
- Read [workflow-output.md](contracts/workflow-output.md) for update routing and reset rules

## Focused automated validation

From the repository root:

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj --filter "FullyQualifiedName~BlogWorkflowTests|FullyQualifiedName~BlogWriterSessionServiceTests"
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~BlogWorkspaceServiceTests|FullyQualifiedName~WorkflowLogTests|FullyQualifiedName~ReviewPaneTests|FullyQualifiedName~HomePageTests"
```

Expected outcome: lifecycle updates are ordered, reviewer updates are routed separately,
duplicates and stale updates are ignored, feedback accumulates across revisions, and
New/session-load resets behave as specified.

Current validation checkpoint: 80 core tests and 49 web tests pass; both projects build
cleanly. The required viewport contract remains 390 × 844 and 1440 × 900.

## Manual workspace validation

1. Start the web application using its existing HTTPS launch profile and sign in.
2. Confirm the New, List, Revise, and Quit buttons are visible.
3. Confirm the workflow log is directly beneath the buttons and the former separate status/validation stack is absent.
4. Submit a prompt and observe progress entries appear in chronological order without moving focus.
5. Confirm reviewer feedback appears in Reviewer notes as soon as it becomes available and does not appear only in the workflow log.
6. Let the workflow complete and confirm a success entry is appended while earlier log entries and Reviewer notes remain visible.
7. Submit a revision and confirm new reviewer feedback is appended after the earlier active-session feedback.
8. Cancel or end an in-flight operation and confirm the outcome is logged while feedback already received remains visible.
9. Trigger validation, conflict, and failure paths and confirm each outcome is logged with distinguishable semantics.
10. Use New and load a different saved session; confirm old transient log and reviewer history do not leak into the new workspace state.
11. Deliver or simulate a late update from a superseded operation; confirm it does not change the current log or Reviewer notes.

## Responsive and accessibility validation

At 390 × 844 and 1440 × 900:

- Command buttons, workflow log, Draft pane, and Reviewer notes do not overlap or create horizontal scrolling.
- Workflow log and Reviewer notes have programmatic labels and readable order.
- New entries are announced politely without moving keyboard focus.
- Long messages remain text-safe, readable, and scrollable.
- Agent-provided markup-like text is displayed as literal text.

Run the existing browser/accessibility checks and verify zero new WCAG 2.2 Level A/AA violations.

## Full regression

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
git diff --check
```

The existing authentication, owner isolation, session persistence, cancellation timeout,
word-count controls, workflow termination, and hosted-agent boundaries must remain unchanged.

## MAF Doctor comparison

Run MAF Doctor before and after implementation. The post-implementation grade and finding
counts must match the recorded pre-feature baseline: F, 4 errors, 3 warnings, 0
silent-starvation risks, and 6 heuristic uncapped-call matches. No new agent call,
prompt, credential, or topology finding is expected.
