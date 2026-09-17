# Implementation Plan: Long-Term Session Memory

**Branch**: `001-cosmos-session-memory` | **Date**: 2026-09-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification for Cosmos-backed user session persistence, discovery, and resumption.

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Replace the local JSON-only session store with a Cosmos DB for NoSQL-backed `IBlogSessionStore` implementation. Persist each complete `BlogSession` under its signed-in Microsoft Entra object ID, expose a console command that lists the owner's 20 newest sessions, and retain the existing `resume <session-id>` and follow-up workflows. Use optimistic concurrency so a stale client cannot overwrite a newer saved session.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 10

**Primary Dependencies**: Azure.Identity; Microsoft.Agents.AI.Foundry (existing); Microsoft.Azure.Cosmos (new)

**Storage**: Azure Cosmos DB for NoSQL; one versioned session document per session, partitioned by Microsoft Entra object ID

**Testing**: xUnit unit tests using a new store abstraction/test double; no live Foundry or Cosmos account required for the focused suite

**Target Platform**: Windows console application for local execution; Azure Cosmos DB for durable session storage

**Project Type**: Console application with remotely hosted MAF agents

**Performance Goals**: List the 20 newest sessions for a user and select one within 30 seconds; resume 99% of representative existing sessions without data loss or substitution

**Constraints**: Entra ID only, no account keys or connection strings; preserve the shared model-call token cap; retain cancellation and structured tracing; reject stale concurrent saves; do not alter hosted-agent workflows

**Scale/Scope**: The first release lists at most 20 sessions per user. Search, pagination, sharing, manual deletion, and migrations of existing local JSON sessions are out of scope.

## Constitution Check

*GATE: Passed before Phase 0 research. Re-checked after Phase 1 design: passed.*

| Principle | Plan response | Status |
| ----------- | --------------- | -------- |
| Hosted-Agent Boundaries | Changes are limited to console-owned session persistence and commands; no hosted-agent code or raw agent HTTP calls. | Pass |
| MAF-Native Workflow Composition | Existing `ResearchState` and bounded follow-up routing remain the workflow source of truth. | Pass |
| Identity, Secrets, and Budget Control | Cosmos uses Entra tokens and Cosmos data-plane RBAC; endpoint/database/container names are configuration values; no model-call behavior changes. | Pass |
| Testable and Observable Behavior | Store behavior is behind `IBlogSessionStore`, with focused unit tests and persistence-operation tracing/logging. | Pass |
| Simple, Compatible Evolution | Extend the existing store interface minimally; keep the session ID resume command and state serialization compatible. | Pass |

MAF Doctor reports an existing F baseline caused by hosted-agent credential and token-cap findings. This plan neither changes nor depends on those files. MAF hard constraints applied here: no `DefaultAzureCredential` in production paths, no secret material in source, no session state stored in MAF provider instance fields, and no changes to workflow message-handler topology.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
./
├── BlogSession.cs                 # Add owner and concurrency metadata as required
├── IBlogSessionStore.cs           # Extend with owner-scoped session listing
├── CosmosBlogSessionStore.cs      # New Cosmos DB implementation
├── Program.cs                     # Build the Cosmos store and handle list command
├── BlogWriter.csproj              # Add Cosmos SDK dependency
├── BlogWriter.Tests/
│   ├── CosmosBlogSessionStoreTests.cs
│   └── ...
├── infra/
│   ├── main.bicep                 # Compose Cosmos resource and RBAC module
│   └── modules/cosmos-session-store.bicep
└── docs/
  ├── configuration.md
  └── deployment.md
```

**Structure Decision**: Keep the existing single-project console layout. Add one concrete store implementation alongside `FileBlogSessionStore`, using the existing interface as the dependency boundary; add one focused Bicep module for Cosmos resources and role assignment.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
| ----------- | ---------- | ------------------------------------- |
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
