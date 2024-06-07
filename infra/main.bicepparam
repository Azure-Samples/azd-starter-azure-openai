using 'main.bicep'

param environmentName = readEnvironmentVariable('AZURE_ENV_NAME', '')
param location = readEnvironmentVariable('AZURE_LOCATION', '')
param openAiHost = readEnvironmentVariable('OPENAI_HOST', '')
param openAiApiKey = readEnvironmentVariable('OPENAI_API_KEY', '')
param openAiModel = readEnvironmentVariable('OPENAI_MODEL', '')
param openAiEmbeddingModel = readEnvironmentVariable('OPENAI_EMBEDDING_MODEL', '')
