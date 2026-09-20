# Word Range Interaction Contract

## Placement and Labels

- Render two compact entries labeled exactly `Min` and `Max`.
- Place the range row after both prompt inputs and before the Draft and Reviewer panes in visual, reading, and keyboard order.
- Keep both controls visible in new, draft, and list workspace modes until the UI session ends.

## Defaults and Reset

- Initialize a new workspace with Min `1000` and Max `2000`.
- New resets visible values and the accepted baseline to 1000/2000 after any required discard confirmation.
- A legacy or unavailable stored range uses the same defaults.

## Validation

- Preserve typed text while validating.
- Reject empty, non-numeric, non-whole, zero, and negative values.
- Reject Max below Min; accept Max equal to Min.
- Associate each error with its input and expose invalid state to assistive technology.
- Clear obsolete errors when input correction creates a valid range.
- Block initial and revision submission before any workflow call while invalid.

## Initial Submission

1. Validate the editable range.
2. Capture an immutable range snapshot.
3. Start the existing new-session operation with that Min and Max.
4. On success, persist the snapshot in the writing session.
5. Make the submitted snapshot the accepted workspace baseline.
6. Preserve any later visible edits made while processing and mark them unsaved.

## Revision Submission

1. Require an active session and a valid editable range.
2. Capture an immutable range snapshot.
3. Apply the snapshot to the candidate session state before starting the follow-up workflow.
4. Persist the completed candidate only after success.
5. Make the submitted snapshot the accepted baseline while preserving later visible edits.
6. On failure or cancellation, preserve the prior stable session, range baseline, and visible inputs.

## Saved Session Loading

- Loading a session replaces visible Min and Max and the accepted baseline with the session's stored targets.
- Loading does not expose or modify another user's session.
- The existing session identifier remains internal.

## Unsaved Range Confirmation

- A valid visible range differing from the accepted baseline is unsaved.
- Invalid visible range input is also unsaved because it differs from the accepted baseline.
- New, List, and Quit use the existing discard-confirmation flow when the range is unsaved, even if prompt inputs are empty.
- Declining confirmation preserves Min, Max, and all other workspace state.

## Accessibility and Responsive Behavior

- Labels are programmatically associated with inputs.
- Inputs expose numeric keyboard hints without relying on browser sanitization for validation.
- Validation is announced and associated with the relevant field.
- The range row does not overlap or introduce horizontal scrolling at 390 × 844 or 1440 × 900.
