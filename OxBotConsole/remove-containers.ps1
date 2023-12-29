# Load shared settings
. $PSScriptRoot\SharedSettings.ps1

$PSScriptRoot

# Check if ResourceGroup is populated
if (-not $ResourceGroup) {
    Write-Error "ResourceGroup is not set. Please ensure SharedSettings.ps1 contains the correct information."
    exit 1
}

# Ensure Azure CLI is available and you are logged in
az login

# List all container instances in the specified resource group
$runningContainers = az container list `
    --resource-group $ResourceGroup `
    --query "[?instanceView.state == 'Running'].{Name:name}" `
    --output json | ConvertFrom-Json

# Display running containers
Write-Host "Running Containers:"
$runningContainers

# Iterate through each running container and delete it
foreach ($container in $runningContainers) {
    Write-Host "Deleting running container: " $container.Name

    # Delete the container
    az container delete --name $container.Name --resource-group $ResourceGroup --yes
}

Write-Host "All running containers in the resource group '$ResourceGroup' have been deleted."
