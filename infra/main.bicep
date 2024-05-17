targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(64)
param environmentName string

@description('Primary location for all resources')
param location string

@description('Azure OpenAI Model Deployment Name')
param azureOpenAIModel string = 'gpt-35-turbo'

@description('Azure OpenAI Model Name')
param azureOpenAIModelName string = 'gpt-35-turbo'

param azureOpenAIModelVersion string = '0613'

@description('Azure OpenAI Embedding Model Deployment Name')
param azureOpenAIEmbeddingModel string = 'text-embedding-ada-002'

@description('Azure OpenAI Embedding Model Name')
param azureOpenAIEmbeddingModelName string = 'text-embedding-ada-002'

param azureOpenAIEmbeddingModelVersion string = '2'

param keyVaultName string = ''

@description('Id of the user or app to assign application roles')
param principalId string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }


// Organize resources in a resource group 
resource group 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module openAI 'br/public:ai/cognitiveservices:1.1.1' = {
  name: 'openai'
  scope: group
  params: {
    tags: tags
    skuName: 'S0'
    kind: 'OpenAI'
    name: 'openai-${resourceToken}'
    location: location
    deployments: [
      {
        name: azureOpenAIModelName
        properties: {
          model: {
            format: 'OpenAI'
            name: azureOpenAIModel
            version: azureOpenAIModelVersion
          }
        }
      }
      {
        name: azureOpenAIEmbeddingModelName
        properties: {
          model: {
            format: 'OpenAI'
            name: azureOpenAIEmbeddingModel
            version: azureOpenAIEmbeddingModelVersion
          }
        }
      }
      // {
      //   name: 'Gpt35Turbo_16k'
      //   properties: {
      //     model: {
      //       format: 'OpenAI'
      //       name: 'gpt-35-turbo-16k'
      //       version: '0613'
      //     }
      //   }
      // }
    ]
  }
}

module searchService 'br/public:search/search-service:1.0.2' = {
  name: 'search-service'
  scope: group
  params: {
    name: 'search-${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'free'
    }
  }
}

module getKey './app/getkey.bicep' = {
  name: 'get-key'
  scope: group
  params: {
    azureOpenAIName: openAI.outputs.name
    azureAISearchName: searchService.outputs.name
  }
}

// Store secrets in a keyvault
module keyVault 'br/public:avm/res/key-vault/vault:0.3.5' = {
  name: 'keyvault'
  scope: group
  params: {
    name: !empty(keyVaultName) ? keyVaultName : '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    tags: tags
    accessPolicies: [
      {
        objectId: principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
    secrets: {
      secureList: [
        {
          name: 'AZURE-OPENAI-API-KEY'
          value: getKey.outputs.openAIKey
        }
        {
          name: 'AZURE-SEARCH-KEY'
          value: getKey.outputs.searchKey
        }
      ]
    }
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = group.name
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_OPENAI_ENDPOINT string = openAI.outputs.endpoint
output AZURE_SEARCH_ENDPOINT string = searchService.outputs.endpoint
output AZURE_OPENAI_API_KEY string = getKey.outputs.openAIKey
output AZURE_SEARCH_KEY string = getKey.outputs.searchKey
output AZURE_OPENAI_MODEL string = azureOpenAIModelName
output AZURE_OPENAI_EMBEDDING_MODEL string = azureOpenAIEmbeddingModelName
