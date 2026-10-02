# UX and State Requirements Checklist: Word Count Controls

**Purpose**: Review the completeness, clarity, consistency, and measurability of the word-count control requirements before implementation
**Created**: 2026-09-20
**Feature**: [spec.md](../spec.md)

**Review Ownership**: This checklist is a reviewer-owned requirements-quality artifact. Mark an item `[x]` only when review determines that the requirement-quality criterion is satisfied. Checked items do not represent completed implementation work.

## Requirement Completeness

- [ ] CHK001 Are requirements defined for the Min and Max controls in every relevant workspace mode: new, draft, saved-session list, loaded session, processing, and ended? [Completeness] [Gap] [Spec §FR-001–FR-002]
- [ ] CHK002 Are both initialization and New-action reset requirements documented for visible values and the accepted baseline? [Completeness] [Spec §FR-003–FR-004, §FR-016]
- [ ] CHK003 Are requirements complete for applying the range to both initial prompts and revision requests? [Completeness] [Spec §FR-009, §FR-011]
- [ ] CHK004 Are saved-session requirements defined for stored ranges, legacy missing ranges, and invalid persisted ranges? [Coverage] [Gap] [Spec §FR-010]
- [ ] CHK005 Are requirements documented for range state after successful completion, workflow failure, cancellation, and concurrency conflict? [Completeness] [Spec §FR-012–FR-013]
- [ ] CHK006 Are discard-confirmation requirements complete for New, List, and Quit when only the word range is unsaved? [Completeness] [Spec §FR-016–FR-018]

## Requirement Clarity

- [ ] CHK007 Is the term "compact" quantified sufficiently to avoid conflicting control-size interpretations? [Ambiguity] [Spec §FR-001]
- [ ] CHK008 Is the exact visual, reading, and keyboard-order placement of Min and Max unambiguous relative to both prompt inputs and both content panes? [Clarity] [Spec §FR-002]
- [ ] CHK009 Are "positive whole number" and whitespace normalization defined consistently for empty, decimal, signed, and non-numeric text? [Clarity] [Spec §FR-005, Edge Cases]
- [ ] CHK010 Is the behavior for values beyond the supported integer representation explicitly defined despite the absence of a product upper limit? [Gap] [Spec §FR-005, Assumptions]
- [ ] CHK011 Is the phrase "unexpectedly replace" replaced or supported by exact state-transition rules for visible and accepted values? [Ambiguity] [Spec §FR-013]
- [ ] CHK012 Is "accepted range" defined clearly enough to distinguish defaults, loaded values, submitted snapshots, and unsaved edits? [Clarity] [Spec §FR-016–FR-018, Key Entities]

## Requirement Consistency

- [ ] CHK013 Are default values consistently specified as Min 1000 and Max 2000 across stories, requirements, success criteria, and assumptions? [Consistency] [Spec §FR-003–FR-004, §SC-001]
- [ ] CHK014 Is Max-equal-to-Min acceptance consistent with the Max-at-least-Min validation rule throughout the specification? [Consistency] [Spec §FR-006, Edge Cases]
- [ ] CHK015 Are processing-time edit requirements consistent with accepted-baseline updates after successful submission? [Consistency] [Spec §FR-012–FR-013, §FR-018]
- [ ] CHK016 Are New reset requirements consistent with the requirement to request confirmation before discarding a changed range? [Consistency] [Spec §FR-004, §FR-016–FR-017]
- [ ] CHK017 Are saved-session loading requirements consistent with the assumption that existing session state already persists Min and Max? [Consistency] [Spec §FR-010, Assumptions]
- [ ] CHK018 Are initial and revision validation requirements identical unless a deliberate difference is explicitly documented? [Consistency] [Spec §FR-007–FR-011]

## Acceptance Criteria Quality

