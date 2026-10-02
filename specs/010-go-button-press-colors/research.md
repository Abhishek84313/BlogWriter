# Phase 0 Research: Go Button Press Color Feedback

No `NEEDS CLARIFICATION` markers remain in the Technical Context — the feature is scoped
entirely within existing, well-understood project conventions. This document records the
small set of decisions made while confirming the approach.

## Decision: Reuse existing color tokens instead of introducing new ones

- **Decision**: Implement "light green" and "dark green" using the existing CSS custom
  properties `--forest` (`#1e5a46`) and `--forest-dark` (`#123d30`), already defined in
  `BlogWriter.Web/wwwroot/app.css` and already used as the Go button's idle background and
  hover background respectively.
- **Rationale**: These tokens already read as "lighter green" vs. "darker green," are used
  consistently elsewhere in the stylesheet (e.g. `.sign-out`), and satisfy FR-007's
  contrast/brightness requirement without introducing new design tokens or inconsistency
  with the rest of the app's palette.
- **Alternatives considered**: Defining new `--go-pressed` / `--go-idle` variables was
  considered but rejected as unnecessary — it would duplicate values that are functionally
  identical to `--forest`/`--forest-dark` and add indirection for no benefit, conflicting
  with Constitution Principle V (smallest change that preserves existing conventions).

## Decision: Implement press feedback via CSS `:active` pseudo-class, not component state

- **Decision**: Add an `:active` (and `:active:disabled` guard) rule to the existing
  `.range-go` selector rather than introducing C#/Razor state (e.g. a `isPressed` field
  toggled by `@onmousedown`/`@onmouseup`).
- **Rationale**: The browser's native `:active` pseudo-class already fires for mouse,
  touch, and keyboard (Space/Enter) activation on a `<button>` element, automatically
  reverts when the pointer leaves the element or the press is cancelled, and requires no
  JavaScript interop or component re-renders. This satisfies FR-002–FR-004 and the edge
  cases (drag-off, keyboard activation) with a single, dependency-free CSS rule, and keeps
  parity with the existing `:hover:not(:disabled)` / `:disabled` rules already on
  `.range-go`.
- **Alternatives considered**:
  - *Blazor event-handler-driven state* (`@onmousedown` / `@onmouseup` toggling a CSS
    class): rejected — adds C# state, re-render churn, and must be re-implemented per
    input modality (mouse/touch/keyboard) to match what `:active` already provides for
    free; higher risk of a "stuck pressed" bug on drag-off (an edge case the spec calls
    out).
  - *JavaScript interop for press detection*: rejected — introduces a new dependency and
    interop surface for a purely visual, already-native browser behavior.

## Decision: Verification strategy given bUnit's CSS limitations

- **Decision**: Use existing bUnit tests in `BlogWriter.Web.Tests` to assert the Go
  button's markup, class name (`range-go`), and disabled-attribute behavior remain
  unchanged, and rely on manual/visual verification (per `quickstart.md`) for the actual
  `:active` color transition, since bUnit renders a virtual DOM and does not execute a real
  browser's CSS engine or pseudo-class matching.
- **Rationale**: This matches Constitution Principle IV (testable/observable business
  logic) while being honest about the boundary of what a component-test framework can
  verify for pure CSS pseudo-class styling; it avoids introducing a heavier E2E/browser
  test framework for a one-selector CSS change.
- **Alternatives considered**: Adding a full browser-automation test (e.g. Playwright) was
  considered but rejected as disproportionate tooling investment for a single CSS rule,
  and no such framework currently exists in this repository.
