# Workspace Interaction Contract

## Authentication Boundary

- The workspace requires an authenticated Microsoft Entra user.
- The server obtains the owner key from the authenticated `oid` claim.
- Browser input never supplies an owner ID, Cosmos credential, Foundry credential, or session identifier.
- Authentication loss blocks all writing and saved-session operations and clears owner-bound transient state.
- Local OIDC uses a client secret stored only in user secrets. Production OIDC uses a certificate stored in Azure Key Vault and retrieved with the web app's managed identity.
- Azure resource authorization uses `AzureCliCredential` locally and `ManagedIdentityCredential` in production, separately from the signed-in user's identity.

## Application Service Operations

### Start New Writing Session

**Input**: Non-empty initial prompt and existing default word-count policy.

**Success**: Create an owner-scoped session, execute the existing workflow once, save the completed state, and return the active session plus draft and review.

**Failure**: Preserve the prior stable workspace output and return a user-safe error. Token-cap failures stop the operation without automatic retry.

### Submit Revision

**Input**: Active owner-scoped session and non-empty revision text.

**Success**: Apply the existing follow-up transition, execute the existing workflow once, save, and return the updated draft and review.

**Failure**: Preserve the prior stable output. A concurrency conflict requires refreshing the saved session rather than overwriting it.

### List Saved Sessions

**Input**: Authenticated owner context.

**Success**: Return at most 20 current-owner summaries in the existing newest-first order. Assign transient one-based display numbers.

**Empty**: Return an explicit empty result; do not show the selection input or enable Revise.

**Failure**: Preserve stable draft/review output, hide selection, disable Revise, and return a recoverable error.

### Load Saved Session

**Input**: Positive display number and the exact currently displayed summary list.

**Success**: Resolve the number to the internal session ID, load through the owner-scoped store, and return its latest draft and review.

**Invalid/Unavailable**: Preserve workspace output and return a validation or unavailable message. Never derive an ID from the number.

### Cancel and Transition

**Input**: Target action New, List, or Quit.

**Order**:

1. If either prompt contains unsaved non-empty text, request discard confirmation.
2. If declined, preserve all state and stop.
3. Request cancellation of active work.
4. Await cancellation completion for up to 10 seconds.
5. Increment the operation version so late results are ignored, whether cancellation confirmed or timed out.
6. Perform the target action.

### Quit

**Success**: Cancel active work, clear owner-bound transient state, enter Ended mode, and reject subsequent writing actions for that circuit. The browser tab remains open.

## UI Contract

- Draft and Review are separate, simultaneously visible, independently scrollable regions on desktop and stack without overlap on narrow viewports.
- Initial Prompt and Revision Prompt are separately labeled multiline inputs.
- Enter submits non-empty text; Shift+Enter inserts a line.
- New, List, Revise, and Quit have equal compact dimensions.
- New, List, and Quit are enabled in every active mode.
- Revise is enabled only for a non-empty displayed list; the number input follows the same visibility condition.
- Processing, validation, cancellation, failure, and completion messages are announced to assistive technology.
- Dialog focus is trapped while open and restored to the initiating control after dismissal.
- All primary journeys conform to WCAG 2.2 Level AA.
