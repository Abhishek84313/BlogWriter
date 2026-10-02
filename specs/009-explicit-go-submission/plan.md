# Implementation Plan: Explicit Go Submission

**Branch**: `009-explicit-go-submission` | **Date**: 2026-09-21 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/009-explicit-go-submission/spec.md`

## Summary

Replace Enter-based prompt submission with one explicit Go control beside the word-range fields. Route that action through the existing workspace service: a non-empty revision request takes priority; otherwise a non-empty initial prompt starts a draft. Derive revision-input and revision-command availability solely from non-whitespace displayed draft content (and existing processing protection), without changing persistence, hosted-agent orchestration, or word-range validation.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10

**Primary Dependencies**: ASP.NET Core Blazor Interactive Server, Microsoft.Identity.Web, existing BlogWriter session/workflow services

**Storage**: Existing session persistence only; no schema or persistence changes

**Testing**: xUnit and bUnit in `BlogWriter.Web.Tests`; existing Playwright suite remains available for browser coverage

**Target Platform**: Authenticated responsive browser workspace

**Project Type**: Blazor web application with an existing .NET service library

**Performance Goals**: UI eligibility and Go routing update synchronously with user input; no additional remote calls before an intentional submission

**Constraints**: Go is the sole draft/revision processing trigger; Enter must retain native text-entry behavior; preserve current range validation, processing safeguards, session ownership, cancellation, and accessibility semantics

**Scale/Scope**: One existing workspace page, two prompt inputs, one word-range component, workspace state/service, and focused web tests

## Constitution Check

*GATE: Passed before Phase 0 research. Re-checked after Phase 1 design: Passed.*

- **Hosted-agent boundaries**: Passed. The change invokes existing session-service operations; it adds no direct hosted-agent calls or transport.
- **MAF-native workflow composition**: Passed. No workflow topology, state transition, or agent implementation changes are planned.
- **Identity, secrets, and budget control**: Passed. Existing authenticated service path and token-budget behavior remain unchanged.
- **Testable and observable behavior**: Passed. The routing decision is centralized in the workspace service and covered with focused bUnit and service tests.
- **Simple, compatible evolution**: Passed. The existing prompt, word-range, and workspace abstractions are extended without persistence or public contract changes.

## Project Structure

### Documentation (this feature)

```text
specs/009-explicit-go-submission/
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
BlogWriter.Web/
├── Components/
│   ├── Pages/Home.razor                 # Wires prompt, word-range, and workspace controls
│   ├── PromptInput.razor                # Text entry without implicit submission
│   └── WordRangeInput.razor             # Min, Max, and explicit Go control
├── Services/
│   ├── BlogWorkspaceService.cs           # Intentional submission routing
│   └── BlogWorkspaceState.cs             # Derived revision availability
└── wwwroot/app.css                       # Existing workspace control layout and styling

BlogWriter.Web.Tests/
├── HomePageTests.cs                      # Rendered controls and keyboard behavior
└── BlogWorkspaceServiceTests.cs           # Routing and eligibility state transitions
```

**Structure Decision**: Extend the existing Blazor components and workspace service/state. No new project, API endpoint, persistence model, or workflow abstraction is warranted.

## Complexity Tracking

No constitution violations or complexity exceptions.
