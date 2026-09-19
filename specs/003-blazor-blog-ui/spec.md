# Feature Specification: Blog Writer Web Interface

**Feature Branch**: `003-blazor-blog-ui`

**Created**: 2026-09-19

**Status**: Draft

**Input**: User description: "Create a front end for this program in Blazor with separate draft and reviewer windows, prompt and revision inputs, and compact New, List, Revise, and Quit buttons. List shows a scrollable numbered Cosmos session list with a visible number input; Revise is enabled only for the displayed list, while New, List, and Quit are always enabled."

## Clarifications

### Session 2026-09-19

- Q: What should New, List, or Quit do when writing or revision work is still processing? → A: Cancel the active operation, wait for cancellation confirmation, then perform the selected action.
- Q: How should users submit text from the initial prompt and revision inputs? → A: Enter submits; Shift+Enter inserts a new line.
- Q: What should happen to unsaved prompt or revision text when the user chooses New, List, or Quit? → A: Confirm before discarding non-empty unsaved text.
- Q: Who is allowed to use the web interface and access saved sessions? → A: Require Microsoft Entra sign-in and show only sessions owned by the signed-in user.
- Q: What accessibility standard should the web interface meet? → A: WCAG 2.2 Level AA.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start New Writing Work (Priority: P1)

As a signed-in blog writer, I can clear the current workspace and enter a new writing prompt so I can begin a new draft without content from earlier work remaining visible.

**Why this priority**: Starting a new writing request is the primary workflow and must be immediately understandable.

**Independent Test**: Populate every workspace area, choose New, and verify that draft, review, prompt, revision, list, and selection state are cleared and the initial prompt is ready for input.

**Acceptance Scenarios**:

1. **Given** any content is visible in the workspace, **When** the user chooses New, **Then** the draft and review areas, both prompt inputs, any session list, and any session selection are cleared.
2. **Given** the cleared workspace, **When** the user enters and submits an initial prompt, **Then** a new writing session begins and its resulting draft and reviewer feedback appear in their respective areas.
3. **Given** a new request is processing, **When** the user views the workspace, **Then** the interface clearly indicates that work is in progress and prevents duplicate submission of the same prompt.
4. **Given** the initial prompt contains text, **When** the user presses Enter, **Then** the prompt is submitted; **When** the user presses Shift+Enter, **Then** a new line is inserted without submission.

---

### User Story 2 - Review and Refine a Draft (Priority: P1)

As a blog writer, I can view the draft beside the review feedback and submit a follow-up revision request so I can iteratively improve the work while retaining context.

**Why this priority**: Draft-review-revision is the central value of the existing writing workflow.

**Independent Test**: Load or create a session with draft and review content, submit a follow-up request, and verify that both areas update to the latest completed results while the session context is retained.

**Acceptance Scenarios**:

1. **Given** a writing run completes, **When** results are displayed, **Then** the draft appears in the draft area and reviewer feedback appears in the separate review area.
2. **Given** an active session has a draft, **When** the user enters and submits a follow-up revision request, **Then** the request applies to that session and updated draft and review content replace the prior displayed results.
3. **Given** the revision input is empty, **When** the user attempts to submit it, **Then** no revision starts and the existing draft and review remain unchanged.
4. **Given** the revision input contains text, **When** the user presses Enter, **Then** the revision is submitted; **When** the user presses Shift+Enter, **Then** a new line is inserted without submission.

---

### User Story 3 - Find and Resume Saved Work (Priority: P1)

As a blog writer, I can view a scrollable numbered list of saved sessions and select one by number so I can continue previous work without handling an internal identifier.

**Why this priority**: Persistent sessions are only useful when users can discover and resume them efficiently.

**Independent Test**: Provide multiple saved sessions, choose List, select a displayed number, choose Revise, and verify that the matching saved draft and reviewer feedback are loaded.

**Acceptance Scenarios**:

