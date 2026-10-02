# Data Model: Explicit Go Submission

## Existing Workspace State

No persisted entities change. The feature refines the following transient workspace state.

| Concept | Source | Validation and transitions |
|---|---|---|
| Initial prompt | Existing workspace input | Go can start a draft only when this text is non-whitespace and no revision request takes priority. |
| Revision request | Existing workspace input | A non-whitespace request takes priority when Go is activated. It is editable only when a non-whitespace draft is displayed and no operation is processing. |
| Displayed draft | Existing workspace output | Empty or whitespace-only draft disables revision input and the revision command; non-empty draft enables them when not processing. |
| Word range | Existing Min and Max inputs | Go uses existing range parsing and validation before either submission type begins. |
| Processing state | Existing workspace state | Prevents duplicate submissions and disables revision controls while an operation is active. |

## Submission Selection

1. A writer enters or changes prompt text; no processing starts.
2. A writer activates Go.
3. The workspace validates the current word range and existing processing eligibility.
4. If the revision request has non-whitespace content, the workspace submits the revision against the active session.
5. Otherwise, if the initial prompt has non-whitespace content, the workspace starts the draft workflow.
6. If neither input is eligible, the workspace starts no processing.

## Revision Availability

| Draft content | Processing | Revision input and command |
|---|---|---|
| Empty or whitespace-only | Any | Disabled |
| Non-whitespace | No | Enabled |
| Non-whitespace | Yes | Disabled |

Revision text is preserved across a transition to disabled state and becomes editable again when draft content returns.