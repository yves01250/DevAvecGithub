$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$publishDirectory = Join-Path $projectRoot "Artifacts\Publish"
$setupDirectory = Join-Path $projectRoot "Artifacts\Setup"
$projectFile = Join-Path $projectRoot "SuiviPortefolio.csproj"
$installerScript = Join-Path $PSScriptRoot "SuiviPortefolio.iss"

New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $setupDirectory -Force | Out-Null

dotnet publish $projectFile `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDirectory `
    -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) {
    throw "La publication .NET a échoué (code $LASTEXITCODE)."
}

& (Join-Path $PSScriptRoot "generate_version.ps1")
if ($LASTEXITCODE -ne 0) {
    throw "La génération de version Inno Setup a échoué (code $LASTEXITCODE)."
}

$programFilesX86 = [Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFilesX86)
$compiler = Join-Path $programFilesX86 "Inno Setup 6\ISCC.exe"
if (-not (Test-Path $compiler)) {
    $compilerCommand = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($null -eq $compilerCommand) {
        throw "ISCC.exe est introuvable. Installez Inno Setup 6 ou ajoutez ISCC.exe au PATH."
    }
    $compiler = $compilerCommand.Source
}

& $compiler $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "La compilation Inno Setup a échoué (code $LASTEXITCODE)."
}
