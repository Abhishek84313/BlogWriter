# Research: Revision Status Controls

## Separate Revision request availability from Revise action availability

**Decision**: Track Revision request availability separately from Revise action availability. New enables the field, a displayed draft/session enables both when not processing, and processing disables both submission paths.

**Rationale**: The accepted clarification explicitly distinguishes editing the field after New from invoking a revision. The current `IsReviseEnabled` is tied to List mode, so a separate field-enabled rule prevents invalid Revise actions while honoring the requested New behavior.

**Alternatives considered**: Enabling both on New permits a revision without an active session; keeping both disabled contradicts the requested editable Revision request field; changing session persistence is unnecessary.

## Project workflow updates to one latest status value

**Decision**: Retain internal workflow update handling for routing and tests, but expose only the newest lifecycle message as a transient `CurrentStatus` string in workspace state. Reviewer feedback continues to update Reviewer notes separately.

**Rationale**: The user requested replacing the scrolling presentation, not removing workflow observability or Reviewer notes. A single projection minimizes UI space while preserving the existing update source and state notifications.

**Alternatives considered**: Deleting the update collection removes useful internal behavior and complicates tests; retaining the rendered list violates the requested presentation; persisting status history is out of scope.

## Compact command-bar grouping

**Decision**: Use a normal grouped layout when the List selector is hidden and a selector-aware layout when it is visible. Reduce button dimensions through shared CSS variables/rules while keeping labels, hit targets, focus outlines, and accessible names intact.

**Rationale**: Conditional layout is the smallest way to move Revise, Quit, and Help right only when List needs space. Shared sizing keeps all commands consistent across modes and viewports.

**Alternatives considered**: Separate command bars duplicate callbacks and accessibility behavior; absolute positioning risks overlap; shrinking only the List button produces inconsistent controls.

## Preserve Reviewer notes and submission protections

**Decision**: Keep Reviewer notes independent from CurrentStatus. Preserve existing operation version, cancellation, word-range validation, and duplicate-submission guards while changing only the state projection and control availability.

**Rationale**: Reviewer feedback has different user value and retention semantics from lifecycle status. Existing protections are required by the constitution and previous features.

**Alternatives considered**: Combining status and Reviewer notes loses review history; bypassing existing submission guards can create concurrent workflows.

## Validation strategy

**Decision**: Add bUnit tests for empty/draft/New/processing revision states, sequential latest-status replacement, safe text rendering, command grouping/sizing, and Reviewer notes separation. Add responsive browser-contract assertions at 390 × 844 and 1440 × 900, then run full regression and accessibility checks.

**Rationale**: The feature is concentrated in circuit state and presentation; deterministic component tests cover the state machine, while browser checks cover layout and overflow.

**Alternatives considered**: Manual-only testing cannot reliably cover transient update replacement or all disabled-state combinations; live workflow tests are unnecessary for this UI projection.
