# Feature Specification: Workflow Log and Reviewer Notes

**Feature Branch**: `005-workflow-log-reviewer-notes`

**Created**: 2026-09-20

**Status**: Draft

**Input**: User description: "Put the log output under the buttons, replacing what is there. Put all the reviewer feedback into the Reviewer notes as they become available."

## Clarifications

### Session 2026-09-20

- Q: When a revision produces new reviewer feedback, should Reviewer notes keep feedback from earlier reviews in the same active session as well as the new feedback? → A: Append feedback across revisions for the active session; clear it on New or when loading another session.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See Workflow Log Output (Priority: P1)

As a blog writer, I can see workflow log output directly beneath the workspace buttons so I know what the application is doing without searching elsewhere on the page.

**Why this priority**: Visible progress and outcomes are the primary requested change and make long-running writing operations understandable.

**Independent Test**: Start, complete, cancel, and fail writing operations while observing the area below the command buttons; verify that log entries appear there in chronological order and the existing status area is no longer rendered separately.

**Acceptance Scenarios**:

1. **Given** the workspace is open, **When** no operation has run yet, **Then** the log region is present beneath the command buttons with an appropriate empty state.
2. **Given** a writing operation starts, **When** progress or a state transition occurs, **Then** a new log entry appears beneath the buttons without removing earlier entries.
3. **Given** an operation completes, is cancelled, or fails, **When** its outcome is known, **Then** the outcome is appended to the log with wording that distinguishes the result.
4. **Given** the workspace has existing status or validation messages, **When** the feature is used, **Then** those messages are represented in the log region rather than displayed in a separate status stack.

---

### User Story 2 - Follow Reviewer Feedback (Priority: P1)

As a blog writer, I can see reviewer feedback in Reviewer notes as soon as each piece becomes available so I can follow the review without waiting for unrelated workflow output.

**Why this priority**: Reviewer feedback is the most important result of the review stage and must remain discoverable in its dedicated pane.

**Independent Test**: Provide multiple reviewer feedback updates during one workflow and verify that each update appears in Reviewer notes in arrival order, remains visible, and is not redirected to the workflow log.

**Acceptance Scenarios**:

1. **Given** reviewer feedback becomes available, **When** the workspace receives it, **Then** Reviewer notes displays it without requiring a page refresh or a later command.
2. **Given** multiple reviewer feedback updates arrive, **When** each update is received, **Then** Reviewer notes retains all updates in their arrival order.
3. **Given** a workflow log event and reviewer feedback arrive close together, **When** both are rendered, **Then** workflow events remain under the buttons and reviewer feedback remains in Reviewer notes.
4. **Given** no reviewer feedback is available, **When** the workspace is displayed, **Then** Reviewer notes shows its existing empty state.
5. **Given** an active session has reviewer feedback from an earlier review, **When** a revision produces more feedback, **Then** the new feedback is appended after the earlier feedback.

---

### User Story 3 - Read the Output Accessibly (Priority: P2)

As a blog writer using keyboard navigation or assistive technology, I can identify new log entries and reviewer feedback without losing context or having content unexpectedly replaced.

**Why this priority**: Incremental output is useful only if users can perceive and navigate it reliably.

**Independent Test**: Use keyboard navigation and an accessibility inspection while log entries and reviewer feedback are appended; verify clear labels, reading order, and announcements for new content.

**Acceptance Scenarios**:

1. **Given** new workflow log output is appended, **When** the user is viewing the workspace, **Then** the log region is identified as live status content without repeatedly stealing focus.
2. **Given** new reviewer feedback is appended, **When** the user is viewing Reviewer notes, **Then** the new feedback is announced or otherwise perceivable while existing notes remain navigable.
3. **Given** the log or Reviewer notes becomes long, **When** the user navigates within it, **Then** the content remains readable, scrollable, and does not overlap the command buttons or content panes.

---

### Edge Cases

