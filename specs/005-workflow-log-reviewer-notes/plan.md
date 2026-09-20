# Implementation Plan: Workflow Log and Reviewer Notes

**Branch**: `005-workflow-log-reviewer-notes` | **Date**: 2026-09-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/005-workflow-log-reviewer-notes/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Replace the separate status/validation stack beneath the command bar with an ordered,
accessible workflow log, and route reviewer feedback updates into the existing Reviewer
notes pane as they arrive. Preserve existing workflow topology, session persistence,
cancellation, stale-result protection, and word-count behavior by carrying an optional
typed output observer through the existing workflow and session-service boundaries.

The workspace will keep two distinct output collections: lifecycle/progress/error log
entries and reviewer feedback entries. Reviewer feedback accumulates across revisions
for the active workspace session, while New and loading a different saved session clear
the accumulated view.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components, HTML, and CSS

**Primary Dependencies**: Existing Interactive Server Blazor host, `IBlogWorkflow`,
`IBlogWriterSessionService`, `BlogWorkspaceService`, Microsoft Agent Framework workflow
events, xUnit, bUnit, and Playwright test infrastructure; no new runtime package

**Storage**: No new persisted schema. Workflow log entries and accumulated reviewer
feedback are circuit/workspace output; the existing saved `ResearchState.ReviewNotes`
remains the source for a newly loaded session.

**Testing**: Focused xUnit service/workflow tests, bUnit component/state tests, browser
viewport/accessibility checks, full core/web test suites, and MAF Doctor comparison

**Target Platform**: Existing server-hosted responsive Blazor application at 390 × 844
and 1440 × 900 acceptance viewports

**Project Type**: Existing shared application assembly plus Interactive Server Blazor
web host and web test project

**Performance Goals**: Publish each accepted workflow update in the next normal circuit
render; append in memory without extra persistence or model calls; do not move keyboard
focus for routine updates

**Constraints**: Log beneath the existing command bar; no separate status stack; append
chronologically; reviewer feedback stays in Reviewer notes; safely render text; preserve
operation-version stale-result guards, cancellation, owner isolation, bounded MAF
workflow topology, token budget, authentication, word-count controls, and WCAG 2.2 AA

**Scale/Scope**: One in-memory log and reviewer-feedback history per interactive
workspace circuit; current operation and active session only; no server-side log
retention, export, cross-session aggregation, or new hosted agent

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-design gate: PASS. Post-design gate: PASS.**

| Principle | Plan response | Status |
| --- | --- | --- |
| Hosted-Agent Boundaries | Reviewer output is observed at the existing workflow/session boundary; no hosted-agent deployment or raw HTTP changes. | Pass |
| MAF-Native Workflow Composition | Existing workflow edges and bounded revision loop remain unchanged; lifecycle and reviewer updates are observations of existing execution. | Pass |
| Identity, Secrets, and Budget Control | No credential, secret, or token-budget changes; output observation does not add model calls or retries. | Pass |
| Testable and Observable Behavior | Typed output updates are injected through existing interfaces and covered with deterministic workflow, service, component, and browser tests. | Pass |
| Simple, Compatible Evolution | No persisted schema change; existing final session contract remains authoritative for load/save, with optional output reporting for live UI state. | Pass |

MAF Doctor baseline is **F** with 4 existing errors, 3 warnings, 0 silent-starvation
risks, and 6 heuristic uncapped-call matches. The feature must not add findings or
change workflow topology. The existing baseline findings are outside this feature's
scope.

## Project Structure

### Documentation (this feature)

```text
specs/005-workflow-log-reviewer-notes/
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
├── WorkflowOutputUpdate.cs                    # Typed lifecycle/reviewer update model
├── IBlogWorkflow.cs                           # Optional output observer contract
├── BlogWorkflow.cs                            # Publish workflow and reviewer updates
├── IBlogWriterSessionService.cs               # Forward output observer to workflow
├── BlogWriterSessionService.cs                # Preserve observer through start/revise
├── Workflows/BlogExecutors.cs                 # Identify reviewer output boundaries
├── BlogWriter.Web/
│   ├── Components/Pages/Home.razor            # Render command bar and log region
│   ├── Components/WorkflowLog.razor           # Accessible chronological log
│   ├── Components/ReviewPane.razor            # Render accumulated reviewer notes
│   ├── Services/BlogWorkspaceState.cs         # Log and feedback collections
│   ├── Services/BlogWorkspaceService.cs       # Route, reset, deduplicate, and publish updates
│   └── wwwroot/app.css                        # Responsive output surfaces
├── BlogWriter.Tests/
│   ├── BlogWorkflowTests.cs
│   └── BlogWriterSessionServiceTests.cs
└── BlogWriter.Web.Tests/
  ├── BlogWorkspaceServiceTests.cs
  ├── WorkflowLogTests.cs
  ├── ReviewPaneTests.cs
  ├── HomePageTests.cs
  └── WorkspaceBrowserTests.cs
```

**Structure Decision**: Keep the update model and observer boundary in the shared
application assembly so workflow, session service, and web workspace use one contract.
Keep aggregation and reset behavior in the circuit-scoped `BlogWorkspaceService`; keep
rendering in focused Razor components. Do not persist transient log history or revise
the saved session schema.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The feature fits existing workflow, session-service, workspace-state, component, and test boundaries. |
