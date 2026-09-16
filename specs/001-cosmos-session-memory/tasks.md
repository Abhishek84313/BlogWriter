# Tasks: Long-Term Session Memory

**Input**: Design documents from `/specs/001-cosmos-session-memory/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), and [console-commands.md](contracts/console-commands.md)

**Tests**: Focused xUnit tests are required by the project constitution for persistence and state changes. Tests use fake Cosmos abstractions or a test double so they do not require a live Foundry or Cosmos account.

**Organization**: Tasks are grouped by user story. Each user-story phase has a stated independent test and may be delivered after the foundational phase.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other marked tasks after its dependencies are complete.
- **[US#]**: User story ownership.

## Phase 1: Setup

**Purpose**: Add the SDK and declarative configuration needed by the long-term session store.

- [X] T001 Add the `Microsoft.Azure.Cosmos` package reference to `BlogWriter.csproj`.
- [X] T002 [P] Add Cosmos endpoint, database, container, and authorized-principal configuration entries to `docs/configuration.md` without account keys or connection strings.
- [X] T003 [P] Create the Cosmos account, NoSQL database, owner-partitioned session container, indexing policy for newest-first owner listings, and Cosmos data-plane RBAC role assignment in `infra/modules/cosmos-session-store.bicep`.
- [X] T004 Compose the Cosmos session-store module and its parameters/outputs in `infra/main.bicep`.

---

## Phase 2: Foundational Persistence

**Purpose**: Define the owner-scoped persistence boundary and implement the shared Cosmos store that blocks all user stories.

**Critical**: Complete this phase before implementing session list, resume, or follow-up behavior.

- [X] T005 Extend session ownership, concurrency-version, and session-summary models in `BlogSession.cs` so each saved session has an immutable Microsoft Entra object ID, a store-managed version, and a summary with ID, original question, creation time, and update time.
- [X] T006 Extend owner-scoped create, read, save, list, conflict, and deletion behavior in `IBlogSessionStore.cs`, retaining cancellation-token support and an explicit result for a conflicting stale save.
- [X] T007 Add an Entra object-ID provider that obtains and validates the signed-in caller identity from the Cosmos access token in `EntraSessionOwnerProvider.cs`.
- [ ] T008 Add focused store tests for required session data, 32-character session-ID validation, owner isolation, malformed-document handling, and ETag conflict rejection in `BlogWriter.Tests/CosmosBlogSessionStoreTests.cs`.
- [X] T009 Implement versioned document serialization, owner-partitioned reads and writes, projected parameterized listing, ETag replacement, and recoverable Cosmos failures in `CosmosBlogSessionStore.cs`.
- [X] T010 Wire one reused Entra-authenticated Cosmos client, `EntraSessionOwnerProvider`, and `CosmosBlogSessionStore` into `Program.cs`, replacing the default `FileBlogSessionStore` production path while preserving cancellation and token-budget behavior.

**Checkpoint**: The console can create, save, and retrieve one owner-scoped session after application restart; stale saves cannot overwrite the latest stored state.

---

## Phase 3: User Story 1 - Find Previous Questions (Priority: P1)

**Goal**: Let the signed-in user list the 20 most recently updated sessions and select a session identifier from the output.

**Independent Test**: Create more than 20 sessions for one owner plus one session for another owner, invoke `list`, and verify only the current owner's newest 20 summaries appear in descending update order; verify the empty response separately.

- [ ] T011 [P] [US1] Add tests for owner-scoped newest-first 20-item session listing and the no-sessions empty result in `BlogWriter.Tests/CosmosBlogSessionStoreTests.cs`.
- [X] T012 [P] [US1] Add parsing tests for the case-insensitive `list` command and command precedence in `BlogWriter.Tests/SessionCommandParserTests.cs`.
- [X] T013 [US1] Implement a case-insensitive session command parser for `list` and `resume <session-id>` in `SessionCommandParser.cs`.
- [X] T014 [US1] Implement the `list` command's summary, empty, identity-failure, and storage-failure console output in `Program.cs` according to `contracts/console-commands.md`.

**Checkpoint**: A user can discover their newest 20 previous questions without entering an ID from memory.

---

## Phase 4: User Story 2 - Resume Saved Work (Priority: P1)

**Goal**: Let a user resume a listed session with its latest complete state while rejecting missing, foreign, invalid, or concurrently changed data safely.

**Independent Test**: Persist a completed state, create a fresh store instance, resume by the listed identifier, and verify the original question, research, draft, review state, and timestamps are restored; test unknown, malformed, and foreign identifiers independently.

- [ ] T015 [P] [US2] Add resume tests for restart-safe full-state restoration and not-found behavior for malformed, unknown, and foreign session identifiers in `BlogWriter.Tests/CosmosBlogSessionStoreTests.cs`.
- [X] T016 [US2] Update the `resume <session-id>` path to use the parsed command and current owner context in `Program.cs`, preserving the existing workflow execution path and user-actionable error output.
- [ ] T017 [US2] Add console integration tests for resume success and user-actionable not-found/conflict messages in `BlogWriter.Tests/ProgramSessionCommandTests.cs`.

**Checkpoint**: A selected session resumes with its latest state after restart, and invalid selections do not create or overwrite data.

---

## Phase 5: User Story 3 - Preserve Follow-Up Context (Priority: P2)

**Goal**: Preserve the complete original question and follow-up history across a resumed session while ensuring later saves update list recency.

**Independent Test**: Resume a session with prior refinements, submit another follow-up, reload it through a fresh store instance, and verify the original question, complete refinement history, and updated list position.

- [ ] T018 [P] [US3] Add tests that reload a followed-up session and verify original task, complete refinement history, updated timestamp, and newest-first listing position in `BlogWriter.Tests/CosmosBlogSessionStoreTests.cs`.
- [X] T019 [US3] Update follow-up persistence and conflict recovery in `Program.cs` so accepted follow-ups and workflow completion save the latest state, while stale saves direct the user to reload without discarding local state.

**Checkpoint**: A follow-up on a resumed session retains prior context and moves the session to the top of the owner's list.

---

## Phase 6: Account Lifecycle, Documentation, and Validation

**Purpose**: Complete account-deletion handling, operational documentation, and end-to-end validation across the feature.

- [X] T020 Implement owner-partition cleanup callable by the organization's Entra account-deletion lifecycle automation in `CosmosBlogSessionStore.cs` and cover deletion isolation in `BlogWriter.Tests/BlogSessionStoreOwnershipTests.cs`.
- [X] T021 Document the account-deletion lifecycle trigger, Cosmos data-plane role assignment, and required local console configuration in `docs/deployment.md`.
- [ ] T022 [P] Add session-store tracing/logging for save, list, resume, malformed data, service failure, and concurrency conflict outcomes in `CosmosBlogSessionStore.cs` and `Program.cs` without logging drafts or research content.
- [ ] T023 Run the session-store test slice and application build documented in `specs/001-cosmos-session-memory/quickstart.md`, then record any manual validation prerequisite or result in `docs/deployment.md`.
- [X] T024 Run `az bicep build --file infra/main.bicep` and correct the Cosmos module composition or parameter errors in `infra/main.bicep` and `infra/modules/cosmos-session-store.bicep`.

---

## Dependencies & Execution Order

### Phase Dependencies

- Phase 1: T001-T004 can begin immediately; T004 depends on T003.
- Phase 2: T005-T010 depends on Phase 1; T009 depends on T005-T008; T010 depends on T009.
- User Story 1: T011-T014 depends on T010. T011 and T012 may run in parallel; T013 precedes T014.
- User Story 2: T015-T017 depends on T010. T015 may run in parallel with User Story 1; T016 precedes T017.
- User Story 3: T018-T019 depends on T010 and the shared follow-up path; it may start after the foundation but should be completed after User Story 2 validates resumption.
- Phase 6: T020-T024 depends on the implementation work above; T022 may proceed in parallel with T020-T021. T023 and T024 are final validation tasks.

### User Story Dependencies

- **US1 - Find Previous Questions**: Depends only on foundational persistence. This is the MVP because it delivers discoverability of prior work.
- **US2 - Resume Saved Work**: Depends only on foundational persistence; it is independently testable but composes naturally with US1 list output.
- **US3 - Preserve Follow-Up Context**: Depends on foundational persistence and verifies the existing follow-up path against durable state.

### Parallel Opportunities

- T002 and T003 can proceed in parallel after T001 planning is understood.
- T011 and T012 can proceed in parallel for US1.
- T015 can proceed in parallel with US1 after the foundation completes.
- T018 can proceed in parallel with US1/US2 test writing after the foundation completes.
- T020, T021, and T022 can proceed in parallel after the core store is complete.

## Parallel Example: User Story 1

```text
Task: "Add newest-first owner-scoped list tests in BlogWriter.Tests/CosmosBlogSessionStoreTests.cs"
Task: "Add list command parsing tests in BlogWriter.Tests/SessionCommandParserTests.cs"
```

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete T011-T014 for US1.
3. Run the US1 tests and manual `list` scenario from `quickstart.md`.
4. Demonstrate that a signed-in user sees only their newest 20 session summaries.

### Incremental Delivery

1. Deliver US1 for session discovery.
2. Deliver US2 for restart-safe resumption and safe invalid-session handling.
3. Deliver US3 for durable follow-up context and recency updates.
4. Complete account lifecycle, observability, documentation, and infrastructure validation.

## Notes

- Every task follows the required checkbox, ID, optional parallel marker, story label, and exact-path format.
- The Cosmos account-deletion trigger is an organization lifecycle integration; T020 provides the owner-scoped deletion behavior consumed by that trigger, and T021 documents its required deployment configuration.
- Existing hosted-agent MAF health findings are outside this feature's scope; do not change them while completing these tasks.