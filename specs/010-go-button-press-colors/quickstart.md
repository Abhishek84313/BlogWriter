# Quickstart: Validate Go Button Press Color Feedback

## Prerequisites

- .NET 10 SDK installed
- Repository checked out on branch `010-go-button-press-colors`
- No additional packages or services required (pure front-end CSS change)

## Setup

```powershell
cd E:\ai\.net\blog\BlogWriter
dotnet build BlogWriter.Web\BlogWriter.Web.csproj
```

## Automated check (regression only)

Run the existing Blazor component tests to confirm the Go button's markup, class name,
and disabled behavior are unchanged (bUnit does not evaluate `:active` CSS, so this does
not verify the color itself — see manual check below):

```powershell
dotnet test BlogWriter.Web.Tests\BlogWriter.Web.Tests.csproj
```

Expected: all existing tests referencing the command bar / Go button continue to pass
with no changes required to the test assertions (see `contracts/workspace-ui.md` for the
behavior these tests should continue to cover).

## Manual validation (visual)

1. Run the web app:
   ```powershell
   dotnet run --project BlogWriter.Web\BlogWriter.Web.csproj
   ```
2. Open the app in a browser and locate the **Go** button in the command bar (after the
   word-range Min/Max fields).
3. **Idle**: Confirm the Go button shows a light green background (`--forest`) when not
   hovered or pressed.
4. **Pressed — mouse**: Click and hold the mouse button down on Go. Confirm the background
   switches to dark green (`--forest-dark`) while held down, and reverts to light green on
   release.
5. **Pressed — drag off**: Press down on Go, then drag the pointer off the button before
   releasing. Confirm the button returns to light green and does not stay stuck in the
   dark green pressed state.
6. **Pressed — keyboard**: Tab to focus the Go button, then press and hold Space or press
   Enter. Confirm the same dark green pressed feedback appears.
7. **Pressed — touch** (if a touch device/emulator is available): Touch and hold the Go
   button. Confirm the same dark green pressed feedback appears, and reverts on release.
8. **Disabled**: Trigger a state where Go is disabled (e.g. per existing validation rules
   in the word-range row) and attempt to press it. Confirm it never shows the dark green
   pressed color and instead keeps its existing disabled (muted) appearance.

## Expected Outcome

- All steps above match the acceptance scenarios and edge cases in
  `spec.md` (User Story 1, Edge Cases).
- No other command-bar button's appearance or behavior changes.
- The Go button's label, size, position, and click/submission behavior are unchanged.