- A workflow fails before producing reviewer feedback; the error appears in the log and Reviewer notes retains its current content.
- A workflow is cancelled while feedback is arriving; feedback already received remains visible and no later stale update replaces newer workspace state.
- A new session or loaded session begins; the log and Reviewer notes follow the workspace's existing reset and publication behavior without leaking output from the prior session. Reviewer notes clear when New starts or when another session is loaded.
- The same reviewer feedback update is delivered more than once; duplicate content is not rendered as an unintended replacement or loss of prior notes.
- The log contains long messages or special characters; entries remain readable and are presented as text rather than executable markup.
- The workspace is in list or ended mode; the log remains below the buttons when applicable and does not obscure session-list or ended-state content.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The workspace MUST render a labeled workflow log region directly beneath the New, List, Revise, and Quit buttons.
- **FR-002**: The workflow log region MUST replace the existing separate status and validation message stack in that location; lifecycle, progress, validation, cancellation, conflict, and failure messages MUST be represented as log entries.
- **FR-003**: Log entries MUST be appended in chronological order and MUST remain visible until the workspace's existing reset, session-load, or end behavior clears or replaces them.
- **FR-004**: Each log entry MUST communicate the event text and distinguish at least progress, success, cancellation, validation, and failure outcomes.
- **FR-005**: The workflow log MUST provide a clear empty state when no log entries are available.
- **FR-006**: The workspace MUST route every reviewer feedback update to Reviewer notes as it becomes available.
- **FR-007**: Reviewer notes MUST preserve all reviewer feedback updates received for the current active session, including feedback from earlier revisions, in arrival order without replacing earlier feedback with a later update.
- **FR-008**: Reviewer feedback MUST NOT be rendered only in the workflow log or discarded when workflow progress is also being logged.
- **FR-009**: Reviewer notes MUST retain already received feedback when a workflow fails or is cancelled, and MUST clear only when New starts or a different saved session is loaded.
- **FR-010**: New reviewer feedback MUST become visible without a manual page refresh or unrelated command.
- **FR-011**: The log and Reviewer notes MUST handle stale or late updates from a superseded operation without overwriting newer workspace state.
- **FR-012**: Log entries and Reviewer notes MUST present received content as safely rendered text and MUST NOT interpret content as executable markup.
- **FR-013**: The workflow log MUST expose an accessible label, readable chronological order, and live-update semantics that do not repeatedly move keyboard focus.
- **FR-014**: Reviewer notes MUST expose an accessible label, preserve keyboard-readable content order, and make appended feedback perceivable to assistive technology.
- **FR-015**: The log and Reviewer notes MUST remain readable and scrollable at the existing required mobile and desktop workspace sizes without overlap or horizontal scrolling.
- **FR-016**: Existing command behavior, session ownership, cancellation, workflow termination, and word-count controls MUST remain unchanged except where needed to publish their output through the new log or Reviewer notes surfaces.
- **FR-017**: The feature MUST include focused tests for log ordering and outcomes, reviewer-feedback accumulation, reset/publication behavior, stale updates, and accessibility semantics.

### Key Entities *(include if feature involves data)*

- **Workflow Log Entry**: A chronological user-visible event containing text and an outcome or severity category.
- **Reviewer Feedback Update**: A unit of reviewer output delivered during a workflow and appended to the current Reviewer notes.
- **Workspace Output Stream**: The current session's ordered collection of workflow log entries and reviewer feedback updates displayed in their respective regions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested progress, success, cancellation, validation, conflict, and failure scenarios, the resulting user-visible message appears beneath the command buttons in the workflow log.
- **SC-002**: In 100% of tests with multiple reviewer feedback updates across initial writing and revisions, Reviewer notes displays every update in arrival order without losing earlier feedback.
- **SC-003**: New log entries and reviewer feedback become visible within one normal workspace update cycle, without a manual page refresh or unrelated user command.
- **SC-004**: Accessibility checks identify no new Level A or AA failures in labels, live-update behavior, keyboard order, text rendering, or responsive overflow.
- **SC-005**: At 390 × 844 and 1440 × 900 viewport sizes, the command buttons, workflow log, Draft pane, and Reviewer notes remain readable and non-overlapping.
- **SC-006**: In 100% of reset, load, cancellation, and stale-update tests, output from a superseded workspace state does not overwrite output belonging to the current state.

## Assumptions

- "The buttons" refers to the existing New, List, Revise, and Quit command bar.
- "The log output" includes the existing progress, status, validation, cancellation, conflict, and failure messages currently shown below the command bar.
- Reviewer notes remain the dedicated destination for reviewer feedback; the workflow log is for workflow and workspace events rather than review content.
- Reviewer feedback may arrive in multiple updates during one operation or across revisions, and each update should be retained for the active session until the user starts New or loads a different session.
- Existing authentication, session persistence, word-count controls, and hosted-agent boundaries are reused.
- No server-side log retention or cross-session export is required for this feature.
