# Tasks: Blog Writer Web Interface

**Input**: Design documents from `/specs/003-blazor-blog-ui/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [workspace-interactions.md](contracts/workspace-interactions.md), and [quickstart.md](quickstart.md)

**Tests**: Test-first tasks are required by the project constitution and plan. Use xUnit for application services, bUnit for components, and Playwright-compatible browser tests for responsive and WCAG 2.2 AA behavior.

**Organization**: Tasks are grouped by user story after shared setup and foundational identity/workflow extraction. Each story has an independent test and can be demonstrated after its phase.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when it touches a different file and has no unfinished dependency.
- **[US#]**: Maps the task to a user story from the specification.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add isolated Blazor and web-test projects without breaking the existing console host.

- [X] T001 Create the .NET 10 Interactive Server Blazor project structure in `BlogWriter.Web/BlogWriter.Web.csproj`, `BlogWriter.Web/Program.cs`, and `BlogWriter.Web/Components/App.razor` with a project reference to `BlogWriter.csproj`.
- [X] T002 Exclude `BlogWriter.Web/**/*.cs` and `BlogWriter.Web.Tests/**/*.cs` from the root project's default compile glob in `BlogWriter.csproj` so the console build remains isolated.
- [X] T003 [P] Create the xUnit/bUnit/browser test project in `BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj` with project references to `BlogWriter.Web/BlogWriter.Web.csproj` and `BlogWriter.csproj`.
- [X] T004 Add `Microsoft.Identity.Web`, Interactive Server, bUnit, and Playwright dependencies to `BlogWriter.Web/BlogWriter.Web.csproj` and `BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj` without changing hosted-agent package versions.
- [X] T005 [P] Add non-secret Entra, Foundry, Cosmos, Key Vault certificate, and credential-mode configuration keys to `BlogWriter.Web/appsettings.json` and `BlogWriter.Web/appsettings.Production.json`; keep credentials out of committed settings.
- [X] T006 [P] Add the Blazor route shell and authorized fallback structure in `BlogWriter.Web/Components/Routes.razor` and `BlogWriter.Web/Components/Layout/MainLayout.razor`.

---

## Phase 2: Foundational Identity and Application Services

**Purpose**: Establish owner-safe, host-neutral workflow/session services and a circuit-scoped state machine before implementing any user story.

**Critical**: All user stories depend on this phase. Razor components must not call Cosmos or hosted agents directly.

### Foundation Tests

- [X] T007 [P] Add owner-provider contract tests in `BlogWriter.Tests/SessionOwnerProviderTests.cs` covering a valid Entra object ID and missing/invalid owner identity.
- [X] T008 [P] Add claims-owner tests in `BlogWriter.Web.Tests/ClaimsSessionOwnerProviderTests.cs` proving the authenticated `oid` claim is required and browser-supplied owner values are ignored.
- [X] T009 [P] Add application-service and console-host regression tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs` and `BlogWriter.Tests/ConsoleHostRegressionTests.cs` for create/run/save, follow-up/run/save, owner-scoped list/load, token-cap propagation, conflict preservation, cancellation, and unchanged list/resume/new-topic/error behavior.
- [X] T010 [P] Add workspace-state tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` enforcing: one active operation per circuit; maximum 20 displayed entries; numeric current-list selection; last stable Draft/Review preservation; and operation-version rejection of late results.

### Foundation Implementation

- [X] T011 Introduce `./ISessionOwnerProvider.cs`, implement it in `./EntraSessionOwnerProvider.cs`, and update `./CosmosBlogSessionStore.cs` to depend on the abstraction without changing session documents or partition keys.
- [X] T012 Implement `BlogWriter.Web/ClaimsSessionOwnerProvider.cs` to read the immutable authenticated `oid` claim and reject missing or unauthenticated identity.
- [X] T013 Define host-neutral operation results and create/run/revise/list/load contracts in `IBlogWriterSessionService.cs` according to `specs/003-blazor-blog-ui/contracts/workspace-interactions.md`.
- [X] T014 Implement workflow and persistence sequencing in `./BlogWriterSessionService.cs`, preserving `ResearchState`, `IBlogWorkflow`, token-cap exceptions, cancellation tokens, optimistic concurrency, and last-stable-state behavior.
- [X] T015 Add shared Foundry agent, workflow, Cosmos client, credential, token-cap, and application-service registrations in `./BlogWriterServiceCollectionExtensions.cs`, selecting `AzureCliCredential` locally and `ManagedIdentityCredential` for deployed production.
- [X] T016 Update the existing console composition in `./Program.cs` to use the shared application registration/service while preserving console commands and output behavior.
- [X] T017 Configure Microsoft Entra OpenID Connect, authorization, Interactive Server rendering, explicit Azure resource credentials, and scoped owner/workspace services in `BlogWriter.Web/Program.cs`.
- [X] T018 Implement the `Workspace State` fields and transitions from `data-model.md` in `BlogWriter.Web/Services/BlogWorkspaceState.cs` and `BlogWriter.Web/Services/BlogWorkspaceService.cs`, keeping all mutable user state circuit-scoped.
- [X] T019 Add operation serialization, discard-confirmation gating, a 10-second cancellation wait, monotonic operation versions, timeout fallback, and stale-result suppression to `BlogWriter.Web/Services/BlogWorkspaceService.cs`.
- [X] T020 Run the focused foundation tests in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs`, `BlogWriter.Tests/ConsoleHostRegressionTests.cs`, `BlogWriter.Tests/SessionOwnerProviderTests.cs`, `BlogWriter.Web.Tests/ClaimsSessionOwnerProviderTests.cs`, and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: Both hosts compile; owner identity is claim-based in the web host; workflow/persistence operations are reusable and testable without Razor components or live Azure services.

