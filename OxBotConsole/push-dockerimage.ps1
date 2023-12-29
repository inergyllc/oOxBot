. e:\scripts\SharedSettings.ps1
. e:\scripts\Publish-DotNetReleaseVersion.ps1
. e:\scripts\Publish-DockerImageToRegistry.ps1

$outputFolder = Publish-DotNetReleaseVersion `
	-ProjectFolder $oxbotConsoleAppFolder `
	-TargetFramework $oxbotConsoleAppFramework

Publish-DockerImageToRegistry `
        -LocalImageName $oxbotConsoleLocalImageName `
        -Tag $oxbotConsoleLocalImageTag `
        -LoginServer $oxaiImageLoginServer `
        -RepositoryName $oxbotConsoleImageRepositoryName `
        -RegistryName $oxaiImageRegistryName `
        -ProjectDir $oxbotConsoleAppFolder `
        -Username $oxaiImageRegistryUsername `
        -Password $oxaiImageRegistryPassword
