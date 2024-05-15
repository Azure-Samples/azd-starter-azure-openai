param azureOpenAIName string
param azureAISearchName string

output openAIKey string = listKeys(resourceId(subscription().subscriptionId, resourceGroup().name, 'Microsoft.CognitiveServices/accounts', azureOpenAIName), '2023-05-01').key1
output searchKey string = listAdminKeys(resourceId(subscription().subscriptionId, resourceGroup().name, 'Microsoft.Search/searchServices', azureAISearchName), '2021-04-01-preview').primaryKey

