targetScope = 'resourceGroup'

param accountName string
param location string
param databaseName string = 'blogwriter'
param containerName string = 'sessions'
param principalId string = ''

resource account 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-11-15' = {
  parent: account
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: database
  name: containerName
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [
          '/OwnerId'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        automatic: true
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        compositeIndexes: [
          [
            {
              path: '/OwnerId'
              order: 'ascending'
            }
            {
              path: '/UpdatedAt'
              order: 'descending'
            }
          ]
        ]
      }
    }
  }
}

resource dataContributor 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-04-15' = if (!empty(principalId)) {
  parent: account
  name: guid(account.id, principalId, '00000000-0000-0000-0000-000000000002')
  properties: {
    principalId: principalId
    roleDefinitionId: '${account.id}/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002'
    scope: account.id
  }
}

output endpoint string = account.properties.documentEndpoint
output database string = databaseName
output container string = containerName
