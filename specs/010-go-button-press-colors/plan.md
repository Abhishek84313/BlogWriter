# Implementation Plan: Go Button Press Color Feedback

**Branch**: `010-go-button-press-colors` | **Date**: 2026-09-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/010-go-button-press-colors/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Add visible press-state feedback to the existing `Go` command-bar button: it already renders with a light-green background (`--forest`) and darkens (`--forest-dark`) on hover, but has no distinct feedback while actually pressed/activated. The fix is a CSS-only addition of an `:active` (and disabled-safe) style rule on `.range-go` in `BlogWriter.Web/wwwroot/app.css`, using the existing `--forest` / `--forest-dark` custom properties, with no Razor markup or C# behavior changes required.

## Technical Context

**Language/Version**: C# / .NET 10 (Blazor Web App, `BlogWriter.Web`); plain CSS for styling

**Primary Dependencies**: ASP.NET Core Blazor (existing `CommandBar.razor` component); no new packages

**Storage**: N/A (purely presentational; no persisted state)

**Testing**: bUnit component tests in `BlogWriter.Web.Tests` (e.g. `HomePageTests.cs`) for markup/class assertions; manual/visual verification for `:active` pseudo-class behavior, which bUnit cannot exercise directly since it does not run real CSS pseudo-class matching

**Target Platform**: Web browser (desktop and touch), served by the Blazor Web App

**Project Type**: Web application — single existing Blazor project (`BlogWriter.Web`) plus its test project; no frontend/backend split to introduce

**Performance Goals**: N/A beyond standard browser rendering (color change must be perceptible within a single frame of press/release, per SC-001)

**Constraints**: Must not alter the button's label, size, position, disabled semantics, or keyboard/pointer accessibility; must preserve sufficient contrast between idle and pressed colors (FR-007)

**Scale/Scope**: Single UI element (`.range-go` in the command bar); no other buttons, pages, or components affected

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Hosted-Agent Boundaries** — N/A. No hosted-agent, workflow, or console-orchestration code is touched; this is a Blazor UI styling change confined to `BlogWriter.Web`.
- **II. MAF-Native Workflow Composition** — N/A. No agents, executors, or workflow edges/states are involved.
- **III. Identity, Secrets, and Budget Control** — N/A. No Azure/Foundry calls, credentials, or token budget are affected.
- **IV. Testable and Observable Behavior** — Satisfied. The change is small enough to verify via existing bUnit component tests (asserting the button still renders with its expected class/attributes and disabled behavior is unchanged) plus a manual/visual check of the new `:active` color, since bUnit does not evaluate CSS pseudo-classes.
- **V. Simple, Compatible Evolution** — Satisfied. This is the smallest possible change (a CSS rule addition reusing existing `--forest`/`--forest-dark` tokens) that preserves the existing markup, component API, and all other button behavior.

No violations identified. Complexity Tracking section is not needed.

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

```text
BlogWriter.Web/
├── Components/
│   └── CommandBar.razor          # Renders the .range-go "Go" button (markup unchanged)
└── wwwroot/
    └── app.css                   # .range-go rule set: add :active / :active:disabled styling here

BlogWriter.Web.Tests/
└── HomePageTests.cs              # Existing bUnit coverage of the command bar / Go button markup
```

**Structure Decision**: This feature stays entirely inside the existing single Blazor web
project `BlogWriter.Web` and its companion test project `BlogWriter.Web.Tests`. No new
projects, folders, or cross-project contracts are introduced; the only source change is a
CSS rule addition in `BlogWriter.Web/wwwroot/app.css` for the `.range-go` selector already
used by `CommandBar.razor`.

## Complexity Tracking

*No Constitution Check violations were identified; this section is not applicable.*