---

## Phase 3: User Story 1 - Start New Writing Work (Priority: P1) MVP

**Goal**: Provide the primary authenticated workspace, clear it with New, and submit one initial prompt without duplicate execution.

**Independent Test**: Populate every workspace field, choose New, verify clean state and focus, submit one prompt with Enter, and verify one completed draft/review result while Shift+Enter only inserts a line.

### Tests for User Story 1

- [X] T021 [P] [US1] Add bUnit layout and New-state tests in `BlogWriter.Web.Tests/HomePageTests.cs` for separate draft/review regions, two labeled prompt inputs, equal compact command buttons, clearing, and initial-input focus.
- [X] T022 [US1] Add initial-prompt interaction tests in `BlogWriter.Web.Tests/HomePageTests.cs` for Enter submission, Shift+Enter line insertion, empty-input rejection, visible processing status, and duplicate-submission prevention.
- [X] T023 [P] [US1] Add application workflow tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving one accepted initial prompt creates, runs, saves, and atomically publishes matching Draft and Review.

### Implementation for User Story 1

- [X] T024 [US1] Build the responsive primary workspace structure in `BlogWriter.Web/Components/Pages/Home.razor` with separately labeled draft, review, initial-prompt, and revision regions.
- [X] T025 [US1] Implement reusable Enter/Shift+Enter multiline input behavior in `BlogWriter.Web/Components/PromptInput.razor` without adding a fifth command-bar button.
- [X] T026 [US1] Wire initial-prompt submission, processing announcements, duplicate prevention, and stable-result publication through `BlogWriter.Web/Services/BlogWorkspaceService.cs` and `BlogWriter.Web/Components/Pages/Home.razor`.
- [X] T027 [US1] Implement New clearing and focus restoration in `BlogWriter.Web/Services/BlogWorkspaceService.cs` and `BlogWriter.Web/Components/Pages/Home.razor`.
- [X] T028 [US1] Run the US1 tests in `BlogWriter.Web.Tests/HomePageTests.cs` and the initial-prompt cases in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: A signed-in user can start a new session, see separate draft/review output, and clear the workspace without using List or saved-session recall.

---

## Phase 4: User Story 2 - Review and Refine a Draft (Priority: P1)

**Goal**: Display the current draft and reviewer feedback together and submit a follow-up revision against the active session.

**Independent Test**: Seed an active session, submit a non-empty revision with Enter, and verify Draft and Review update together after save while empty input and failures preserve the stable result.

### Tests for User Story 2

