# Tasks: Workflow Log and Reviewer Notes

**Input**: Design documents from `specs/005-workflow-log-reviewer-notes/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workflow-output.md`, and `quickstart.md`

**Tests**: Included because the feature specification requires focused workflow, service, component, stale-update, and accessibility tests.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested independently after the shared output foundation is complete.

## Phase 1: Setup

**Purpose**: Establish the feature test seams and preserve the existing project structure.

- [X] T001 [P] Add feature-specific test doubles for typed workflow output observers in `BlogWriter.Tests/TestChatClients.cs` or a new `BlogWriter.Tests/WorkflowOutputTestDoubles.cs`, preserving existing test-client conventions.
- [X] T002 [P] Add web workspace output test helpers for ordered log entries, reviewer updates, and operation-version control in `BlogWriter.Web.Tests/BlogWorkspaceOutputTestHelpers.cs`.
- [X] T003 [P] Record the feature validation commands and expected output surfaces in `specs/005-workflow-log-reviewer-notes/quickstart.md` before implementation tests are added.

---

## Phase 2: Foundational Output Contract

**Purpose**: Create the shared typed observer boundary required by all user stories without changing workflow topology, persistence, authentication, or token-budget behavior.

- [X] T004 [P] Define `WorkflowOutputKind`, `WorkflowOutputOutcome`, and immutable `WorkflowOutputUpdate` in `WorkflowOutputUpdate.cs`, requiring a non-empty text message, operation identity, sequence, update key, and optional revision number as specified in `specs/005-workflow-log-reviewer-notes/data-model.md`.
- [X] T005 [P] Extend the workflow contract in `IBlogWorkflow.cs` with an optional typed output observer that does not control routing, cancellation, persistence, or retries.
- [X] T006 [P] Extend the session boundary in `IBlogWriterSessionService.cs` so initial and revision operations can forward an optional output observer while preserving existing cancellation and word-count parameters.
- [X] T007 Add shared contract tests in `BlogWriter.Tests/WorkflowOutputUpdateTests.cs` proving lifecycle/reviewer kinds, outcome values, required message validation, stable update keys, and sequence metadata follow `specs/005-workflow-log-reviewer-notes/contracts/workflow-output.md`.
- [X] T008 Add observer-forwarding tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` proving StartAsync and ReviseAsync pass the same observer to the workflow, do not persist transient output, and preserve existing stable-session failure behavior.
- [X] T009 Update `BlogWorkflow.cs` to publish operation-start, executor-invoked, executor-completed, final-success, cancellation, and failure updates from the existing streaming event loop without adding workflow edges, model calls, retries, or unbounded observer work.
- [X] T010 Update `Workflows/BlogExecutors.cs` to publish reviewer feedback at each reviewer boundary with operation/revision identity and sequence metadata while preserving the existing bounded reviewer-to-author loop and final `ResearchState` output.
- [X] T011 Update `BlogWriterSessionService.cs` to forward the optional observer through StartAsync and ReviseAsync and preserve Cosmos save/ETag/owner behavior unchanged.
- [X] T012 Run the foundational core tests in `BlogWriter.Tests/WorkflowOutputUpdateTests.cs`, `BlogWriter.Tests/BlogWorkflowTests.cs`, and `BlogWriter.Tests/BlogWriterSessionServiceTests.cs`; verify observer failures do not alter workflow success/failure semantics.

**Checkpoint**: Typed output updates are available to the web workspace, and the existing workflow/session behavior remains compatible.

---

## Phase 3: User Story 1 - See Workflow Log Output (Priority: P1) MVP

**Goal**: Replace the separate status/validation stack below the command bar with an ordered workflow log that shows lifecycle and outcome messages.

**Independent Test**: Start, complete, cancel, validate, conflict, and fail operations; verify all messages appear chronologically beneath the command bar, earlier entries remain visible, and no separate status stack is rendered.

### Tests for User Story 1

- [X] T013 [P] [US1] Add `BlogWorkspaceService` tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for start/progress/success/cancellation/validation/conflict/failure log entries, chronological order, empty state, and removal of the old separate status/validation representation.
- [X] T014 [P] [US1] Add component contract tests in `BlogWriter.Web.Tests/WorkflowLogTests.cs` for the labeled log region, outcome semantics, ordered text rendering, empty state, and safe rendering of markup-like messages.
- [X] T015 [P] [US1] Add page integration assertions in `BlogWriter.Web.Tests/HomePageTests.cs` proving the workflow log is directly beneath the command bar and before unrelated workspace output, with no duplicate status stack.

### Implementation for User Story 1

- [X] T016 [US1] Add ordered workflow-log state and append/reset methods to `BlogWriter.Web/Services/BlogWorkspaceState.cs`, retaining operation-version guards and rendering messages as plain text.
- [X] T017 [US1] Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` to subscribe each submission to the typed observer, route lifecycle/validation/failure/conflict/cancellation updates to the log, ignore stale updates, and notify the circuit after each accepted update.
- [X] T018 [US1] Create `BlogWriter.Web/Components/WorkflowLog.razor` with an accessible label, chronological entries, outcome semantics, empty state, polite live-update behavior, and text-safe rendering.
- [X] T019 [US1] Update `BlogWriter.Web/Components/Pages/Home.razor` to render `WorkflowLog` directly under `CommandBar` and remove the separate `status-stack` rendering while preserving New/List/Revise/Quit callbacks.
- [X] T020 [US1] Update `BlogWriter.Web/wwwroot/app.css` to size and scroll the workflow log beneath the command buttons without overlap or horizontal overflow at 390 × 844 and 1440 × 900.
- [X] T021 [US1] Run the US1 tests from `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`, `BlogWriter.Web.Tests/WorkflowLogTests.cs`, and `BlogWriter.Web.Tests/HomePageTests.cs`; verify the MVP independently before starting US2.

