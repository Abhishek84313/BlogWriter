# Quickstart: Single Review Round

## Prerequisites

- .NET 10 SDK
- Repository dependencies restored
- No live Foundry service is required; the focused tests use test doubles for Blogger, Researcher, Author, and Reviewer.

## Focused validation

From the repository root:

```powershell
dotnet test .\BlogWriter.Tests\BlogWriter.Tests.csproj --filter FullyQualifiedName~BlogWorkflowTests --no-restore -p:BaseOutputPath=.\obj\single-review-test\
```

Expected result: all `BlogWorkflowTests` pass.

## Scenarios to verify

1. **Initial approval**
   - Reviewer returns `APPROVED` for the initial draft.
   - Expected: the initial draft is emitted, the reviewer is called once, and the author is called once.

2. **Initial rejection and one revision**
   - Reviewer returns revision feedback for the initial draft.
   - Expected: the author receives the feedback, produces one revised draft, the revised draft is emitted, the reviewer is still called only once, and the author is called twice.

3. **Revision fallback**
   - The author returns no replacement content on the revision pass.
   - Expected: the latest available draft remains final and no second review is attempted.

## Broader validation

After the focused tests pass, run the affected project test suite:

```powershell
dotnet test .\BlogWriter.Tests\BlogWriter.Tests.csproj --no-restore -p:BaseOutputPath=.\obj\single-review-test\
```

The implementation should also be checked with the repository's MAF workflow topology simulation to confirm that both terminal output sources and the conditional edges complete without silent starvation.
