# Tasks: Word Count Controls

**Input**: Design documents from `/specs/004-word-count-controls/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [word-range-interactions.md](contracts/word-range-interactions.md), and [quickstart.md](quickstart.md)

**Tests**: Focused xUnit, bUnit, and browser tests are required by the project constitution for shared validation, state changes, persistence propagation, responsive behavior, and accessibility.

**Organization**: Tasks are grouped by user story after a shared range-validation and session-service foundation. Each story has an independent test and may be validated separately.

## Phase 1: Setup

**Purpose**: Establish focused test files without adding runtime dependencies or projects.

- [X] T001 [P] Create the shared range test file in `BlogWriter.Tests/WordRangeTests.cs` using existing xUnit conventions.
- [X] T002 [P] Create the component test file in `BlogWriter.Web.Tests/WordRangeInputTests.cs` using existing bUnit conventions.

---

## Phase 2: Foundational Range and Service Contracts

**Purpose**: Define one validation model and application-service contract used by all three stories.

**Critical**: Complete this phase before adding controls or workspace transitions.

### Foundation Tests

- [X] T003 Add parsing tests in `BlogWriter.Tests/WordRangeTests.cs` for the exact rules: "Min and Max must each parse as a positive whole number," "Max must be greater than or equal to Min," surrounding whitespace is accepted, equal values are valid, and integer overflow is rejected.
- [X] T004 [P] Add application-service tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` proving initial creation and revision persist the exact Min and Max parameters while invalid ranges fail before `IBlogWorkflow.RunAsync`.

### Foundation Implementation

- [X] T005 Implement the immutable shared parser/validator in `./WordRange.cs` with defaults from `ResearchState.DefaultMinWords` and `ResearchState.DefaultMaxWords` and field-specific error results.
- [X] T006 Extend `./IBlogWriterSessionService.cs` so revision accepts an explicit validated Min and Max range while preserving cancellation-token support.
- [X] T007 Update `./BlogWriterSessionService.cs` to validate initial and revision ranges, apply revision values to the copied `ResearchState` before `StartFollowUp`, and preserve the stable session on validation/workflow/save failure.
- [X] T008 Run the focused foundation tests in `BlogWriter.Tests/WordRangeTests.cs` and `BlogWriter.Tests/BlogWriterSessionServiceTests.cs`.

**Checkpoint**: Both initial and revision application operations accept one validated immutable range and persist it through existing session state without workflow or schema changes.

---

## Phase 3: User Story 1 - Set the Draft Word Range (Priority: P1) MVP

**Goal**: Show compact Min and Max controls in the required location, default them to 1000/2000, reset them with New, and apply them to initial submissions.

**Independent Test**: Open a new workspace, verify placement/defaults, enter a valid range, submit a prompt, and verify the created session contains the exact range; choose New and verify defaults return.

### Tests for User Story 1

- [X] T009 [P] [US1] Add component contract tests in `BlogWriter.Web.Tests/WordRangeInputTests.cs` for labels exactly `Min` and `Max`, numeric input hints, compact grouping, bound values, and accessible description/error associations.
- [X] T010 [US1] Add page-order and default tests in `BlogWriter.Web.Tests/HomePageTests.cs` proving the range row follows both prompt inputs, precedes Draft/Reviewer, and displays 1000/2000.
- [X] T011 [P] [US1] Add workspace tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving new-state defaults, valid initial range propagation, submitted-range snapshotting, accepted-baseline update, and New reset to 1000/2000.

### Implementation for User Story 1

- [X] T012 [P] [US1] Implement the labeled compact range component in `BlogWriter.Web/Components/WordRangeInput.razor` with Min/Max values, numeric keyboard hints, and field-level error hooks.
- [X] T013 [US1] Add editable Min/Max strings, accepted-range values, and default initialization to `BlogWriter.Web/Services/BlogWorkspaceState.cs` using the constraints from `data-model.md`.
- [X] T014 [US1] Add range validation and immutable initial-submission snapshots to `BlogWriter.Web/Services/BlogWorkspaceService.cs`, passing parsed values to `IBlogWriterSessionService.StartAsync` before any workflow call.
- [X] T015 [US1] Insert `BlogWriter.Web/Components/WordRangeInput.razor` after the prompt strip and before the work grid in `BlogWriter.Web/Components/Pages/Home.razor`.
- [X] T016 [US1] Add stable compact range-row sizing and spacing to `BlogWriter.Web/wwwroot/app.css` without changing existing command-button dimensions.
- [X] T017 [US1] Run the US1 tests in `BlogWriter.Web.Tests/WordRangeInputTests.cs`, `BlogWriter.Web.Tests/HomePageTests.cs`, and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: A new prompt uses the displayed valid range, and New restores 1000/2000.

