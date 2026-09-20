# Research: List Launcher Workflow

## Keep selection in the command bar

**Decision**: Move the existing one-based list selection input into the command bar beside List and remove the footer selector. Keep the input visible only when the saved-session list is active and use a compact three-digit width.

**Rationale**: The current page already exposes the list mode and selection state through `BlogWorkspaceState`; moving the existing interaction avoids a second selection model and keeps related actions together.

**Alternatives considered**: Keeping both selectors duplicates state and creates conflicting entry points; making every list row a button changes the requested numbered workflow and requires a larger interaction redesign.

## Use MainTask and CurrentSubTask for restoration

**Decision**: Restore `ResearchState.MainTask` into New writing prompt and non-empty `ResearchState.CurrentSubTask` into Revision request. If `CurrentSubTask` is empty, leave Revision request empty.

**Rationale**: These fields already represent the persisted original topic and latest follow-up refinement. Reusing them satisfies the accepted clarification without adding a persisted revision-request property or Cosmos migration.

**Alternatives considered**: Adding a new revision-request field increases schema and compatibility scope; leaving the field empty discards existing saved refinement context.

## Launch through the existing initial operation

**Decision**: After a valid selection is loaded, clear visible Draft, Reviewer notes, and prompt fields, restore the selected prompt values, then invoke the existing initial-prompt submission path exactly once using the restored `MainTask`.

**Rationale**: The user explicitly requested behavior equivalent to entering the restored new prompt and pressing Enter. Reusing the existing initial operation preserves word-count snapshots, cancellation, output logging, ownership, and workflow termination.

**Alternatives considered**: Calling a new direct workflow method duplicates submission behavior; invoking a revision operation would incorrectly require an active stable session and change persistence semantics.

## Keep invalid selection local and non-operative

**Decision**: Trim input, require a positive integer within the currently displayed list bounds, and reject invalid values before session load or workflow start. Publish the error through the existing workflow log/validation surface.

**Rationale**: The selection is a UI position, not a session identifier. Local validation prevents unnecessary storage access and guarantees malformed input cannot trigger processing.

**Alternatives considered**: Passing arbitrary IDs to the store leaks an internal identity boundary and produces less useful errors; browser-only numeric validation does not cover range bounds or whitespace consistently.

## Use a dedicated Help dialog with clipboard fallback

**Decision**: Add a small question-mark command that opens an accessible modal dialog containing the fixed HTTPS launch command and a copy action. Use existing JavaScript interop for clipboard access; preserve the visible command and show a copy-failure message when clipboard access is unavailable.

**Rationale**: A dedicated dialog keeps the command discoverable without adding permanent page text, and a fallback state makes the action understandable when browser permissions deny clipboard access.

**Alternatives considered**: Navigating away to documentation interrupts the workflow; silently attempting clipboard access gives no feedback; adding a new server endpoint is unnecessary for a static command.

## Bound the log viewport, not retained history

**Decision**: Set the workflow log container to a height that displays three normal log-entry lines and retains overflow scrolling.

**Rationale**: The user requested a smaller scrolling window, not deletion of older workflow history. A visual viewport cap preserves observability while giving more room to the writing panes.

**Alternatives considered**: Deleting older entries loses diagnostic context; an unbounded log can push primary content below the fold.

## Preserve disabled-state semantics

**Decision**: Derive Revision request disabled state from the same condition as Revise: no active/list-selected session or processing is disabled; an eligible selected session enables both controls.

**Rationale**: Sharing the state rule prevents the button and input from disagreeing and keeps existing processing protections intact.

**Alternatives considered**: Separate flags can drift; leaving the input enabled invites a submission that the disabled command cannot complete.

## Validation strategy

**Decision**: Add focused selection/parser and workspace tests, bUnit command/dialog/page tests, and browser checks for compact layout, three-line scrolling, clipboard semantics, and 390 × 844 / 1440 × 900 responsive behavior. Run full core/web regression and compare MAF Doctor before and after.

**Rationale**: The feature crosses session restoration, asynchronous workflow launch, command layout, JavaScript clipboard behavior, and responsive presentation. Deterministic tests cover state transitions while browser checks cover the visual contract.

**Alternatives considered**: Manual testing alone cannot reliably prove exactly-one launch or invalid-input non-operation; live Azure tests are unnecessary for the UI/state contract.
