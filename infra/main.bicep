// Editable infrastructure for an existing Foundry project. The account and
// project are referenced only. ACR behavior was selected when this file was
// generated, so the graph contains no runtime mode switch.

targetScope = 'subscription'

type deploymentType = {
  name: string
  model: {
    name: string
    format: string
    version: string
  }
  sku: {
    name: string
    capacity: int
  }
}

type connectionType = {
  name: string
  category: string
  target: string
  authType: string
  metadata: object?
}

param projectResourceId string
param deployments deploymentType[] = []
param projectEndpoint string
param connections connectionType[] = []
@secure()
param connectionCredentials object = {}
param sessionStorePrincipalId string = ''
param cosmosLocation string = 'eastus2'

var projectIdParts = split(projectResourceId, '/')
var projectSubscriptionId = projectIdParts[2]
var projectResourceGroupName = projectIdParts[4]
var accountName = projectIdParts[8]
var projectName = projectIdParts[10]

resource foundryAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' existing = {
  scope: resourceGroup(projectSubscriptionId, projectResourceGroupName)
  name: accountName

  resource project 'projects' existing = {
    name: projectName
  }
}

module projectResources 'modules/foundry-project.bicep' = {
  name: 'foundry-project-resources'
  scope: resourceGroup(projectSubscriptionId, projectResourceGroupName)
  params: {
    accountName: accountName
    projectName: projectName
    deployments: deployments
    connections: connections
    connectionCredentials: connectionCredentials
  }
}

module sessionStore 'modules/cosmos-session-store.bicep' = {
  name: 'cosmos-session-store'
  scope: resourceGroup(projectSubscriptionId, projectResourceGroupName)
  params: {
    accountName: 'blogwriter${uniqueString(projectResourceId, 'session-store-v3')}'
    location: cosmosLocation
    principalId: sessionStorePrincipalId
  }
}

output AZURE_AI_PROJECT_ID string = projectResourceId
output AZURE_AI_ACCOUNT_NAME string = accountName
output AZURE_AI_PROJECT_NAME string = projectName
output AZURE_OPENAI_ENDPOINT string = 'https://${accountName}.openai.azure.com/'
output FOUNDRY_PROJECT_ENDPOINT string = projectEndpoint
output AZURE_FOUNDRY_RESOURCE_GROUP string = ''
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = ''
output AZURE_CONTAINER_REGISTRY_RESOURCE_ID string = ''
output AZURE_AI_PROJECT_ACR_CONNECTION_NAME string = projectResources.outputs.acrConnectionName
output AZURE_AI_PROJECT_CONNECTION_NAMES string = projectResources.outputs.connectionNames
output AZURE_AI_PROJECT_CONNECTIONS_PROJECT_ENDPOINT string = projectEndpoint
output AZD_FOUNDRY_ACR_MODE string = 'none'
output COSMOS_ENDPOINT string = sessionStore.outputs.endpoint
output COSMOS_DATABASE_NAME string = sessionStore.outputs.database
output COSMOS_CONTAINER_NAME string = sessionStore.outputs.container
