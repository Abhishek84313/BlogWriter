# Feature Specification: List Launcher Workflow

**Feature Branch**: `006-list-launcher-workflow`

**Created**: 2026-09-20

**Status**: Draft

**Input**: User description: "When List is clicked open an entry field next to the list button. Move the buttons to the right of list over to make room. Make the entry field just big enough to hold 3 numbers and remove the entry field at the bottom of the form. When the user enters a valid number from the list clear the draft and Reviewer Notes and clear the revision request and new writing prompt. Then fill the new writing prompt with the prompt from the selected number and, if we have it, fill the revision request as well. Then immediately begin processing those values as if the user had just entered them and hit enter. Also: make the scrolling window with the logging entries smaller (holding only 3 lines at a time). Also, next to the other buttons add a small button with a question mark. When this is pressed, open a dialog box with the command needed to run the application, and copy that text to the clipboard. Also, when the Revise button is disabled the Revision Request entry should be disabled."

## Clarifications

### Session 2026-09-20

- Q: Should the saved session's `CurrentSubTask` value be used to restore the Revision request field when launching a session from List? → A: Use `CurrentSubTask` as the saved Revision request when it is non-empty.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Launch a Saved Session from the Command Bar (Priority: P1)

As a blog writer, I can enter a saved-session number beside List and immediately resume that session's prompts and processing without using a second selector at the bottom of the form.

**Why this priority**: The current saved-session flow requires separate list selection and a later Revise action; the requested behavior makes recall a direct, compact command-bar workflow.

**Independent Test**: Open List, enter a valid displayed session number, and verify that the selected session's draft and Reviewer notes are cleared before its prompts are restored and processing starts automatically.

**Acceptance Scenarios**:

1. **Given** the workspace is not displaying the saved-session list, **when** List is clicked, **then** a compact numeric entry appears beside List and the commands to its right move right to make room.
2. **Given** the saved-session list is visible, **when** the user enters a valid number corresponding to a displayed session, **then** the draft, Reviewer notes, revision request, and new writing prompt are cleared before the selected session's prompt data is restored.
3. **Given** a selected session has an original prompt and a non-empty `CurrentSubTask`, **when** the valid number is entered, **then** the original prompt fills New writing prompt, `CurrentSubTask` fills Revision request, and processing begins immediately as one normal submission.
4. **Given** a selected session has an empty `CurrentSubTask`, **when** it is launched, **then** Revision request remains empty and processing still begins from the restored new writing prompt.
5. **Given** a valid session number is entered, **when** processing starts, **then** the selected session is processed through the existing workflow and word-count/session behavior remains unchanged.

---

### User Story 2 - Use Compact Command Controls (Priority: P1)

As a blog writer, I can use a compact command bar that keeps List selection beside List, removes the redundant bottom selector, and provides a question-mark help action.

**Why this priority**: The requested layout reduces visual travel and keeps command-related controls together while adding a discoverable way to copy the run command.

**Independent Test**: Render the command bar in normal, list, and processing states and verify placement, sizing, removal of the bottom selector, and the help dialog's copy behavior.

**Acceptance Scenarios**:

1. **Given** the command bar is visible, **when** the user inspects it, **then** a small question-mark button appears beside the existing commands.
2. **Given** the user presses the question-mark button, **when** the help dialog opens, **then** it shows the command needed to run the application and offers a copy action that places the same text on the clipboard.
3. **Given** List is active, **when** the command bar is rendered, **then** the three-number entry is next to List, commands to the right remain usable, and no session-number entry appears at the bottom of the form.
4. **Given** the entry field is visible, **when** the user types a value, **then** its width is sufficient for three digits and does not expand or overlap neighboring controls.
5. **Given** the user dismisses the help dialog, **when** the workspace remains open, **then** the underlying command bar and workspace state are unchanged.

---

### User Story 3 - Control Processing Inputs Clearly (Priority: P2)

As a blog writer, I can tell when revision input is unavailable and see a compact workflow log while processing is active.

**Why this priority**: Clear disabled-state behavior prevents submissions that cannot succeed, and a smaller log preserves room for the primary writing panes.

**Independent Test**: Render the workspace without an active session, during processing, and after completion; verify Revision request follows Revise availability and the log displays no more than three visible lines at a time.

**Acceptance Scenarios**:

1. **Given** Revise is disabled because no saved session is selected, **when** the prompt area is rendered, **then** Revision request is disabled.
2. **Given** Revise becomes enabled for a selected session, **when** the prompt area is rendered, **then** Revision request becomes available unless processing independently disables it.
3. **Given** workflow log entries exceed three lines, **when** the log is displayed, **then** its scrolling viewport shows three lines at a time and remains scrollable for older entries.
4. **Given** the workspace is at the required mobile or desktop viewport, **when** the log, command bar, and panes are displayed, **then** controls remain readable and do not overlap or introduce horizontal scrolling.

---

### Edge Cases

