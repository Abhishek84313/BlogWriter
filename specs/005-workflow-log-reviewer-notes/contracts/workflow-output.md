# Workflow Output Contract

This contract describes the application-to-workspace output boundary for feature 005. It is an in-process contract; no public HTTP endpoint or persisted document shape is added.

## Observer Contract

The workflow/session boundary MAY receive an optional output observer for one operation. The observer receives typed `WorkflowOutputUpdate` values and does not control workflow routing, cancellation, persistence, or retry behavior.

Required behavior:

- The observer is invoked for lifecycle events that are meaningful to the workspace: operation started, executor progress, completion, success, cancellation, validation, conflict, and failure.
- Reviewer output is emitted as `ReviewerFeedback` updates, not as generic lifecycle log messages.
- Updates include operation identity and sequence information so the workspace can reject stale and duplicate delivery.
- Observer failures MUST NOT change workflow success/failure semantics or create an unbounded retry loop.
- Cancellation continues to use the existing cancellation token.

## Destination Rules

| Update kind | Destination | Required behavior |
| --- | --- | --- |
| Lifecycle | Workflow log beneath command bar | Append in chronological order with outcome semantics |
| ReviewerFeedback | Reviewer notes pane | Append in arrival order and retain across revisions for the active session |
| Validation | Workflow log | Append before any workflow call when submission input is invalid |
| Failure/Conflict | Workflow log | Append outcome and preserve already received reviewer notes |

## Workspace Reset Rules

- New accepted: clear workflow log and Reviewer notes.
- Different saved session loaded: replace transient output and seed Reviewer notes from the loaded session's stored final review text.
- Revision in the same active session: preserve existing Reviewer notes and append new feedback.
- Superseded operation: ignore all late updates from the old operation.
- Ended workspace: ignore late updates and preserve the ended-state output rules.

## Accessibility Rules

- The workflow log has a programmatic label and polite live-update semantics.
- Reviewer notes has a programmatic label and polite live-update semantics for appended content.
- Updates do not move keyboard focus.
- Long content is text-safe, readable, and scrollable without horizontal overflow at 390 × 844 and 1440 × 900.

## Compatibility Rules

- Existing command callbacks and session ownership remain unchanged.
- Existing final `BlogSession.State.ReviewNotes` remains authoritative when loading a saved session.
- Existing word-count, cancellation, conflict, and bounded revision behavior remains unchanged.
- No new hosted agent, model call, credential, token budget, Cosmos property, or deployment resource is required.
