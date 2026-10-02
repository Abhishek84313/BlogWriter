# Implementation Plan: Revision Status Controls

**Branch**: `007-revision-status-controls` | **Date**: 2026-09-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/007-revision-status-controls/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Decouple Revision request availability from List mode so New-state users can edit a
revision request while Revise becomes actionable only after a draft/session exists.
Replace the retained scrolling workflow-log list with a single latest-status projection,
while preserving Reviewer notes separately. Add compact command-bar sizing and a
conditional layout that reserves space for the List selector by moving Revise, Quit, and
Help to the right when the selector is visible.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components, HTML, and CSS

**Primary Dependencies**: Existing Interactive Server Blazor host, `BlogWorkspaceState`,
`BlogWorkspaceService`, `PromptInput`, `CommandBar`, `WorkflowLog`, xUnit, bUnit, and
browser checks; no new runtime package

**Storage**: No persisted schema change. Latest status is transient workspace state;
Reviewer notes and workflow/session persistence remain unchanged.

**Testing**: Focused bUnit state/component tests, responsive browser-contract checks,
full core/web regression, and accessibility validation

**Target Platform**: Existing server-hosted responsive Blazor application at 390 × 844
and 1440 × 900

**Project Type**: Existing Interactive Server Blazor web host and web test project

**Performance Goals**: Each accepted workflow update replaces one in-memory status value
and triggers one normal workspace render; no extra persistence or model call

**Constraints**: New enables Revision request but not Revise until a draft/session exists;
processing disables submissions; only newest status is rendered; Reviewer notes remain
separate; command buttons get smaller; List selector movement must preserve responsive
layout and WCAG 2.2 AA; no workflow topology or session contract changes

**Scale/Scope**: Existing workspace command bar, prompt strip, and workflow status only;
no new persistence, API, hosted agent, or deployment resource

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-design gate: PASS. Post-design gate: PASS.**

| Principle | Plan response | Status |
| --- | --- | --- |
| Hosted-Agent Boundaries | UI/state projection only; no hosted-agent or raw HTTP changes. | Pass |
| MAF-Native Workflow Composition | Existing workflow execution and bounded revision loop remain unchanged. | Pass |
| Identity, Secrets, and Budget Control | No credentials, secrets, model calls, or token-budget changes. | Pass |
| Testable and Observable Behavior | State transitions, latest-status replacement, component semantics, and responsive layout receive focused tests. | Pass |
| Simple, Compatible Evolution | Reuses existing workspace update notifications and Reviewer notes; removes only transient log-list presentation. | Pass |

No MAF scanner change is expected because the feature does not alter agent calls or
workflow topology.

## Project Structure

### Documentation (this feature)

```text
specs/007-revision-status-controls/
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
│   ├── Components/Pages/Home.razor       # Bind revision availability and latest status
│   ├── Components/PromptInput.razor      # Preserve input semantics and disabled state
│   ├── Components/CommandBar.razor       # Compact buttons and conditional grouping
│   ├── Components/WorkflowLog.razor      # Render one current status line
│   ├── Services/BlogWorkspaceState.cs    # Revision flags and current status projection
│   ├── Services/BlogWorkspaceService.cs  # Replace status on accepted updates
│   └── wwwroot/app.css                   # Compact commands and responsive layout
├── BlogWriter.Web.Tests/
│   ├── HomePageTests.cs                  # Revision/New/status behavior
│   ├── CommandBarTests.cs                # Grouping and button dimensions
│   ├── WorkflowLogTests.cs               # Latest-status rendering
│   └── WorkspaceBrowserTests.cs          # Responsive/accessibility contracts
└── README.md / docs/configuration.md     # User-facing behavior documentation
```

**Structure Decision**: Keep latest-status projection and revision availability in the
existing circuit-scoped workspace state/service. Keep command and status rendering in
the existing Razor components and CSS. Do not change session persistence, Reviewer notes,
workflow routing, or hosted-agent boundaries.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The feature fits existing workspace state, components, and test boundaries. |
