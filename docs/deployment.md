# Deployment guide

BlogWriter has three runtime parts: four independently deployed Azure AI Foundry Hosted
Agents, the local console host, and the optional Blazor web host. See
[architecture.md](architecture.md) for how they fit together and
[configuration.md](configuration.md) for the full environment variable reference.

## 1. One-time setup

```powershell
azd ext install microsoft.foundry
azd auth login
az login
```

## 2. Deploy (or redeploy) a hosted agent

From inside each `HostedAgents/<Name>` folder (`Blogger`, `Researcher`, `Author`,
`Reviewer`):

```powershell
# First time only per project — scaffold/replace azure.yaml against your real Foundry project
azd ai agent init --deploy-mode code
azd ai agent init --infra=bicep

# Provision Foundry project/model/ACR resources (skip if reusing an existing project)
azd provision

# Test locally before shipping
azd ai agent run
azd ai agent invoke "Hello!"

# Deploy the source to Foundry Agent Service (direct code deploy, no container build)
azd deploy

# Test the deployed version and stream its logs
azd ai agent invoke --new-session "Hello!"
azd ai agent monitor --tail 100
azd ai agent monitor --tail 100 --type system
```

Repeat `azd deploy` for each of the four agents whenever their code changes — they
deploy independently of each other and of the console app.

> `azure.yaml` in each folder is a starting-point manifest, not a generated one —
> regenerate it via `azd ai agent init` against your real Foundry project before
> deploying for real. `Microsoft.Agents.AI.Foundry.Hosting` is still a **prerelease**
> package; re-validate versions before production use.

## 3. Run the console app locally

The console app is never deployed to Azure — it runs locally and calls the four
already-deployed hosted agents by name over the network.

```powershell
dotnet user-secrets set "FOUNDRY_PROJECT_ENDPOINT" "https://<account>.services.ai.azure.com/api/projects/<project>"
dotnet user-secrets set "AZURE_TENANT_ID" "<tenant-id>"
dotnet user-secrets set "COSMOS_ENDPOINT" "https://<account>.documents.azure.com:443/"
dotnet user-secrets set "COSMOS_DATABASE_NAME" "blogwriter"
dotnet user-secrets set "COSMOS_CONTAINER_NAME" "sessions"
dotnet run --project .
```

It will prompt for a topic and a min/max word count, then stream workflow progress
(`[trace] → ...` / `[trace] ← ...` lines) before printing the final approved draft.
The signed-in Azure CLI user requires the Cosmos DB Built-in Data Contributor role
assigned by `sessionStorePrincipalId`. Enter `list` at the topic prompt to display the
20 newest sessions for that user, or `resume <number>` to continue one after
restarting the application.

## Session lifecycle

Session records are partitioned by the Microsoft Entra object ID of the signed-in
user. Account-deletion automation must call `CosmosBlogSessionStore.DeleteOwnerSessionsAsync`
using an identity with the Cosmos DB data contributor role; this console application
does not observe Microsoft Entra account deletion events itself.

## 4. Run the Blazor web app

Register a confidential web application in Microsoft Entra ID with an HTTPS redirect
URI ending in `/signin-oidc`. For local development, store its client secret using the
commands in [configuration.md](configuration.md), run `az login`, and grant that user
the existing Foundry and Cosmos roles.

```powershell
dotnet run --project BlogWriter.Web/BlogWriter.Web.csproj
```

In production, configure `AzureResources:CredentialMode` as `ManagedIdentity`. Store
the OIDC certificate in Key Vault, register its public certificate on the Entra app,
and grant the web app's system-assigned managed identity permission to read both the
Key Vault certificate and its linked secret. Grant the same managed identity the
required Foundry and Cosmos data-plane roles.

The authenticated user's immutable `oid` claim remains the Cosmos session owner. The
managed identity only authorizes the server to reach Azure resources and must never be
used as the session partition owner.

New, List, and Quit remain available while workflow work is active. After discard
confirmation, the web host requests cancellation, waits up to 10 seconds, suppresses
any late result, and completes the requested transition.

## 5. Verifying a deployment

After `azd deploy` for a given agent, confirm it's healthy before wiring the console app
to it:

1. `azd ai agent invoke --new-session "<test prompt>"` — should return real assistant
   text, not an error.
2. `azd ai agent monitor --tail 100` — look for `HTTP 200` on the `/responses` request
   and no unhandled exceptions. Startup warnings about Kestrel address binding or a 404
   on the very first task-storage lookup (before the task exists) are expected noise, not
   failures.
3. Run the console or web app end-to-end once against the redeployed agent and confirm the
   reviewer reaches `APPROVED` (or a clear revision-cap message) with no exceptions.

## Never do this

Do not call an agent's `/responses` endpoint directly with `HttpClient` or similar, in
either the console app or a hosted agent. All agent-to-agent and app-to-agent
communication must go through Microsoft Agent Framework (`AsAIAgent` /
`AIAgent.RunAsync`) — see [architecture.md](architecture.md).
