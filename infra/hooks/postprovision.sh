#!/bin/bash

if [$OPENAI_HOST == "azure"]; then
    AZURE_OPENAI_API_KEY=$(az cognitiveservices account keys list --name $AZURE_OPEN_SERVICE --resource-group $AZURE_RESOURCE_GROUP --query key1 --output tsv)
    azd env set AZURE_OPENAI_API_KEY $AZURE_OPENAI_API_KEY
fi

azd env get-values > .env
