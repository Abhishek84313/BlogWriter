# Data Model: Long-Term Session Memory

## Saved Session

One Cosmos document represents a single blog-writing conversation.

| Field | Type | Rules |
|-------|------|-------|
| `id` | String | Existing 32-character session identifier; unique within its owner partition. |
| `ownerId` | String | Immutable Microsoft Entra object ID; partition key; every operation supplies this value. |
| `schemaVersion` | Integer | Starts at `1`; permits future compatible document evolution. |
| `createdAt` | Timestamp | Set only when the session is created. |
| `updatedAt` | Timestamp | Updated after a save, including accepted follow-ups and workflow completion. |
| `state` | Object | Serialized current `ResearchState`, including original task, refinements, research, draft, review, revision state, and routing state. |
| `_etag` | String | Store-managed concurrency version retained on load and used for replacement. |

## Session Summary

The list operation returns a compact projection rather than loading full session state.

| Field | Source | Purpose |
|-------|--------|---------|
| `id` | Saved Session `id` | Input to resume. |
| `mainTask` | Saved Session `state.mainTask` | Identifies the previous question. |
| `createdAt` | Saved Session `createdAt` | Displays age and disambiguates similar questions. |
| `updatedAt` | Saved Session `updatedAt` | Sorts newest first and displays recency. |

## Relationships and Transitions

- One Entra user owns zero or more saved sessions.
- One saved session has one workflow state and zero or more follow-up refinements embedded in that state.
- `Create`: generate `id`, attach the current owner ID, initialize timestamps and ETag.
- `Load`: read by `(ownerId, id)`; absence returns not found.
- `Save`: replace using the loaded ETag; on mismatch return a conflict result and retain the stored document unchanged.
- `List`: filter by `ownerId`, sort `updatedAt` descending, and return at most 20 summaries.
- `Account deletion`: remove all documents belonging to the deleted owner through the account-lifecycle integration specified in implementation tasks.

## Validation Rules

- A session ID remains a 32-character hexadecimal GUID.
- Owner ID, original task, and workflow state are required before creation or save.
- The store rejects documents with an unsupported schema version or invalid required state and reports a recoverable error.
- A caller cannot read, list, or overwrite documents outside its owner partition.