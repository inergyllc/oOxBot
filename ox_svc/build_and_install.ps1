# build_and_install.ps1

function Update-PyModuleVersion {
    param(
        [string]$SetupFilePath = ".\setup.py"
    )

    # Read setup.py content
    $content = Get-Content $SetupFilePath

    # Find the line containing the version number
    $versionLine = $content | Where-Object { $_ -match "version=" }

    # Extract the current version number
    $currentVersion = [regex]::Match($versionLine, '(?<=")(.*?)(?=")').Value
    $versionParts = $currentVersion.Split('.')

    $major = [int]$versionParts[0]
    $mid = [int]$versionParts[1]
    $minor = [int]$versionParts[2]

    # Apply version update rules
    if ($minor -lt 9) {
        $minor++
    } elseif ($mid -lt 9) {
        $minor = 0
        $mid++
    } else {
        $minor = 0
        $mid = 0
        $major++
    }

    # Construct new version number
    $newVersion = "$major.$mid.$minor"

    # Replace the old version number with the new one in the content
    $newContent = $content -replace "version=""$currentVersion""", "version=""$newVersion"""

    # Write the updated content back into setup.py
    Set-Content $SetupFilePath $newContent

    Write-Host "Updated version to $newVersion"
}

function Remove-OldPyModule {
    param(
        [string]$moduleName
    )

    # Get list of outdated Python packages
    $outdatedPackages = pip list --outdated | Where-Object { $_.name -eq $moduleName }

    foreach ($package in $outdatedPackages) {
        # Uninstall the package
        pip uninstall -y $package.name
        Write-Host "Removed outdated version of $package.name"
    }
}

# Go to the root directory for the github code
cd "E:\oxai\src\OxBot" 

# Update the version number in setup.py
Update-PyModuleVersion -SetupFilePath ".\ox_svc\setup.py"

# Navigate to the directory containing setup.py
cd "E:\oxai\src\OxBot\ox_svc" 

# Remove any previous build artifacts
Remove-Item .\dist -Recurse -ErrorAction Ignore

# Build the package (wheel and source distribution)
python setup.py sdist bdist_wheel

# Install or upgrade the package using the generated wheel
# Adjust the following line to match the naming pattern of your .whl file or find the .whl file automatically
$wheelFile = Get-ChildItem -Path .\dist\ -Filter ox_svc*.whl | Select-Object -First 1
pip install --upgrade $wheelFile.FullName

# Navigate back to the root directory
Remove-OldPyModule -moduleName "ox-svc"

git add .

$commitMessage = Read-Host -Prompt "Enter commit message"
git commit -m "$commitMessage"

git push origin master