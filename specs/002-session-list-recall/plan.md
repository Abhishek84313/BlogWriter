# Implementation Plan: Session List and Recall

**Branch**: `002-session-list-recall` | **Date**: 2026-09-18 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-session-list-recall/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Add one-based `[n]` labels to the existing ordered previous-session display and allow
`resume <n>` to resolve the number against a freshly loaded session list before loading
the underlying session. Raw session identifiers are not accepted as user recall input.
The change is limited to the console command boundary and presentation; persistence,
ownership, ordering, and workflow execution remain unchanged. Raw session identifiers
are no longer accepted as user recall input.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10

**Primary Dependencies**: Existing console application, session command records, and `IBlogSessionStore`

**Storage**: Existing `IBlogSessionStore` implementations; no storage schema change

**Testing**: Existing xUnit test project with focused parser, selection, and store tests

**Target Platform**: .NET console application on the existing supported host environments

**Project Type**: Console application with a reusable session-command parsing surface

**Performance Goals**: Numbering and selection are linear in the displayed list, which is capped at 20 entries; no additional model or storage round trips beyond the current list/load flow

**Constraints**: Preserve session ownership checks, list ordering, empty-state behavior, cancellation, and active-session safety. Recall accepts displayed numbers only; raw session identifiers are rejected. Do not add raw agent calls, new persistence fields, or authentication changes.

**Scale/Scope**: At most 20 displayed sessions per list; numeric labels must remain unambiguous for at least 100 entries in isolated formatting/selection tests.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Hosted-agent boundaries remain unchanged; the feature is console session handling only.
- [x] No new MAF workflow stage, executor, message handler, or agent call is introduced.
- [x] Entra ownership and existing persistence authorization remain unchanged.
- [x] Focused unit tests will cover parsing, formatting, selection validation, and active-session preservation.
- [x] The change preserves existing public session formats and numeric recall behavior.
- [ ] Repository-wide MAF health is currently F because of pre-existing findings in hosted-agent credentials and uncapped model call sites; this feature does not expand that scope.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
SessionCommandParser.cs                 # Parse list, numeric resume, and invalid-selection input
SessionListSelection.cs                 # Format numbered entries and resolve numeric selections
Program.cs                              # Load current summaries, format labels, resolve selection, load session
BlogWriter.Tests/SessionCommandParserTests.cs
                    # Parser and selector validation coverage
BlogWriter.Tests/SessionListFormattingTests.cs
                    # Numbered output and selection mapping coverage
specs/002-session-list-recall/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
  └── console-commands.md
```

## Structure Decision

Keep the feature in the existing console command and session
store boundary. Add only the small `SessionListSelection.cs` helper; add no new project,
storage schema, hosted-agent endpoint, or workflow stage. Feature-specific design
artifacts live beside the specification.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The feature fits the existing parser, console loop, and session summary flow. |