- List is clicked while there is unsaved prompt, revision, or word-range work; the existing discard confirmation still applies before changing workspace mode.
- The entered list number is empty, non-numeric, zero, negative, decimal, outside the displayed list, or contains surrounding whitespace; no session is launched and a clear validation message remains available.
- A valid number is entered while a workflow is already processing; the current operation is not replaced by a second launch.
- The selected session has no non-empty `CurrentSubTask`; the restored Revision request remains empty.
- The selected session's restored prompt or revision text contains markup-like characters; it is handled as text and not executable markup.
- Clipboard access is unavailable or denied; the command remains visible and the dialog reports that copying did not complete without changing workspace state.
- The help dialog is opened repeatedly or dismissed with keyboard controls; focus and underlying command state remain usable.
- The log contains long entries; the viewport remains capped at three visible lines while older entries remain reachable by scrolling.
- The selected session cannot be loaded or processing fails; the existing error/log behavior remains visible and no unrelated workspace content is lost.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The command bar MUST show a compact numeric session entry beside List when List mode is active.
- **FR-002**: The numeric session entry MUST be sized to accommodate three digits without resizing or overlapping neighboring commands.
- **FR-003**: Commands to the right of List MUST move right to make room for the List entry while remaining visible and operable.
- **FR-004**: The separate session-number entry at the bottom of the form MUST be removed.
- **FR-005**: Entering a valid displayed session number MUST clear the current Draft, Reviewer notes, Revision request, and New writing prompt before restoring selected-session prompt data.
- **FR-006**: After a valid session number is accepted, the system MUST restore the selected session's original prompt to New writing prompt.
- **FR-007**: After a valid session number is accepted, the system MUST restore the selected session's non-empty `CurrentSubTask` value to Revision request, and leave it empty when `CurrentSubTask` is empty.
- **FR-008**: After prompt data is restored from a valid selection, the system MUST immediately begin processing it using the existing writing workflow as if the user had submitted the restored New writing prompt.
- **FR-009**: Invalid, unavailable, or malformed session-number input MUST NOT begin processing and MUST provide a clear validation outcome in the existing workflow output surface.
- **FR-010**: List selection MUST preserve existing session ownership, cancellation, word-count, persistence, and workflow-termination behavior.
- **FR-011**: The command bar MUST include a compact question-mark help button beside the existing commands.
- **FR-012**: Activating the question-mark button MUST open an accessible dialog showing the command needed to run the application.
- **FR-013**: The help dialog MUST provide a copy action that copies the displayed run command to the clipboard.
- **FR-014**: Clipboard failure MUST leave the command visible and communicate that copying did not complete without changing workspace state.
- **FR-015**: The workflow log scrolling viewport MUST display no more than three lines at a time while allowing access to older entries by scrolling.
- **FR-016**: When Revise is disabled, the Revision request input MUST be disabled; when Revise is enabled and no workflow is processing, the Revision request input MUST be enabled.
- **FR-017**: The feature MUST preserve accessible labels, keyboard operation, focus behavior, safe text rendering, and responsive behavior at 390 × 844 and 1440 × 900.
- **FR-018**: The feature MUST include focused tests for valid and invalid List selection, automatic processing, prompt restoration from `MainTask`, optional revision restoration from `CurrentSubTask`, command-bar layout, help-dialog clipboard behavior, disabled revision input, and three-line log scrolling.

### Key Entities *(include if feature involves data)*

- **List Selection Input**: The compact numeric value entered beside List to identify one displayed saved session.
- **Restored Session Prompts**: The selected session's `MainTask` and optional `CurrentSubTask` used to populate the prompt inputs before automatic processing.
- **Run Command Help Dialog**: The accessible transient dialog containing the application launch command and copy action state.
- **Three-Line Workflow Log Viewport**: The bounded scrolling presentation of existing workflow log entries.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of valid selection tests, entering one displayed session number clears old workspace content, restores the selected prompt data, and starts exactly one processing operation.
- **SC-002**: In 100% of invalid selection tests, no processing operation starts and a user-visible validation outcome is provided.
- **SC-003**: In 100% of command-bar layout tests, List selection appears beside List, the bottom selector is absent, and the right-side commands remain operable.
- **SC-004**: In 100% of help-dialog tests, the displayed run command matches the configured application command and a successful copy places the same text on the clipboard.
- **SC-005**: In 100% of disabled-state tests, Revision request is disabled whenever Revise is disabled and enabled only when the existing processing/session conditions permit revision.
- **SC-006**: At 390 × 844 and 1440 × 900, the command bar, three-digit selection input, three-line log viewport, prompt inputs, and content panes remain readable without overlap or horizontal scrolling.
- **SC-007**: In 100% of regression tests, existing session ownership, cancellation, word-count behavior, workflow termination, and safe text rendering remain unchanged.

## Assumptions

- The selected number is the one-based position shown in the current saved-session list, not a persisted session identifier.
- The application run command shown by Help defaults to `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https` and is treated as text that can be copied.
- Automatic processing uses the restored New writing prompt as the primary submission; an available stored revision request is restored for the active workspace and remains available according to existing revision behavior.
- Existing discard confirmation applies when List is clicked with unsaved workspace state.
- Existing session records already contain `MainTask` and `CurrentSubTask`; a non-empty `CurrentSubTask` supplies the optional Revision request and no new persistence schema is introduced.
- The workflow log's three-line limit is a visual viewport limit, not a limit on retained entries.
- Existing authentication, session ownership, word-count controls, cancellation, workflow topology, and Reviewer notes behavior are reused.
