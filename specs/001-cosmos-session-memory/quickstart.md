# Quickstart: Validate Long-Term Session Memory

## Prerequisites

- .NET 10 SDK.
- An Azure subscription with the Bicep resources for the session store deployed.
- A Microsoft Entra user with the configured Cosmos data-plane role.
- Console settings for the Cosmos endpoint, database, and container, set through user secrets or environment variables; no account key or connection string.
- Existing Foundry console settings from [docs/configuration.md](../../../docs/configuration.md).

## Focused Automated Validation

Run the session-store test slice:

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj --filter "FullyQualifiedName~BlogSessionStore"
```

Expected result: tests cover round-trip persistence, owner-scoped listing limited to 20 and ordered by latest update, not-found handling, invalid data handling, and ETag conflict rejection without a live Cosmos dependency.

## Manual End-to-End Validation

1. Start the console application under an Entra account that has access to the session store.
2. Enter two distinct topics and complete or pause both sessions.
3. Restart the console, enter `list`, and confirm that no more than 20 sessions are displayed newest first, with their identifiers and original questions.
4. Resume one listed session with `resume <session-id>`, submit a follow-up, and confirm the original question and earlier refinement context are used.
5. Start two console instances, load the same session, save a follow-up in the first, then attempt to save a follow-up in the second.

Expected result: the first save persists. The second is rejected with a reload instruction, and the first session state remains intact after re-listing and resuming.

## Infrastructure Validation

Build the infrastructure after the Bicep module is added:

```powershell
az bicep build --file infra/main.bicep
```

Expected result: the template compiles and provisions an Entra-authenticated Cosmos session store with the expected container partitioning and data-plane role assignment.