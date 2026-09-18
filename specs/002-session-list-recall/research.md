# Research: Session List and Recall

## Decision

### Resolve numeric selections from the current displayed list

- **Decision**: When the user enters `resume <n>`, load the current previous-session summaries, validate `n` as a positive one-based index, and use that entry's existing session identifier for the normal session load.

### Rationale

The store already returns the display order and the list is capped at 20 entries. Resolving against a fresh list prevents a number from silently referring to a stale ordering and avoids exposing internal identifiers as the primary interaction.

### Alternatives considered

Persisting a number-to-session map would add state and stale-data risks; changing stored session identifiers would break compatibility; resolving against the last printed list would permit stale selections.

## Decision

### Use numeric-only resume

- **Decision**: Accept only `resume <number>` for previous-session recall. Raw session identifiers are rejected as user input.

### Rationale

The requested interaction replaces opaque identifiers with the visible list number. The underlying identifier remains an internal implementation detail used after selection.

### Alternatives considered

Keeping identifier recall would preserve an interaction the feature explicitly replaces and would leave two competing selection models.

## Decision

### Keep storage and workflow contracts unchanged

- **Decision**: Do not modify `IBlogSessionStore`, `BlogSessionSummary`, Cosmos queries, file persistence, authentication, or MAF workflow execution.

### Rationale

Numbering is a presentation and command-selection concern. The existing summaries already contain the identifier, task, and timestamps needed for display and resolution.

### Alternatives considered

Adding a persisted display index would duplicate transient ordering and create consistency problems.

## Decision

### Validate at the command boundary

- **Decision**: Reject empty, non-numeric, zero, negative, and out-of-range numeric selections before loading a session; preserve the active session on failure.

### Rationale

Validation belongs close to user input, gives deterministic messages, and prevents invalid input from reaching identifier-based storage APIs.

### Alternatives considered

Letting stores interpret numbers would mix presentation concerns into persistence and could produce inconsistent behavior across store implementations.

## Open design constraints resolved

- The current store order is newest-updated first and is the source of truth for labels.
- The current list is limited to 20 entries, while formatting and selection tests should also cover multi-digit labels.
- No new MAF symbol or agent call is needed, so no MAF API migration research is required for this feature.
