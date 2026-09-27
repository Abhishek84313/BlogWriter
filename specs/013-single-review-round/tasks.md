---

description: "Task list for the single review round feature"
---

# Tasks: Single Review Round

**Input**: Design documents from `/specs/013-single-review-round/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, and `quickstart.md`

**Organization**: Tasks are grouped by user story. Both stories are P1; User Story 1 is the MVP approval path and User Story 2 adds the rejected-draft revision path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the focused validation baseline before changing the workflow.

- [X] T001 Run the focused baseline command from `specs/013-single-review-round/quickstart.md` against `BlogWriter.Tests/BlogWriter.Tests.csproj` and record the current approval-path result before implementation.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Preserve the shared state and output contracts used by both user stories.

- [X] T002 [P] Verify and, only if wording is misleading, update the `MaxRevisions` and `RevisionNumber` documentation in `ResearchState.cs` so the cap explicitly represents author passes 1 and 2 without changing the persisted state shape.
- [X] T003 [P] Add reusable author/reviewer call-count and output-capture test support in `BlogWriter.Tests/BlogWorkflowTests.cs` without changing production interfaces.

**Checkpoint**: Shared state semantics and test instrumentation are ready; user story work can proceed in priority order.

---

## Phase 3: User Story 1 - Display an accepted initial draft (Priority: P1) 🎯 MVP

**Goal**: An initially approved draft is emitted as the final result with no revision.

**Independent Test**: Configure the test reviewer to return `APPROVED`; verify the initial draft is emitted, the author runs once, the reviewer runs once, and the workflow publishes completion.

### Tests for User Story 1

- [X] T004 [US1] Add an approval-path regression test in `BlogWriter.Tests/BlogWorkflowTests.cs` that asserts the initial draft remains final, the author and reviewer call counts are both one, and the final output event is emitted.

### Implementation for User Story 1

- [X] T005 [US1] Update `BlogWorkflow.cs` to register `ReviewerExecutor` as the approval terminal output and preserve the existing reviewer-to-author conditional edge for rejected drafts.
- [X] T006 [US1] Preserve the approval behavior in `Workflows/BlogExecutors.cs` so `ReviewerExecutor` yields an approved initial state and publishes reviewer feedback exactly once.

**Checkpoint**: User Story 1 is independently testable as the MVP approval flow.

---

## Phase 4: User Story 2 - Display one revision after rejection (Priority: P1)

**Goal**: A rejected initial draft receives exactly one author revision, which is displayed immediately without a second review.

**Independent Test**: Configure the reviewer to reject the initial draft with feedback; verify the author runs twice, the reviewer runs once, the revised/latest draft is emitted, and no second review occurs.

### Tests for User Story 2

- [X] T007 [US2] Add rejection-and-revision regression tests in `BlogWriter.Tests/BlogWorkflowTests.cs` covering reviewer feedback delivery, exactly one author revision, exactly one reviewer call, final revised draft output, and the no-replacement-content fallback.

### Implementation for User Story 2

- [X] T008 [US2] Make the author-to-reviewer edge conditional on `RevisionNumber < ResearchState.MaxRevisions` and register `AuthorExecutor` as the second terminal output source in `BlogWorkflow.cs`.
- [X] T009 [US2] Update `AuthorExecutor` in `Workflows/BlogExecutors.cs` to yield the state when the single revision reaches `ResearchState.MaxRevisions`, while retaining the existing author node and fallback-draft behavior.
- [X] T010 [US2] Update `BlogWriter.Tests/ResearchStateTests.cs` only if the implementation changes a revision-boundary invariant, asserting that the capped revised state cannot request another review or revision. No state invariant changed, so existing tests remain sufficient.

**Checkpoint**: User Stories 1 and 2 both terminate with a displayed final draft and no workflow review loop beyond the requested single revision.

---

## Phase 5: Polish & Cross-Cutting Validation

**Purpose**: Validate the complete feature and preserve documented operational behavior.

- [X] T011 Run the full `BlogWriter.Tests/BlogWriter.Tests.csproj` suite using the isolated output command in `specs/013-single-review-round/quickstart.md` and resolve only regressions caused by this feature.
- [X] T012 Run the MAF workflow topology simulation and affected-project build for `BlogWorkflow.cs` and `Workflows/BlogExecutors.cs`; confirm both terminal output paths complete without silent starvation and nullable/compiler diagnostics remain clean.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: T001 has no dependencies and establishes the baseline.
- **Phase 2 (Foundational)**: T002 and T003 depend on T001 and can run in parallel because they touch different files.
- **Phase 3 (US1 MVP)**: T004 depends on T003; T005 and T006 depend on T004 and can be completed together as the approval-path implementation.
- **Phase 4 (US2)**: T007 depends on the shared test support and the US1 baseline; T008 and T009 depend on T007; T010 is conditional on whether the state invariant changes.
- **Phase 5 (Polish)**: T011 and T012 depend on the completed desired user stories.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational tasks; independently validates the accepted initial draft and is the MVP.
- **User Story 2 (P1)**: Extends the same workflow topology after User Story 1; it depends on the approval output path but remains independently testable with rejection-focused doubles.

### Parallel Opportunities

- T002 and T003 can run in parallel after T001.
- Within User Story 1, T005 and T006 can be implemented in parallel after T004 when the shared topology contract is agreed.
- T011 and T012 can run in parallel after both story checkpoints.

## Parallel Example: User Story 1

```text
Task: T005 Update BlogWorkflow.cs for the reviewer terminal output path
Task: T006 Preserve ReviewerExecutor approval output in Workflows/BlogExecutors.cs
```

## Parallel Example: User Story 2

```text
Task: T008 Update conditional edges and output sources in BlogWorkflow.cs
Task: T009 Add terminal output after the capped author revision in Workflows/BlogExecutors.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 through T003.
2. Complete T004 through T006.
3. Run the User Story 1 focused test and stop for validation if only approval behavior is required.

### Incremental Delivery

1. Establish the baseline and shared test/state contracts.
2. Deliver User Story 1 as the approval-path MVP.
3. Add User Story 2's conditional routing and terminal revised-author output.
4. Run full tests and MAF topology/build validation.

### Notes

- Every task is independently actionable, uses the required checkbox/ID format, and names the file or validation artifact it affects.
- No new project, storage migration, external contract, credential, or prompt change is required.

## Phase 6: Convergence

- [X] T013 Update the stale final-output comment in `BlogWorkflow.cs` to document that `WorkflowOutputEvent` may be emitted by either `ReviewerExecutor` after approval or `AuthorExecutor` after the single revision per plan: Implementation Design 1-2 (partial)