- [X] T029 [P] [US2] Add bUnit tests in `BlogWriter.Web.Tests/HomePageTests.cs` for independently scrollable draft/review regions, revision-input visibility, Enter/Shift+Enter behavior, and empty revision rejection.
- [X] T030 [P] [US2] Add revision service tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` covering `StartFollowUp`, one workflow execution, save, atomic Draft/Review replacement, failure preservation, conflict handling, and cancellation.

### Implementation for User Story 2

- [X] T031 [P] [US2] Implement accessible draft and review regions in `BlogWriter.Web/Components/DraftPane.razor` and `BlogWriter.Web/Components/ReviewPane.razor` with stable dimensions and independent overflow scrolling.
- [X] T032 [US2] Wire the separate revision input to active-session follow-up execution in `BlogWriter.Web/Components/Pages/Home.razor` and `BlogWriter.Web/Services/BlogWorkspaceService.cs`.
- [X] T033 [US2] Preserve prior stable Draft/Review and announce recoverable errors for revision, token-cap, cancellation, and conflict outcomes in `BlogWriter.Web/Services/BlogWorkspaceService.cs`.
- [X] T034 [US2] Run the US2 component and service tests in `BlogWriter.Web.Tests/HomePageTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: Revision is independently demonstrable from a seeded active session and never exposes partial workflow output.

---

## Phase 5: User Story 3 - Find and Resume Saved Work (Priority: P1)

**Goal**: Show the current user's newest saved sessions as a numbered scrollable list and load one by display number.

**Independent Test**: Return multiple owner-scoped summaries, choose List, verify numbering and conditional controls, select a valid number, and load the matching draft/review; verify empty, invalid, foreign, and unavailable cases separately.

### Tests for User Story 3

- [X] T035 [P] [US3] Add list rendering tests in `BlogWriter.Web.Tests/HomePageTests.cs` for newest-first `[1]` numbering, 20-entry scrolling, empty state, refresh without duplication, hidden selection input, and disabled Revise.
- [X] T036 [US3] Add selection tests in `BlogWriter.Web.Tests/HomePageTests.cs` for visible compact number input, enabled Revise, valid number loading, and empty/malformed/zero/negative/out-of-range validation.
- [X] T037 [P] [US3] Add owner-isolation and unavailable-session tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving only current-owner summaries are shown and stable output survives list/load failures.

### Implementation for User Story 3

