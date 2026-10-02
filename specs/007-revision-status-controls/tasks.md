# Tasks: Revision Status Controls

**Input**: Design documents from `specs/007-revision-status-controls/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/revision-status-ui.md`, and `quickstart.md`

**Tests**: Included because the specification requires focused state, latest-status, accessibility, and responsive tests.

**Organization**: Tasks are grouped by user story and executed tests-first within each story.

## Phase 1: Setup

**Purpose**: Establish focused test helpers and update the feature validation guide.

- [X] T001 [P] Add revision-state test helpers for New, empty, draft, session, and processing workspace states in `BlogWriter.Web.Tests/RevisionStatusTestHelpers.cs`.
- [X] T002 [P] Add workflow-output test helpers for sequential lifecycle updates and Reviewer feedback in `BlogWriter.Web.Tests/RevisionStatusTestHelpers.cs`.
- [X] T003 [P] Update `specs/007-revision-status-controls/quickstart.md` with focused commands and expected latest-status/revision-control outcomes.

---

## Phase 2: Foundational Workspace Projection

**Purpose**: Establish transient state projections without changing persistence or workflow topology.

- [X] T004 [P] Add state tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for `RevisionInputEnabled`, `ReviseActionEnabled`, `CurrentStatus`, empty status, and Reviewer notes separation.
- [X] T005 [P] Add `RevisionInputEnabled`, `CurrentStatus`, and `CurrentStatusOutcome` projections to `BlogWriter.Web/Services/BlogWorkspaceState.cs`, preserving existing operation, word-range, selection, and Reviewer notes state.
- [X] T006 Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` so accepted lifecycle/status updates replace the current status projection while Reviewer feedback continues updating Reviewer notes independently.
- [X] T007 Add shared status/availability assertions to `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving processing and cancellation protections remain unchanged.

**Checkpoint**: Workspace state exposes independent revision availability and one latest workflow status.

---

## Phase 3: User Story 1 - Enable Revision for Drafts (Priority: P1) MVP

**Goal**: Enable the Revision request field after New, enable both Revision request and Revise when a draft/session exists, and preserve processing protections.

**Independent Test**: Render empty, New, draft, session, and processing states and verify the input/action state matrix.

### Tests for User Story 1

- [X] T008 [P] [US1] Extend `BlogWriter.Web.Tests/HomePageTests.cs` to assert Revision request disabled with no draft/session, enabled after New, enabled with a displayed draft/session, and disabled during processing.
- [X] T009 [P] [US1] Add service-level tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for New reset availability, draft publication availability, and duplicate revision protection.

### Implementation for User Story 1

- [X] T010 [US1] Update `BlogWriter.Web/Services/BlogWorkspaceState.cs` to derive `RevisionInputEnabled` from New-state or draft/session context and `ReviseActionEnabled` from draft/session plus non-processing state.
- [X] T011 [US1] Update `BlogWriter.Web/Components/Pages/Home.razor` to bind PromptInput disabled state to the separate revision-input projection and bind CommandBar Revise state to the action projection.
- [X] T012 [US1] Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` reset/publication/processing transitions so New enables only Revision request, draft publication enables both, and processing disables submissions.
- [X] T013 [US1] Run US1 tests in `BlogWriter.Web.Tests/HomePageTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: Revision controls follow the clarified availability matrix independently of List mode.

---

## Phase 4: User Story 2 - See the Latest Status (Priority: P1)

**Goal**: Replace the rendered scrolling workflow-log list with one accessible latest-status line while retaining separate Reviewer notes.

**Independent Test**: Publish sequential lifecycle updates and Reviewer feedback; verify only the newest lifecycle message is visible and Reviewer notes remain independent.

### Tests for User Story 2

- [X] T014 [P] [US2] Update `BlogWriter.Web.Tests/WorkflowLogTests.cs` to assert empty status, newest-message replacement, safe markup-like text rendering, polite live semantics, and absence of an ordered scrolling list.
- [X] T015 [P] [US2] Extend `BlogWriter.Web.Tests/HomePageTests.cs` to assert the status line appears in the workspace and Reviewer notes remains a separate region.
- [X] T016 [P] [US2] Add workspace update tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving progress, validation, success, cancellation, and failure replace `CurrentStatus` while Reviewer feedback remains in `Review`.

### Implementation for User Story 2

