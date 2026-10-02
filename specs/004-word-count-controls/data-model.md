# Data Model: Word Count Controls

No persisted schema change is required. Existing writing sessions already store minimum and maximum targets in their workflow state.

## Word Range

| Field | Type | Rules |
| --- | --- | --- |
| Min | Positive whole number | Required; defaults to 1000 |
| Max | Positive whole number | Required; defaults to 2000; must be greater than or equal to Min |

A Word Range is immutable once accepted for a submission.

## Editable Word Range

| Field | Type | Rules |
| --- | --- | --- |
| MinInput | String | Preserves the user's current text, including invalid or incomplete input |
| MaxInput | String | Preserves the user's current text, including invalid or incomplete input |
| MinError | String or null | Identifies empty, non-whole, zero, or negative Min input |
| MaxError | String or null | Identifies empty, non-whole, zero, negative, or Max-less-than-Min input |

## Accepted Workspace Range

| Field | Type | Rules |
| --- | --- | --- |
| AcceptedRange | Word Range | Defaults to 1000/2000; changes only after successful submission or saved-session load |
| HasUnsavedRange | Boolean | True when parsed visible values differ from AcceptedRange or either visible value is invalid |

## Accepted Submission Range

One immutable snapshot captured before a writing operation begins.

| Field | Type | Rules |
| --- | --- | --- |
| Range | Word Range | Validated before workflow execution |
| OperationVersion | Integer | Associates the snapshot with the existing workspace operation version |

## Relationships

- One workspace has one Editable Word Range and one Accepted Workspace Range.
- One accepted initial or revision operation has one Accepted Submission Range.
- One active or saved Writing Session has one persisted Word Range through its existing workflow state.
- A loaded Writing Session replaces both editable and accepted workspace ranges with its stored range.

## State Transitions

| From | Event | Result |
| --- | --- | --- |
| New workspace | Initialize | Inputs `1000` and `2000`; accepted baseline 1000/2000 |
| Any workspace | Edit Min or Max | Preserve input text; recompute validation and unsaved status |
| Valid editable range | Submit initial prompt | Capture snapshot and pass it to new-session operation |
| Valid editable range | Submit revision | Capture snapshot and pass it to revision operation |
| Processing | Edit Min or Max | Keep submitted snapshot unchanged; retain later edits for next operation |
| Processing | Successful completion | Persist snapshot; make it accepted baseline; preserve later visible edits if present |
| Processing | Failure/cancellation | Keep prior accepted baseline and visible input unchanged |
| List | Load session | Set inputs and accepted baseline from stored session range or defaults |
| Any active mode | New | Reset inputs and accepted baseline to 1000/2000 |
| Unsaved range | New/List/Quit | Require existing discard confirmation; decline preserves all state |

## Validation Rules

- Trim surrounding whitespace before parsing.
- Min and Max must each parse as a positive whole number.
- Max must be greater than or equal to Min.
- Equal values are valid.
- No product-specific upper bound is added; values remain constrained by the existing integer representation.
- Invalid ranges block initial and revision workflow calls.
- Correcting the values clears obsolete range errors before submission.
