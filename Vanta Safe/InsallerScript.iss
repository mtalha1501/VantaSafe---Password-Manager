[Setup]
AppName=Vanta Safe
AppVersion=1.0.1
DefaultDirName={pf}\Vanta Safe
DefaultGroupName=Vanta Safe
UninstallDisplayIcon={app}\Vanta Safe.exe
OutputDir=InstallerOutput
OutputBaseFilename=VantaSafeSetup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
SetupIconFile=Assets\icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Vanta Safe"; Filename: "{app}\Vanta Safe.exe"; IconFilename: "{app}\icon.ico"
Name: "{group}\Vanta Safe"; Filename: "{app}\Vanta Safe.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\Vanta Safe"; Filename: "{app}\Vanta Safe.exe"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
Filename: "{app}\Vanta Safe.exe"; Description: "Launch Vanta Safe Designed by Senior Develpors Talha and Saqi"; Flags: nowait postinstall skipifsilent

[Files]
Source: "Assets\icon.ico"; DestDir: "{app}"; Flags: ignoreversion
