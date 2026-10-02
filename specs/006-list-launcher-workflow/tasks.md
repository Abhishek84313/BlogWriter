# Tasks: List Launcher Workflow

**Input**: Design documents from `specs/006-list-launcher-workflow/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/list-launcher-ui.md`, and `quickstart.md`

**Tests**: Included because the feature specification explicitly requires focused selection, automatic-processing, dialog, disabled-state, responsive, and regression tests.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently after the shared selection contract is established.

## Phase 1: Setup

**Purpose**: Establish focused test seams and document the feature validation surface.

- [X] T001 [P] Add list-launcher test data helpers for summaries and sessions containing `MainTask` and optional `CurrentSubTask` in `BlogWriter.Web.Tests/ListLauncherTestHelpers.cs`.
- [X] T002 [P] Add clipboard and Help-dialog JavaScript interop test helpers in `BlogWriter.Web.Tests/HelpDialogTestHelpers.cs` using existing bUnit JSInterop conventions.
- [X] T003 [P] Add the feature's focused commands, launch command, and expected manual scenarios to `specs/006-list-launcher-workflow/quickstart.md`.

---

## Phase 2: Foundational Workspace Contract

**Purpose**: Establish shared selection validation and workspace state rules before story-specific UI work.

- [X] T004 [P] Add one-based displayed-list selection parsing and bounds tests in `BlogWriter.Tests/SessionListSelectionTests.cs` for trimming, positive whole numbers, zero/negative/decimal/text rejection, and out-of-range rejection.
- [X] T005 [P] Add transient selector, restored-prompt, Help-dialog, and copy-status state fields to `BlogWriter.Web/Services/BlogWorkspaceState.cs`, preserving existing `HasUnsavedText`, operation-version, word-range, and Reviewer notes state.
- [X] T006 [P] Define the fixed run command and copy-status values in `BlogWriter.Web/Components/RunCommandDialog.razor` or a shared web constant file, using exactly `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https`.
- [X] T007 Update `BlogWriter.Web/Services/BlogWorkspaceService.cs` with a validated one-based selection entry point that rejects invalid input before `LoadAsync` or workflow start and preserves existing discard/cancellation semantics.
- [X] T008 Update `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` with foundational assertions that invalid selection performs zero loads/workflow calls and valid selection uses the existing owner-scoped session service.

**Checkpoint**: Selection state and validation are deterministic, non-operative for invalid input, and ready for command-bar integration.

---

## Phase 3: User Story 1 - Launch a Saved Session from the Command Bar (Priority: P1) MVP

**Goal**: Enter a displayed saved-session number beside List, restore `MainTask` and optional `CurrentSubTask`, and start exactly one initial workflow operation.

**Independent Test**: Open List, enter a valid number, and verify old Draft/Reviewer/prompt state clears, selected prompt values restore, and one processing operation starts; invalid numbers start none.

### Tests for User Story 1

- [X] T009 [P] [US1] Add `BlogWorkspaceService` tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` for clearing Draft/Reviewer/prompt state before restoration, `MainTask` restoration, non-empty `CurrentSubTask` restoration, empty `CurrentSubTask`, exactly-one automatic start, and processing-conflict rejection.
- [X] T010 [P] [US1] Add command-bar selection interaction tests in `BlogWriter.Web.Tests/CommandBarTests.cs` for selector visibility after List, one-based displayed numbering, invalid input feedback, and removal of the bottom selector.
- [X] T011 [P] [US1] Add saved-session compatibility assertions in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` proving automatic launch reuses existing range, cancellation, persistence, and owner-scoped session behavior.

### Implementation for User Story 1

- [X] T012 [US1] Add the compact List selector and its one-based input binding beside List in `BlogWriter.Web/Components/CommandBar.razor`, retaining New, List, Revise, and Quit callbacks and exposing an accessible three-digit field.
- [X] T013 [US1] Update `BlogWriter.Web/Components/Pages/Home.razor` to pass selector state/actions into `CommandBar`, remove the bottom `session-selector` markup, and preserve existing discard-dialog and workflow-log placement.
- [X] T014 [US1] Implement valid-selection restoration and automatic initial submission in `BlogWriter.Web/Services/BlogWorkspaceService.cs`: clear Draft, Reviewer notes, Revision request, and New writing prompt; load the selected session; restore `MainTask` and non-empty `CurrentSubTask`; invoke the existing initial submission exactly once.
- [X] T015 [US1] Update `BlogWriter.Web/Services/BlogWorkspaceState.cs` to expose selection visibility and restored prompt values without changing persisted `BlogSession` or `ResearchState` schemas.
- [X] T016 [US1] Update `BlogWriter.Web/wwwroot/app.css` to place the three-digit selector beside List, shift right-side commands without overlap, and remove obsolete bottom-selector layout rules at 390 × 844 and 1440 × 900.
- [X] T017 [US1] Run US1 tests from `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`, `BlogWriter.Web.Tests/CommandBarTests.cs`, and `BlogWriter.Tests/BlogWriterSessionServiceTests.cs`; verify one valid selection launches one operation and invalid selections launch none.

