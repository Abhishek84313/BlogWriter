# MAF Doctor Assessment

**Assessment date:** 2026-10-02  
**Repository:** BlogWriter  
**MAF Doctor:** 1.18.0

## Summary

MAF Doctor assigned the repository a **B** health grade. It found no anti-pattern errors, prompt-lint errors, or workflow fan-out starvation risks. It reported 7 warnings and 6 agent call sites without a detected `MaxOutputTokens` cap, for 13 findings total. Four findings are marked auto-fixable; the other nine are heuristic and should be checked against the actual runtime configuration before changing code.

The report inspected four `[MessageHandler]` methods and six `RunAsync` / `RunStreamingAsync` sites. It did not report an incomplete scan.

## Findings and suggested actions

### 1. Production credentials: `DefaultAzureCredential` (4 findings)

MAF Doctor flags `DefaultAzureCredential` in these hosted-agent programs:

- [HostedAgents/Author/Program.cs](../HostedAgents/Author/Program.cs#L18)
- [HostedAgents/Blogger/Program.cs](../HostedAgents/Blogger/Program.cs#L19)
- [HostedAgents/Researcher/Program.cs](../HostedAgents/Researcher/Program.cs#L25)
- [HostedAgents/Reviewer/Program.cs](../HostedAgents/Reviewer/Program.cs#L18)

**Suggested action:** Confirm how each hosted agent authenticates in its deployed environment. Where production uses managed identity, use `ManagedIdentityCredential` explicitly, as required by this repository's MAF guidance. Preserve a deliberate local-development credential path if developers need one; do not let a production deployment fall back to credential-chain discovery. MAF Doctor marks these findings as mechanically auto-fixable, but review the preview and resulting diff before applying it, then build and test the affected hosts.

### 2. Agent calls without a detected output-token cap (6 heuristic findings)

MAF Doctor recommends setting `MaxOutputTokens` on the relevant `ChatOptions`. These findings are heuristic; in particular, workflow `RunAsync` calls may inherit limits from the underlying agents or chat-client configuration.

- [AuthorAgent.cs](../AuthorAgent.cs#L68)
- [BlogWriterSessionService.cs](../BlogWriterSessionService.cs#L27)
- [BlogWriterSessionService.cs](../BlogWriterSessionService.cs#L57)
- [BloggerAgent.cs](../BloggerAgent.cs#L115)
- [ResearcherAgent.cs](../ResearcherAgent.cs#L51)
- [ReviewerAgent.cs](../ReviewerAgent.cs#L61)

**Suggested action:** Trace each listed call to the actual agent/chat-client options and verify whether an output-token limit is already applied. If not, choose limits based on expected output size and product needs, and apply them at the owning configuration point. Check that normal long-form drafts and structured responses still complete acceptably; avoid adding duplicate or ineffective caps at workflow call sites.

### 3. Agent chat pipelines without detected OpenTelemetry setup (3 heuristic findings)

MAF Doctor did not detect `UseOpenTelemetry` in the files that construct these agents:

- [AuthorAgent.cs](../AuthorAgent.cs#L34)
- [BloggerAgent.cs](../BloggerAgent.cs#L43)
- [ReviewerAgent.cs](../ReviewerAgent.cs#L34)

**Suggested action:** Check whether telemetry is configured in a shared chat-client factory, host startup, or another layer the file-level heuristic cannot see. If traces and metrics for agent calls are not emitted, configure OpenTelemetry on the `IChatClient` pipeline and ensure the host has an exporter and appropriate filtering. Verify spans are emitted, and avoid duplicate instrumentation if telemetry is already wired elsewhere.

## Recommended order

1. Review the four credential findings first because they affect deployed identity behavior. Preview the deterministic autofixes, inspect the diff, and apply only if the result matches the deployment design.
2. Verify whether the six call sites truly lack effective output-token limits. Add limits only where the underlying agent configuration has none.
3. Verify whether the three agent pipelines already emit telemetry. Wire OpenTelemetry only for pipelines that are actually uninstrumented.
4. Build and run relevant tests after changes, then rerun `maf-doctor doctor` and compare the grade and findings.

## Remediation Progress

After the credential and output-cap changes on `fix/maf-doctor-findings`:

- All four hosted agents use an explicit system-assigned managed identity. Each hosted-agent project builds without warnings.
- The shared chat-client factory now supplies an 8192-token per-response default while preserving explicit per-call values. Override it with `MAX_OUTPUT_TOKENS` in the console app, or `Foundry:MaxOutputTokens` / `MAX_OUTPUT_TOKENS` in the web app.
- The latest maf-doctor scan remains grade **B** and still reports the six `RunAsync` cost heuristics because the scanner does not follow the shared chat-client wrapper. The focused tests verify the cap is applied before the underlying chat client and that caller options are not mutated.
- Three `UseOpenTelemetry` heuristics remain. No telemetry changes were made; the Foundry tracing guidance requires opening its trace viewer first, and the available VS Code command tool is restricted to workspace-creation flows.
- Validation: all 92 console tests, all 89 web tests, and builds of all four hosted-agent projects pass.

## Commands

Preview supported mechanical fixes without writing files by calling `MafAutoFixAll` in an MCP client with `repoPath` set to the repository root and `dryRun: true` (the default).

Apply the CLI autofixes only after reviewing the preview:

```powershell
maf-doctor autofix-all "e:\AI\.NET\blog\BlogWriter" --apply
```

Reassess after remediation:

```powershell
maf-doctor doctor "e:\AI\.NET\blog\BlogWriter"
```

Treat this assessment as advisory: verify heuristic findings against runtime and host configuration before changing code.
