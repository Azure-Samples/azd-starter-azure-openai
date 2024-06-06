using 'main.bicep'

param environmentName = readEnvironmentVariable('AZURE_ENV_NAME', 'env_name')
param location = readEnvironmentVariable('AZURE_LOCATION', 'location')
param openAiHost = readEnvironmentVariable('OPENAI_HOST', 'azure')
param openAiApiKey = readEnvironmentVariable('OPENAI_API_KEY', 'openai_api_key')
param openAiModel = readEnvironmentVariable('OPENAI_MODEL', 'gpt-35-turbo')
param openAiEmbeddingModel = readEnvironmentVariable('OPENAI_EMBEDDING_MODEL', 'text-embedding-ada-002')