---

## Phase 4: User Story 2 - Retain the Range While Revising (Priority: P1)

**Goal**: Populate stored ranges when sessions load and apply a changed visible range to the next revision without losing edits made during processing.

**Independent Test**: Load a session with stored targets, change the range, submit a revision, verify persistence, then edit the controls during processing and verify the submitted snapshot is saved while later edits remain visible and unsaved.

### Tests for User Story 2

- [X] T018 [P] [US2] Add workspace tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for loading stored ranges, defaulting unavailable targets, applying changed values to revisions, and updating the accepted baseline after success.
- [X] T019 [US2] Add processing-edit tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving an in-flight operation retains its submitted snapshot and later visible Min/Max edits remain unsaved after success, failure, or cancellation.
- [X] T020 [P] [US2] Add session-service revision tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` proving the candidate copy receives Min/Max before follow-up execution and the original stable session remains unchanged on failure.

### Implementation for User Story 2

- [X] T021 [US2] Update saved-session publication in `BlogWriter.Web/Services/BlogWorkspaceService.cs` to populate editable and accepted ranges from `ResearchState`, using 1000/2000 for unavailable or invalid legacy targets.
- [X] T022 [US2] Implement revision-range snapshotting and post-completion baseline synchronization in `BlogWriter.Web/Services/BlogWorkspaceService.cs`, preserving visible edits made after submission.
- [X] T023 [US2] Run the US2 tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: Loaded and revised sessions display and persist the correct range while concurrent UI edits remain pending for the next operation.

---

## Phase 5: User Story 3 - Correct Invalid and Unsaved Ranges (Priority: P2)

**Goal**: Block invalid submissions with accessible feedback and include changed range values in the existing discard-confirmation workflow.

**Independent Test**: Exercise every invalid-input class, correction, unsaved-range confirmation, decline/accept behavior, keyboard order, accessibility associations, and both required viewport sizes without starting a workflow for invalid data.

### Tests for User Story 3

- [X] T024 [P] [US3] Add workspace validation tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for empty, text, decimal, zero, negative, overflow, and Max-below-Min values, proving zero initial/revision workflow calls and field-specific messages.
- [X] T025 [US3] Add correction and equal-range tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving obsolete errors clear and Min equal to Max is accepted.
- [X] T026 [P] [US3] Add unsaved-range tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving changed or invalid visible values trigger New/List/Quit confirmation, decline preserves all state, unchanged values do not prompt, and success updates the baseline.
- [X] T027 [P] [US3] Add bUnit accessibility and placement tests in `BlogWriter.Web.Tests/WordRangeInputTests.cs` and `BlogWriter.Web.Tests/HomePageTests.cs` for programmatic labels, `aria-invalid`, error associations, live feedback, and keyboard order after prompts and before content panes.
- [X] T028 [P] [US3] Extend viewport contract tests in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` for visible, non-overlapping Min/Max controls with no horizontal overflow at 390 × 844 and 1440 × 900.

### Implementation for User Story 3

