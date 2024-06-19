if ($env:OPENAI_HOST -eq "azure") {  
    $AZURE_OPENAI_API_KEY = az cognitiveservices account keys list --name $env:AZURE_OPENAI_SERVICE --resource-group $env:AZURE_RESOURCE_GROUP --query key1 --output tsv 
    azd env set AZURE_OPENAI_API_KEY $AZURE_OPENAI_API_KEY  
}  

azd env get-values | Out-File -FilePath .env  