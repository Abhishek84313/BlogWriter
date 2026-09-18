# Tasks: Session List and Recall

**Input**: Design documents from `/specs/002-session-list-recall/`

**Tests**: Focused xUnit tests cover formatting, numeric selection, parser behavior, and active-session safety.

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 [P] Add the planned session-list test file at `BlogWriter.Tests/SessionListFormattingTests.cs`.
- [X] T002 [P] Record focused validation scenarios in `specs/002-session-list-recall/quickstart.md`.

## Phase 2: Foundational (Blocking Prerequisites)

- [X] T003 [P] Add shared formatting and numeric-selection tests in `BlogWriter.Tests/SessionListFormattingTests.cs` for one-based labels, input order, positive in-range values, and internal identifier preservation.
- [X] T004 [P] Add validation tests in `BlogWriter.Tests/SessionListFormattingTests.cs` for empty, non-numeric, zero, negative, out-of-range, and raw identifier input.
- [X] T005 Implement the transient formatting and numeric-selection helper in `./SessionListSelection.cs`.

## Phase 3: User Story 1 - Number Previous Sessions (Priority: P1) MVP

## User Story 1

### Goal

Display saved sessions with unique, gap-free one-based labels while preserving order and the empty state.

### Independent Test

Supply ordered summaries and verify `[1]`, `[2]`, and multi-digit labels; verify an empty list emits no numbered entries.

### Tests

- [X] T006 [P] [US1] Add numbered-output tests in `BlogWriter.Tests/SessionListFormattingTests.cs` for display order, `[1]`, `[100]`, and empty output.

### Implementation

- [X] T007 [US1] Update the session-entry formatting path in `./SessionListSelection.cs` so each entry begins with `[n]` and retains task and timestamp fields.
- [X] T008 [US1] Update `./Program.cs` `PrintSessions` to use numbered formatting and preserve newest-first ordering and the empty-state message.
- [X] T009 [US1] Run focused US1 tests and existing ordering tests in `BlogWriter.Tests/FileBlogSessionStoreTests.cs`.

## Phase 4: User Story 2 - Recall by List Number (Priority: P1)

## User Story 2

### Goal

Resolve `resume <number>` against the current list and load the matching saved session.

### Independent Test

Select a valid number from an ordered summary list and verify the matching internal identifier is loaded; invalid values do not replace the active session.

### Tests

- [X] T010 [P] [US2] Add numeric-selection tests in `BlogWriter.Tests/SessionListFormattingTests.cs` for valid, multi-digit, whitespace, zero, negative, non-numeric, empty, and out-of-range input.
- [X] T011 [P] [US2] Add numeric resume parsing tests in `BlogWriter.Tests/SessionCommandParserTests.cs` and cover raw identifier rejection at the selection boundary.

### Implementation

- [X] T012 [US2] Make `./SessionListSelection.cs` represent numeric selections only and reject raw identifiers.
- [X] T013 [US2] Update `./SessionCommandParser.cs` to preserve trimmed numeric `resume` values without changing list or new-topic parsing.
- [X] T014 [US2] Update the resume branch in `./Program.cs` to list current summaries, resolve the number through `./SessionListSelection.cs`, and load the resolved identifier.
- [X] T015 [US2] Add invalid-selection and unavailable-session handling in `./Program.cs` while preserving the active session.
- [X] T016 [US2] Run focused feature, parser, and store tests.

## Phase 5: User Story 3 - Preserve Existing Session Safety (Priority: P2)

## User Story 3

### Goal

Preserve ownership checks, cancellation, and active-session safety while rejecting raw identifier input.

### Independent Test

Exercise raw identifier rejection, invalid numeric recall, and unavailable entries; verify no unrelated session is loaded or created.

### Tests

- [X] T017 [P] [US3] Add safety tests in `BlogWriter.Tests/SessionCommandParserTests.cs` for identifier-shaped rejection, whitespace, list, and new-topic parsing.
- [X] T018 [P] [US3] Add active-session safety tests in `BlogWriter.Tests/SessionListFormattingTests.cs`.

### Implementation

- [X] T019 [US3] Preserve ownership, cancellation, and unavailable-session handling in `./Program.cs` while rejecting raw identifier recall.
- [X] T020 [US3] Verify `./SessionListSelection.cs` does not alter `./BlogSession.cs`, `./IBlogSessionStore.cs`, `./CosmosBlogSessionStore.cs`, or `./FileBlogSessionStore.cs` contracts.
- [X] T021 [US3] Run the focused session command and store tests and verify new-topic and follow-up paths.

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T022 [P] Update `specs/002-session-list-recall/contracts/console-commands.md` with final numeric-only wording.
- [X] T023 [P] Update `specs/002-session-list-recall/quickstart.md` with final validation scenarios.
- [X] T024 Run `dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj` and `dotnet build BlogWriter.csproj`.
- [X] T025 Run every scenario in `specs/002-session-list-recall/quickstart.md`, including raw identifier rejection.

## Requirement and Success-Criterion Coverage

| Requirement | Tasks | Notes |
|---|---|---|
| FR-001 | T006-T008 | Numbered display |
| FR-002 | T006-T008 | Display order |
| FR-003 | T010-T015 | Numeric recall |
| FR-004 | T012-T014 | Current-list resolution |
| FR-005 | T004, T010, T015 | Invalid input |
| FR-006 | T004, T015, T018 | Active-session preservation |
| FR-007 | T005, T012, T014 | Internal ID and state preservation |
| FR-008 | T004, T011, T012, T017, T019 | Numeric-only recall |
| FR-009 | T006, T010 | Multi-digit labels |
| FR-010 | T006, T008 | Empty list |

| Success Criterion | Tasks | Notes |
|---|---|---|
| SC-001 | T025 | Post-launch usability outcome |
| SC-002 | T010, T014, T016 | Correct mapping |
| SC-003 | T010, T015, T018 | Invalid selection safety |
| SC-004 | T006, T007, T008 | Gap-free labels |
| SC-005 | T010, T014, T025 | No user-entered ID |

## Dependencies & Execution Order

- Setup T001-T002 precedes foundation.
- Foundation tests T003-T004 precede helper implementation T005.
- User Story 1 depends on T003-T005.
- User Story 2 depends on User Story 1 display ordering and T007-T008.
- User Story 3 depends on User Story 2 integration.
- Polish depends on all desired stories.

## Parallel Opportunities

- T001-T002, T003-T004, T006-T007, T010-T011, T017-T018, and T022-T023 can run in parallel where their file ownership permits.

## Implementation Strategy

1. MVP: complete setup, foundation, and User Story 1, then validate numbered `list` output.
2. Add numeric recall and invalid-selection handling.
3. Verify safety, documentation, full tests, build, and quickstart scenarios.
