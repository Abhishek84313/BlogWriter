# Research: Word Count Controls

## Centralize word-range validation

**Decision**: Add a shared immutable Word Range value that parses Min and Max input and enforces positive whole numbers with Max greater than or equal to Min.

**Rationale**: The web state and session-service boundary must apply identical rules. Central validation prevents the UI from accepting a range that the application service later interprets differently and guarantees rejection before model work begins.

**Alternatives considered**: Component-only validation could be bypassed by other callers; duplicating checks in the component and service would drift; browser-native number validity alone does not produce the required domain message.

## Keep editable text separate from accepted numeric values

**Decision**: Store Min and Max as editable strings plus an accepted numeric Word Range baseline in the circuit-scoped workspace state.

**Rationale**: String values preserve empty, non-numeric, and partially entered text long enough to show useful validation. The accepted range provides a stable comparison for unsaved-change confirmation and reflects the last successful submission or loaded session.

**Alternatives considered**: Integer-only binding cannot represent invalid text for validation; deriving the baseline from the active session fails when the user successfully changes the range during a revision.

## Snapshot the range at submission

**Decision**: Parse and capture one immutable range immediately before each initial or revision operation and pass that snapshot through the application service.

**Rationale**: An in-flight workflow must retain the values accepted at its start even when the visible controls are edited while processing. The snapshot also makes tests deterministic and avoids mutable-state races across awaits.

**Alternatives considered**: Reading workspace values after workflow completion could apply later edits to the wrong operation; disabling the controls during processing would contradict the specification.

## Preserve edits made during processing

**Decision**: After successful completion, set the accepted baseline to the submitted snapshot. If the visible inputs still equal that snapshot, synchronize them to the persisted result; if the user changed either input during processing, retain the visible edits and keep the range marked unsaved.

**Rationale**: This preserves user input while ensuring the completed session records exactly the submitted range. New, List, and Quit can then correctly request confirmation for the later edits.

**Alternatives considered**: Always replacing visible values loses edits; always treating the visible values as accepted falsely persists changes that were never submitted.

## Extend revisions without changing workflow topology

**Decision**: Add Min and Max parameters to the existing revision application-service operation, update the copied `ResearchState` before `StartFollowUp`, and save the completed candidate session as today.

**Rationale**: `ResearchState` already carries and persists both targets, and the author/reviewer already consume them. No agent prompt contract, workflow edge, Cosmos schema, or hosted-agent deployment changes are required.

**Alternatives considered**: Mutating the active session before workflow completion would violate stable-state preservation; a separate word-count preference store is unnecessary.

## Use compact text entries with numeric input hints

**Decision**: Render two explicitly labeled compact text entries with numeric input hints and accessible invalid-state associations rather than relying solely on browser number controls.

**Rationale**: The application must detect and explain empty, non-whole, and non-numeric values consistently. Text binding preserves the exact invalid value, while input hints still provide an appropriate mobile keypad.

**Alternatives considered**: Number inputs vary in how browsers sanitize invalid text and can hide the value that needs explanation; sliders are unsuitable because the feature has no product upper bound.
