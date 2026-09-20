# Feature Specification: Word Count Controls

**Feature Branch**: `004-word-count-controls`

**Created**: 2026-09-20

**Status**: Draft

**Input**: User description: "Add a labeled entry for minimum word and maximum words. Label them Min and Max. Place them after the entry for the prompts and before the Draft and revision."

## Clarifications

### Session 2026-09-20

- Q: Should changing Min or Max count as unsaved work when the user selects New, List, or Quit? → A: Confirm when the range differs from the current session's accepted range, or from the defaults in a new workspace.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Set the Draft Word Range (Priority: P1)

As a blog writer, I can set minimum and maximum word targets before submitting a prompt so the generated draft is reviewed against the length I need.

**Why this priority**: The controls provide value only when the selected range governs the next writing request.

**Independent Test**: Enter valid Min and Max values, submit a new prompt, and verify that the resulting writing session retains and uses the selected range.

**Acceptance Scenarios**:

1. **Given** a new workspace, **When** it is displayed, **Then** labeled Min and Max entries show the existing default targets of 1000 and 2000.
2. **Given** valid Min and Max values, **When** the user submits a new prompt, **Then** the new writing session uses those values as its target word range.
3. **Given** the user chooses New, **When** the workspace is cleared, **Then** Min and Max return to 1000 and 2000.
4. **Given** Min or Max differs from the new workspace defaults, **When** the user chooses New, List, or Quit, **Then** the changed range is treated as unsaved work and requires discard confirmation.

---

### User Story 2 - Retain the Range While Revising (Priority: P1)

As a blog writer, I can see and adjust the word range for an active or loaded session so follow-up revisions target the currently displayed limits.

**Why this priority**: Revisions must not silently use limits that differ from the values visible to the user.

**Independent Test**: Load a saved session, verify its stored Min and Max values appear, adjust the range, submit a revision, and verify the revised session retains the updated range.

**Acceptance Scenarios**:

1. **Given** a saved session is loaded, **When** its draft and review are displayed, **Then** Min and Max show that session's stored word targets.
2. **Given** an active session and a valid changed range, **When** the user submits a revision request, **Then** the revision uses and saves the displayed Min and Max values.
3. **Given** a completed new prompt or revision, **When** the results appear, **Then** the displayed Min and Max values remain unchanged from the accepted submission.
4. **Given** Min or Max differs from the active session's accepted range, **When** the user chooses New, List, or Quit, **Then** the changed range is treated as unsaved work and requires discard confirmation.

---

### User Story 3 - Correct Invalid Word Ranges (Priority: P2)

As a blog writer, I receive clear validation when the word range is invalid so I can correct it before starting costly writing work.

**Why this priority**: Preventing invalid submissions avoids confusing output and unnecessary workflow calls.

**Independent Test**: Try empty, non-whole, zero, negative, and reversed ranges and verify that no writing operation begins until both entries are valid.

**Acceptance Scenarios**:

1. **Given** either entry is empty, non-numeric, non-whole, zero, or negative, **When** the user attempts to submit a prompt or revision, **Then** no writing operation starts and the invalid entry is identified.
2. **Given** Max is less than Min, **When** the user attempts to submit, **Then** no writing operation starts and the user is told that Max must be at least Min.
3. **Given** the range is invalid, **When** the user corrects both entries, **Then** the validation clears and submission can proceed.

### Edge Cases

