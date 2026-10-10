# Data Model: Restore Saved Session for Editing

No persisted schema changes are required. The feature changes how an existing saved session is selected and how a subsequent new run is initiated.

## Saved Session Summary

A row in the current user's displayed list, identified internally by the session ID and shown with its existing query summary and update time.

| Field | Source | Use |
|---|---|---|
| Session ID | Existing saved record | Resolve the selected row through the owner-scoped load operation. |
| Main task | Existing session state | Restore the New query field. |
| Created/updated time | Existing record | Preserve current list presentation and ordering. |

The list remains limited to the existing 20 newest current-owner summaries. The visible row number is presentational only and is not an identifier.

## Restored Workspace

A transient UI state populated after a successful row selection and before Go.

| Field | Restore behavior |
|---|---|
| New query | Copy the selected record's `MainTask`; remain editable. |
| Revision request | Copy non-empty `CurrentSubTask` according to the existing restore behavior; remain editable for the normal revision flow after a new draft exists. |
| Min / Max | Copy the saved positive whole-number range; use the existing defaults if legacy values are unavailable. |
| Draft / Reviewer notes | Clear according to the established list-launch behavior. |
| Active session | No source session is made active for writes. |
| Workflow state | Pending; selection does not invoke an agent or create a new record. |

Edits to the query or range are validated using existing workspace rules. The restored values are transient until Go is activated.

## New Run Entry

Go starts the existing new-session operation. That operation creates a fresh session record for the authenticated owner, initializes its state from the submitted query and word range, executes the existing workflow, and saves the resulting state. The source saved record retains its ID and content unchanged. No additional fields or schema version are introduced.

## Lifecycle

```text
List displayed
  -> row selected
  -> owner-scoped load
  -> restored editable workspace (no run, no new record)
  -> Go
  -> new session created with a distinct ID
  -> workflow completes or follows existing failure/cancellation behavior
  -> new result is saved; source record remains unchanged
```

A load failure leaves the current workspace intact. Repeated selection or Go activation while a conflicting operation is busy follows the existing busy-state and duplicate-submission protections.
