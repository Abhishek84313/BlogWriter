# Data Model: Workflow Log and Reviewer Notes

No persisted schema change is required. Workflow log history and incremental reviewer feedback are transient workspace output. A loaded saved session seeds Reviewer notes from the existing persisted `ResearchState.ReviewNotes` value.

## Workflow Output Update

A typed update emitted by the workflow/session boundary.

| Field | Type | Rules |
| --- | --- | --- |
| Kind | Lifecycle or ReviewerFeedback | Determines the destination surface |
| OperationVersion | Long integer | Identifies the workspace operation that produced the update |
| Sequence | Long integer | Monotonically increasing within one operation; preserves arrival order |
| Message | String | Required user-visible text; rendered as text, never markup |
| Outcome | Progress, Success, Cancellation, Validation, Conflict, or Failure | Required for lifecycle entries; optional/Review for reviewer updates |
| RevisionNumber | Integer or null | Set for reviewer feedback associated with a workflow revision |
| UpdateKey | String | Stable identity used to ignore duplicate delivery |

## Workflow Log Entry

A lifecycle-oriented projection of a `WorkflowOutputUpdate`.

| Field | Type | Rules |
| --- | --- | --- |
| Message | String | Preserved exactly as user-visible text |
| Outcome | Log outcome | Controls styling and accessible status semantics |
| Sequence | Long integer | Ordered ascending |
| OperationVersion | Long integer | Must match the current workspace operation before append |

The log contains progress, success, cancellation, validation, conflict, and failure messages. It is not persisted with the saved blog session.

## Reviewer Feedback Update

A reviewer-oriented projection of a `WorkflowOutputUpdate`.

| Field | Type | Rules |
| --- | --- | --- |
| Message | String | Reviewer output rendered as text |
| RevisionNumber | Integer | Identifies the review pass when available |
| Sequence | Long integer | Ordered by arrival within the active workspace session |
| UpdateKey | String | Duplicate updates with the same key are ignored |

## Workspace Output State

Circuit-scoped state owned by `BlogWorkspaceState`.

| Field | Type | Rules |
| --- | --- | --- |
| WorkflowLog | Ordered read-only collection | Appends current-operation lifecycle entries; cleared/replaced with workspace transitions |
| ReviewerNotes | Ordered read-only collection | Appends unseen feedback across revisions; seeded from a loaded session's final review |
| CurrentOperationVersion | Long integer | Existing workspace version; rejects late updates |
| IsProcessing | Boolean | Existing processing state; output updates do not change command semantics |

## State Transitions

| From | Event | Result |
| --- | --- | --- |
| New workspace | Initialize | Empty workflow log and Reviewer notes empty state |
| Any active mode | Initial submission starts | Append progress/start entry; begin accepting updates for a new operation version |
| Processing | Lifecycle event | Append one ordered log entry if operation version is current |
| Processing | Reviewer feedback update | Append unseen update to Reviewer notes if operation version is current |
| Processing | Success | Append success entry; publish final draft; retain accumulated Reviewer notes |
| Processing | Cancellation/failure | Append outcome entry; retain feedback already received |
| Active session | Revision starts | Preserve existing Reviewer notes; begin accepting new reviewer updates |
| Any active mode | New accepted | Clear log and Reviewer notes, then reset workspace |
| List mode | Different session loaded | Replace log according to existing load behavior and seed Reviewer notes from the loaded session's persisted review |
| Any active mode | Superseded/late update | Ignore update; do not mutate current output |
| Ended workspace | Any late update | Ignore update |

## Validation Rules

- Every update has a non-empty user-visible message and a destination kind.
- Only the current operation version may mutate workspace output.
- Reviewer updates with an already-seen `UpdateKey` are ignored.
- Log and Reviewer notes content is rendered as text.
- Collection changes notify the existing workspace event so the UI renders without a manual refresh.
- Transient output is not written to Cosmos and does not alter session ownership or ETag behavior.
