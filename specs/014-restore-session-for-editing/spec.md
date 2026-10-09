# Feature Specification: Restore Saved Session for Editing

**Feature Branch**: `014-restore-session-for-editing`

**Created**: 2026-10-09

**Status**: Draft

**Input**: User description: "Remove the entry field next to the list button and replace it by allowing the user to click on the list item they want to restore. When a list item is restored, do not invoke it; wait for the user to click Go. Keep the query text visible, restore the minimum and maximum word counts to their respective fields, and leave all windows enabled so the user can modify the item before running it. Record the new run as a new entry in Cosmos."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Restore a Saved Session for Editing (Priority: P1)

As a blog writer, I can select a saved item from the list and review or edit its inputs before running it.

**Why this priority**: Restoring a saved item without immediately starting costly writing work gives the user control over what will be submitted.

**Independent Test**: Display saved items, select one, and verify its query and word limits appear in editable fields while no workflow starts.

**Acceptance Scenarios**:

1. **Given** the saved-item list is displayed, **when** the user selects an item, **then** the adjacent numeric entry field is no longer needed and the selected item is restored into the workspace.
2. **Given** a saved item is selected, **when** the workspace is restored, **then** its query remains visible and its minimum and maximum word counts appear in their respective fields.
3. **Given** a saved item has been restored, **when** the user edits the query or word-count values, **then** the controls remain enabled and accept the changes before submission.
4. **Given** a saved item has been restored, **when** the user has not activated Go, **then** no workflow is invoked and no new run is recorded.

---

### User Story 2 - Run the Edited Item as a New Session (Priority: P1)

As a blog writer, I can explicitly submit the restored or edited item with Go and keep the original saved item intact in my history.

**Why this priority**: Users need an explicit execution point and a separate history entry so they can reuse prior work without overwriting it.

**Independent Test**: Restore an item, change one or more inputs, activate Go, and verify one run uses the displayed values and is saved as a distinct item.

**Acceptance Scenarios**:

1. **Given** a saved item has been restored and its inputs are ready, **when** the user activates Go, **then** exactly one workflow run begins using the current values in the workspace.
2. **Given** the run is submitted, **when** it is recorded in session history, **then** it appears as a new entry and the source saved item remains unchanged.
3. **Given** a restored item has not been changed, **when** the user activates Go, **then** it still runs only after that explicit action and is recorded as a new entry.

### Edge Cases

- The saved list is empty; there is no item to restore and no workflow can start from list selection.
- A selected item cannot be loaded; the current workspace remains intact and the user receives a recoverable error.
- A saved item has no stored word limits; the existing default minimum and maximum values are shown.
- Restored or edited word limits are invalid; the existing validation prevents Go from starting a workflow and identifies the correction needed.
- The user edits restored values and selects another item; existing unsaved-work confirmation behavior is preserved where applicable.
- A workflow is already running; selecting or submitting another item does not replace or duplicate the in-progress run.
- The source record belongs to another user or is otherwise unavailable; existing ownership checks prevent it from being restored.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The List interaction MUST remove the numeric entry field adjacent to the List button and allow the user to select a saved item directly from the displayed list.
- **FR-002**: Selecting a saved item MUST restore its query text to the query field and its saved minimum and maximum word counts to their respective fields.
- **FR-003**: If a saved item has no stored word limits, the workspace MUST show the existing default minimum and maximum values.
- **FR-004**: Restoring a saved item MUST NOT invoke the workflow or create a new session record.
- **FR-005**: After restoration, all workspace windows and input controls MUST remain enabled so the user can modify the item before submission.
- **FR-006**: The workflow MUST start only when the user activates Go, using the current editable values at that time.
- **FR-007**: Submitting a restored item MUST record the resulting run as a new session-history entry and MUST NOT overwrite the source saved item.
- **FR-008**: Invalid inputs MUST follow existing validation behavior and MUST NOT start a workflow.
- **FR-009**: Saved-item selection MUST preserve existing session ownership, authorization, cancellation, word-count validation, and workflow-termination behavior.
- **FR-010**: Saved-item selection and Go submission MUST remain accessible by keyboard and assistive technology, with the existing responsive workspace behavior preserved.

### Key Entities *(include if feature involves data)*

- **Saved Item**: An existing session-history entry containing the query and word-count limits available for restoration.
- **Restored Workspace**: The editable workspace populated from a selected saved item before submission.
- **New Run Entry**: The distinct session-history entry created when the user explicitly submits the restored workspace with Go.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of saved-item selection tests, the selected query and word limits are restored to the correct fields and remain editable.
- **SC-002**: In 100% of selection-only tests, no workflow run starts and no new history entry is created before Go is activated.
- **SC-003**: In 100% of Go-submission tests, exactly one run starts with the values currently displayed in the workspace.
- **SC-004**: In 100% of successful restored-item runs, history contains a new entry while the source saved item remains unchanged.
- **SC-005**: In 100% of invalid-input tests, existing validation prevents a workflow run and communicates the required correction.
- **SC-006**: Existing ownership, cancellation, word-count, workflow-termination, accessibility, and responsive behavior pass their relevant regression checks.

## Assumptions

- The saved item's existing query remains the source for the restored query field; this feature does not change list ordering or item summaries.
- Existing default minimum and maximum word counts apply when a saved item lacks those values.
- Fields not specifically called out for restoration retain their existing restore/reset behavior, but remain enabled for editing.
- Go remains the only action that starts writing or revision work.
- A submission creates a new history entry regardless of whether the restored inputs were edited; it does not continue by overwriting the selected historical entry.
- Existing session ownership, persistence behavior, and retention remain unchanged apart from adding a distinct entry for the new run.
