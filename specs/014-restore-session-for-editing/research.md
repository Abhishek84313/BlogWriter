# Research: Restore Saved Session for Editing

## Decisions

### Restore through the existing owner-scoped session service

- **Decision**: A list row passes its selected `BlogSessionSummary` to the workspace. The workspace loads the record through `IBlogWriterSessionService.LoadAsync` and never derives an internal ID from user-entered text.
- **Rationale**: The existing service and store enforce current-owner access. The displayed list is already capped at 20 and ordered newest first.
- **Alternatives considered**: Read Cosmos directly from the component or continue resolving a numeric input. Direct store access would bypass the workspace/service boundary; numeric entry is explicitly removed by the feature.

### Treat selection as a pending, editable new run

- **Decision**: Selection restores the saved `MainTask`, `CurrentSubTask`, and word range; clears prior Draft and Reviewer output as the established list-launch behavior does; and returns to an editable workspace without invoking the workflow.
- **Rationale**: This preserves established restoration semantics while separating selection from the user's explicit Go action. Existing range defaults and validation remain authoritative.
- **Alternatives considered**: Calling the workflow during restore or making the loaded record the active session would violate the explicit-Go and non-overwrite requirements.

### Create a separate saved run using the existing new-session path

- **Decision**: Go from a restored pending workspace uses `IBlogWriterSessionService.StartAsync`, not `ReviseAsync` on the selected source session.
- **Rationale**: `StartAsync` calls `IBlogSessionStore.CreateAsync`, which assigns a fresh session ID and owner, runs the existing workflow, and saves the result. `ReviseAsync` preserves the supplied session ID and replaces that record, so it is not appropriate for this flow.
- **Alternatives considered**: Reusing the selected session's ID would overwrite history; adding a new Cosmos-specific write path would duplicate existing persistence behavior.

### Keep persistence and schema unchanged

- **Decision**: Use the existing `IBlogSessionStore` and Cosmos implementation without adding fields, containers, or queries.
- **Rationale**: Session documents already contain the query context and word range. `CreateAsync` writes a fresh owner-partitioned document; the existing store handles ownership, ETags, and timestamps. The shared `CosmosClient` remains reused.
- **Alternatives considered**: A schema migration or direct Cosmos operation is unnecessary for a new record using existing fields.

### Validate with local service and browser-component tests

- **Decision**: Cover the selection/no-run boundary and new-session submission in `BlogWorkspaceServiceTests`, and cover the clickable list and removal of the numeric field in bUnit browser tests. Keep Cosmos behavior behind existing service/store interfaces.
- **Rationale**: These tests exercise the changed user flow without requiring live Foundry or Cosmos resources.
- **Alternatives considered**: A live cloud test is not needed to verify the UI/service contract and would make focused validation environment-dependent.

## Integration Notes

- The existing first-run path consumes the restored main query and Min/Max values. A saved non-empty `CurrentSubTask` remains in the Revision request field for the normal revision flow after the new run has produced a draft; selection itself does not invoke a revision.
- Existing Entra identity, token-budget middleware, cancellation, workflow-output publication, and session ownership remain in force.
- The MAF Doctor baseline is grade B with cost-cap heuristics; the feature does not modify agent call sites, and those findings are outside this plan's scope.