- [X] T029 [US3] Implement live field/range validation and obsolete-error clearing in `BlogWriter.Web/Services/BlogWorkspaceService.cs` while retaining the exact invalid strings in workspace state.
- [X] T030 [US3] Extend unsaved-state calculation and New/List/Quit discard behavior in `BlogWriter.Web/Services/BlogWorkspaceState.cs` and `BlogWriter.Web/Services/BlogWorkspaceService.cs` to compare visible values with the accepted range.
- [X] T031 [US3] Render field-specific validation semantics and messages in `BlogWriter.Web/Components/WordRangeInput.razor` and announce summary feedback through `BlogWriter.Web/Components/Pages/Home.razor`.
- [X] T032 [US3] Add mobile/desktop range-row behavior, label protection, input width constraints, visible focus, and invalid styling in `BlogWriter.Web/wwwroot/app.css`.
- [X] T033 [US3] Run the US3 service, component, browser-contract, and accessibility tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`, `BlogWriter.Web.Tests/WordRangeInputTests.cs`, `BlogWriter.Web.Tests/HomePageTests.cs`, and `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs`.

**Checkpoint**: Invalid values never start workflow work, and unsaved range edits participate consistently in confirmation and recovery flows.

---

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Validate compatibility, documentation, responsive behavior, and unchanged MAF boundaries.

- [X] T034 [P] Document Min/Max defaults, validation, revision behavior, loading, and unsaved confirmation in `README.md` and `docs/configuration.md` without introducing new configuration keys.
- [X] T035 Run all core and web tests with `dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj` and `dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj`, then build `BlogWriter.csproj` and `BlogWriter.Web/BlogWriter.Web.csproj`.
- [X] T036 Run every scenario in `specs/004-word-count-controls/quickstart.md`, including invalid ranges, saved-session restore, in-flight edits, confirmation behavior, and both viewport dimensions.
- [X] T037 Run MAF Doctor against the repository and record in `specs/004-word-count-controls/quickstart.md` that the feature introduces no new error, warning, prompt, cost, or topology finding.

---

## Requirement Coverage

| Requirement | Primary tasks |
| --- | --- |
| FR-001 | T009-T010, T012, T015-T016 |
| FR-002 | T010, T015, T027-T028, T032 |
| FR-003 | T011, T013-T014 |
| FR-004 | T011, T013-T014 |
| FR-005 | T003, T005, T024-T025, T029 |
| FR-006 | T003, T005, T024-T025, T029 |
| FR-007 | T004, T024-T025, T029 |
| FR-008 | T009, T024, T027, T029, T031 |
| FR-009 | T004, T007, T011, T014 |
| FR-010 | T018, T021 |
| FR-011 | T004, T006-T007, T018, T020, T022 |
| FR-012 | T011, T019, T022 |
| FR-013 | T019, T021-T022 |
| FR-014 | T010, T016, T028, T032 |
| FR-015 | T009, T027-T028, T031-T032 |
| FR-016 | T026, T030 |
| FR-017 | T026, T030 |
| FR-018 | T018-T019, T022, T026, T030 |

## Success Criteria Coverage

| Success criterion | Primary tasks |
| --- | --- |
| SC-001 | T010-T017 |
| SC-002 | T004, T007, T011, T018-T023 |
| SC-003 | T003-T005, T024-T025, T029, T033 |
| SC-004 | T018, T021, T023 |
| SC-005 | T010, T016, T028, T032-T033, T036 |
| SC-006 | T009, T027-T028, T031-T033, T036 |
| SC-007 | T026, T030, T033 |

## Dependencies and Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 and T002 can start immediately in parallel.
- **Foundation (Phase 2)**: Depends on setup; T003-T004 precede T005-T007, and T008 validates the boundary before story work.
- **User Story 1 (Phase 3)**: Depends on foundation and delivers the MVP.
- **User Story 2 (Phase 4)**: Depends on foundation and the accepted-range state from US1; tests can be drafted after foundation.
- **User Story 3 (Phase 5)**: Depends on foundation and composes validation/confirmation behavior across US1-US2.
- **Polish (Phase 6)**: Depends on all desired stories.

### User Story Dependencies

- **US1 - Set Draft Word Range**: Depends only on foundation; this is the MVP.
- **US2 - Retain Range While Revising**: Depends on foundation and accepted-range fields introduced for US1.
- **US3 - Correct Invalid Ranges**: Depends on foundation; full unsaved-state integration depends on US1-US2 transitions.

## Parallel Opportunities

- T001 and T002 can run in parallel.
- T003 and T004 can run in parallel before shared implementation.
- T009 and T011 can run in parallel; T010 follows T009 because both touch `HomePageTests.cs`.
- T018 and T020 can run in parallel; T019 follows T018 in the shared workspace test file.
- T024, T026, T027, and T028 can be drafted in parallel by file ownership; T025 follows T024.
- T034 can proceed in parallel with final code cleanup after story behavior stabilizes.

## Parallel Example: User Story 1

```text
Task T009: Add WordRangeInput component tests in BlogWriter.Web.Tests/WordRangeInputTests.cs
Task T011: Add workspace default/snapshot tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T012: Implement BlogWriter.Web/Components/WordRangeInput.razor after T009
```

## Parallel Example: User Story 2

```text
Task T018: Add load/revision workspace tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T020: Add revision-copy tests in BlogWriter.Tests/BlogWriterSessionServiceTests.cs
```

## Parallel Example: User Story 3

```text
Task T024: Add invalid-range service tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T027: Add accessibility/placement tests in BlogWriter.Web.Tests/WordRangeInputTests.cs
Task T028: Add viewport contract tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs
```

## Implementation Strategy

### MVP First

1. Complete setup and foundational range/service contracts.
2. Complete US1 through T017.
3. Validate exact labels, placement, defaults, New reset, and initial-session propagation.
4. Demonstrate the MVP before adding loaded-session/revision and unsaved-range behavior.

### Incremental Delivery

1. Shared range validation and service propagation.
2. US1: visible controls and initial submissions.
3. US2: saved-session loading, revisions, and processing-time edits.
4. US3: invalid-input feedback, unsaved confirmation, responsive and accessibility coverage.
5. Full regression and MAF Doctor comparison.
