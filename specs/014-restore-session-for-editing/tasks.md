---

description: "Task list for restoring saved sessions as editable new runs"
---

# Tasks: Restore Saved Session for Editing

**Input**: Design documents from `specs/014-restore-session-for-editing/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/workspace-interactions.md`, and `quickstart.md`

**Tests**: Focused tests are required by FR-010 in `spec.md`; write tests before implementation and verify they fail for the current behavior.

**Organization**: Tasks are grouped by the two P1 user stories. Existing projects, service contracts, authentication, and Cosmos schema are reused.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel in different files without depending on incomplete tasks
- **[Story]**: User story represented by the task; setup/foundational/polish tasks have no story label
- Every task includes the exact repository-relative file path(s)

## Path Conventions

This is an existing Blazor web application with shared services. Paths below are relative to the repository root.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm prerequisites for implementation.

No setup tasks are required. The .NET 10 web/test projects, workspace service, session service, and Cosmos store already exist.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No new shared infrastructure is needed; preserve the current owner-scoped session and test-service boundaries.

No foundational tasks are required. User-story work can begin using the existing `IBlogWriterSessionService` and workspace state.

---

## Phase 3: User Story 1 - Restore a Saved Session for Editing (Priority: P1)

**Goal**: Selecting a saved row restores its prompt context and word range into editable fields without invoking the workflow or creating a record.

**Independent Test**: Click a listed session and verify the selected query, revision context, and Min/Max values are restored; Draft and Reviewer output follow existing reset behavior; inputs stay enabled; neither `StartAsync` nor `ReviseAsync` is called.

### Tests for User Story 1

- [X] T001 [P] [US1] Add workspace-service tests for selected-session restoration, Min/Max defaults, enabled inputs, cleared outputs, and zero start/revision calls in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.
- [X] T002 [P] [US1] Add browser tests that activate a saved-session row, verify the numeric selector is absent, and confirm selection causes no workflow call in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` and `BlogWriter.Web.Tests/HomePageTests.cs`.

### Implementation for User Story 1

- [X] T003 [P] [US1] Render each saved-session row as a keyboard-operable selection button and expose a callback carrying that row's `BlogSessionSummary` in `BlogWriter.Web/Components/SessionList.razor`; style its focus, disabled, and hover states in `BlogWriter.Web/wwwroot/app.css`.
- [X] T004 [P] [US1] Implement owner-scoped row restoration in `BlogWriter.Web/Services/BlogWorkspaceService.cs` and pending-restoration control state in `BlogWriter.Web/Services/BlogWorkspaceState.cs`; restore `MainTask`, optional `CurrentSubTask`, and saved Min/Max (or existing defaults), clear Draft/Reviewer output, keep editable inputs enabled, leave the source session inactive, and do not invoke or save a run.
- [X] T005 [US1] Wire the selected-summary callback from `BlogWriter.Web/Components/Pages/Home.razor` to the workspace service, remove the adjacent numeric input and its parameters from `BlogWriter.Web/Components/CommandBar.razor`, and delete its obsolete selectors from `BlogWriter.Web/wwwroot/app.css`.

**Checkpoint**: Saved rows restore the intended editable values and selection alone produces no workflow call or history entry.

---

## Phase 4: User Story 2 - Run the Edited Item as a New Session (Priority: P1)

**Goal**: Go submits the currently edited restored values as one new run and leaves the selected historical session unchanged.

**Independent Test**: Seed a restored pending workspace, edit the query and range, activate Go, and verify one `StartAsync` call with those values, no `ReviseAsync` call against the source, and a distinct saved session ID.

### Tests for User Story 2

- [X] T006 [P] [US2] Add a workspace-service test proving Go after restoration calls `StartAsync` exactly once with the edited query and Min/Max values and never calls `ReviseAsync` on the selected source in `BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs`.
- [X] T007 [P] [US2] Add a session-service persistence test proving a new-run `StartAsync` creates a distinct session record and saves its completed state in `BlogWriter.Tests/BlogWriterSessionServiceTests.cs`.
- [X] T008 [P] [US2] Add a browser test proving row selection does not run work and the Go button does, with the current edited values, in `BlogWriter.Web.Tests/WorkspaceBrowserTests.cs` and `BlogWriter.Web.Tests/HomePageTests.cs`.

### Implementation for User Story 2

- [X] T009 [US2] Update submission dispatch in `BlogWriter.Web/Services/BlogWorkspaceService.cs` and restored-pending state transitions in `BlogWriter.Web/Services/BlogWorkspaceState.cs` so Go starts a fresh session through `IBlogWriterSessionService.StartAsync`; never pass the selected historical session to `ReviseAsync`, and retain normal revision behavior after the new run becomes active.

**Checkpoint**: Go creates exactly one distinct history entry from the current fields; the previously selected entry remains unchanged.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validate regression, accessibility, persistence, and the complete user journey.

- [X] T010 Run focused and complete web/session tests with `dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj` and `dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj`.
- [X] T011 Build the affected web application with `dotnet build BlogWriter.Web/BlogWriter.Web.csproj`.
- [ ] T012 Execute the authenticated manual browser scenario in `specs/014-restore-session-for-editing/quickstart.md`, including keyboard row activation, no run before Go, invalid-range rejection, and verifying the new history entry does not replace the source.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No tasks; existing solution and test projects are ready.
- **Foundational (Phase 2)**: No tasks; existing workspace/session abstractions are sufficient.
- **User Story 1 (Phase 3)**: Starts immediately. Complete the restore-only tests before implementation; T003 and T004 can then proceed in parallel, followed by UI wiring in T005.
- **User Story 2 (Phase 4)**: Depends on the pending-restoration state and row-selection flow from US1. Complete T006-T008 before T009.
- **Polish (Phase 5)**: Depends on both user stories; run the tests/build and authenticated manual scenario.

### User Story Dependencies

- **US1 (P1)**: No dependency on another story; independently verifies row selection and inert restoration.
- **US2 (P1)**: Integrated flow depends on US1's restored workspace. Its service and persistence tests can use a seeded pending state independently.

### Parallel Opportunities

- In US1, T001 and T002 target separate test files and can run in parallel. After both tests are in place, T003 and T004 target separate implementation files and can run in parallel.
- In US2, T006, T007, and T008 target separate test files and can run in parallel after US1 is complete.
- T005 depends on the callback and restore method from T003/T004; T009 depends on the pending state from US1 and the failing US2 tests.

## Parallel Example: User Story 1

```text
Task T001: Add restore/no-invocation service tests in BlogWriter.Web.Tests/BlogWorkspaceServiceTests.cs
Task T002: Add clickable-row and no-numeric-input tests in BlogWriter.Web.Tests/WorkspaceBrowserTests.cs and BlogWriter.Web.Tests/HomePageTests.cs

