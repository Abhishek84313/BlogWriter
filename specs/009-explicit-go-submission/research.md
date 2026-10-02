# Research: Explicit Go Submission

## Decision: Use the word-range component as the Go control owner

**Rationale**: The component already renders the adjacent Min and Max fields. Adding Go there fulfills the placement requirement while keeping the page responsible only for connecting the click callback to workspace behavior.

**Alternatives considered**:

- Put Go in the command bar: rejected because it is not adjacent to the word-range fields and would mix workflow transitions with submission.
- Add a button per prompt: rejected because the feature requires one explicit control and a single routing rule.

## Decision: Remove keyboard submission from prompt inputs

**Rationale**: `PromptInput` currently calls its submit callback from Enter. It will remain a text-entry component, allowing Enter to follow ordinary multiline textarea behavior and ensuring Go is the only processing trigger.

**Alternatives considered**:

- Intercept Enter and ignore it: rejected because native multiline entry is more predictable and avoids special handling.
- Keep Shift+Enter as the only newline entry: rejected because it conflicts with the explicit requirement that Enter must not process content.

## Decision: Centralize Go routing in the existing workspace service

**Rationale**: The service already owns range validation, duplicate-submission protection, output publication, cancellation, and calls to start or revise a session. A single intentional-submission method can choose revision when the revision request is non-empty and otherwise choose the initial prompt.

**Alternatives considered**:

- Route directly in the page component: rejected because it would duplicate service-level workflow safeguards.
- Change the underlying session service: rejected because its separate start and revise operations remain correct and reusable.

## Decision: Derive revision availability from displayed draft content

**Rationale**: The requirement defines an empty draft window as the disabling condition. The existing `HasDraft` state already applies whitespace-aware detection and can become the single eligibility basis alongside processing protection.

**Alternatives considered**:

- Keep active-session or New-mode exceptions: rejected because they allow revision controls without a displayed draft, contradicting the requested state rule.
- Clear revision text when disabled: rejected because the specification preserves existing revision content while controls are disabled.

## Decision: Cover behavior with bUnit and workspace-service tests

**Rationale**: bUnit can validate DOM placement, disabled state, clicks, and Enter key behavior; service tests can verify deterministic routing and state transitions without live agents.

**Alternatives considered**:

- Use only browser tests: rejected because focused unit/component tests are faster and already match local conventions.