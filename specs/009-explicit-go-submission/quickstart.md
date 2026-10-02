# Quickstart: Validate Explicit Go Submission

## Prerequisites

- .NET 10 SDK installed.
- Restore dependencies from the repository root.

## Focused Automated Validation

Run the focused web tests:

```powershell
dotnet test .\BlogWriter.Web.Tests\BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~HomePageTests|FullyQualifiedName~BlogWorkspaceServiceTests"
```

Build the affected web project:

```powershell
dotnet build .\BlogWriter.Web\BlogWriter.Web.csproj
```

## Manual Validation

1. Start a new workspace and confirm the revision request and revision command are disabled while the displayed draft is empty.
2. Enter text in the initial prompt, press Enter, and confirm no processing begins.
3. Activate Go and confirm one draft operation begins using the selected Min and Max values.
4. After a draft appears, confirm the revision request and revision command become enabled.
5. Enter revision instructions, press Enter, and confirm no processing begins.
6. Activate Go and confirm the revision operation begins rather than a new draft operation.
7. Clear the displayed draft and confirm revision controls become disabled while retaining any existing revision text.

See [workspace-ui.md](contracts/workspace-ui.md) for the UI behavior contract and [data-model.md](data-model.md) for state transitions.