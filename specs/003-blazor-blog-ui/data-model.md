# Data Model: Blog Writer Web Interface

No Cosmos document schema changes are required. The feature adds transient web workspace state and an owner-provider abstraction around existing saved sessions.

## Workspace State

One scoped instance exists per authenticated Blazor circuit.

| Field | Type | Rules |
| --- | --- | --- |
| Mode | `New`, `Draft`, `List`, or `Ended` | Determines visible content and allowed actions |
| InitialPrompt | String | Unsaved until accepted; Enter submits, Shift+Enter adds a line |
| RevisionPrompt | String | Applies only to an active writing session; same keyboard rules |
| Draft | String | Last stable completed draft; preserved on failures and cancelled runs |
| Review | String | Last stable completed reviewer output; preserved with Draft |
| ActiveSession | Existing `BlogSession` or null | Owned by the signed-in user; never stored in singleton state |
| DisplayedSessions | Ordered list of Saved Session Entry | Populated only by List; maximum 20 from existing store behavior |
| SelectionInput | String | Valid only while a non-empty list is displayed |
| IsProcessing | Boolean | Allows at most one active workflow operation per circuit |
| ValidationMessage | String or null | User-facing input or availability problem |
| PendingAction | `New`, `List`, `Quit`, or null | Action awaiting discard confirmation or cancellation completion |
| OperationVersion | Monotonic integer | Late results apply only when their version still matches |
| CancellationDeadline | Timestamp or null | Ten seconds after cancellation is requested; transition proceeds when confirmation arrives or this deadline passes |

## Saved Session Entry

A transient presentation projection over existing `BlogSessionSummary`.

| Field | Type | Rules |
| --- | --- | --- |
| DisplayNumber | Positive integer | One-based, gap-free, assigned in returned order |
| SessionId | Existing internal identifier | Never accepted or displayed as user selection input |
| MainTask | String | Identifies the saved writing request |
| CreatedAt | Timestamp | Existing persisted value |
| UpdatedAt | Timestamp | Existing persisted value; current list is newest first |

## Session Owner

| Field | Type | Rules |
| --- | --- | --- |
| ObjectId | Entra object ID | Required `oid` claim; immutable for the circuit; used as Cosmos partition owner |
| IsAuthenticated | Boolean | Writing and saved-session operations require true |

The session owner is distinct from the credential used by the server to access Cosmos and Foundry.

## Relationships

- One authenticated session owner has zero or more existing saved writing sessions.
- One workspace state has zero or one active writing session.
- One workspace state has zero to 20 displayed saved-session entries.
- One display number resolves to exactly one entry in the current displayed list.
- Draft and Review always represent the same last stable workflow completion.

## State Transitions

| From | Event | To | Required behavior |
| --- | --- | --- | --- |
| Any active mode | New | New | Confirm unsaved text; cancel and wait up to 10 seconds; suppress late results; clear workspace |
| New | Submit initial prompt | Draft | Create session, run workflow, save, then atomically publish Draft and Review |
| Draft | Submit revision prompt | Draft | Start follow-up, run workflow, save, then atomically replace Draft and Review |
| Any active mode | List | List | Confirm unsaved text; cancel and wait up to 10 seconds; suppress late results; load owner summaries; show selection only when non-empty |
| List | Revise valid number | Draft | Resolve current entry, load owner session, show its Draft and Review |
| List | Revise invalid number | List | Preserve current state and show validation message |
| Any active mode | Quit | Ended | Confirm unsaved text; cancel and wait up to 10 seconds; suppress late results; clear sensitive circuit state; reject further actions |
| Processing | Operation fails | Prior stable mode | Preserve stable Draft/Review and show recoverable error |
| Any active mode | Authentication lost | Signed-out state | Block operations, clear owner-bound transient state, require sign-in |

## Validation Rules

- Initial and revision prompts must contain non-whitespace text before submission.
- Revision submission requires an active session.
- Revise is enabled only when `Mode` is `List` and `DisplayedSessions` is non-empty.
- Selection input must be a positive integer within the current displayed list.
- Only the current operation version may update displayed state.
- Unsaved non-empty input requires confirmation before New, List, or Quit.
- All owner-scoped operations require an authenticated `oid` claim.
