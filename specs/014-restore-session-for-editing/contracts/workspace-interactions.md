# Workspace Interaction Contract

This is a browser interaction contract for the existing authenticated Blazor workspace. No external HTTP/API contract changes are introduced.

## List Saved Sessions

- List continues to display up to 20 current-owner session summaries in the existing newest-first order.
- Each session row is a keyboard-operable selection action with an accessible name identifying the session by its query summary and displayed position.
- The numeric selection input adjacent to List is removed.

## Restore a Selected Row

**Input**: The selected summary from the currently displayed list.

**Success**:

- Load the selected record through the existing owner-scoped session service.
- Restore its main query, optional saved revision request, and Min/Max values into the existing fields.
- Clear Draft and Reviewer output according to the established list-launch behavior.
- Keep the workspace inputs editable; return to an eligible pending state.
- Do not invoke the writing workflow and do not create or save a new record during selection.

**Unavailable or failure**: Show a recoverable message and preserve the current workspace; never load a different row or expose a record owned by another user.

## Submit the Restored Workspace

**Input**: User activates Go with a valid current query and word range.

**Success**:

- Start one new-session operation from the current query and Min/Max values.
- Create and save a distinct session-history entry using the existing session service/store.
- Leave the selected source record unchanged.
- Preserve the saved revision request for the normal revision flow once the new run has produced an active draft.

**Invalid or busy state**: Existing validation and duplicate-operation protections prevent an invalid or overlapping run and provide the current user-visible outcome.

## Accessibility and State

- Session rows expose button semantics, visible focus, and Enter/Space activation.
- During row loading, repeated selection is prevented and the UI exposes the existing busy state.
- Selection alone never displays workflow-started or run-completed feedback.
- Existing keyboard navigation, accessible labels, responsive layout, and authenticated ownership behavior are preserved.
