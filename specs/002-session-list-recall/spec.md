# Feature Specification: Session List and Recall

**Feature Branch**: `002-session-list-recall`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description: "When listing previous sessions number them at the beginning of the line like this [1] and allow the user to recall the session by that number rather than the session id."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Number Previous Sessions (Priority: P1)

As a blog writer, I can see previous sessions with a simple number at the beginning of each line so I can quickly distinguish and select a session.

**Why this priority**: Clear, stable numbering is the visible behavior requested and makes session selection faster than reading opaque identifiers.

**Independent Test**: Prepare a user with several saved sessions, request the previous-session list, and verify that every listed line begins with a unique number in square brackets.

**Acceptance Scenarios**:

1. **Given** a user has saved sessions, **When** they request the previous-session list, **Then** each session appears on its own line with a one-based number formatted as `[1]`, `[2]`, and so on at the beginning of the line.
2. **Given** sessions are listed in a defined order, **When** the list is displayed, **Then** numbering follows that displayed order without gaps or duplicates.
3. **Given** a user has no saved sessions, **When** they request the list, **Then** the existing empty-state response is shown and no numbered entries are displayed.

---

### User Story 2 - Recall by List Number (Priority: P1)

As a blog writer, I can enter the number shown beside a previous session to recall it without knowing or entering its session identifier.

**Why this priority**: Number-based recall is the core interaction change and removes the need for users to handle internal identifiers.

**Independent Test**: Display a list of saved sessions, select one by its displayed number, and verify that the selected session is loaded for continuation.

**Acceptance Scenarios**:

1. **Given** a numbered previous-session list is visible, **When** the user enters a valid displayed number, **Then** the corresponding session is recalled and its saved state is made available for continuation.
2. **Given** the user enters a number that is not present in the current list, **When** the recall request is processed, **Then** the user receives a clear invalid-selection message and no different session is loaded.
3. **Given** the user enters a non-numeric, empty, negative, or otherwise malformed selection, **When** the recall request is processed, **Then** the user receives a clear invalid-selection message and the current session remains unchanged.
4. **Given** the list has been refreshed since the user last viewed it, **When** the user selects a number, **Then** the number is resolved against the current displayed list and cannot silently map to a stale entry.

---

### User Story 3 - Preserve Existing Session Safety (Priority: P2)

As a blog writer, I can still use existing session commands and safely recover from invalid selections, so adding number-based recall does not disrupt established workflows.

**Why this priority**: The new shortcut should improve discovery without making existing sessions inaccessible or changing unrelated behavior.

**Independent Test**: Exercise the existing list, recall, and new-session flows before and after using number-based selection, then verify that unrelated commands retain their previous outcomes.

**Acceptance Scenarios**:

1. **Given** a user is in an active session, **When** they provide an invalid numbered selection, **Then** the active session is not replaced or modified.
2. **Given** a user chooses a listed session by number, **When** that session is recalled, **Then** the loaded session retains its original identifier and saved contents.
3. **Given** a user enters a raw session identifier instead of a displayed number, **When** the recall request is processed, **Then** the input is rejected with a clear numeric-selection message and no session is loaded.

### Edge Cases

- The displayed list is longer than a single digit; numbering continues as `[10]`, `[11]`, and so on without truncation or ambiguous parsing.
- A selection has leading or trailing whitespace; harmless surrounding whitespace is accepted, while the meaningful selection remains validated.
- The user enters a number larger than the list size or zero; no session is loaded and the response explains that the selection is unavailable.
- A listed session disappears or becomes unreadable before recall; the user receives a recoverable error and the active session is preserved.
- Two sessions have similar titles or questions; their distinct list numbers remain unique and selectable.
- The list is empty or cannot be loaded; the system does not invent a numbered choice.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST prefix each listed previous session with a unique, one-based numeric label enclosed in square brackets, beginning at the start of the line.
- **FR-002**: The system MUST assign labels in the same order as the sessions are displayed.
- **FR-003**: The system MUST allow a user to recall a listed session by entering its displayed numeric label.
- **FR-004**: The system MUST resolve a numeric selection against the current session list rather than deriving a session identifier from user input.
- **FR-005**: The system MUST reject selections that are empty, non-numeric, zero, negative, out of range, or otherwise malformed with a clear user-facing message.
- **FR-006**: The system MUST leave the active session unchanged when a selection is rejected or the selected session cannot be loaded.
- **FR-007**: The system MUST preserve the recalled session's existing identifier and saved contents after resolving it by number.
- **FR-008**: The system MUST accept only a valid number from the current displayed list for previous-session recall and MUST reject raw session identifiers entered by the user.
- **FR-009**: The system MUST handle numbering and selection consistently for lists containing at least 100 sessions without ambiguous labels.
- **FR-010**: The system MUST not display numbered entries when the previous-session list is empty.

### Key Entities *(include if feature involves data)*

- **Previous Session Entry**: A listed saved session with its display label, identifying summary, and reference to the underlying saved session.
- **Session Selection**: The user's numeric choice, validated and resolved against the current displayed entries.
- **Saved Session**: The existing persisted workflow state that is loaded after a successful selection.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In usability testing, users can identify and select a target from a list of 20 previous sessions using the displayed number without entering a session identifier in at least 95% of attempts.
- **SC-002**: At least 99% of valid selections from a current list load the session represented by the chosen number, with no selection mapped to a different listed session.
- **SC-003**: 100% of invalid, empty, negative, zero, and out-of-range selections leave the active session unchanged and produce an understandable response.
- **SC-004**: Every non-empty previous-session list has unique, gap-free labels beginning at `[1]`, and every label is visible at the start of its line.
- **SC-005**: Users can recall a listed session without knowing its session identifier, reducing the primary recall input to a numeric choice.

## Assumptions

- The existing previous-session list order remains the source of truth for numbering; this feature does not redefine sorting or retention.
- Numbered recall applies to the current displayed list and does not require storing new long-term data.
- Existing session ownership, authorization, persistence, and conflict behavior remain unchanged.
- The application continues to support its current interaction mode and user-facing command conventions.
- Session identifiers remain available internally for loading the selected record, but users recall previous sessions only through displayed numbers.
