# Deployment Summary

Status: failed

The declarative ARM redeployment `BlogWriterCosmosSessionStore-20260916-eastus2` targeted the approved `eastus2` location for resource group `AgentFramework`.

Azure returned `InvalidResourceLocation`: the deterministic Cosmos DB account `blogwriterowy7vriqafrdq` already exists in `eastus`, so it cannot be created with the same name in `eastus2`. A read-only query confirmed that existing account remains in `eastus` with provisioning state `Failed`. Cosmos database `blogwriter`, container `sessions`, SQL role assignment, and Cosmos user-secret configuration were not completed.

The deployment inventory reported `blogwriterowy7vriqafrdq` as unattributed during this deployment window. It requires review and is not confirmed safe to delete. No resources were deleted or otherwise modified outside the ARM deployment attempt.

Migration: not applicable.