1. **Given** saved sessions exist, **When** the user chooses List, **Then** the draft area shows a scrollable list whose entries begin with unique one-based numbers in current saved-session order.
2. **Given** the saved-session list is displayed, **When** it becomes visible, **Then** a compact selection input appears next to the list and the Revise button becomes enabled.
3. **Given** a valid displayed number is entered, **When** the user chooses Revise, **Then** the corresponding saved session is loaded and its latest draft and reviewer feedback are displayed.
4. **Given** the selection is empty, malformed, or outside the displayed range, **When** the user chooses Revise, **Then** a clear validation message appears and the current workspace state is not replaced.
5. **Given** no saved sessions exist, **When** the user chooses List, **Then** the draft area shows a clear empty state, no numbered entries are invented, and Revise remains disabled.

---

### User Story 4 - Use Predictable Workspace Controls (Priority: P2)

As a blog writer, I can rely on consistent compact controls and clear enabled states so I always know which actions are available.

**Why this priority**: Predictable controls prevent accidental workflow transitions and make repeated use efficient.

**Independent Test**: Move through new, active-draft, list, invalid-selection, and ended-session states and verify each control's visibility, size, and enabled state.

**Acceptance Scenarios**:

1. **Given** the application is active, **When** any normal workspace state is displayed, **Then** New, List, and Quit are visible, compact, equal in size, and enabled.
2. **Given** the saved-session list is not displayed, **When** the user views the controls, **Then** Revise is disabled and the list-selection input is hidden.
3. **Given** the user chooses Quit, **When** the action completes, **Then** the active workspace session ends, no further writing action is accepted in that UI session, and the user receives clear confirmation.
4. **Given** either prompt input contains non-empty unsaved text, **When** the user chooses New, List, or Quit, **Then** the system asks for confirmation before discarding the text; cancelling the confirmation preserves the workspace unchanged.
5. **Given** both prompt inputs are empty, **When** the user chooses New, List, or Quit, **Then** the selected action proceeds without a discard confirmation.

### Edge Cases

- Saved-session retrieval fails; the current draft and review remain available, Revise stays disabled, and a recoverable error is shown.
- Authentication expires or the user is not signed in; writing and saved-session actions are unavailable until successful sign-in, and no other user's session data is displayed.
- A listed session is deleted or becomes unavailable before selection; no replacement session is loaded and the user can refresh the list.
- A long draft, long reviewer response, or long saved-session list remains independently scrollable without hiding the compact controls.
- Repeated List actions refresh the numbered list from the current saved sessions rather than duplicating entries.
- New, List, or Quit is chosen while a request is processing; the application requests cancellation, waits for confirmation, suppresses stale results, and then performs the selected action.
- The browser viewport is narrow; all inputs and controls remain usable without overlapping content or truncating button labels.
- A writing or revision request fails; the prior stable draft and reviewer feedback remain visible with a clear error.
- A discard confirmation is declined; no cancellation request or selected action proceeds, and unsaved text remains available.
- The interface is used without a pointing device or with assistive technology; all actions, inputs, dialogs, processing states, validation messages, and updated results remain operable and understandable.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a browser-based Blog Writer workspace delivered through Blazor.
- **FR-002**: The workspace MUST provide separate, simultaneously visible areas for the current draft and current reviewer feedback.
- **FR-003**: The workspace MUST provide a compact input for an initial writing prompt and a separate compact input for subsequent revision requests.
- **FR-004**: Submitting a non-empty initial prompt MUST start a new writing session and display the completed draft and reviewer feedback in their respective areas.
- **FR-005**: Submitting a non-empty revision request MUST continue the active session and replace the displayed draft and reviewer feedback with the latest completed results.
- **FR-006**: The workspace MUST provide New, List, Revise, and Quit buttons with the same compact dimensions.
- **FR-007**: New, List, and Quit MUST remain enabled in every active workspace state.
- **FR-008**: Choosing New MUST clear the draft, review, prompt, revision, saved-session list, validation messages, and selection state, then prepare the initial prompt for entry.
- **FR-009**: Choosing List MUST retrieve the current user's saved sessions from the existing long-term session store and display them as a scrollable, one-based numbered list in the draft area.
- **FR-010**: While a non-empty saved-session list is displayed, the system MUST show a compact number input adjacent to the list and enable Revise.
- **FR-011**: When a saved-session list is not displayed or is empty, the system MUST hide the number input and disable Revise.
- **FR-012**: Choosing Revise with a valid displayed number MUST load that saved session using the existing ownership rules and display its latest draft and reviewer feedback.
- **FR-013**: Choosing Revise with an invalid selection MUST preserve the current workspace state and show a clear validation message.
- **FR-014**: The saved-session list, draft area, and review area MUST support scrolling when their content exceeds the available visible area.
- **FR-015**: The system MUST prevent duplicate submissions while a writing or revision operation is in progress and clearly indicate processing state.
- **FR-016**: Choosing Quit MUST end the active UI session, reject further writing actions in that session, and present a clear ended-session state.
- **FR-017**: The frontend MUST preserve the existing session ownership, persistence, workflow termination, and token-budget behavior.
- **FR-018**: User-facing failures MUST preserve the most recent stable draft and reviewer feedback whenever those results remain valid.
- **FR-019**: If New, List, or Quit is chosen while writing or revision work is processing, the system MUST request cancellation, wait up to 10 seconds for confirmation, suppress all later results from the cancelled operation, and then perform the selected action even if cancellation confirmation times out.
- **FR-020**: In both prompt inputs, Enter MUST submit non-empty text and Shift+Enter MUST insert a new line without submitting.
- **FR-021**: Before New, List, or Quit discards non-empty unsaved prompt or revision text, the system MUST request confirmation; declining confirmation MUST preserve the workspace and take no further action.
- **FR-022**: The system MUST require Microsoft Entra sign-in before allowing writing, revision, listing, or saved-session selection actions.
- **FR-023**: The system MUST list and load only sessions owned by the signed-in user and MUST NOT expose another user's session summaries or content.
- **FR-024**: The web interface MUST conform to WCAG 2.2 Level AA, including keyboard operation, visible focus, programmatic labels, sufficient contrast, logical focus order, and announced processing, validation, error, and completion states.

