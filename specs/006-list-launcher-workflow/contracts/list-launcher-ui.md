# List Launcher UI Contract

This is an in-process UI/state contract. No public HTTP endpoint or persisted document shape is added.

## Command Bar

- Existing commands remain New, List, Revise, and Quit.
- A compact numeric selector appears immediately beside List while List mode is active.
- Commands to the right of List shift right and remain operable.
- A compact `?` Help button appears beside the existing commands.
- The former bottom-of-form session-number selector is not rendered.

## Selection

- The displayed list is one-based and capped by the existing list behavior.
- Whitespace around the selector is accepted; empty, non-integer, non-positive, and out-of-range values are rejected.
- Invalid selection appends a validation entry to the workflow log and performs no load or workflow call.
- A valid selection is ignored/rejected while processing is active.
- A valid selection clears Draft, Reviewer notes, New writing prompt, and Revision request before restoration.
- `MainTask` becomes New writing prompt.
- Non-empty `CurrentSubTask` becomes Revision request; empty `CurrentSubTask` leaves it empty.
- After restoration, exactly one existing initial-writing operation begins from `MainTask`.

## Revision Input

- Revision request is disabled whenever Revise is disabled.
- Revision request is enabled only when an active/list-selected session is eligible and no operation is processing.
- Existing Enter/Shift+Enter prompt behavior remains unchanged.

## Help Dialog

- The dialog has an accessible name and modal semantics.
- It displays exactly: `dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj --launch-profile https`.
- Copy action attempts clipboard copy of the displayed command.
- Success is communicated without hiding the command.
- Failure leaves the command visible and communicates that copying did not complete.
- Closing the dialog leaves workspace state unchanged and restores usable focus.

## Workflow Log

- Retained entries remain in chronological order.
- The visible scroll viewport displays three normal log-entry lines.
- Older entries remain reachable by scrolling.
- Log content remains text-safe and the viewport remains usable at 390 × 844 and 1440 × 900.
