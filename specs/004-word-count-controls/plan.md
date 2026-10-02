# Implementation Plan: Word Count Controls

**Branch**: `004-word-count-controls` | **Date**: 2026-09-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-word-count-controls/spec.md`

## Summary

Add compact Min and Max word-count inputs to the existing Blazor workspace between the
prompt strip and Draft/Reviewer panes. Centralize positive-whole-number and ordered-range
validation in a shared value object, snapshot a valid range for each initial or revision
submission, pass it through the existing session service into `ResearchState`, and track
the last accepted range separately from editable UI text so in-flight edits remain unsaved.

## Technical Context

**Language/Version**: C# / .NET 10, Razor components, HTML and CSS

**Primary Dependencies**: Existing Interactive Server Blazor host, `IBlogWriterSessionService`, `ResearchState`, bUnit, xUnit, and Playwright test infrastructure; no new runtime package

**Storage**: Existing `ResearchState.MinWords` and `ResearchState.MaxWords` fields in the owner-scoped Cosmos session document; no schema or partition-key change

**Testing**: xUnit value/service tests, bUnit workspace/component tests, and existing browser viewport/accessibility checks

**Target Platform**: Existing server-hosted responsive Blazor application at 390 × 844 and 1440 × 900 acceptance viewports

**Project Type**: Existing shared application assembly plus Interactive Server Blazor web host and web test project

**Performance Goals**: Validation completes locally before any workflow call; invalid ranges produce zero workflow submissions; changing the fields introduces no additional Azure request

**Constraints**: Labels exactly `Min` and `Max`; controls follow prompts and precede Draft/Reviewer in DOM and visual order; defaults 1000/2000; positive whole numbers with Max ≥ Min; no product upper bound beyond the existing integer representation; submitted values are immutable for an in-flight operation; changes from the accepted range count as unsaved work; preserve MAF topology, token cap, authentication, owner isolation, cancellation, stable-output behavior, and WCAG 2.2 AA

**Scale/Scope**: Two inputs on the existing primary workspace; existing saved sessions and initial/revision workflows only; no user preference store, new API, hosted-agent, infrastructure, or Cosmos migration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Pre-design gate: PASS. Post-design gate: PASS.**

| Principle | Plan response | Status |
| --- | --- | --- |
| Hosted-Agent Boundaries | Word targets pass through the existing application service and `ResearchState`; no agent endpoint or hosted-agent code changes. | Pass |
| MAF-Native Workflow Composition | Existing workflow topology and bounded revision loop remain unchanged; only input state values change. | Pass |
| Identity, Secrets, and Budget Control | No identity, credential, secret, or token-budget behavior changes; invalid ranges are rejected before model calls. | Pass |
| Testable and Observable Behavior | Shared range validation and workspace transitions receive focused tests; existing cancellation and logging paths remain intact. | Pass |
| Simple, Compatible Evolution | Reuse persisted `ResearchState` fields and existing defaults; add one small value object and one focused component. | Pass |

MAF Doctor reports the existing F baseline (four credential errors, three warnings, and
heuristic uncapped-call findings). This feature changes neither MAF calls nor agent
topology and must introduce no new findings.

## Project Structure

### Documentation (this feature)

```text
specs/004-word-count-controls/
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
├── WordRange.cs                              # Shared parsing and validation value object
├── IBlogWriterSessionService.cs              # Add range to revision contract
├── BlogWriterSessionService.cs               # Apply range to initial/revision state snapshots
├── BlogWriter.Web/
│   ├── Components/WordRangeInput.razor        # Labeled compact Min/Max controls
│   ├── Components/Pages/Home.razor            # Place controls between prompts and panes
│   ├── Services/BlogWorkspaceState.cs         # Editable and accepted range state
│   ├── Services/BlogWorkspaceService.cs       # Validation, snapshots, load/reset/unsaved transitions
│   └── wwwroot/app.css                        # Responsive compact control row
├── BlogWriter.Tests/
│   ├── WordRangeTests.cs
│   └── BlogWriterSessionServiceTests.cs
└── BlogWriter.Web.Tests/
    ├── WordRangeInputTests.cs
    ├── HomePageTests.cs
    ├── BlogWorkspaceServiceTests.cs
    └── WorkspaceBrowserTests.cs
```

## Structure Decision

Keep range validation in the shared application assembly so UI and session-service
boundaries enforce identical rules. Store editable strings and accepted numeric values
in the circuit-scoped workspace state: strings retain invalid user input for feedback,
while accepted values provide a stable unsaved-change baseline. A submission captures
a `WordRange` snapshot before awaiting the workflow. Publication updates the accepted
baseline to that snapshot but preserves visible edits made after submission.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| None | N/A | The feature fits existing state, service, component, and test boundaries. |