### Key Entities *(include if feature involves data)*

- **Workspace State**: The current mode, prompt values, displayed draft, reviewer feedback, processing state, validation message, and whether the UI session has ended.
- **Saved Session Entry**: A numbered summary of a saved writing session available to the current user, including identifying prompt text and recency information.
- **Session Selection**: The user-entered display number resolved against the currently displayed saved-session list.
- **Writing Session**: The existing persisted workflow context containing the prompt, revisions, draft, review feedback, and ownership information.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 90% of first-time users can start a new writing session, view draft and review results, and submit one follow-up revision without assistance.
- **SC-002**: Users can locate and resume one of 20 saved sessions by displayed number in under 30 seconds in at least 95% of usability trials.
- **SC-003**: In 100% of tested workspace states, New, List, and Quit are enabled, while Revise is enabled only when a non-empty saved-session list is displayed.
- **SC-004**: In 100% of tested invalid-selection, retrieval-failure, and writing-failure cases, the most recent stable draft and reviewer feedback are not replaced with unrelated or partial content.
- **SC-005**: Drafts, reviewer feedback, and lists containing at least 20 entries remain fully accessible through scrolling without overlap or clipped controls at 390 × 844 and 1440 × 900 viewport sizes.
- **SC-006**: 100% of accepted initial prompts and revision requests produce at most one workflow submission per user action.
- **SC-007**: Automated and manual accessibility checks confirm WCAG 2.2 Level AA conformance for the primary new, revise, list, selection, confirmation, cancellation, and quit journeys.

## Assumptions

- Initial prompt and revision text use Enter to submit and Shift+Enter to insert a new line; the four requested buttons retain only their named responsibilities.
- Revise is the action for loading the saved session identified by the adjacent list number; subsequent revision text is submitted from the separate revision input.
- Quit ends the active application session and shows an ended state; a browser application is not expected to close the user's browser tab.
- Existing Microsoft Entra sign-in and ownership rules, Cosmos-backed persistence, session ordering and limits, and workflow behavior are reused without changing their business rules.
- The first version targets responsive browser use and does not require a separate native desktop or mobile application.
