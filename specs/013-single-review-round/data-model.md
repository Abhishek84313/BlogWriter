# Data Model: Single Review Round

## ResearchState

Existing workflow state remains the single source of truth.

| Field | Role in this feature | Rules |
|---|---|---|
| `Draft` | Latest draft displayed to the user | Initial author output is replaced only by the one permitted revision when replacement content exists. |
| `ReviewNotes` | Reviewer decision and suggested changes | Approval uses the existing `APPROVED` marker; rejection notes are passed to the one revision and remain available for final display. |
| `RevisionNumber` | Number of author passes | Initial draft is `1`; one revision may reach `2`; no author pass beyond `2` is allowed for this workflow run. |
| `MaxRevisions` | Existing upper bound | Remains `2`, representing the initial draft plus one revision. |
| `NextStep` | Existing workflow status marker | Existing values remain unchanged unless current stage behavior requires the terminal revised-author path to mark completion. |

## State transitions

```text
Initial state
  -> Author pass 1: Draft populated, RevisionNumber = 1
  -> Reviewer pass 1
      -> APPROVED: yield final state from ReviewerExecutor
      -> Rejected: retain ReviewNotes and route to AuthorExecutor
  -> Author pass 2: revise using ReviewNotes, RevisionNumber = 2
  -> yield final state from AuthorExecutor

No transition from Author pass 2 to ReviewerExecutor exists.
```

## Validation rules

- Reviewer call count is at most one per workflow run.
- Author call count is one for approval and two for rejection followed by revision.
- The final state is emitted after initial approval or after the single revision attempt.
- If the revision produces no replacement content, the existing latest draft remains the displayed final draft, consistent with current author fallback behavior.
