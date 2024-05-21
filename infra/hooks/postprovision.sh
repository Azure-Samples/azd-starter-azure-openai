#!/bin/bash

# Load the azd environment variables
DIR=$(dirname "$(realpath "$0")")
"$DIR/load_azd_env.sh"

azd env get-values > .env

if [$OPENAI_HOST == "openai"]; then
    exit 0
fi

AZURE_OPENAI_API_KEY=$(az cognitiveservices account keys list --name $AZURE_OPEN_SERVICE --resource-group $AZURE_RESOURCE_GROUP --query key1 --output tsv)

echo "AZURE_OPENAI_API_KEY=\"${AZURE_OPENAI_API_KEY}\"" >> .env  
