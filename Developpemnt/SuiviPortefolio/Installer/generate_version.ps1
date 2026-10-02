<#
.SYNOPSIS
Génère le fichier version.iss pour InnoSetup à partir de l'EXE compilé.
.DESCRIPTION
Ce script lit la version de SuiviPortefolio.exe et met à jour version.iss
avec les bonnes valeurs pour InnoSetup.
#>

# =============================================
# CONFIGURATION (à adapter si nécessaire)
# =============================================

$projectRoot = Split-Path -Parent $PSScriptRoot
$exePath = Join-Path $projectRoot "Artifacts\Publish\SuiviPortefolio.exe"
$versionIssPath = Join-Path $PSScriptRoot "version.iss"

# Nom de l'application
$appName = "Suivi Portefolio"
$appPublisher = "yves Couchoux"
$appExeName = "SuiviPortefolio.exe"
$appIcon = "app.ico"

# =============================================
# GÉNÉRATION DU FICHIER version.iss
# =============================================

# Vérifier que l'EXE existe
if (-not (Test-Path -Path $exePath)) {
    Write-Host "❌ ERREUR: Le fichier EXE n'existe pas à l'emplacement: $exePath" -ForegroundColor Red
    Write-Host "Vérifie que tu as bien compilé le projet avec 'dotnet publish'" -ForegroundColor Yellow
    exit 1
}

# Lire la version de l'EXE
try {
    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath)
    $version = $versionInfo.FileVersion
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "L'exécutable ne contient pas de numéro de version."
    }
    
    Write-Host "📄 Version détectée dans l'EXE: $version" -ForegroundColor Green
    
    # Contenu du fichier version.iss
    $content = @"
; =============================================
; Fichier de version généré automatiquement
; par generate_version.ps1
; Ne pas modifier manuellement
; =============================================

#define MyAppName "$appName"
#define MyAppVersion "$version"
#define MyAppPublisher "$appPublisher"
#define MyAppExeName "$appExeName"
#define MyAppIcon "$appIcon"
"@

    # Écrire le fichier
    $content | Out-File -FilePath $versionIssPath -Encoding UTF8 -Force
    
    Write-Host "✅ Fichier version.iss généré avec la version: $version" -ForegroundColor Green
    Write-Host "   → Chemin: $versionIssPath" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ ERREUR: Impossible de lire la version de l'EXE" -ForegroundColor Red
    Write-Host "Détails: $_" -ForegroundColor Yellow
    exit 1
}
