using 'main.bicep'

param environmentName = readEnvironmentVariable('AZURE_ENV_NAME', '')
param location = readEnvironmentVariable('AZURE_LOCATION', '')
param openAiHost = readEnvironmentVariable('OPENAI_HOST', 'azure')
param openAiModel = readEnvironmentVariable('OPENAI_MODEL', 'gpt-35-turbo')
param openAiEmbeddingModel = readEnvironmentVariable('OPENAI_EMBEDDING_MODEL', 'text-embedding-ada-002')
