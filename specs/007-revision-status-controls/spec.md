# Feature Specification: Revision Status Controls

**Feature Branch**: `007-revision-status-controls`

**Created**: 2026-09-20

**Status**: Draft

**Input**: User description: "Enable the revision field and button when a draft is displayed or the new button is clicked. Remove the scrolling list box that displays the log and replace it with a single line text that updates for each update of the log. Initially keep the buttons together, moving the revise, quit and question mark buttons to the right when the user presses List and the list value box is displayed. That is, have them make room for the list value box. Make all the buttons a little smaller."

## Clarifications

### Session 2026-09-20

- Q: After clicking New with no active saved session, should Revise be enabled immediately, or only after a new draft has been generated? → A: Enable Revision request after New; enable Revise only after a draft/session exists.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Enable Revision for Drafts (Priority: P1)

As a blog writer, I can revise whenever a draft is displayed or after choosing New so the Revision request field and Revise command are available at the right time.

**Why this priority**: Revision is the primary follow-up workflow and should not be blocked by the current list-selection state model.

**Independent Test**: Render the workspace with no draft, with a draft, after New, and while processing; verify Revise and Revision request states match the intended availability rules.

**Acceptance Scenarios**:

1. **Given** no draft is displayed and no eligible session is active, **when** the workspace renders, **then** Revision request and Revise remain disabled.
2. **Given** a draft is displayed, **when** the workspace renders, **then** Revision request and Revise are enabled unless processing is active.
3. **Given** the user clicks New, **when** the new workspace is ready for input, **then** Revision request is enabled but Revise remains disabled until a draft/session exists.
4. **Given** processing is active, **when** the workspace renders, **then** existing processing protections still prevent duplicate submissions.

---

### User Story 2 - See the Latest Status (Priority: P1)

As a blog writer, I can see the latest workflow status in one compact line without a scrolling log panel taking space from the writing panes.

**Why this priority**: The requested status presentation is the primary workspace simplification.

**Independent Test**: Cause successive workflow updates and verify one status line replaces the previous line while remaining visible and accessible.

**Acceptance Scenarios**:

1. **Given** no workflow update exists, **when** the workspace renders, **then** a compact empty status appears.
2. **Given** a new workflow update arrives, **when** the workspace refreshes, **then** the single status line displays that newest update.
3. **Given** several updates arrive in sequence, **when** each update is received, **then** only the latest message is visible and no scrolling log list is rendered.
4. **Given** a status message contains long or markup-like text, **when** it is displayed, **then** it remains readable and text-safe without horizontal overflow.

---

### User Story 3 - Keep Commands Compact (Priority: P2)

As a blog writer, I can use a compact command bar that makes room for the List number field without losing the nearby Revise, Quit, or Help actions.

**Why this priority**: The command bar must remain usable on narrow screens while accommodating the inline List selector.

**Independent Test**: Render the command bar before and after List is clicked at mobile and desktop sizes; verify smaller buttons, stable grouping, and right-side command movement.

**Acceptance Scenarios**:

1. **Given** List is inactive, **when** the command bar renders, **then** all buttons remain grouped together.
2. **Given** List is clicked and the number field appears, **when** the command bar renders, **then** Revise, Quit, and Help move right to make room.
3. **Given** any required viewport, **when** the compact command bar renders, **then** all controls remain readable, focusable, and non-overlapping.
4. **Given** button labels and the Help symbol are displayed, **when** the controls render, **then** their dimensions are smaller than the previous command-bar dimensions while remaining operable.

### Edge Cases

- A draft exists while processing is active; Revision request remains protected from duplicate submission until processing finishes.
- New is clicked while unsaved content exists; existing discard confirmation remains in force before resetting state.
- List is active with a valid or invalid selector; status updates do not displace the selector or command controls.
- A workflow failure, cancellation, validation message, or success message replaces the prior single-line status.
- Long status text wraps or is clipped safely without creating horizontal page overflow.
- Reviewer notes remain in their dedicated pane and are not replaced by the single workflow status line.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Revision request and Revise MUST be enabled when a draft is displayed and no workflow is processing.
- **FR-002**: Clicking New MUST enable Revision request while leaving Revise disabled until a draft/session exists, unless processing protections apply.
- **FR-003**: Revision request and Revise MUST remain disabled when no draft/session is eligible and no New-state revision context is available.
- **FR-004**: Existing processing protections MUST continue to prevent duplicate revision or initial submissions.
- **FR-005**: The workspace MUST display one current workflow status line instead of the scrolling workflow-log list.
- **FR-006**: Each accepted workflow update MUST replace the displayed status line with its newest message.
- **FR-007**: The status line MUST preserve accessible live-update semantics and render content as safe text.
- **FR-008**: The workflow status line MUST not create horizontal overflow at 390 × 844 or 1440 × 900.
- **FR-009**: When List is inactive, command buttons MUST remain grouped together.
- **FR-010**: When the List number field is visible, Revise, Quit, and Help MUST move to the right to make room without overlap.
- **FR-011**: All command buttons MUST use smaller dimensions than the current command-bar buttons while remaining readable and operable.
- **FR-012**: Reviewer notes MUST remain separate from the single workflow status line.
- **FR-013**: The feature MUST include focused tests for revision availability, New-state availability, latest-status replacement, safe rendering, command-bar movement, button sizing, and responsive layout.

### Key Entities *(include if feature involves data)*

- **Revision Availability State**: Whether Revision request and Revise are enabled based on draft, New-state context, active session, and processing state; New enables only the field, while a draft/session enables both.
- **Current Workflow Status**: The newest accepted workflow update displayed as one accessible text line.
- **Compact Command Bar**: The grouped commands and conditional List selector with smaller control dimensions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested draft, New, empty, and processing states, Revision request and Revise match the defined availability rules.
- **SC-002**: In 100% of sequential update tests, only the newest workflow message is visible and no scrolling log list is rendered.
- **SC-003**: At 390 × 844 and 1440 × 900, the command bar, conditional List selector, prompts, status line, and content panes remain readable without overlap or horizontal scrolling.
- **SC-004**: Accessibility checks find no new Level A or AA failures in revision controls, status announcements, labels, keyboard order, or text rendering.
- **SC-005**: Reviewer notes remain visible independently while workflow status updates replace one another.

## Assumptions

- “New button clicked” means the New workspace is ready for a revision request field after any required discard confirmation; Revise remains disabled until a draft/session exists.
- Processing remains the highest-priority disable condition for both initial and revision submissions.
- The current status line retains only the latest message; no historical workflow-log persistence is added.
- The existing List selector and Help button remain in the command bar; only their spacing and dimensions change.
- Existing authentication, session ownership, word-count controls, cancellation, workflow termination, and Reviewer notes behavior are reused.
