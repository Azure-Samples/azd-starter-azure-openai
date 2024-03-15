targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(64)
param environmentName string

@description('Primary location for all resources')
param location string

var tags = { 'azd-env-name': environmentName }
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

// Organize resources in a resource group 
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module openAi 'br/public:avm/res/cognitive-services/account:0.4.0' = {
  name: 'openai-${resourceToken}'
  scope: rg
  params: {
    tags: tags
    kind: 'OpenAI'
    name: 'openai-${resourceToken}'
    location: location
  }
}

module cogServiceDeploy 'app/cog-service-account-deployments.bicep' = {
  name: 'openai-model-deploy'
  scope: rg
  params: {
    openAiServiceName: openAi.outputs.name
    deployments: [
      {
        name: 'Gpt35Turbo_0301'
        model: {
          format: 'OpenAI'
          name: 'gpt-35-turbo'
          version: '0301'
        }
      }
      {
        name: 'TextEmbeddingAda002_1'
        model: {
          format: 'OpenAI'
          name: 'text-embedding-ada-002'
          version: '2'
        }
      }
      {
        name: 'Gpt35Turbo_16k'
        model: {
          format: 'OpenAI'
          name: 'gpt-35-turbo-16k'
          version: '0613'
        }
      }
    ]
  }
}

module searchService 'br/public:avm/res/search/search-service:0.3.0' = {
  name: 'search-service'
  scope: rg
  params: {
    name: 'search-${resourceToken}'
    location: location
    tags: tags
    sku: 'free'
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_OPENAI_ENDPOINT string = openAi.outputs.endpoint
output AZURE_SEARCH_ENDPOINT string = 'https://${searchService.outputs.name}.search.windows.net'
