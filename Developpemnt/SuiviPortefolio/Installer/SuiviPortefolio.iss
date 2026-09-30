#define MyAppName "Suivi Portefolio"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "yves Couchoux"
#define MyAppExeName "SuiviPortefolio.exe"

[Setup]
AppId={{D0D59EFE-1E3B-48F8-9C92-B2EC4CE7F76A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\Artifacts\Setup
OutputBaseFilename=Setup_SuiviPortefolio
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "..\Artifacts\Publish\*"; \
  DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; \
  Filename: "{app}\{#MyAppExeName}"

Name: "{autodesktop}\{#MyAppName}"; \
  Filename: "{app}\{#MyAppExeName}"; \
  Tasks: desktopicon

[Tasks]
Name: "desktopicon"; \
  Description: "Créer un raccourci sur le bureau"; \
  GroupDescription: "Raccourcis supplémentaires :"

[Run]
Filename: "{app}\{#MyAppExeName}"; \
  Description: "Lancer {#MyAppName}"; \
  Flags: nowait postinstall skipifsilent