**Checkpoint**: User Story 1 is independently demonstrable as the saved-session launcher MVP.

---

## Phase 4: User Story 2 - Use Compact Command Controls (Priority: P1)

**Goal**: Add the question-mark Help action with an accessible dialog and clipboard copy while preserving command-bar behavior.

**Independent Test**: Open Help, verify the exact HTTPS run command, copy it successfully, exercise copy failure, dismiss the dialog, and confirm workspace state is unchanged.

### Tests for User Story 2

- [X] T018 [P] [US2] Add command-bar tests in `BlogWriter.Web.Tests/CommandBarTests.cs` for the compact question-mark button, accessible name, List-selector placement, and right-side command operability.
- [X] T019 [P] [US2] Add Help dialog tests in `BlogWriter.Web.Tests/RunCommandDialogTests.cs` for exact command text, modal labeling, successful clipboard copy, clipboard failure feedback, repeated open/close, and unchanged workspace state.
- [X] T020 [P] [US2] Add page integration assertions in `BlogWriter.Web.Tests/HomePageTests.cs` proving the bottom session selector is absent and Help can be opened from the workspace command bar.

### Implementation for User Story 2

- [X] T021 [US2] Create `BlogWriter.Web/Components/RunCommandDialog.razor` with accessible modal semantics, the exact HTTPS launch command, Copy and close actions, visible copy success/failure feedback, and existing focus/JS interop conventions.
- [X] T022 [US2] Update `BlogWriter.Web/Components/CommandBar.razor` to render the compact `?` Help button and raise the Help dialog callback without altering existing command callbacks.
- [X] T023 [US2] Update `BlogWriter.Web/Components/Pages/Home.razor` to own Help-dialog visibility, render `RunCommandDialog`, and preserve underlying workspace state when the dialog opens or closes.
- [X] T024 [US2] Update `BlogWriter.Web/wwwroot/workspace.js` and `BlogWriter.Web/wwwroot/app.css` for clipboard fallback, modal focus/dismissal behavior, compact Help-button styling, and command-bar responsiveness.
- [X] T025 [US2] Run US2 command-bar, dialog, and page tests from `BlogWriter.Web.Tests/CommandBarTests.cs`, `BlogWriter.Web.Tests/RunCommandDialogTests.cs`, and `BlogWriter.Web.Tests/HomePageTests.cs`.

**Checkpoint**: User Stories 1 and 2 provide the compact List launcher and Help/copy command-bar experience.

---

## Phase 5: User Story 3 - Control Processing Inputs Clearly (Priority: P2)

**Goal**: Keep Revision request availability synchronized with Revise and constrain the workflow log to a three-line visible viewport while retaining scrollable history.

**Independent Test**: Render no-session, eligible-session, and processing states; verify Revision request disabled state matches Revise and the log shows three lines while older entries remain scrollable.

### Tests for User Story 3

- [X] T026 [P] [US3] Add prompt-state tests in `BlogWriter.Web.Tests/HomePageTests.cs` proving Revision request is disabled whenever Revise is disabled and enabled only for an eligible non-processing session.
- [X] T027 [P] [US3] Add workflow-log viewport assertions in `BlogWriter.Web.Tests/WorkflowLogTests.cs` for three visible normal entry lines, retained overflow scrolling, and safe text rendering.
- [X] T028 [P] [US3] Extend `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` for selector width, command-bar non-overlap, three-line log viewport, and no horizontal scrolling at 390 × 844 and 1440 × 900.

### Implementation for User Story 3

- [X] T029 [US3] Update `BlogWriter.Web/Components/Pages/Home.razor` and `BlogWriter.Web/Components/PromptInput.razor` so Revision request `Disabled` follows `!Workspace.State.IsReviseEnabled || Workspace.State.IsProcessing` while preserving Enter/Shift+Enter behavior.
- [X] T030 [US3] Update `BlogWriter.Web/wwwroot/app.css` to cap `.workflow-log` at three normal entry lines, retain overflow scrolling, and preserve readable command/prompt/pane layout on mobile and desktop.
- [X] T031 [US3] Update `BlogWriter.Web/Components/WorkflowLog.razor` only as needed to preserve chronological retained entries, accessible live semantics, and scrollable text-safe output under the three-line viewport.
- [X] T032 [US3] Run US3 component and browser-contract tests from `BlogWriter.Web.Tests/HomePageTests.cs`, `BlogWriter.Web.Tests/WorkflowLogTests.cs`, and `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs`.

**Checkpoint**: All requested command, input, log, and responsive behaviors are independently validated.

---

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Document the new interaction and verify full compatibility and unchanged MAF health.

