# Feature Specification: Single Review Round

**Feature Branch**: `013-single-review-round`

**Created**: 2026-09-27

**Status**: Draft

**Input**: User description: "At most one round of revision should occur. The reviewer either accepts the initial draft (in which case it is displayed and the round ends) or the reviewer rejects the draft and sends it back to the author with suggested changes. In that event, the author revises the draft and it is immediately displayed -- it does not go back to the reviewer for a second review."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Display an accepted initial draft (Priority: P1)

As a blog writer, I want an initially generated draft that passes review to be displayed immediately so I can read the completed result without unnecessary additional processing.

**Why this priority**: Approval is the shortest successful path and must remain reliable for every accepted draft.

**Independent Test**: Start a writing workflow with a reviewer that accepts the initial draft and verify that the initial draft is displayed and no revision is requested.

**Acceptance Scenarios**:

1. **Given** an initial draft has been generated, **When** the reviewer accepts it, **Then** the system displays that initial draft as the final result.
2. **Given** the reviewer accepts the initial draft, **When** the workflow completes, **Then** no author revision is requested and no additional review occurs.

---

### User Story 2 - Display one revision after rejection (Priority: P1)

As a blog writer, I want reviewer feedback to produce one revised draft so that clear improvements are incorporated without creating an open-ended review loop.

**Why this priority**: A rejected initial draft needs one opportunity to incorporate actionable feedback while the workflow remains bounded and predictable.

**Independent Test**: Start a writing workflow with a reviewer that rejects the initial draft and supplies suggested changes, then verify that the author receives the feedback once and the revised draft is displayed without a second review.

**Acceptance Scenarios**:

1. **Given** an initial draft has been generated, **When** the reviewer rejects it with suggested changes, **Then** the author receives the rejected draft and the suggestions for one revision.
2. **Given** the author receives the review suggestions, **When** the author completes the revision, **Then** the revised draft is displayed as the final result.
3. **Given** the revised draft is displayed, **When** the workflow completes, **Then** the reviewer is not invoked again.

---

### Edge Cases

- If the reviewer rejects the initial draft without usable suggestions, the author still receives one revision opportunity and the workflow remains bounded.
- If the author cannot produce a different draft after rejection, the latest available draft is displayed and the workflow ends without another review attempt.
- A reviewer approval on the initial draft must not consume or trigger a revision round.
- A rejected initial draft must never cause more than one author revision.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST submit the initial draft to the reviewer exactly once before deciding whether a revision is needed.
- **FR-002**: When the reviewer accepts the initial draft, the system MUST display that draft as the final result and end the workflow.
- **FR-003**: When the reviewer rejects the initial draft, the system MUST provide the author's next revision with the reviewer's suggested changes.
- **FR-004**: The system MUST allow no more than one author revision after rejection of the initial draft.
- **FR-005**: After the single revision is produced, the system MUST display the revised draft as the final result without submitting it for a second review.
- **FR-006**: The workflow MUST terminate after either initial approval or display of the single revised draft.
- **FR-007**: The system MUST preserve the latest available draft for display when a requested revision cannot produce replacement content.
- **FR-008**: The system MUST make the completed result and workflow termination observable to the user through the existing result and status presentation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of workflows with an accepted initial draft display that draft and invoke no revision.
- **SC-002**: 100% of workflows with a rejected initial draft invoke at most one author revision and at most one review.
- **SC-003**: 100% of workflows display a final draft after either initial approval or the single revision attempt, including when replacement content is unavailable.
- **SC-004**: In usability checks, users can identify the final displayed draft and completion state on the first result view in at least 95% of attempts.
- **SC-005**: No workflow remains active or requests further review after the initial approval or single revision result is displayed.

## Assumptions

- The existing reviewer determines acceptance or rejection and provides textual suggestions when rejecting a draft.
- "One round of revision" means at most one author revision after review of the initial draft; it does not include generating the initial draft.
- The revised draft is considered the final displayed result without a second reviewer decision, as explicitly requested.
- Existing workflow cancellation, error reporting, token-budget, and session behavior remain unchanged except where needed to terminate the review loop.
- This feature changes review-loop bounds and result presentation only; it does not change research, initial drafting, or reviewer evaluation criteria.
