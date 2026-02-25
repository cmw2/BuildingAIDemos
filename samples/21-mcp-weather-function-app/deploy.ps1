$appName  = "app-name"
$rgName   = "rg-name"
$location = "centralus"
$sku      = "B1"

Push-Location $PSScriptRoot
az webapp up --name $appName --resource-group $rgName --location $location --sku $sku --os-type Linux --runtime "DOTNETCORE:8.0"
Pop-Location

$hostname = az webapp show --name $appName --resource-group $rgName --query defaultHostName -o tsv
Write-Host "MCP Endpoint: https://$hostname/mcp"