- [X] T033 [P] Update `README.md` and `docs/configuration.md` with the command-bar List launcher, `MainTask`/`CurrentSubTask` restoration, Help command/copy behavior, disabled Revision request, and three-line log viewport.
- [X] T034 [P] Update `specs/006-list-launcher-workflow/quickstart.md` with focused test results and any browser/manual validation outcomes.
- [X] T035 Run the full core and web test suites and build `BlogWriter.csproj` plus `BlogWriter.Web/BlogWriter.Web.csproj` using the commands in `specs/006-list-launcher-workflow/quickstart.md`.
- [X] T036 Run `git diff --check` and inspect changed files for bottom-selector remnants, unsafe markup rendering, duplicate workflow launches, clipboard regressions, and unrelated changes.
- [X] T037 Run MAF Doctor and compare with the baseline: F, 4 errors, 3 warnings, 0 silent-starvation risks, and 6 heuristic uncapped-call matches; investigate any new finding.
- [X] T038 Run every manual quickstart scenario, including invalid/valid selection, automatic launch, optional CurrentSubTask restoration, Help copy success/failure, disabled Revision request, three-line scrolling, and both required viewport sizes.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001-T003 can run in parallel immediately.
- **Foundational (Phase 2)**: T004-T008 depends on Setup and blocks all user stories; T004-T006 can run in parallel, followed by T007-T008.
- **User Story 1 (Phase 3)**: Depends on the foundational selection contract and is the MVP; complete before broad UI polish.
- **User Story 2 (Phase 4)**: Depends on the command-bar surface from US1; dialog tests and implementation can proceed after the command-bar callback contract is defined.
- **User Story 3 (Phase 5)**: Depends on the rendered command bar and workflow log from US1/US2; its disabled-state tests can begin after the shared workspace state is available.
- **Polish (Phase 6)**: Depends on all selected stories and focused tests.

### User Story Dependencies

- **User Story 1 (P1)**: Requires Phase 2; no dependency on US2 or US3. MVP candidate.
- **User Story 2 (P1)**: Requires Phase 2 and the US1 command-bar integration; independently testable as a Help/copy increment.
- **User Story 3 (P2)**: Requires the existing workflow log and command bar; validates shared layout and disabled-state behavior.

### Parallel Opportunities

- Phase 1: T001, T002, and T003 can run in parallel.
- Phase 2: T004, T005, and T006 can run in parallel; T007-T008 follow the shared state contract.
- US1: T009-T011 can run in parallel; T012-T013 can follow the test contract; T016 can proceed alongside service work after layout expectations are set.
- US2: T018-T020 can run in parallel; T021 and T024 can proceed in parallel after the dialog contract is fixed.
- US3: T026-T028 can run in parallel; T029-T031 can be split by page/input, CSS, and component responsibilities.
- Phase 6: T033, T034, and T036 can run in parallel; T035, T037, and T038 are final validation tasks.

## Parallel Execution Examples

### User Story 1

```text
Task T009: Add workspace restoration and exactly-one-launch tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T010: Add command-bar selector tests in BlogWriter.Web.Tests/CommandBarTests.cs
Task T011: Add session-service compatibility tests in BlogWriter.Tests/BlogWriterSessionServiceTests.cs
```

### User Story 2

```text
Task T018: Add compact command-bar tests in BlogWriter.Web.Tests/CommandBarTests.cs
Task T019: Add RunCommandDialog tests in BlogWriter.Web.Tests/RunCommandDialogTests.cs
Task T020: Add Home page Help/bottom-selector tests in BlogWriter.Web.Tests/HomePageTests.cs
```

### User Story 3

```text
Task T026: Add Revision request disabled-state tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T027: Add three-line WorkflowLog tests in BlogWriter.Web.Tests/WorkflowLogTests.cs
Task T028: Add responsive browser-contract tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases.
2. Complete US1 List launcher and automatic session processing.
3. Run US1 focused tests and manually verify one valid selection starts one operation.
4. Stop for an MVP demonstration before adding Help/copy and viewport polish.

### Incremental Delivery

1. Establish selection validation and transient workspace state.
2. Deliver US1 saved-session launcher.
3. Deliver US2 compact command bar and Help/copy dialog.
4. Deliver US3 disabled Revision request and three-line log viewport.
5. Complete documentation, full regression, browser checks, and MAF comparison.

## Notes

- Every task uses `- [ ] T### [P?] [US#?] description` and names an exact file path.
- Tests are written before their corresponding implementation tasks and must fail for the new behavior before implementation is considered complete.
- No task changes hosted-agent deployment, credentials, token budgets, Cosmos schema, session ownership, or workflow topology.

## Phase 7: Convergence

- [X] T039 [P] Add the planned clipboard JS interop helper in `BlogWriter.Web.Tests/HelpDialogTestHelpers.cs` and extend `BlogWriter.Web.Tests/RunCommandDialogTests.cs` to exercise clipboard failure feedback as required by FR-013–FR-014 (missing).
- [X] T040 [P] Replace the placeholder viewport-only assertions in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` with executable checks for three-digit selector width, command-bar non-overlap, three-line workflow-log containment, and no horizontal overflow at 390 × 844 and 1440 × 900 per T028, FR-017, and SC-006 (partial).
- [X] T041 Add a deterministic saved-session fixture for browser validation in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` or the Testing host setup, seed at least one session with `MainTask` and optional `CurrentSubTask`, and exercise the complete valid List-selection launch flow required by T038 and US1/AC1–AC5 (partial).
