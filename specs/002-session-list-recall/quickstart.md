# Quickstart: Session List and Recall

## Prerequisites

- .NET 10 SDK
- Repository dependencies restored
- A configured session store and signed-in identity for live console checks

## Focused automated validation

From the repository root:

```powershell
dotnet test BlogWriter.Tests/BlogWriter.Tests.csproj --filter "FullyQualifiedName~SessionCommandParserTests|FullyQualifiedName~SessionListFormattingTests"
```

Expected outcome: parser, numbered formatting, valid numeric selection, invalid selection, and identifier compatibility tests pass.

## Manual console validation

1. Start the console application with the existing configuration.
2. Enter `list`.
3. Confirm each saved session begins with `[1]`, `[2]`, and so on in the displayed order.
4. Enter `resume 1`.
5. Confirm the first displayed session becomes active and its saved workflow state is used.
6. Repeat with a multi-digit entry if available, such as `resume 10`.
7. Enter `resume 0`, `resume -1`, `resume 999`, and `resume not-a-number` where appropriate.
8. Confirm each invalid selection reports an error and does not replace the active session.
9. Enter `resume <existing-session-id>` and confirm the raw identifier is rejected with a numeric-selection message.
10. Run `list` with no saved sessions and confirm no numbered entries are printed.

## Expected behavior

- Numbers are transient labels for the current list, not persisted session identifiers.
- A numeric selection is resolved against a fresh current list.
- Storage ownership, ordering, and workflow execution behavior remain unchanged; raw session identifiers are not accepted as user recall input.
