targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(64)
param environmentName string

@description('Primary location for all resources')
param location string

@description('Azure OpenAi Model Deployment Name')
param azureOpenAiModel string = 'gpt-35-turbo'

@description('Azure OpenAi Model Name')
param azureOpenAiModelName string = 'gpt-35-turbo'

param azureOpenAiModelVersion string = '0613'

@description('Azure OpenAi Embedding Model Deployment Name')
param azureOpenAiEmbeddingModel string = 'text-embedding-ada-002'

@description('Azure OpenAi Embedding Model Name')
param azureOpenAiEmbeddingModelName string = 'text-embedding-ada-002'

param azureOpenAiEmbeddingModelVersion string = '2'

@allowed(['azure', 'openai'])
param openAiHost string 
param openAiModel string = ''
param openAiEmbeddingModel string = ''

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }


// Organize resources in a resource group 
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module openAi 'br/public:avm/res/cognitive-services/account:0.5.3' = {
  name: 'openai'
  scope: rg
  params: {
    tags: tags
    kind: 'OpenAI'
    name: 'openai-${resourceToken}'
    location: location
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
    deployments: [
      {
        name: azureOpenAiModelName
        model: {
          format: 'OpenAI'
          name: azureOpenAiModel
          version: azureOpenAiModelVersion
        }
        sku: {
          capacity: 10
          name: 'Standard'
        }
      }
      {
        name: azureOpenAiEmbeddingModelName
        model: {
          format: 'OpenAI'
          name: azureOpenAiEmbeddingModel
          version: azureOpenAiEmbeddingModelVersion
        }
        sku: {
          capacity: 10
          name: 'Standard'
        }
      }
    ]
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output OPENAI_HOST string = openAiHost
output AZURE_OPENAI_ENDPOINT string = (openAiHost == 'azure') ? openAi.outputs.endpoint: ''
output AZURE_OPENAI_SERVICE string = (openAiHost == 'azure') ? openAi.outputs.name: ''
output AZURE_OPENAI_MODEL string = (openAiHost == 'azure') ? azureOpenAiModelName: ''
output AZURE_OPENAI_EMBEDDING_MODEL string = (openAiHost == 'azure') ? azureOpenAiEmbeddingModelName: ''

output OPENAI_MODEL string = (openAiHost == 'openai') ? openAiModel : ''
output OPENAI_EMBEDDING_MODEL string = (openAiHost == 'openai') ? openAiEmbeddingModel : ''
