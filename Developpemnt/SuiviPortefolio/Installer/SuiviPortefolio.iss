; =============================================
; Script InnoSetup pour Suivi Portefolio
; Version gérée via version.iss (généré automatiquement)
; =============================================

; --- Inclure le fichier de version généré ---
#include "version.iss"

[Setup]
; --- Configuration de base ---
AppId={{D0D59EFE-1E3B-48F8-9C92-B2EC4CE7F76A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

; --- Chemins de sortie ---
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\Artifacts\Setup
OutputBaseFilename=Setup_SuiviPortefolio

; --- Options d'installation ---
DisableProgramGroupPage=yes
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; --- Icône de l'installateur et de la désinstallation ---
SetupIconFile={#MyAppIcon}
UninstallDisplayIcon={app}\{#MyAppIcon}

; --- Langue ---
[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
; --- Fichiers de l'application ---
Source: "..\Artifacts\Publish\*"; \
  DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

; --- Icône utilisée par les raccourcis et la désinstallation ---
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; --- Raccourci dans le menu Démarrer ---
Name: "{autoprograms}\{#MyAppName}"; \
  Filename: "{app}\{#MyAppExeName}"; \
  IconFilename: "{app}\{#MyAppIcon}"

; --- Raccourci sur le bureau (optionnel) ---
Name: "{autodesktop}\{#MyAppName}"; \
  Filename: "{app}\{#MyAppExeName}"; \
  IconFilename: "{app}\{#MyAppIcon}"; \
  Tasks: desktopicon

[Tasks]
Name: "desktopicon"; \
  Description: "Créer un raccourci sur le bureau"; \
  GroupDescription: "Raccourcis supplémentaires :"

[Run]
; --- Lancer l'application après installation ---
Filename: "{app}\{#MyAppExeName}"; \
  Description: "Lancer {#MyAppName}"; \
  Flags: nowait postinstall skipifsilent
