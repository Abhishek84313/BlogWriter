# Revision Status UI Contract

This is an in-process UI/state contract. No public endpoint or persisted document shape is added.

## Revision Controls

- New state enables the Revision request input but not Revise.
- A displayed draft/session enables both Revision request and Revise when not processing.
- Processing disables the Revision request input and Revise action.
- No active draft/session keeps both disabled unless the New-state field rule applies.
- Existing Enter/Shift+Enter behavior remains unchanged.

## Current Status

- Render one labeled status region instead of the scrolling workflow-log list.
- Show the newest accepted lifecycle/status message only.
- Replace the prior message on each accepted update.
- Preserve polite live-update semantics and safe text rendering.
- Keep Reviewer notes in its separate pane.

## Command Bar

- With the List selector hidden, commands remain grouped.
- With the List selector visible, Revise, Quit, and Help move right to make room.
- All buttons use smaller shared dimensions while retaining readable labels, focus outlines, and operable hit targets.
- Layout remains non-overlapping at 390 × 844 and 1440 × 900.
