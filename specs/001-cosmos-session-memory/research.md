# Research: Long-Term Session Memory

## Decision: Use Azure Cosmos DB for NoSQL with one session document per session

**Rationale**: A session and its `ResearchState` are read and written together when the workflow completes or accepts a follow-up. Embedding that state in one document preserves the existing model and avoids distributed writes.

**Alternatives considered**:

- Keep local JSON files: does not satisfy cross-restart, user-scoped durable cloud storage or session discovery requirements.
- Split session metadata from workflow state: adds coordination and recovery complexity without benefiting the fixed 20-item listing.

## Decision: Partition session documents by signed-in Microsoft Entra object ID

**Rationale**: The required list and resume paths are always scoped to one user. This makes user isolation explicit and lets the list query avoid cross-partition scans.

**Alternatives considered**:

- Partition by session ID: supports direct reads but makes per-user listings cross-partition.
- A shared partition: risks a hot partition and does not align with the ownership query pattern.

## Decision: Use an Entra access-token object ID as session owner identity

**Rationale**: The console already authenticates through Microsoft Entra. The Cosmos data-plane access token includes the authenticated object ID, which becomes the immutable owner value stored on each session and applied to every read and list operation.

**Alternatives considered**:

- A typed profile name: is mutable and cannot prove tenant identity.
- A shared application identity: cannot identify individual session owners.

## Decision: Use SDK optimistic concurrency with ETags

**Rationale**: A loaded session carries its version. Replacement writes use that version so a stale copy is rejected rather than overwriting a more recent follow-up or workflow result.

**Alternatives considered**:

- Last-write-wins: violates the confirmed conflict requirement by silently discarding data.
- Automatic merging: cannot reliably merge two evolving workflow states and drafts.

## Decision: Query a projected, ordered top 20 session summaries

**Rationale**: Listing needs only identity, question, and timestamps. A parameterized owner-filtered query ordered by update time returns the required 20 items without reading full drafts and research findings.

**Alternatives considered**:

- Read all sessions then trim in the console: wastes transfer and request units.
- Add search or pagination now: explicitly out of scope.

## Decision: Reuse a single Cosmos client and use asynchronous APIs

**Rationale**: Reusing the client avoids connection churn, and asynchronous cancellation-aware operations match the existing store contract. Cosmos SDK retry behavior handles transient throttling; conflict responses remain user-actionable and are not retried as writes.

**Alternatives considered**:

- Construct a client per operation: adds latency and risks socket exhaustion.
- Retry every write: may hide stale-write conflicts and violate the specified reload behavior.

## Decision: Provision Cosmos and data-plane access through Bicep

**Rationale**: The project already uses subscription-scope Bicep. The module will provision the Cosmos account, database, and container, configure the owner partition key and listing index, and assign the configured console principal the least-privileged Cosmos data role.

**Alternatives considered**:

- Manual portal configuration: cannot be reviewed or reproduced.
- Account keys or connection strings: conflicts with the project’s Entra-only rule.