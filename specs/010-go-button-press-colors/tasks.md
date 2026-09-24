---

description: "Task list template for feature implementation"
---

# Tasks: Go Button Press Color Feedback

**Input**: Design documents from `/specs/010-go-button-press-colors/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification. This feature is a CSS-only visual change; bUnit cannot evaluate `:active` pseudo-class rendering, so verification relies on (a) existing regression tests confirming markup/class/disabled behavior is unchanged, and (b) the manual visual checks in `quickstart.md`. No new automated test tasks are generated.

**Organization**: This feature has a single user story (US1, P1). Tasks are grouped by phase; the User Story 1 phase is also the MVP and the entire scope of this feature.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1)
- Include exact file paths in descriptions

## Path Conventions

- Single existing Blazor web project: `BlogWriter.Web/` (app) and `BlogWriter.Web.Tests/` (tests) — see `plan.md` Project Structure.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing project builds cleanly before making the change (no new project scaffolding needed — this feature reuses the existing `BlogWriter.Web` project).

- [ ] T001 Build the existing web project to confirm a clean baseline: run `dotnet build BlogWriter.Web\BlogWriter.Web.csproj` from the repository root.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: N/A for this feature — there is no shared infrastructure, model, or framework change required before implementing the single user story. This phase is intentionally empty; proceed directly to Phase 3.

**Checkpoint**: Foundation ready (nothing to build) — User Story 1 implementation can begin immediately.

---

## Phase 3: User Story 1 - Visual confirmation that Go was activated (Priority: P1) 🎯 MVP

**Goal**: The existing "Go" command-bar button (`.range-go` in `BlogWriter.Web/Components/CommandBar.razor`) shows a light green background when idle and switches to a dark green background while actively pressed (mouse, touch, or keyboard), reverting to light green on release — without ever showing the pressed color while disabled.

**Independent Test**: Run the app, observe the Go button's default light green background, press and hold it (mouse/touch/keyboard) and confirm it turns dark green, release/drag-off and confirm it returns to light green, then disable it and confirm it never shows the dark green pressed color. Fully covered by `quickstart.md`.

### Implementation for User Story 1

- [X] T002 [US1] In `BlogWriter.Web/wwwroot/app.css`, add an `.range-go:active:not(:disabled)` rule setting `background: var(--forest-dark);` immediately after the existing `.range-go:hover:not(:disabled)` rule (around line 130), so a mouse, touch, or keyboard press on the enabled Go button shows the dark green pressed color per FR-002/FR-004.
- [X] T003 [US1] In `BlogWriter.Web/wwwroot/app.css`, add an `.range-go:active:disabled` rule (or extend the existing `.range-go:disabled` rule) that explicitly keeps the disabled background (`#ecece7`) so a disabled Go button never shows the dark green pressed color even under an `:active` press attempt, per FR-005.
- [X] T004 [US1] Visually confirm in `BlogWriter.Web/Components/CommandBar.razor` that the `.range-go` button's markup, label ("Go"), and `disabled` binding are unchanged — this task is a review/no-op check, not a code edit, to satisfy FR-006 (no layout/label/position change).
- [X] T005 [US1] Run `dotnet test BlogWriter.Web.Tests\BlogWriter.Web.Tests.csproj` to confirm existing bUnit coverage of the command bar / Go button (class name, disabled attribute, markup) still passes unchanged after the CSS edit.
- [ ] T006 [US1] Perform the manual validation steps in `specs/010-go-button-press-colors/quickstart.md` (idle, mouse-press, drag-off, keyboard-press, touch-press if available, disabled) and confirm every acceptance scenario and edge case in `spec.md` passes. **Partially verified only**: static/served-CSS review confirmed via `curl` that the `.range-go:active:not(:disabled)` / `.range-go:active:disabled` rules are present, correctly ordered, and served; true interactive click-and-hold browser verification could not be completed in this sandbox because the app requires Microsoft Entra ID sign-in (the home page returns HTTP 302 to the auth flow with no real credentials configured). A human with sign-in access should complete the remaining interactive checks before merging.

**Checkpoint**: At this point, User Story 1 is fully functional and independently testable — this is the entire feature scope (single-story MVP).

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Final documentation/regression pass; no cross-cutting code beyond User Story 1 exists for this feature.

- [ ] T007 [P] Re-run `specs/010-go-button-press-colors/quickstart.md` end-to-end once more after all changes are in place, confirming SC-001 through SC-004 in `spec.md` are met and no other command-bar button's appearance changed. Deferred to a human reviewer for the same reason as T006 (requires interactive sign-in and a real browser to observe `:active` rendering).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately (T001).
- **Foundational (Phase 2)**: Empty for this feature — does not block Phase 3.
- **User Story 1 (Phase 3)**: Depends on Phase 1 (build baseline) completing; T002 and T003 touch the same file (`app.css`) so they are sequential, not parallel; T004–T006 depend on T002/T003 being complete.
- **Polish (Phase 4)**: Depends on Phase 3 (T002–T006) being complete.

### User Story Dependencies

- **User Story 1 (P1)**: The only user story in this feature; no dependencies on other stories.

### Within User Story 1

- T002 (add pressed-state rule) before T003 (add disabled-precedence guard) — both edit the same file/selector block, so they must be done in order to avoid rule-ordering conflicts.
- T004 (markup/label review) can be done any time after T002/T003, in parallel with T005 if desired.
- T005 (automated regression test run) and T006 (manual quickstart validation) both depend on T002 and T003 being complete, but are independent of each other and can run in parallel.

### Parallel Opportunities

- T002 and T003 are NOT parallel (same file, same selector block — sequential edits).
- T005 and T006 CAN run in parallel once T002/T003 are complete (one runs `dotnet test`, the other is a manual browser check).
- T007 has no parallel counterpart in this feature (single polish task) but is marked [P] as it touches no other in-flight task.

---

## Parallel Example: User Story 1

```bash
# After T002 and T003 (sequential CSS edits) are complete, run these together:
Task: "Run dotnet test BlogWriter.Web.Tests\BlogWriter.Web.Tests.csproj (T005)"
Task: "Perform manual quickstart.md validation in a browser (T006)"
```

---

## Implementation Strategy

### MVP First (and Only) Scope

1. Complete Phase 1: Setup (T001).
2. Skip Phase 2: Foundational (empty for this feature).
3. Complete Phase 3: User Story 1 (T002–T006).
4. **STOP and VALIDATE**: Confirm quickstart.md scenarios pass.
5. Complete Phase 4: Polish (T007) and consider the feature done — there is no further increment planned beyond this single user story.

## Notes

- [P] tasks = different files, no dependencies; T002/T003 are intentionally NOT marked [P] because they edit adjacent rules in the same file/selector block.
- [Story] label maps every implementation task to US1 for traceability.
- This feature has only one user story, so there is no incremental multi-story delivery plan — Phase 3 is both the MVP and the complete feature.
- Commit after T002/T003 (the CSS change) and again after T005/T006 (verification) to keep history reviewable.
