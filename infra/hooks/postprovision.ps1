# Load the azd environment variables
$DIR = Split-Path $MyInvocation.MyCommand.Path
& "$DIR\load_azd_env.ps1"
  
azd env get-values | Out-File -FilePath .env  
  
if ($env:OPENAI_HOST -eq "openai") {  
    exit  
}  
  
$AZURE_OPENAI_API_KEY = az cognitiveservices account keys list --name $env:AZURE_OPENAI_SERVICE --resource-group $env:AZURE_RESOURCE_GROUP --query key1 --output tsv  
  
Add-Content -Path .env -Value "AZURE_OPENAI_API_KEY=`"$AZURE_OPENAI_API_KEY`""  
