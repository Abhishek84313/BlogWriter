# Data Model: List Launcher Workflow

No persisted schema change is required. The feature reuses existing saved `ResearchState` fields and adds transient circuit-scoped UI state.

## List Selection Input

| Field | Type | Rules |
| --- | --- | --- |
| Value | String | Preserves current text, including invalid input; surrounding whitespace is trimmed for validation |
| DisplayPosition | Positive integer | One-based position in the current displayed saved-session list |
| Error | String or null | Set for empty, non-numeric, non-positive, out-of-range, or processing-conflict input |
| IsVisible | Boolean | True while the workspace is in List mode with the command-bar selector active |

## Restored Session Prompts

| Field | Source | Rules |
| --- | --- | --- |
| NewPrompt | `ResearchState.MainTask` | Required for automatic initial processing; rendered as text |
| RevisionPrompt | `ResearchState.CurrentSubTask` | Non-empty value is restored; empty value remains empty |
| SessionId | Existing `BlogSession.Id` | Internal identity used for owner-scoped load; not entered by the user |

## Run Command Help Dialog

| Field | Type | Rules |
| --- | --- | --- |
| CommandText | String | `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https` |
| IsOpen | Boolean | Transient dialog state |
| CopyStatus | Idle, Copied, Failed | Visible feedback after clipboard action; command remains visible on failure |

## Workflow Log Viewport

| Field | Type | Rules |
| --- | --- | --- |
| Entries | Existing ordered collection | Retains all accepted log entries |
| VisibleLineLimit | Integer | Fixed at 3 visible normal entry lines |
| Scrollable | Boolean | True when retained entries exceed the visible viewport |

## Workspace State Changes

| From | Event | Result |
| --- | --- | --- |
| Any non-list mode | Click List and transition accepted | Enter List mode, load displayed summaries, show compact selector beside List |
| List mode | Invalid selector input | Keep list and selector visible; append validation outcome; perform no load/workflow call |
| List mode | Valid selector input | Clear Draft, Reviewer notes, both prompt fields, and transient selection state before loading selected session |
| Valid selector input | Session loaded | Restore `MainTask` and optional `CurrentSubTask`; start exactly one initial workflow operation using `MainTask` |
| Valid selector input | Load/process failure | Preserve existing error/log behavior; do not discard unrelated current state beyond the required selection reset |
| Any state | Revise unavailable | Disable Revise and Revision request together |
| Any state | Help opened | Show command dialog without changing workspace content |
| Help dialog | Copy succeeds | Set CopyStatus to Copied; retain command |
| Help dialog | Copy fails | Set CopyStatus to Failed; retain command and dialog |
| Any state | New/List/Quit transitions | Preserve existing discard confirmation and cancellation behavior |

## Validation Rules

- Trim selector text before parsing.
- Require a positive whole number.
- Require the number to be within `1..DisplayedSessions.Count`.
- Reject selector changes while another workflow operation is processing.
- Do not call `LoadAsync` or start processing for invalid input.
- Automatic launch uses the restored `MainTask`; restored `CurrentSubTask` is UI context and remains available as Revision request.
- Clipboard failure is non-fatal and must not change session/workspace content.
- Log history remains retained while only the visible viewport is capped at three lines.