- [ ] CHK019 Can each invalid-input class in SC-003 be objectively mapped to a required corrective message or invalid field? [Measurability] [Spec §SC-003, §FR-008]
- [ ] CHK020 Does SC-002 distinguish exact session persistence from merely retaining values in the visible workspace? [Clarity] [Spec §SC-002]
- [ ] CHK021 Does SC-004 define what qualifies as stored targets being "unavailable" and how that condition is established? [Ambiguity] [Spec §SC-004, §FR-010]
- [ ] CHK022 Are the mobile and desktop layout outcomes objectively measurable at both specified viewport sizes? [Acceptance Criteria] [Spec §SC-005, §FR-014]
- [ ] CHK023 Is the accessibility success criterion tied explicitly to the workspace's existing WCAG 2.2 Level AA requirement? [Traceability] [Spec §SC-006, §FR-015]
- [ ] CHK024 Does SC-007 separately cover confirmation required, confirmation declined, and no-confirmation-needed outcomes? [Acceptance Criteria] [Spec §SC-007]

## Scenario and Edge-Case Coverage

- [ ] CHK025 Are primary-flow requirements complete from entering a range through submission, workflow completion, persistence, and redisplay? [Coverage] [Spec User Stories 1–2]
- [ ] CHK026 Are alternate-flow requirements defined for equal Min/Max values and harmless surrounding whitespace? [Coverage] [Spec Edge Cases]
- [ ] CHK027 Are exception requirements defined for invalid input correction without losing prompt, draft, review, or active-session state? [Coverage] [Gap] [Spec User Story 3]
- [ ] CHK028 Are recovery requirements defined when a saved session contains missing, zero, negative, reversed, or otherwise corrupt range values? [Coverage] [Gap] [Spec §FR-010]
- [ ] CHK029 Are concurrent-edit requirements complete when one or both visible entries change during an in-flight successful, failed, or cancelled operation? [Coverage] [Spec §FR-012–FR-013]
- [ ] CHK030 Are list-mode requirements clear about whether Min and Max remain visible and which accepted baseline applies before a saved session is selected? [Coverage] [Gap] [Spec §FR-002, §FR-010]

## Non-Functional Requirements

- [ ] CHK031 Are keyboard requirements defined for reaching, editing, and leaving both controls without altering existing Enter/Shift+Enter prompt behavior? [Accessibility] [Spec §FR-002, §FR-015]
- [ ] CHK032 Are programmatic labels, invalid-state semantics, error associations, and announcement behavior all explicitly required? [Accessibility] [Spec §FR-015, Edge Cases]
- [ ] CHK033 Are responsive requirements sufficient to prevent label clipping, input overflow, and overlap at both required viewport dimensions? [Responsive Design] [Spec §FR-014, §SC-005]
- [ ] CHK034 Is local validation required to occur before any workflow or persistence operation, with no additional external request? [Performance/Cost] [Plan §Performance Goals]

## Dependencies and Assumptions

- [ ] CHK035 Is reliance on existing `ResearchState` persistence documented and validated as a dependency rather than an unverified assumption? [Dependency] [Spec Assumptions]
- [ ] CHK036 Are compatibility requirements defined for existing saved sessions, console defaults, session ownership, token budgets, and workflow termination? [Dependency] [Gap] [Plan §Constraints]
- [ ] CHK037 Is the absence of a separate preference record clearly scoped so no cross-session default preference is implied? [Scope] [Spec Assumptions]
- [ ] CHK038 Are requirements explicit that changing visible values alone does not persist or alter the active saved session? [Clarity] [Spec §FR-018, Assumptions]

## Ambiguities and Conflicts

- [ ] CHK039 Is there any conflict between allowing edits during processing and the existing workspace behavior that disables prompt inputs while processing? [Conflict] [Spec Edge Cases]
- [ ] CHK040 Are the terms "accepted range," "submitted range," "stored range," and "visible range" used consistently or formally distinguished? [Terminology] [Spec Key Entities]

## Notes

- Mark items `[x]` only after reviewer evaluation confirms that the referenced requirements are sufficiently complete, clear, consistent, and measurable.
- Leave unresolved items unchecked and record findings inline or in review comments.
- `/speckit-implement` reads custom checklist state as a gate and must not modify these markers.
- This checklist evaluates requirements writing; it does not verify implementation behavior.