**Checkpoint**: User Story 1 is independently demonstrable with lifecycle output below the buttons and no separate status stack.

---

## Phase 4: User Story 2 - Follow Reviewer Feedback (Priority: P1)

**Goal**: Route each reviewer feedback update into Reviewer notes immediately, preserve feedback across revisions, and clear it only for New or a different loaded session.

**Independent Test**: Deliver multiple reviewer updates during an initial operation and a revision; verify all updates appear in Reviewer notes in arrival order, duplicate/stale updates are ignored, and reset/load behavior does not leak prior-session feedback.

### Tests for User Story 2

- [X] T022 [P] [US2] Add workflow/session observer tests in `BlogWriter.Tests/BlogWorkflowTests.cs` and `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` proving each reviewer boundary emits a reviewer update without changing final state or workflow termination.
- [X] T023 [P] [US2] Add workspace accumulation tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for initial feedback, multiple updates, feedback across revisions, duplicate update keys, failure/cancellation retention, New reset, and loaded-session seeding.
- [X] T024 [P] [US2] Add Reviewer notes component tests in `BlogWriter.Web.Tests/ReviewPaneTests.cs` for empty state, ordered accumulated content, live-update semantics, and literal rendering of markup-like reviewer text.

### Implementation for User Story 2

- [X] T025 [US2] Add ordered reviewer-feedback state and deduplication by `UpdateKey` to `BlogWriter.Web/Services/BlogWorkspaceState.cs`, preserving active-session history across revisions.
- [X] T026 [US2] Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` to route reviewer updates separately from lifecycle logs, append across revisions, retain received feedback on failure/cancellation, clear on New, and seed from persisted `ReviewNotes` when loading another session.
- [X] T027 [US2] Update `BlogWriter.Web/Components/ReviewPane.razor` to render accumulated reviewer feedback in arrival order, retain its existing heading and empty state, and expose polite accessible updates without moving focus.
- [X] T028 [US2] Update `BlogWriter.Web/Components/Pages/Home.razor` to bind Reviewer notes to accumulated workspace feedback while keeping workflow events exclusively in `WorkflowLog`.
- [X] T029 [US2] Run the US2 workflow, session, workspace, and Reviewer notes tests; verify feedback routing and cross-revision accumulation independently of browser layout tests.

**Checkpoint**: User Stories 1 and 2 work together without mixing lifecycle log output and reviewer feedback.

---

## Phase 5: User Story 3 - Read the Output Accessibly (Priority: P2)

**Goal**: Make both live output surfaces readable, keyboard-safe, responsive, and perceivable without focus theft.

**Independent Test**: Inspect keyboard order, live announcements, long text, markup-like content, and layout at both required viewports while updates are appended.

### Tests for User Story 3

- [X] T030 [P] [US3] Extend `BlogWriter.Web.Tests/WorkflowLogTests.cs` and `BlogWriter.Web.Tests/ReviewPaneTests.cs` with accessible labels, polite live-region semantics, focus preservation, ordered content, and text-safety assertions.
- [X] T031 [P] [US3] Extend `BlogWriter.Web.Tests/HomePageTests.cs` with DOM order assertions for command bar, workflow log, Draft, and Reviewer notes plus list/ended-mode output behavior.
- [X] T032 [P] [US3] Extend `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` for 390 × 844 and 1440 × 900 output visibility, non-overlap, scrollability, no horizontal overflow, and zero new WCAG 2.2 Level A/AA violations.

### Implementation for User Story 3

- [X] T033 [US3] Refine `BlogWriter.Web/Components/WorkflowLog.razor` and `BlogWriter.Web/Components/ReviewPane.razor` live-region labels, focus behavior, and long-content structure to satisfy the accessibility contract without duplicating announcements.
- [X] T034 [US3] Refine `BlogWriter.Web/wwwroot/app.css` for bounded output heights, readable wrapping, visible focus, mobile/desktop spacing, and non-overlapping command/content surfaces.
- [X] T035 [US3] Update `BlogWriter.Web/Components/Pages/Home.razor` for final reading order, list/ended-mode handling, and live output semantics while retaining existing command and word-count controls.
- [X] T036 [US3] Run the US3 component, page, browser, and accessibility tests and record the required viewport outcomes in `specs/005-workflow-log-reviewer-notes/quickstart.md`.

**Checkpoint**: All three user stories satisfy the requested behavior and accessibility contract.

---

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Confirm compatibility, documentation, regression safety, and unchanged MAF health.

- [X] T037 [P] Update `README.md` and `docs/configuration.md` with the workflow log location, lifecycle outcome behavior, Reviewer notes accumulation/reset rules, and the fact that transient output is not persisted.
- [X] T038 [P] Add or update focused test documentation in `specs/005-workflow-log-reviewer-notes/quickstart.md` for duplicate, stale, cancellation, and loaded-session scenarios.
- [X] T039 Run the full core and web suites from `specs/005-workflow-log-reviewer-notes/quickstart.md` and build `BlogWriter.csproj` plus `BlogWriter.Web/BlogWriter.Web.csproj`.
- [X] T040 Run `git diff --check` and inspect changed files for accidental status-stack remnants, unsafe markup rendering, public contract incompatibilities, or unrelated changes.
- [X] T041 Run MAF Doctor against the repository and compare with the pre-feature baseline: F, 4 errors, 3 warnings, 0 silent-starvation risks, and 6 heuristic uncapped-call matches; investigate any new finding before completion.
- [X] T042 Run every manual scenario in `specs/005-workflow-log-reviewer-notes/quickstart.md`, including initial/revision accumulation, reset/load, stale updates, cancellation/failure, responsive layout, and accessibility checks.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001-T003 can start immediately and may run in parallel.
- **Foundational (Phase 2)**: T004-T012 depends on Setup and blocks all user stories; T004-T006 and T007 can proceed in parallel, then T008-T012 follow the shared contract.
- **User Story 1 (Phase 3)**: Depends on the foundational output observer; it is the MVP and should be completed before broad integration.
- **User Story 2 (Phase 4)**: Depends on the foundational observer and integrates with the US1 log routing, but its workflow/session tests can begin once Phase 2 is complete.
- **User Story 3 (Phase 5)**: Depends on the US1 and US2 surfaces being present; accessibility refinements and browser checks follow both output paths.
- **Polish (Phase 6)**: Depends on all selected stories and their focused tests.

### User Story Dependencies

- **User Story 1 (P1)**: Requires Phase 2; no dependency on US2 or US3. MVP candidate.
- **User Story 2 (P1)**: Requires Phase 2; integrates with the output observer and existing Reviewer notes surface, but is independently testable with service/component tests.
- **User Story 3 (P2)**: Requires US1 and US2 rendered surfaces to validate combined reading order and responsive behavior.

### Parallel Opportunities

- Phase 1: T001, T002, and T003 can run in parallel.
- Phase 2: T004, T005, T006, and T007 can run in parallel when touching separate files; T008-T012 follow the shared model.
- US1: T013-T015 can run in parallel; T018 and T020 can proceed in parallel after state/service contracts are established.
- US2: T022-T024 can run in parallel; T025 and T027 can proceed in parallel after the shared update model exists.
- US3: T030-T032 can run in parallel; T033-T035 can be split by component/style/page after the tests define expected semantics.
- Phase 6: T037, T038, and T040 can run in parallel; T039, T041, and T042 are final validation tasks.

## Parallel Execution Examples

### User Story 1

```text
Task T013: Add workspace lifecycle/outcome tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T014: Add WorkflowLog component tests in BlogWriter.Web.Tests/WorkflowLogTests.cs
Task T015: Add Home page placement tests in BlogWriter.Web.Tests/HomePageTests.cs
```

### User Story 2

```text
Task T022: Add workflow/session reviewer observer tests in BlogWriter.Tests/
Task T023: Add workspace accumulation/reset tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T024: Add ReviewPane rendering tests in BlogWriter.Web.Tests/ReviewPaneTests.cs
```

### User Story 3

```text
Task T030: Add component accessibility tests in BlogWriter.Web.Tests/WorkflowLogTests.cs and ReviewPaneTests.cs
Task T031: Add page-order and mode tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T032: Add viewport/accessibility browser tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup and Phase 2 foundational observer contract.
2. Complete Phase 3 User Story 1.
3. Run the US1 focused tests and verify the workflow log beneath the command bar.
4. Stop for an MVP demonstration before adding reviewer-history accumulation.

### Incremental Delivery

1. Add the typed observer foundation and preserve existing workflow/session behavior.
2. Deliver US1 as the visible workflow-log MVP.
3. Deliver US2 to route and accumulate reviewer feedback across revisions.
4. Deliver US3 for final accessibility and responsive behavior.
5. Complete Phase 6 regression, browser validation, and MAF baseline comparison.

## Notes

- Every implementation task names an exact file path and follows `- [ ] T### [P?] [US#?] description` format.
- Tests are written before implementation within each story and must fail for the new behavior before the corresponding implementation tasks are completed.
- No task changes hosted-agent deployment, credentials, token budgets, Cosmos schema, session ownership, or workflow edges.
