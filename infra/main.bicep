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

@allowed(['azure', 'openai'])
param openAiHost string 
param openAiApiKey string = ''
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

module openAi 'br/public:ai/cognitiveservices:1.1.1' = {
  name: 'openai'
  scope: rg
  params: {
    tags: tags
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
    ]
  }
}

output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = group.name
output OPENAI_HOST string = openAiHost
output AZURE_OPENAI_ENDPOINT string = (openAiHost == 'azure') ? openAi.outputs.endpoint: ''
output AZURE_OPENAI_SERVICE string = (openAiHost == 'azure') ? openAi.outputs.name: ''
output AZURE_OPENAI_MODEL string = (openAiHost == 'azure') ? azureOpenAIModelName: ''
output AZURE_OPENAI_EMBEDDING_MODEL string = (openAiHost == 'azure') ? azureOpenAIEmbeddingModelName: ''

output OPENAI_API_KEY string = (openAiHost == 'openai') ? openAiApiKey : ''
output OPENAI_MODEL string = (openAiHost == 'openai') ? openAiModel : ''
output OPENAI_EMBEDDING_MODEL string = (openAiHost == 'openai') ? openAiEmbeddingModel : ''
