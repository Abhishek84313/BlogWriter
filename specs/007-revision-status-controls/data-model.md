# Data Model: Revision Status Controls

No persisted schema change is required. The feature adds transient workspace projections for revision availability and the latest workflow status.

## Revision Availability State

| Field | Type | Rules |
| --- | --- | --- |
| RevisionInputEnabled | Boolean | True after New or when a draft/session is displayed, unless processing is active |
| ReviseActionEnabled | Boolean | True only when a draft/session exists and processing is inactive |
| HasDraft | Boolean | Derived from non-empty displayed Draft |
| HasNewRevisionContext | Boolean | True for a New workspace ready for input; does not by itself enable Revise |
| IsProcessing | Boolean | Existing processing guard; disables both revision input and action |

## Current Workflow Status

| Field | Type | Rules |
| --- | --- | --- |
| Message | String or null | Newest accepted lifecycle/validation/status text; rendered as safe text |
| Outcome | Workflow output outcome or null | Corresponds to the newest lifecycle update |
| IsLive | Boolean | Exposed through an accessible polite live region |

Only one status message is rendered. New accepted lifecycle updates replace the prior
message. The internal update source may remain ordered for service behavior, but the UI
must not render the collection as a scrolling list.

## Compact Command Bar

| State | Layout |
| --- | --- |
| List selector hidden | New, List, Revise, Quit, and Help remain grouped together |
| List selector visible | List and selector retain their relationship; Revise, Quit, and Help move right into the remaining command group |
| Processing | Existing disabled command/input behavior remains in force |

## State Transitions

| From | Event | Result |
| --- | --- | --- |
| New | Initialize after accepted New | RevisionInputEnabled true; ReviseActionEnabled false; status unchanged/cleared per existing reset |
| New | Draft completes | RevisionInputEnabled true; ReviseActionEnabled true; latest status shown |
| Draft/session | Revision starts | Revision input and action disabled while processing |
| Processing | Lifecycle update | Replace Current Workflow Status with newest message |
| Any active state | New/List/Quit | Preserve existing discard confirmation and cancellation behavior |
| Any state | Reviewer update | Reviewer notes change independently; Current Workflow Status is not used as review content |

## Validation Rules

- Status messages are rendered as text, never markup.
- Empty status uses a compact accessible empty state.
- Revision request cannot submit while processing.
- Revise cannot submit without a draft/session.
- Command layout must not introduce horizontal overflow at 390 × 844 or 1440 × 900.
