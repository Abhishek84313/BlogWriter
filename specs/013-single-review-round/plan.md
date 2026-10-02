# Implementation Plan: Single Review Round

**Branch**: `013-single-review-round` | **Date**: 2026-09-27 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/013-single-review-round/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Bound the blog workflow to one reviewer pass and, when needed, one author revision. The existing revision counter remains the state boundary: the initial draft is author pass 1, a rejected draft may return to the author for pass 2, and pass 2 becomes a terminal output without a second reviewer invocation.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# on .NET 10 with nullable reference types enabled

**Primary Dependencies**: Microsoft Agent Framework Workflows 1.10.0, workflow generators 1.10.0, Microsoft.Extensions.AI 10.9.0, xUnit test projects

**Storage**: Existing session stores persist `ResearchState`; no schema or storage format change is planned

**Testing**: Focused xUnit tests in `BlogWriter.Tests`, followed by the affected project suite and MAF topology validation

**Target Platform**: Existing console and hosted web application workflow runtime

**Project Type**: .NET console workflow service with a hosted web presentation and independently deployed Foundry agents

**Performance Goals**: Rejected drafts must complete with no more than one revision and one reviewer call; approved drafts must complete without a revision

**Constraints**: Preserve the existing `ResearchState` persistence contract, token-budget handling, cancellation, output publishing, and bounded termination. Do not add credentials, raw HTTP, retained agent session state, or new message-handler shapes.

**Scale/Scope**: One workflow run at a time per session; changes are limited to the workflow topology, terminal output routing, and focused regression tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Hosted-Agent Boundaries**: PASS. The change only routes existing Blogger, Researcher, Author, and Reviewer stages; it does not move model logic or add transport calls.
- **MAF-Native Workflow Composition**: PASS. Routing remains explicit through `ResearchState`, `WorkflowBuilder` edges, executor handlers, and workflow output events.
- **Identity, Secrets, and Budget Control**: PASS. No authentication or secret changes are introduced; existing cancellation and token-cap propagation remain intact.
- **Testable and Observable Behavior**: PASS. Add call-count and final-output tests using existing test doubles and preserve lifecycle/reviewer output publishing.
- **Simple, Compatible Evolution**: PASS. Reuse `RevisionNumber` and `MaxRevisions`; do not add a persisted flag or change session serialization.
- **MAF baseline**: MAF Doctor currently reports four pre-existing credential errors, three observability warnings, and six heuristic uncapped-call findings. These are outside this feature's scope and are not worsened by the planned changes.

## Project Structure

### Documentation (this feature)

```text
specs/013-single-review-round/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
BlogWorkflow.cs                         # Conditional author/reviewer edges and output sources
Workflows/BlogExecutors.cs              # Terminal output after the single revised author pass
ResearchState.cs                         # Existing revision-boundary semantics and documentation
BlogWriter.Tests/BlogWorkflowTests.cs   # Approval/rejection call-count and output regressions
BlogWriter.Tests/ResearchStateTests.cs  # Revision-boundary state assertions if semantics change
```

**Structure Decision**: Keep the existing root workflow implementation and focused unit-test project. No new source project, API contract, persistence model, or frontend component is needed.


## Phase 0: Research Summary

Research findings are recorded in [research.md](research.md). The design uses the existing counter and explicit conditional edges. The installed MAF API supports multiple output sources through `WithOutputFrom(ExecutorBinding[])`, and the relevant APIs are safe according to the current registry.

## Phase 1: Design Summary

- [data-model.md](data-model.md) defines the existing state fields and the author/reviewer lifecycle.
- No `contracts/` artifact is required because this is an internal workflow topology change with no external interface.
- [quickstart.md](quickstart.md) defines focused approval, rejection/revision, fallback, and broader test validation.

## Implementation Design

1. In `BlogWorkflow.cs`, keep the reviewer-to-author edge conditioned on `NeedsRevision`, make the author-to-reviewer edge conditional on `RevisionNumber < MaxRevisions`, and register both `ReviewerExecutor` and `AuthorExecutor` as output sources.
2. In `Workflows/BlogExecutors.cs`, have `AuthorExecutor` yield the state when its author pass reaches `MaxRevisions`; preserve the existing reviewer yield for initial approval and existing reviewer feedback publishing.
3. In `ResearchState.cs`, retain the value `MaxRevisions = 2` and clarify that it bounds total author passes for this workflow, if the current wording is misleading. Preserve `NeedsRevision` and `RevisionLimitReached` behavior unless focused tests show a necessary adjustment.
4. In `BlogWriter.Tests/BlogWorkflowTests.cs`, add counting test doubles and assertions for initial approval, initial rejection followed by one revision, and no second reviewer call. Verify final draft and output events.
5. Update `ResearchStateTests.cs` only for any directly affected boundary wording or state invariant; do not broaden unrelated state tests.

## Validation Plan

- Run the focused `BlogWorkflowTests` command in [quickstart.md](quickstart.md).
- Run the full `BlogWriter.Tests` project suite.
- Run MAF workflow topology simulation and confirm both terminal-capable paths complete without silent starvation.
- Build the affected projects and confirm nullable/compiler diagnostics remain clean.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | The existing workflow and test projects are sufficient. |
