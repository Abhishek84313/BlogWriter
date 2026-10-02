# Implementation Plan: List Launcher Workflow

**Branch**: `006-list-launcher-workflow` | **Date**: 2026-09-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/006-list-launcher-workflow/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Move saved-session selection into the command bar, replace the bottom selector, and
launch a selected session's `MainTask` through the existing initial-writing workflow
after restoring `CurrentSubTask` when present. Add a small Help action that displays and
copies the HTTPS launch command, disable Revision request whenever Revise is unavailable,
and constrain the workflow-log viewport to three visible lines while retaining history.

The design keeps session identity and persistence in the existing session service, keeps
selection validation in the circuit-scoped workspace service, and uses focused Razor
components for the command-bar input and Help dialog. It does not add a database field,
new workflow edge, hosted agent, model call, or authentication path.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components, HTML, and CSS

**Primary Dependencies**: Existing Interactive Server Blazor host, `BlogWorkspaceService`,
`BlogWorkspaceState`, `IBlogWriterSessionService`, `ResearchState`, existing dialog and
JavaScript interop patterns, xUnit, bUnit, and Playwright/browser checks; no new runtime
package

**Storage**: No schema change. Saved `ResearchState.MainTask` and
`ResearchState.CurrentSubTask` are reused; list selection remains owner-scoped through
the existing session service.

**Testing**: Focused xUnit and bUnit workspace/component tests, full core/web regression,
responsive browser checks at 390 × 844 and 1440 × 900, and clipboard/dialog behavior tests

**Target Platform**: Existing server-hosted responsive Blazor application

**Project Type**: Existing shared application assembly plus Interactive Server Blazor web
host and web test project

**Performance Goals**: Valid selection starts exactly one existing workflow operation;
invalid input performs no load or workflow request; Help opens without a server request;
log rendering remains bounded to three visible lines

**Constraints**: One-based displayed-list numbering; three-digit compact input; no bottom
selector; commands to the right remain operable; Help copies
`dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https`;
`CurrentSubTask` is the optional restored Revision request; preserve discard confirmation,
session ownership, cancellation, word-count controls, workflow termination, safe text
rendering, and WCAG 2.2 AA

**Scale/Scope**: One command-bar input, one Help dialog, and one bounded log viewport in
the existing workspace; at most the existing displayed session list; no new persistence,
API, hosted agent, or deployment resource

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-design gate: PASS. Post-design gate: PASS.**

| Principle | Plan response | Status |
| --- | --- | --- |
| Hosted-Agent Boundaries | Selection uses the existing session service and initial workflow; no hosted-agent or raw HTTP changes. | Pass |
| MAF-Native Workflow Composition | Automatic launch reuses the existing initial workflow and does not add edges or alter bounded revision routing. | Pass |
| Identity, Secrets, and Budget Control | No credential, secret, or token-budget changes; Help exposes a local run command only. | Pass |
| Testable and Observable Behavior | Selection, restoration, disabled states, dialog copy, and log viewport receive focused deterministic tests and browser checks. | Pass |
| Simple, Compatible Evolution | Reuses `MainTask`, `CurrentSubTask`, existing session ownership, and existing workspace transitions; no schema migration. | Pass |

MAF Doctor baseline is **F** with 4 existing errors, 3 warnings, 0 silent-starvation
risks, and 6 heuristic uncapped-call matches. This feature must not add findings or
change workflow topology.

## Project Structure

### Documentation (this feature)

```text
specs/006-list-launcher-workflow/
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
├── BlogWriter.Web/
│   ├── Components/CommandBar.razor             # List input and Help action
│   ├── Components/RunCommandDialog.razor       # Accessible command/copy dialog
│   ├── Components/Pages/Home.razor            # Remove bottom selector and wire controls
│   ├── Components/PromptInput.razor            # Preserve disabled Revision request state
│   ├── Services/BlogWorkspaceState.cs           # Selection input and restored prompts
│   ├── Services/BlogWorkspaceService.cs         # Validate, load, restore, and auto-start
│   └── wwwroot/app.css                          # Command layout and three-line log viewport
├── BlogWriter.Tests/
│   └── SessionListSelectionTests.cs             # Selection validation/numbering tests
├── BlogWriter.Web.Tests/
│   ├── BlogWorkspaceServiceTests.cs              # Restore and automatic processing tests
│   ├── CommandBarTests.cs                        # Layout/input/disabled-state tests
│   ├── RunCommandDialogTests.cs                  # Dialog and copy behavior tests
│   ├── HomePageTests.cs                          # Placement and removal tests
│   └── WorkspaceBrowserTests.cs                  # Responsive/log viewport checks
└── README.md / docs/configuration.md             # User-facing behavior documentation
```

**Structure Decision**: Keep selection and automatic-launch orchestration in the existing
circuit-scoped workspace service. Keep command-bar presentation and Help/copy behavior in
focused Razor components, while reusing the existing discard dialog and JavaScript
interop conventions. Continue using `BlogSession.State.MainTask` and
`BlogSession.State.CurrentSubTask`; do not introduce a new persistence model.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The feature fits the existing workspace, session-service, component, and test boundaries. |
