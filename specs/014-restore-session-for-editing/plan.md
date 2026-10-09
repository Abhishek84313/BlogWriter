# Implementation Plan: Restore Saved Session for Editing
**Branch**: `014-restore-session-for-editing` | **Date**: 2026-10-09 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification for selecting a saved session as an editable, pending new run.

## Summary

Replace numeric saved-session selection with direct row selection. A selected session restores its prompt context and word range into an editable workspace without invoking the workflow. Go submits through the existing new-session path so the source record is not updated and the new run receives its own history entry.

## Technical Context
**Language/Version**: C# / .NET 10

**Primary Dependencies**: Blazor Interactive Server, existing `IBlogWriterSessionService`, Microsoft Agent Framework workflow abstraction, Azure Cosmos DB session store

**Storage**: Existing owner-partitioned Cosmos session documents; no schema change

**Testing**: xUnit and bUnit in `BlogWriter.Web.Tests`; existing service/store tests in `BlogWriter.Tests`

**Target Platform**: Authenticated browser workspace hosted by ASP.NET Core

**Project Type**: Existing Blazor web application with shared session/workflow services

**Performance Goals**: Preserve the existing maximum of 20 displayed sessions; selecting a row loads only that session; no model call occurs before Go.

**Constraints**: Keep owner-scoped reads, cancellation, validation, token-budget enforcement, and workflow termination. A restored submission must use the new-session path, never revise the selected historical record.

**Scale/Scope**: One current user's displayed saved-session list and one new run per explicit Go submission. No search, paging, or schema migration.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Constitution gate | Status | Evidence |
|---|---|---|
| Hosted-agent boundaries | PASS | Reuse the existing session service and workflow; no hosted-agent code, endpoint, or prompt changes. |
| MAF-native workflow composition | PASS | Go uses the existing `StartAsync`/workflow path; no new workflow topology or direct model calls. |
| Identity, secrets, and budget control | PASS | Use owner-scoped `LoadAsync` and the existing shared credential/token-budget wiring. |
| Testable and observable behavior | PASS | Keep restore behavior in the workspace service and cover it with unit and browser-component tests. |
| Simple, compatible evolution | PASS | Keep existing session document and store contracts; only add a new record through the existing creation path. |

**Post-design re-check**: PASS. The design retains the existing contracts and data schema and delegates persistence and workflow execution to current abstractions.

## Project Structure
### Documentation (this feature)

```text
specs/014-restore-session-for-editing/
├── plan.md
├── research.md
├── data-model.md
├── contracts/workspace-interactions.md
└── quickstart.md
```

### Source Code (repository root)

```text
BlogWriter.Web/
├── Components/
│   ├── CommandBar.razor
│   ├── Pages/Home.razor
│   └── SessionList.razor
└── Services/
    ├── BlogWorkspaceService.cs
    └── BlogWorkspaceState.cs
BlogWriter.Web.Tests/
├── BlogWorkspaceServiceTests.cs
├── HomePageTests.cs
└── WorkspaceBrowserTests.cs
BlogWriterSessionService.cs
IBlogWriterSessionService.cs
IBlogSessionStore.cs
CosmosBlogSessionStore.cs
```

**Structure Decision**: Keep the behavior in the existing web workspace and shared session-service boundaries. `SessionList` emits a selected summary; `BlogWorkspaceService` loads and stages it; the existing session service creates and saves the new run. No new project, public API, or Cosmos document shape is needed.

## Complexity Tracking

No constitution violations or added architectural complexity.
