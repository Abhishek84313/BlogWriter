# Workspace UI Contract: Go Button Press Color Feedback

## Controls

| Control | Identifier/relationship | Behavior |
|---|---|---|
| Go | Existing command-bar button, `.range-go` (`CommandBar.razor`) | Idle/enabled: light green background (`--forest`). Hover (enabled, not pressed): dark green background (`--forest-dark`), unchanged from existing behavior. Pressed (enabled, mouse-down/touch-active/keyboard-activation in progress): dark green background (`--forest-dark`) via `:active`. Disabled: existing muted/gray disabled styling takes precedence over pressed styling. |

## Visual State Transitions

- Idle → Pressed: on press start (`:active`), while enabled.
- Pressed → Idle: on press end, cancellation, or pointer/focus leaving the button before release; no stuck-pressed state is permitted.
- Any state → Disabled: the `disabled` attribute styling always wins; a disabled button never shows the pressed (dark green) color even if a press gesture is attempted.

## Accessibility and Keyboard Behavior

- No change to the Go button's accessible name, role, tab order, or `disabled` semantics.
- The pressed color is a supplementary visual cue only; it does not replace or alter any existing text, label, or ARIA attributes conveying the button's action or state.
- Keyboard activation (Space/Enter) on a focused, enabled Go button must trigger the same pressed-color feedback as a mouse or touch press.

## Non-Goals

- No new HTTP, public API, storage, or hosted-agent contract.
- No change to the Go button's click/submission behavior, word-range parsing, session ownership, cancellation, or workflow-output contracts (see `009-explicit-go-submission`).
- No change to any other command-bar control's styling or behavior.
