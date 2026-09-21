# Tasks: Explicit Go Submission

**Input**: Design documents from `/specs/009-explicit-go-submission/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [workspace UI contract](contracts/workspace-ui.md), [quickstart.md](quickstart.md)

**Tests**: Focused xUnit and bUnit tasks are included because the project constitution requires test coverage for affected workflow behavior.

**Organization**: Tasks are grouped by user story so each increment can be implemented and verified independently.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other marked tasks because it changes a different file with no incomplete-task dependency.
- **[Story]**: Maps a task to its user story.

## Phase 1: Setup

**Purpose**: Establish the focused validation baseline.

- [X] T001 Verify restore and the existing focused web-test baseline using `BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj`

---

## Phase 2: Foundational

**Purpose**: No new shared infrastructure is required; use the existing workspace state, session service, range validation, and workflow safeguards.

**Checkpoint**: Existing workspace abstractions are ready for user-story work.

---

## Phase 3: User Story 1 - Submit Work Explicitly (Priority: P1) MVP

**Goal**: Give the writer one Go control beside Min and Max; only its activation starts processing, with a revision request taking precedence over an initial prompt.

**Independent Test**: Enter initial and revision text, press Enter in each relevant input without starting work, then activate Go and verify exactly one revision or draft operation begins according to the priority rule.

### Tests for User Story 1

- [X] T002 [P] [US1] Add Go-routing service tests for revision priority, initial-prompt fallback, empty-input no-op, range validation, and duplicate-operation protection in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`
- [X] T003 [P] [US1] Replace Enter-submission component coverage with Go placement, click submission, and Enter no-submission coverage in `BlogWriter.Web.Tests/HomePageTests.cs`

### Implementation for User Story 1

- [X] T004 [P] [US1] Remove the submit callback and Enter key handler from `BlogWriter.Web/Components/PromptInput.razor` so both prompt textareas retain native text-entry behavior and no longer advertise Enter submission
- [X] T005 [P] [US1] Add an accessible Go button and click callback parameter after the Max field in `BlogWriter.Web/Components/WordRangeInput.razor`
- [X] T006 [US1] Add one intentional-submission entry point in `BlogWriter.Web/Services/BlogWorkspaceService.cs` that validates the current word range, submits non-whitespace revision text first, otherwise submits non-whitespace initial-prompt text, and preserves the existing start/revise safeguards
- [X] T007 [US1] Wire the WordRange Go callback to the new workspace submission entry point and remove prompt-level submission wiring in `BlogWriter.Web/Components/Pages/Home.razor`
- [X] T008 [US1] Style the word-range Go control beside Min and Max without overlap or layout shift in `BlogWriter.Web/wwwroot/app.css`
- [X] T009 [US1] Run the focused explicit-submission tests in `BlogWriter.Web.Tests/HomePageTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`

**Checkpoint**: Go is the only draft/revision processing trigger, and User Story 1 passes its independent tests.

---

## Phase 4: User Story 2 - Prevent Revision Without a Draft (Priority: P1)

**Goal**: Make revision input and the revision command unavailable whenever the displayed draft has no non-whitespace content, while preserving revision text for later reuse.

**Independent Test**: Render the workspace with an empty, whitespace-only, and non-empty draft, then transition between them and verify the revision textarea and command have the correct disabled state without clearing revision text.

### Tests for User Story 2

- [X] T010 [P] [US2] Add state-level tests for empty, whitespace-only, non-empty, and processing draft availability in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`
- [X] T011 [P] [US2] Add rendered workspace tests for revision textarea and command disable/enable transitions while retaining revision text in `BlogWriter.Web.Tests/HomePageTests.cs`

### Implementation for User Story 2

- [X] T012 [US2] Update revision eligibility in `BlogWriter.Web/Services/BlogWorkspaceState.cs` so both revision input and revision command require a non-whitespace displayed draft and no active processing, with no New-mode or active-session exception
- [X] T013 [US2] Run the focused revision-availability tests in `BlogWriter.Web.Tests/HomePageTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`

**Checkpoint**: Revision controls consistently follow the displayed-draft rule, and User Story 2 passes its independent tests.

---

## Phase 5: Polish and Cross-Cutting Validation

**Purpose**: Keep operator documentation aligned and verify the affected web project end to end.

- [X] T014 Update the workspace-control and submission documentation in `docs/configuration.md` to describe Go-only submission and draft-gated revision availability
- [X] T015 Run all validation scenarios from `specs/009-explicit-go-submission/quickstart.md`
- [X] T016 Build the affected web project using `BlogWriter.Web/BlogWriter.Web.csproj`

---

## Dependencies and Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Starts immediately.
- **Foundational (Phase 2)**: Has no code change; confirms existing abstractions are sufficient.
- **User Story 1 (Phase 3)**: Starts after Setup and delivers the MVP.
- **User Story 2 (Phase 4)**: Can begin after Setup, but should follow User Story 1 because both update the same rendered-page and test files.
- **Polish (Phase 5)**: Begins after both stories pass their focused tests.

### User Story Dependencies

- **US1**: No dependency on US2. It is independently demonstrable through explicit Go routing and the removal of Enter submission.
- **US2**: No behavioral dependency on US1, but shares `HomePageTests.cs` and `BlogWorkspaceServiceTests.cs`; implement after US1 to avoid test-file conflicts.

### Parallel Opportunities

- `T002` and `T003` can be written in parallel because they modify different test files.
- `T004` and `T005` can be implemented in parallel because they modify separate components.
- `T010` and `T011` can be written in parallel because they modify different test files.

## Parallel Example: User Story 1

```text
Task: "T002 Add Go-routing service tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs"
Task: "T003 Add Go component tests in BlogWriter.Web.Tests/HomePageTests.cs"

Task: "T004 Remove Enter submission in BlogWriter.Web/Components/PromptInput.razor"
Task: "T005 Add Go control in BlogWriter.Web/Components/WordRangeInput.razor"
```

## Implementation Strategy

### MVP First

1. Complete `T001`.
2. Complete User Story 1 (`T002` through `T009`).
3. Validate Go-only routing before changing revision eligibility.

### Incremental Delivery

1. Deliver explicit Go submission with focused tests.
2. Apply the strict draft-gated revision-control rule with its own state and UI tests.
3. Update documentation and run the quickstart validation plus web-project build.

## Phase 6: Convergence

- [X] T017 Remove the obsolete `Submit` callback parameter from `BlogWriter.Web/Components/PromptInput.razor` per T004 / plan submission decision (partial)