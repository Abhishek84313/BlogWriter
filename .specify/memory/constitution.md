# BlogWriter Constitution
<!-- Governing principles for the BlogWriter console and hosted-agent services. -->

## Core Principles

### I. Hosted-Agent Boundaries
The console application is responsible for workflow orchestration and session handling. Blogger, Researcher, Author, and Reviewer remain independently deployable Azure AI Foundry Hosted Agents. The console must communicate with them through Microsoft Agent Framework abstractions and must not issue ad hoc raw HTTP calls or recreate their model logic in-process.

### II. MAF-Native Workflow Composition
Workflow stages must use Microsoft Agent Framework agents, executors, message handlers, and workflow edges. State transitions must be explicit through `ResearchState`; revision routing must remain bounded and terminating. New orchestration behavior requires tests that cover the affected route or state transition.

### III. Identity, Secrets, and Budget Control
All Azure and Foundry access uses Microsoft Entra ID credentials. API keys and secrets must not be committed or embedded in source. Every model call must respect the shared token budget, and budget breaches must stop the workflow rather than silently continuing or retrying without bounds.

### IV. Testable and Observable Behavior
Business logic must remain unit-testable independently of live Foundry services by accepting the relevant interfaces or test clients. Changes to agent adapters, state handling, persistence, or workflow routing must include focused tests. Production paths must preserve structured logging, cancellation, and useful tracing around workflow and agent calls.

### V. Simple, Compatible Evolution
Prefer the smallest change that preserves existing public interfaces, session formats, prompt contracts, and deployment conventions. Avoid speculative abstractions. When a breaking change is necessary, document the migration and update the affected tests and deployment documentation in the same change.

## Technical Constraints

- Target .NET 10 and preserve nullable-reference-type correctness.
- Use the repository's existing MAF and Azure SDK patterns before introducing new infrastructure or transport code.
- Keep hosted-agent prompts and the shared prompt library aligned when either changes.
- Treat prerelease hosted-agent packages and deployment configuration as compatibility-sensitive; validate builds and relevant tests after upgrades.

## Development Workflow

- Before implementation, identify the owning workflow or agent boundary and state the expected behavior in the feature specification.
- For code changes, run the narrowest relevant unit tests first, then build the affected project; broaden validation when shared contracts or deployment files change.
- Pull requests must describe authentication, token-budget, workflow-termination, and hosted-agent impact when those areas are touched.
- Deployment changes must be validated with the applicable `azd` or Foundry checks before release.

## Governance

This constitution governs feature specifications, plans, implementation tasks, code review, and deployment changes for BlogWriter. Existing repository instructions and Microsoft Agent Framework constraints remain mandatory; where they are more specific than this document, the more specific rule applies. Any violation must be called out in the plan or review and either corrected or explicitly justified.

Amendments require an update to this file, a concise rationale, and any required migration or documentation changes. The constitution should be reviewed when the deployment model, agent boundary, authentication model, or workflow runtime changes.

**Version**: 1.0.0 | **Ratified**: 2026-09-16 | **Last Amended**: 2026-09-16
