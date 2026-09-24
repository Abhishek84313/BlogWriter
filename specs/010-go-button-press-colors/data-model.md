# Phase 1 Data Model: Go Button Press Color Feedback

This feature introduces no persisted data, domain entities, or state stored beyond the
browser's native rendering of CSS pseudo-classes. There is no `IBlogSessionStore` /
`ResearchState` change and nothing to add to the workflow's domain model.

## Conceptual state (not persisted)

| State | Trigger | Visual Result | Notes |
|-------|---------|----------------|-------|
| Idle | Default; button enabled, not pressed | Light green background (`--forest`) | Existing behavior, unchanged |
| Hover | Pointer over enabled, non-pressed button | Dark green background (`--forest-dark`) | Existing `:hover:not(:disabled)` rule, unchanged |
| Pressed | Pointer/touch/keyboard press held on enabled button | Dark green background (`--forest-dark`) | New `:active:not(:disabled)` rule (this feature) |
| Disabled | `disabled` attribute present | Muted gray background, no pointer cursor | Existing `:disabled` rule; MUST take precedence over Pressed (FR-005), enforced by an `:active:disabled` override |

These are transient CSS/browser render states, not application entities — they have no
identifiers, relationships, or persistence lifecycle, and require no repository, store, or
model class changes.
