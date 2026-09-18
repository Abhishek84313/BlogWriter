# Data Model: Session List and Recall

This feature introduces no persisted entities or schema changes. It defines the transient values used to display and resolve a session selection.

## Previous Session Entry

Represents one item in the current displayed list.

| Field | Type | Rules |
|---|---|---|
| DisplayNumber | positive integer | One-based, gap-free, assigned in displayed order |
| SessionId | existing session identifier | Preserved from `BlogSessionSummary`; used internally for loading |
| MainTask | existing session summary text | Displayed as the identifying session description |
| CreatedAt | existing timestamp | Displayed without changing its value |
| UpdatedAt | existing timestamp | Determines existing list order and is displayed without changing its value |

## Session Selection

Represents the raw value supplied after `resume`.

| Field | Type | Rules |
|---|---|---|
| RawValue | string | Trim surrounding whitespace before classification |
| SelectionKind | numeric index | Only numeric values use list resolution; raw identifiers are rejected |
| DisplayNumber | positive integer, when numeric | Must be greater than zero and no greater than the current list count |
| ResolvedSessionId | existing identifier, when valid | Taken from the current list entry; never generated from the number |

## State Transitions

1. `list` loads summaries and assigns display numbers in returned order.
2. `resume <n>` loads a fresh summary list, validates the number, and resolves it to a session identifier.
3. A resolved identifier enters the existing session load path.
4. Invalid selection or failed load leaves the current active session unchanged.
5. A raw session identifier entered after `resume` is rejected as an invalid selection.

## Validation Rules

- Empty, zero, negative, non-numeric, and out-of-range numeric values are rejected.
- Labels are formatted as `[n]` at the beginning of each displayed session entry.
- Multi-digit labels are treated as complete integers and are not truncated.
- The selected session identifier remains internal and subject to existing ownership and availability checks.
