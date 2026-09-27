# Research: Single Review Round

## Decision: Use the existing bounded state counter and add a terminal revised-author path

- Keep `ResearchState.RevisionNumber` as the count of author passes: the initial draft is pass 1 and the single permitted revision is pass 2.
- Keep `ResearchState.MaxRevisions` at 2 so the existing state cap represents the initial draft plus one revision.
- Make the author-to-reviewer edge conditional on the author pass being below the cap.
- Allow `AuthorExecutor` to yield the state directly when it completes the capped revision.
- Configure both `ReviewerExecutor` and `AuthorExecutor` as workflow output sources. The reviewer yields an accepted initial draft; the author yields the final revised draft.
- Retain the reviewer-to-author edge only when `ResearchState.NeedsRevision` is true.

## Rationale

The current topology always sends every author result to the reviewer, so a rejected initial draft is reviewed twice. Conditional routing at the author edge prevents the second reviewer invocation while preserving the existing reviewer decision and feedback state. Explicit output from the author executor makes the revised draft observable through the same `WorkflowOutputEvent` path used by the reviewer.

## API and compatibility findings

- The installed Microsoft Agent Framework workflow API exposes `WorkflowBuilder.WithOutputFrom(ExecutorBinding[])`, allowing both terminal-capable executors to be registered.
- `WorkflowBuilder.AddEdge`, `IWorkflowContext.YieldOutputAsync`, and `WorkflowBuilder.WithOutputFrom` are marked SAFE by maf-doctor for the current tracked MAF release.
- Existing handlers return `ValueTask<ResearchState>`, which the workflow topology simulator reports as complete and fan-in safe.
- No new agent, credential, session-state, or message-handler pattern is required.

## Alternatives considered

### Keep the existing unconditional author-to-reviewer edge and lower the cap

Rejected because lowering the cap alone still invokes the reviewer after the revision; it only changes whether that second review yields output.

### Add a separate boolean revision-complete flag

Rejected because the existing revision counter already distinguishes the initial author pass from the single revised pass and adding another state field would expand persistence and cloning requirements unnecessarily.

### Bypass the reviewer after rejection by changing the reviewer executor to invoke the author

Rejected because it would combine stage responsibilities, obscure workflow topology, and make author/reviewer call counts harder to test independently.

## Baseline risks to track separately

MAF Doctor currently reports pre-existing repository findings: four production `DefaultAzureCredential` errors, three observability warnings, and six heuristic uncapped-call findings. This feature does not alter those areas. The plan preserves the existing token-budget and observability behavior and does not include unrelated remediation.