- [X] T017 [US2] Change `BlogWriter.Web/Components/WorkflowLog.razor` to render one current status message with a compact empty state, safe text rendering, and polite live-region semantics instead of an ordered list.
- [X] T018 [US2] Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` and `BlogWriter.Web/Services/BlogWorkspaceState.cs` to project every accepted lifecycle update to the newest status without changing Reviewer feedback routing.
- [X] T019 [US2] Update `BlogWriter.Web/wwwroot/app.css` to remove scrolling-log presentation and style a single compact status line that wraps safely without horizontal overflow.
- [X] T020 [US2] Run US2 tests in `BlogWriter.Web.Tests/WorkflowLogTests.cs`, `BlogWriter.Web.Tests/HomePageTests.cs`, and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: The workspace shows only the latest workflow status while Reviewer notes remain independently visible.

---

## Phase 5: User Story 3 - Keep Commands Compact (Priority: P2)

**Goal**: Keep commands grouped normally, reserve space for the List selector when visible, and reduce button dimensions without losing operability.

**Independent Test**: Render command bar with and without List selector at both required viewports and verify movement, sizing, focus, and no overlap.

### Tests for User Story 3

- [X] T021 [P] [US3] Extend `BlogWriter.Web.Tests/CommandBarTests.cs` with grouped/default and selector-visible layout assertions, Help/Revise/Quit ordering, smaller button dimensions, and accessible focus targets.
- [X] T022 [P] [US3] Replace placeholder assertions in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` with executable CSS/DOM contract checks for 390 × 844 and 1440 × 900, including no horizontal overflow and conditional selector movement.

### Implementation for User Story 3

- [X] T023 [US3] Update `BlogWriter.Web/Components/CommandBar.razor` to expose conditional layout state/classes while preserving existing command callbacks and accessible names.
- [X] T024 [US3] Update `BlogWriter.Web/wwwroot/app.css` to reduce shared button dimensions, keep default commands grouped, and move Revise/Quit/Help into the right-side group when the List selector is visible.
- [X] T025 [US3] Update `BlogWriter.Web/Components/Pages/Home.razor` only as needed to pass selector visibility and preserve List/Help behavior during layout changes.
- [X] T026 [US3] Run US3 component and responsive contract tests in `BlogWriter.Web.Tests/CommandBarTests.cs`, `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs`, and `BlogWriter.Web.Tests/HomePageTests.cs`.

**Checkpoint**: Compact command controls remain usable and responsive in both selector states.

---

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Document, regress, and validate the complete feature.

- [X] T027 [P] Update `README.md` and `docs/configuration.md` with revision availability, latest-status behavior, separate Reviewer notes, and compact command layout.
- [X] T028 [P] Update `specs/007-revision-status-controls/quickstart.md` with focused test results and responsive/accessibility outcomes.
- [X] T029 Run full core/web tests and build `BlogWriter.csproj` and `BlogWriter.Web/BlogWriter.Web.csproj`.
- [X] T030 Run `git diff --check` and inspect for retained scrolling-log markup, broken Reviewer notes separation, invalid Revise enablement, and layout regressions.
- [X] T031 Run responsive/accessibility browser validation at 390 × 844 and 1440 × 900.
- [X] T032 Confirm existing List selection, Help copy, word-count, cancellation, session ownership, and workflow termination behavior remains green.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001-T003 can run in parallel immediately.
- **Foundational (Phase 2)**: T004-T007 depends on Setup and blocks user stories.
- **User Story 1 (Phase 3)**: Depends on foundational projections and is the MVP.
- **User Story 2 (Phase 4)**: Depends on the foundational status projection; independent of command layout.
- **User Story 3 (Phase 5)**: Depends on the existing command bar and List selector; independent of status projection implementation.
- **Polish (Phase 6)**: Depends on all stories and focused tests.

### User Story Dependencies

- **US1 (P1)**: Requires Phase 2; no dependency on US2 or US3.
- **US2 (P1)**: Requires Phase 2; preserves Reviewer notes behavior from existing implementation.
- **US3 (P2)**: Requires existing List selector command surface; can proceed alongside US2 after Phase 2.

### Parallel Opportunities

- Phase 1: T001-T003 can run in parallel.
- Phase 2: T004-T006 can run in parallel; T007 follows the state projection.
- US1: T008-T009 can run in parallel; T010-T012 follow test expectations.
- US2: T014-T016 can run in parallel; T017-T019 follow the component/state contract.
- US3: T021-T022 can run in parallel; T023-T025 follow layout assertions.
- Phase 6: T027-T028 and T030 can run in parallel; T029-T032 are final validation tasks.

## Parallel Execution Examples

### User Story 1

```text
Task T008: Add HomePage revision-state tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T009: Add workspace availability tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
```

### User Story 2

```text
Task T014: Add latest-status component tests in BlogWriter.Web.Tests/WorkflowLogTests.cs
Task T015: Add Home page status/Reviewer notes tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T016: Add workspace update projection tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
```

### User Story 3

```text
Task T021: Add CommandBar layout tests in BlogWriter.Web.Tests/CommandBarTests.cs
Task T022: Add responsive contract tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Complete US1 revision availability behavior.
3. Run US1 focused tests and verify New/draft/processing states.
4. Stop for MVP validation before replacing status presentation.

### Incremental Delivery

1. Establish independent revision availability and latest-status state projections.
2. Deliver US1 revision controls.
3. Deliver US2 latest-status presentation.
4. Deliver US3 compact command layout.
5. Complete documentation, full regression, browser, and accessibility validation.

## Notes

- Every task uses `- [ ] T### [P?] [US#?]` and names exact file paths.
- Tests are written before implementation tasks within each story.
- No task changes hosted-agent deployment, credentials, token budgets, session schemas, Reviewer notes persistence, or workflow topology.