- [X] T038 [P] [US3] Implement the numbered scrollable session-list component in `BlogWriter.Web/Components/SessionList.razor` using structured `BlogSessionSummary` data and transient display numbers.
- [X] T039 [US3] Implement owner-scoped List refresh, empty/failure states, and current-list retention in `BlogWriter.Web/Services/BlogWorkspaceService.cs`.
- [X] T040 [US3] Add the conditional number input and Revise enabled state to `BlogWriter.Web/Components/Pages/Home.razor`, resolving selections only against the currently displayed list.
- [X] T041 [US3] Implement selected-session loading through `IBlogWriterSessionService` in `BlogWriter.Web/Services/BlogWorkspaceService.cs`, retaining internal IDs server-side and publishing matching Draft/Review atomically.
- [X] T042 [US3] Run the US3 component/service tests in `BlogWriter.Web.Tests/HomePageTests.cs` and `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.

**Checkpoint**: A signed-in user can list and resume only their own saved sessions without seeing or entering a session ID.

---

## Phase 6: User Story 4 - Predictable, Safe, Accessible Controls (Priority: P2)

**Goal**: Enforce always-enabled New/List/Quit controls, conditional Revise, safe discard/cancellation ordering, ended state, and WCAG 2.2 AA behavior.

**Independent Test**: Traverse idle, processing, unsaved-text, list, invalid-selection, authentication-loss, and ended states using keyboard and assistive-technology checks; verify exact enabled states, confirmation order, cancellation completion, stale-result suppression, and focus behavior.

### Tests for User Story 4

- [X] T043 [P] [US4] Add command-state tests in `BlogWriter.Web.Tests/HomePageTests.cs` proving New/List/Quit remain enabled in every active state and Revise is enabled only for a non-empty displayed list.
- [X] T044 [P] [US4] Add discard/cancellation tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving declined confirmation changes nothing and accepted New/List/Quit proceeds after cancellation confirmation or a 10-second timeout while ignoring late results.
- [X] T045 [US4] Add Quit and authentication-loss tests in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs` proving owner-bound state is cleared and further actions are rejected until a new authenticated UI session exists.
- [X] T046 [P] [US4] Add component accessibility tests in `BlogWriter.Web.Tests/AccessibilityTests.cs` for labels, landmarks, live regions, dialog semantics, logical focus order, focus restoration, button names, and disabled-state exposure.
- [X] T047 [P] [US4] Add browser tests in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` at 390 × 844 and 1440 × 900 for independent scrolling, keyboard-only journeys, visible focus, contrast, no overlap, and automated WCAG 2.2 Level AA scans.

### Implementation for User Story 4

- [X] T048 [P] [US4] Implement the equal-size compact New/List/Revise/Quit command bar and state-derived enablement in `BlogWriter.Web/Components/CommandBar.razor`.
- [X] T049 [P] [US4] Implement the accessible discard-confirmation dialog with focus trap, initiating-control restoration, accept, and decline behavior in `BlogWriter.Web/Components/ConfirmDiscardDialog.razor`.
- [X] T050 [US4] Integrate confirmation-before-cancellation and New/List/Quit transitions in `BlogWriter.Web/Components/Pages/Home.razor` and `BlogWriter.Web/Services/BlogWorkspaceService.cs`.
- [X] T051 [US4] Implement the ended-session and authentication-required states in `BlogWriter.Web/Components/Pages/Home.razor` and clear owner-bound scoped state on quit or authentication loss.
- [X] T052 [US4] Add semantic regions, programmatic labels, live status announcements, focus management, reduced-motion support, and accessible validation associations in `BlogWriter.Web/Components/Pages/Home.razor` and shared components.
- [X] T053 [US4] Implement responsive workspace layout at 390 × 844 and 1440 × 900, stable region sizing, independent scrolling, equal compact controls, visible focus, and WCAG-compliant contrast in `BlogWriter.Web/wwwroot/app.css`.
- [X] T054 [US4] Run the US4 component, service, browser, and accessibility tests in `BlogWriter.Web.Tests/HomePageTests.cs`, `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`, `BlogWriter.Web.Tests/AccessibilityTests.cs`, and `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs`.

**Checkpoint**: All controls follow the clarified state rules and primary journeys pass keyboard, responsive, cancellation, and WCAG 2.2 AA validation.

---

## Phase 7: Polish and Cross-Cutting Validation

**Purpose**: Align documentation, preserve existing hosts, and validate the complete feature.

- [X] T055 [P] Document the local user-secret OIDC client secret, production Key Vault OIDC certificate, local Azure CLI resource credential, deployed managed identity, callback configuration, and non-secret settings in `docs/configuration.md`.
- [X] T056 [P] Document local startup, Entra app registration prerequisites, production Key Vault certificate access, owner isolation, 10-second cancellation fallback, and hosting prerequisites in `docs/deployment.md`.
- [X] T057 [P] Update `./README.md` with the Blazor web-host entry point while preserving console and hosted-agent instructions.
- [X] T058 Run all existing and web tests with `dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj` and `dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj`, then build `BlogWriter.csproj` and `BlogWriter.Web/BlogWriter.Web.csproj`.
- [X] T059 Run every scenario in `specs/003-blazor-blog-ui/quickstart.md`, including authenticated owner isolation, cancellation confirmation and 10-second timeout, stale-result suppression, 390 × 844 and 1440 × 900 viewports, and WCAG 2.2 AA checks.
- [X] T060 Run MAF Doctor against the repository and verify this feature introduces no new MAF findings in `BlogWriter.Web/`, `BlogWriterSessionService.cs`, or shared composition files; document the unchanged pre-existing baseline in `specs/003-blazor-blog-ui/quickstart.md`.

---

## Requirement Coverage

| Requirement | Primary tasks |
| --- | --- |
| FR-001 | T001, T017, T024 |
| FR-002 | T021, T024, T031 |
| FR-003 | T021-T022, T024-T025, T029, T032 |
| FR-004 | T009, T014, T023, T026 |
| FR-005 | T009, T014, T030, T032-T033 |
| FR-006 | T021, T043, T048, T053 |
| FR-007 | T043, T048, T050 |
| FR-008 | T027, T044, T049-T050 |
| FR-009 | T035, T038-T039 |
| FR-010 | T035-T036, T040 |
| FR-011 | T035-T036, T040 |
| FR-012 | T036-T037, T040-T041 |
| FR-013 | T036-T037, T040-T041 |
| FR-014 | T029, T031, T035, T047, T053 |
| FR-015 | T010, T019, T022-T023, T026 |
| FR-016 | T045, T050-T051 |
| FR-017 | T009, T014-T019, T030, T037, T041, T060 |
| FR-018 | T010, T030, T033, T037, T044 |
| FR-019 | T010, T019, T044, T049-T050 |
| FR-020 | T022, T025, T029, T032 |
| FR-021 | T044, T049-T050 |
| FR-022 | T008, T010, T012, T017-T018, T045, T051 |
| FR-023 | T007-T012, T017-T018, T037, T045, T051 |
| FR-024 | T046-T047, T052-T054 |

## Success Criteria Coverage

| Success criterion | Primary tasks |
| --- | --- |
| SC-001 | T021-T028, T029-T034, T059 |
| SC-002 | T035-T042, T059 |
| SC-003 | T043, T048, T054 |
| SC-004 | T030, T033, T037, T044-T045 |
| SC-005 | T029, T031, T035, T047, T053-T054 |
| SC-006 | T010, T019, T022-T023, T026 |
| SC-007 | T046-T047, T052-T054, T059 |

## Dependencies and Execution Order

### Phase Dependencies

- **Phase 1** starts immediately; T003-T006 may proceed after T001 establishes project paths, and T002 must complete before root builds include the new folders.
- **Phase 2** depends on Phase 1 and blocks every story. Tests T007-T010 precede implementations T011-T019; T020 validates the foundation.
- **User Stories 1-3** each depend on Phase 2. They can be staffed in parallel after the shared component/state contracts stabilize, but the recommended delivery order is US1, US2, then US3.
- **User Story 4** depends on the shared state machine and integrates states introduced by US1-US3; its tests may be drafted earlier, but final validation follows those stories.
- **Phase 7** depends on all selected stories.

### User Story Dependencies

- **US1 - Start New Writing Work**: Depends only on Phase 2 and is the MVP.
- **US2 - Review and Refine**: Depends only on Phase 2 when tested with a seeded active session; integrates naturally after US1.
- **US3 - Find and Resume**: Depends only on Phase 2 when tested with fake summaries/sessions; integrates naturally after US1 output regions exist.
- **US4 - Predictable Controls**: Depends on Phase 2 for isolated state tests and on US1-US3 for full browser journeys.

## Parallel Opportunities

- T003, T005, and T006 can proceed in parallel after project paths are established; T004 follows project creation because it edits both project files.
- T007-T010 are parallel test tasks in separate files.
- US1 tests T021 and T023 can run in parallel, followed by T022 in the shared component-test file; US2 tests T029-T030 can run in parallel; US3 tests T035 and T037 can run in parallel, followed by T036 in the shared component-test file.
- US4 tests T043-T047 can run in parallel by test surface.
- Component implementations in different files, such as T031, T038, T048, and T049, can proceed in parallel after their tests exist.
- Documentation tasks T055-T057 can proceed in parallel.

## Parallel Example: User Story 1

```text
Task T021: Add Home layout/New tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T023: Add initial workflow tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
```

## Parallel Example: User Story 3

```text
Task T035: Add list rendering tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T037: Add owner-isolation tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
```

## Parallel Example: User Story 4

```text
Task T043: Add command-state tests in BlogWriter.Web.Tests/HomePageTests.cs
Task T044: Add discard/cancellation tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T046: Add component accessibility tests in BlogWriter.Web.Tests/AccessibilityTests.cs
Task T047: Add browser accessibility tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs
```

## Implementation Strategy

### MVP First

1. Complete setup and foundational identity/application services.
2. Complete US1 through T028.
3. Validate authenticated new-session creation, one submission per Enter action, separate draft/review output, New clearing, and keyboard behavior.
4. Demonstrate the MVP before adding saved-session and cross-state controls.

### Incremental Delivery

1. Foundation: secure identity separation and reusable workflow/session service.
2. US1: new writing workspace.
3. US2: follow-up revision.
4. US3: owner-scoped saved-session list and recall.
5. US4: cancellation, discard confirmation, quit, responsive polish, and WCAG 2.2 AA.
6. Full regression, MAF Doctor comparison, documentation, and quickstart validation.
