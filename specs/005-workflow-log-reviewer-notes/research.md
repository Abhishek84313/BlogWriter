# Research: Workflow Log and Reviewer Notes

## Observe existing workflow lifecycle without changing topology

**Decision**: Add an optional typed output observer to the existing workflow/session-service path. Translate existing workflow executor lifecycle events into user-visible log updates, and publish reviewer-state updates at the reviewer executor boundary.

**Rationale**: `BlogWorkflow` already consumes a streaming workflow run and sees executor invoked, completed, failed, and final-output events. `BlogWriterSessionService` is the existing boundary used by the web workspace, so forwarding an optional observer exposes live output without raw HTTP, a new agent, a second workflow, or a persistence change.

**Alternatives considered**: Polling session storage would delay output and add unnecessary reads; writing directly to a browser channel from executors would violate the application/workflow boundary; replacing the workflow with a new streaming topology would increase risk and conflict with the constitution.

## Separate lifecycle log entries from reviewer feedback

**Decision**: Use a typed update that distinguishes workflow log events from reviewer feedback updates. The workspace appends lifecycle events to a chronological log collection and routes reviewer updates only to the Reviewer notes collection.

**Rationale**: The requested surfaces have different meaning and retention rules. A typed discriminant prevents review text from being rendered as status output and makes unit tests deterministic.

**Alternatives considered**: One untyped string stream would make routing and accessibility semantics ambiguous; duplicating reviewer content into both surfaces would violate the dedicated Reviewer notes requirement.

## Accumulate reviewer notes across revisions in one active session

**Decision**: Give each reviewer update an operation/revision identity and append only unseen updates to the circuit-scoped Reviewer notes state. Clear the accumulated view when New starts or a different saved session is loaded. Loading a saved session initializes the view from its persisted final `ReviewNotes`.

**Rationale**: This matches the accepted clarification, preserves review history across revisions, avoids duplicate delivery from late or repeated events, and keeps persisted sessions backward-compatible because transient history is not added to the schema.

**Alternatives considered**: Replacing the pane on every revision loses review history; persisting every incremental update changes the session schema and creates conflict/merge behavior that the feature does not require.

## Preserve stale-operation protection at the workspace boundary

**Decision**: Associate observer updates with the existing workspace operation version and ignore updates from superseded operations. The observer must not mutate the workspace after New, List, Quit, cancellation timeout, or a newer submission supersedes the operation.

**Rationale**: `BlogWorkspaceService` already uses versioning to suppress late final results. Applying the same guard to incremental updates prevents old feedback or log entries from leaking into a new session.

**Alternatives considered**: Cancelling alone is insufficient because a workflow may not stop within the configured timeout; clearing output without version checks still permits late events to repopulate it.

## Keep rendering text-safe and accessible

**Decision**: Render update content as text in a labeled, scrollable log region and Reviewer notes region. Use a polite live-update mechanism that announces new content without moving focus; preserve existing focusable pane content and keyboard order.

**Rationale**: Agent output is untrusted text and may contain markup-like characters. Text rendering prevents interpretation, while polite announcements preserve the user's reading position during long workflows.

**Alternatives considered**: Injecting HTML would create a content-safety risk; aggressive assertive announcements could interrupt typing and make repeated workflow updates hard to follow; replacing the whole pane makes assistive-technology context unstable.

## Validation strategy

**Decision**: Extend existing xUnit and bUnit tests, add focused workflow/session observer tests, and extend Playwright viewport/accessibility checks. Run the full core/web regression and compare MAF Doctor findings before and after.

**Rationale**: The feature crosses workflow, session, circuit state, rendering, and responsive accessibility boundaries. Focused tests isolate routing and stale-update rules; browser checks cover placement and live surfaces; the MAF comparison protects the unchanged agent topology.

**Alternatives considered**: Manual browser testing alone cannot reliably cover duplicate, cancellation, and stale-update paths; full end-to-end live-agent tests are too slow and nondeterministic for the core contract.
