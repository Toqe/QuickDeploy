#define MyAppName "QuickDeploy Server"
#define MyAppVersion "2.3.0"
#define MyAppPublisher "QuickDeploy"
#define MyAppURL "https://github.com/Toqe/QuickDeploy"
#define ServiceFileName "QuickDeploy.ServerService.exe"

[Setup]
AppId={{70718A33-9119-40CA-9857-A4AE950FCDA4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\QuickDeploy
DefaultGroupName={#MyAppName}
OutputDir=..\..\..\bin\setup
OutputBaseFilename=quickDeploy-server-setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
;Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
;Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "..\..\..\bin\*.*"; DestDir: "{app}\Server"; Flags: ignoreversion;Excludes:*.pdb,*.xml,*.pfx

[Icons]
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Code]

const
  NET_FW_SCOPE_ALL = 0;
  NET_FW_IP_VERSION_ANY = 2;

procedure SetFirewallException(AppName,FileName:string);
var
  FirewallObject: Variant;
  FirewallManager: Variant;
  FirewallProfile: Variant;
begin
  try
    FirewallObject := CreateOleObject('HNetCfg.FwAuthorizedApplication');
    FirewallObject.ProcessImageFileName := FileName;
    FirewallObject.Name := AppName;
    FirewallObject.Scope := NET_FW_SCOPE_ALL;
    FirewallObject.IpVersion := NET_FW_IP_VERSION_ANY;
    FirewallObject.Enabled := True;
    FirewallManager := CreateOleObject('HNetCfg.FwMgr');
    FirewallProfile := FirewallManager.LocalPolicy.CurrentProfile;
    FirewallProfile.AuthorizedApplications.Add(FirewallObject);
  except
  end;
end;

procedure RemoveFirewallException( FileName:string );
var
  FirewallManager: Variant;
  FirewallProfile: Variant;
begin
  try
    FirewallManager := CreateOleObject('HNetCfg.FwMgr');
    FirewallProfile := FirewallManager.LocalPolicy.CurrentProfile;
    FireWallProfile.AuthorizedApplications.Remove(FileName);
  except
  end;
end;

function KillProcess(FileName:string):Boolean;
var
  ResultCode:Integer;
begin
  if Exec(ExpandConstant('{sys}\taskkill.exe'), '/im '+ FileName + ' /f /t ', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) = false then  
  begin
    if Exec(ExpandConstant('{sys}\tskill.exe'), FileName + ' /A', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) = false then  
      result := false
    else
      result := true;
  end;
end;

function DoServiceOperation(op:string;canKillProcess:Boolean):Boolean;
var
  ResultCode:Integer;
begin
  if (canKillProcess) then
  begin
    KillProcess('mmc');
    KillProcess('{#ServiceFileName}');
  end;

  if Exec(ExpandConstant('{app}\Server\{#ServiceFileName}'), op, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then  
  begin
    Result := ResultCode = 0;
    Exit;
  end;
  Result := False;
end;

function UninstallService:Boolean;
begin
  Result := DoServiceOperation('uninstall',true);
end;

function InstallService:Boolean;
begin
  Result := DoServiceOperation('install',true);
end;

function StartService:Boolean;
begin
  Result := DoServiceOperation('start',true);
end;

procedure CustomUpdateUnInstallStatus(statusMsg:string);
begin
  if (UninstallProgressForm <> nil) and (UninstallProgressForm.StatusLabel <> nil) then
    UninstallProgressForm.StatusLabel.Caption := statusMsg;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssPostInstall) then
  begin
    if InstallService() = false then
      MsgBox('Unable to install the Service', mbError, MB_OK);
      
    SetFirewallException('QuickDeployServer', ExpandConstant('{app}\Server\{#ServiceFileName}'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if (CurUninstallStep=usUninstall) then
  begin
    CustomUpdateUnInstallStatus('Stopping and uninstalling the services');        
    UnInstallService();
  end;

  if (CurUninstallStep=usPostUninstall) then
  begin    
    RemoveFirewallException(ExpandConstant('{app}\Server\{#ServiceFileName}'));
  end;
end;
