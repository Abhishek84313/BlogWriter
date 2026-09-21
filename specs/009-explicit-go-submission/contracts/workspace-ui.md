# Workspace UI Contract: Explicit Go Submission

## Controls

| Control | Identifier/relationship | Behavior |
|---|---|---|
| Go | A labeled button in the word-range row after the Max field | Activates exactly one existing workspace submission path; revision request has priority over initial prompt. |
| Initial prompt | Existing initial-prompt textarea | Updates workspace text on input. Enter adds text-line input and does not submit. |
| Revision request | Existing revision-prompt textarea | Updates workspace text on input. It is disabled when no non-whitespace draft is displayed or processing is active. Enter does not submit. |
| Revision command | Existing command-bar revision control | Is disabled when no non-whitespace draft is displayed or processing is active. Existing saved-session selection behavior remains separate from Go submission. |

## Accessibility and Keyboard Behavior

- Go has the accessible name "Go" and is reachable in normal tab order directly after the word-range inputs.
- Disabled revision controls expose their native disabled state and remain non-editable/non-activatable.
- Existing input labels and validation descriptions remain connected to their controls.
- Enter in either prompt or a word-range input must not invoke a submission callback or start processing.

## Non-Goals

- No new HTTP, public API, storage, or hosted-agent contract.
- No change to word-range parsing, session ownership, cancellation, or workflow-output contracts.