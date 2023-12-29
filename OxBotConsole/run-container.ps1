. $PSScriptRoot\SharedSettings.ps1

az login

az container list `
    --resource-group "${ResourceGroup}" `
    --output table

# Create the container with registry credentials
<#
$azcmd = @"
az container create `
  --resource-group "${ResourceGroup}" `
  --name "${ContainerName}" `
  --image "${loginServer}/${azureImageName}:${tag}" `
  --dns-name-label "${uniqueDnsNameLabel}" `
  --ports "${ports}" `
  --registry-login-server "${loginServer}" `
  --registry-username "${ACRUsername}" `
  --registry-password "${ACRPassword}"
"@
$azcmd
#>
az container create `
  --resource-group "${ResourceGroup}" `
  --name "${ContainerName}" `
  --image "${loginServer}/${azureImageName}:${tag}" `
  --dns-name-label "${uniqueDnsNameLabel}" `
  --ports "${ports}" `
  --registry-login-server "${loginServer}" `
  --registry-username "${ACRUsername}" `
  --registry-password "${ACRPassword}"

# Get container details
$jsonResult = az container show `
  --resource-group "${ResourceGroup}" `
  --name "${ContainerName}" `
  --query "{ip:ipAddress.ip, fqdn:ipAddress.fqdn}" `
  --output json | ConvertFrom-Json

# Extract IP Address and FQDN into variables
$ipAddress = $jsonResult.ip
$fqdn = $jsonResult.fqdn
Write-Host "IP Address: $ipAddress"
Write-Host "FQDN: $fqdn"

$azcmd = @"
az container delete `
	--resource-group "${ResourceGroup}" `
	--name "${azureContainerName}"
"@
$azcmd
az container delete `
	--resource-group "${ResourceGroup}" `
	--name "${azureContainerName}"