After those tests fail against the current behavior:
Task T003: Implement accessible row selection in BlogWriter.Web/Components/SessionList.razor
Task T004: Implement pending restore behavior in BlogWriter.Web/Services/BlogWorkspaceService.cs and BlogWriter.Web/Services/BlogWorkspaceState.cs
```

## Implementation Strategy

### MVP

Both P1 stories are required for a feature-complete MVP: US1 makes restore safe and editable; US2 gives the user the explicit Go action and distinct saved result. US1 can be tested as an intermediate checkpoint but should not ship alone if it leaves the restored item without the specified new-run path.

### Incremental Delivery

1. Add and fail US1 tests, then implement accessible row selection and restore-only state.
2. Add and fail US2 tests, then route Go through the existing new-session operation.
3. Run web and session-service test suites, build the web project, and complete the authenticated quickstart scenario.

## Notes

- `[P]` tasks touch different files and have no dependency on incomplete tasks.
- Every task line uses the required checkbox, sequential ID, optional parallel marker, story label where applicable, and concrete file path.
- No task changes `CosmosBlogSessionStore.cs`, `IBlogSessionStore.cs`, the session document schema, hosted agents, or MAF workflow topology; the existing `StartAsync` path already creates and saves a new session.
- T012 remains open because the local app redirects to Microsoft Entra sign-in; no authenticated manual browser run was performed.

## Phase 6: Convergence

**Purpose**: Complete the authenticated manual verification not covered by automated component and service tests.

- [ ] T013 Complete the authenticated browser scenario in `specs/014-restore-session-for-editing/quickstart.md` after sign-in, verifying keyboard row selection, no workflow or history change before Go, editable query and word limits, invalid-range rejection, and a distinct new history entry that leaves the source unchanged per FR-010, US1/AC3, US1/AC4, US2/AC2, and T012 (partial).
