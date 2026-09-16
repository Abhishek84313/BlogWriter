# Feature Specification: Long-Term Session Memory

**Feature Branch**: `001-cosmos-session-memory`

**Created**: 2026-09-16

**Status**: Draft

**Input**: User description: "Add long term memory using Cosmos so that we can store what is currently in the session to allow listing of previous questions and allow followup"

## Clarifications

### Session 2026-09-16

- Q: How should the application identify the owner of each saved session? -> A: Use the signed-in Microsoft Entra ID user identity.
- Q: What should happen to saved sessions when a user leaves the organization or their Entra account is deleted? -> A: Automatically delete sessions when the account is deleted.
- Q: How should the application behave when two copies of the same saved session are updated at nearly the same time? -> A: Reject the later conflicting save and require reload.
- Q: Should the session list be limited to the 100 most recently updated sessions, as currently specified? -> A: List the latest 20 sessions.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find Previous Questions (Priority: P1)

As a blog writer, I can list previous writing questions so I can identify the conversation I want to continue without retaining its session identifier.

**Why this priority**: Discovering prior work is required before a user can meaningfully use long-term memory.

**Independent Test**: Create multiple sessions with distinct questions, request the session list, and verify that each question is shown with enough identifying context to select it.

**Acceptance Scenarios**:

1. **Given** a user has completed or paused multiple writing sessions, **When** they request previous questions, **Then** they receive a list containing each session's original question, session identifier, and most recent update time.
2. **Given** a user has no prior sessions, **When** they request previous questions, **Then** they receive an empty-state response that clearly indicates no saved sessions are available.
3. **Given** sessions have different update times, **When** they request previous questions, **Then** the most recently updated session appears first.

---

### User Story 2 - Resume Saved Work (Priority: P1)

As a blog writer, I can resume a previously listed session so I can continue working from its saved question, research, draft, and review progress.

**Why this priority**: The primary value of persistent memory is preserving work across application restarts and allowing it to continue intact.

**Independent Test**: Save a session containing a question and in-progress workflow results, restart the application, resume it using the listed session identifier, and verify the prior state is available to the next workflow run.

**Acceptance Scenarios**:

1. **Given** a saved session is selected, **When** the user resumes it, **Then** the workflow continues with the session's saved state rather than a new blank session.
2. **Given** a user enters an unknown or malformed session identifier, **When** they try to resume it, **Then** they receive a clear not-found response and no new session is created.
3. **Given** a saved session has been updated by a follow-up, **When** it is resumed, **Then** its latest saved state is used.

---

### User Story 3 - Preserve Follow-Up Context (Priority: P2)

As a blog writer, I can submit a follow-up after resuming a session so the new request builds on the original question and previously saved context.

**Why this priority**: A resumed session is useful only when its conversational context remains available for continued refinement.

**Independent Test**: Save a session with an original question and follow-up history, resume it, submit another follow-up, and verify the resulting session retains the original question and the complete refinement history.

**Acceptance Scenarios**:

1. **Given** a resumed session has an original question and prior follow-ups, **When** the user submits a new follow-up, **Then** the new request is recorded alongside the existing session context.
2. **Given** a follow-up workflow completes, **When** the user lists previous questions, **Then** that session's update time reflects the completed follow-up and affects its list order.

### Edge Cases

- A saved session is missing required state or cannot be read; the user receives a clear recoverable error and other sessions remain available for listing.
- The long-term store is temporarily unavailable while saving, listing, or resuming; the operation reports failure without silently discarding or replacing the current session state.
- Multiple sessions share the same original question; each remains separately selectable by its unique session identifier and timestamp.
- A previous question is lengthy; its listing remains identifiable without losing access to the full original question when resumed.
- A user's Entra account is deleted; all of that user's saved sessions are removed and cannot be listed or resumed.
- Two copies of the same session are updated concurrently; the later conflicting save is rejected, and the user is directed to reload the latest saved session before continuing.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST persist every newly created session and every subsequent update so that the latest workflow state remains available after the application stops and starts.
- **FR-002**: The persisted session state MUST include the original question, follow-up history, workflow progress, research findings, draft, review feedback, revision status, creation time, and latest update time.
- **FR-003**: The system MUST allow users to retrieve a saved session by its unique session identifier.
- **FR-004**: The system MUST allow users to list their 20 most recently updated saved sessions, showing each session's unique identifier, original question, creation time, and latest update time.
- **FR-005**: The system MUST present listed sessions in descending order by latest update time.
- **FR-006**: The system MUST allow a user to resume a listed session and use its latest saved state for continued workflow processing.
- **FR-007**: The system MUST preserve the original question and prior follow-up context when a user adds a follow-up to a resumed session.
- **FR-008**: The system MUST update the saved session after a follow-up is accepted and after the resulting workflow completes.
- **FR-009**: The system MUST report clear, user-actionable outcomes when a requested session does not exist, its saved data is invalid, or long-term storage is unavailable.
- **FR-010**: The system MUST identify each saved session by its signed-in Microsoft Entra ID user and ensure that one user's saved sessions are not returned to another user.
- **FR-011**: The system MUST automatically delete all saved sessions owned by a user when that user's Microsoft Entra ID account is deleted.
- **FR-012**: The system MUST detect concurrent updates to the same saved session, reject a conflicting later save without overwriting the latest stored state, and direct the user to reload before retrying.

### Key Entities *(include if feature involves data)*

- **Saved Session**: A durable record of one blog-writing conversation, identified uniquely and containing its creation and latest-update times plus its complete workflow state.
- **Question**: The original writing request that identifies a session in listings and anchors later follow-ups.
- **Follow-Up**: A refinement request associated with a saved session and retained in the session's historical context.
- **Session Listing**: An ordered collection of saved-session summaries used to identify and select previous work.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can find and select any one of their 20 most recently updated saved sessions from the session list in under 30 seconds.
- **SC-002**: 100% of successfully saved sessions can be resumed after an application restart with their original question, follow-up history, and latest workflow state intact.
- **SC-003**: In a representative set of 100 resume attempts for existing sessions, at least 99 complete without loss or substitution of saved session data.
- **SC-004**: At least 90% of representative users can find a previous question and submit a follow-up on their first attempt without needing to enter a session identifier manually.

## Assumptions

- Saved sessions are retained long term while their owner has an active Microsoft Entra ID account; account deletion removes the owner's saved sessions.
- The signed-in Microsoft Entra ID user identity is available when sessions are created, listed, and resumed.
- Session listings initially cover the user's 20 most recently updated sessions; searching, filtering, deletion, and sharing are out of scope.
- The feature preserves the current session state model and existing bounded workflow behavior.
- Access to the managed long-term session store is configured outside of source code and follows the project's identity and secret-management policies.