- Min and Max are equal; the exact target is accepted.
- Values contain surrounding whitespace; harmless whitespace does not change the entered whole number.
- A loaded legacy session has no explicit word targets; the existing default values are displayed.
- The user changes Min or Max while a writing operation is processing; the in-flight operation retains the range accepted when it began, and the changed values apply only to a later submission.
- The viewport is narrow; both labeled entries remain visible, readable, and operable without overlapping the prompt or content panes.
- Assistive technology announces each label, current value, invalid state, and validation message.
- A discard confirmation for a changed range is declined; Min, Max, and the rest of the workspace remain unchanged.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The workspace MUST provide two compact word-count entries labeled exactly `Min` and `Max`.
- **FR-002**: The Min and Max entries MUST appear after the prompt-entry area and before the Draft and Reviewer content areas in the reading and keyboard order.
- **FR-003**: A new workspace MUST initialize Min to 1000 and Max to 2000.
- **FR-004**: Choosing New MUST reset Min to 1000 and Max to 2000.
- **FR-005**: Min and Max MUST accept positive whole numbers only.
- **FR-006**: Max MUST be greater than or equal to Min.
- **FR-007**: Invalid Min or Max values MUST prevent both initial-prompt and revision submission.
- **FR-008**: Validation MUST identify the invalid entry or explain that Max must be at least Min.
- **FR-009**: A valid range MUST be applied to the writing session created by the next initial-prompt submission.
- **FR-010**: Loading a saved session MUST populate Min and Max from that session's stored word targets, using 1000 and 2000 when stored targets are unavailable.
- **FR-011**: A valid changed range MUST be applied and saved with the next revision submission for the active session.
- **FR-012**: An in-progress writing operation MUST retain the Min and Max values accepted at its start, even if the visible entries are changed before completion.
- **FR-013**: Completing or failing a writing operation MUST NOT unexpectedly replace the currently accepted Min and Max values.
- **FR-014**: The Min and Max entries MUST remain usable at the existing required mobile and desktop workspace sizes without overlap or clipped labels.
- **FR-015**: The entries and validation feedback MUST meet the workspace's existing keyboard and accessibility requirements.
- **FR-016**: A Min or Max value that differs from the active session's accepted range, or from 1000 and 2000 in a new workspace, MUST be treated as unsaved work for New, List, and Quit confirmation.
- **FR-017**: Declining discard confirmation for a changed range MUST preserve Min, Max, and all other workspace state.
- **FR-018**: After a successful initial or revision submission, the submitted Min and Max values MUST become the accepted range used to determine whether later changes are unsaved.

### Key Entities *(include if feature involves data)*

- **Word Range**: The pair of positive whole-number targets, Min and Max, where Max is at least Min.
- **Accepted Submission Range**: The immutable snapshot of Min and Max used by one initial or revision operation.
- **Accepted Workspace Range**: The most recently successful session range, or the defaults in a new workspace, used as the comparison baseline for unsaved changes.
- **Writing Session**: The existing saved session that stores and reuses its minimum and maximum word targets.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested new-workspace and New-action cases, Min displays 1000 and Max displays 2000.
- **SC-002**: In 100% of valid-range tests, the created or revised session contains the exact submitted Min and Max values.
- **SC-003**: In 100% of empty, non-whole, zero, negative, and reversed-range tests, no writing workflow begins and a corrective message is presented.
- **SC-004**: In 100% of saved-session tests, loading a session displays its stored word range or the defined defaults when targets are unavailable.
- **SC-005**: At 390 × 844 and 1440 × 900 viewport sizes, both labels and entries remain visible and operable without overlap or horizontal scrolling.
- **SC-006**: Automated and manual accessibility checks find no new Level A or AA failures in labeling, keyboard order, invalid-state communication, or validation feedback.
- **SC-007**: In 100% of New, List, and Quit tests, a changed range triggers discard confirmation, declining preserves the range, and an unchanged range proceeds without a range-related prompt.

## Assumptions

- The requested word-count controls are placed between the two prompt inputs and the Draft/Reviewer panes.
- The existing default word targets of 1000 and 2000 remain the product defaults.
- Min and Max apply to both new writing requests and subsequent revisions.
- Saved sessions already carry word-count targets as part of their writing state; this feature does not introduce a separate preference record.
- No upper product limit is introduced beyond requiring a positive whole number and Max greater than or equal to Min.
- The accepted range is updated only after a successful initial or revision submission; merely editing Min or Max does not persist it.
