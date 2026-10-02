# Quickstart: Word Count Controls

## Prerequisites

- Existing BlogWriter web implementation and test projects
- .NET 10 SDK
- No live Foundry, Cosmos, or Entra dependency for focused tests

## Focused automated validation

From the repository root:

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj --filter "FullyQualifiedName~WordRangeTests|FullyQualifiedName~BlogWriterSessionServiceTests"
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj --filter "FullyQualifiedName~WordRangeInputTests|FullyQualifiedName~HomePageTests|FullyQualifiedName~BlogWorkspaceServiceTests|FullyQualifiedName~WorkspaceBrowserTests"
```

Expected outcome: parsing, validation, defaults, session propagation, accepted-baseline tracking, unsaved confirmation, component placement, responsive layout, and accessibility checks pass without live model calls.

## Manual workspace validation

1. Start the web application using its existing HTTPS launch profile and sign in.
2. Confirm `Min` and `Max` appear after the prompt inputs and before Draft and Reviewer.
3. Confirm a new workspace displays `1000` and `2000`.
4. Enter a new valid range, submit an initial prompt, and confirm the resulting saved session retains the range.
5. Load the session from List and confirm its stored Min and Max are restored.
6. Change the range, submit a revision, and confirm the revised session retains the new range.
7. Try empty, text, decimal, zero, negative, and Max-below-Min values; confirm no writing operation begins and the correct field is identified.
8. Confirm equal Min and Max values are accepted.
9. Change a range without submitting, choose New/List/Quit, and confirm the discard dialog appears.
10. Decline the dialog and confirm all values remain unchanged; accept it and confirm the selected action proceeds.
11. Start an operation, change the visible range while it runs, and confirm the completed session uses the submitted snapshot while the later edits remain visible and unsaved.

## Responsive and accessibility validation

At 390 × 844 and 1440 × 900:

- Min and Max labels and entries remain visible without overlap or horizontal scrolling.
- Keyboard order moves from prompt inputs to Min, Max, then Draft and Reviewer content.
- Each label is announced with its field.
- Invalid state and correction messages are announced and associated with the appropriate field.
- Existing command controls and content panes retain their prior layout and accessibility behavior.

## Full regression

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj
dotnet test BlogWriter.Web.Tests/BlogWriter.Web.Tests.csproj
dotnet build BlogWriter.csproj
dotnet build BlogWriter.Web/BlogWriter.Web.csproj
```

The console defaults, saved-session compatibility, workflow topology, token cap, owner isolation, authentication, and cancellation behavior must remain unchanged.

## MAF Doctor baseline

The post-implementation scan remains grade F with 4 errors, 3 warnings, 0
silent-starvation risks, and 6 heuristic uncapped-call matches. These counts are
unchanged from the pre-implementation baseline; the word-range feature introduces no
new MAF call, prompt, or workflow-topology finding.
