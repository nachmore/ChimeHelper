!include LogicLib.nsh
!define DOTNET_VERSION "3.5"
!include "MUI2.nsh"

Unicode True

; The name of the installer
Name "Chime Helper"

; The file to write
OutFile "ChimeHelperSetup.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\Chime Helper"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

; Set custom branding
BrandingText "Chime Helper (© Oren Nachman)"

;--------------------------------

; Pages
  !define MUI_ICON "..\ChimeHelperUX\Icons\fan.ico"

  !define MUI_COMPONENTSPAGE_TEXT_TOP "Select the Components you want to install and uncheck the ones you you do not want to install. Click next to continue."
  !define MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE "Description"
  !define MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO "Description info"
  !insertmacro MUI_PAGE_COMPONENTS

  !insertmacro MUI_PAGE_DIRECTORY

  !insertmacro MUI_PAGE_INSTFILES

  ; MUI_PAGE_FINISH
      !define MUI_FINISHPAGE_AUTOCLOSE
      !define MUI_FINISHPAGE_RUN
      !define MUI_FINISHPAGE_RUN_CHECKED
      !define MUI_FINISHPAGE_RUN_TEXT "Launch Chime Helper Now"
      !define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
    !insertmacro MUI_PAGE_FINISH

; Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
Function LaunchLink
  ShellExecAsUser::ShellExecAsUser "" "$INSTDIR\ChimeHelper.exe"
FunctionEnd

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

; According to https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
; we can determine if we need to run the .NET installer based on:
;
; 1. Check for the existance of *Release* in *HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full*
; 2. Ensure that the version >= NETRelease (see table on page for version map)

!define NET_RELEASE "461808" 
!define NETInstaller "NDP472-KB4054531-Web.exe"

Section "Microsoft .NET Framework Updater (required)" SecDotNet
  SectionIn RO

  ReadRegStr $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

  IntCmp $0 ${NET_RELEASE} UpdateNotNeeded UpdateDotNet UpdateNotNeeded

  UpdateDotNet:
    File /oname=$TEMP\${NETInstaller} ${NETInstaller}
 
    DetailPrint "Starting Microsoft .NET Framework v${NET_RELEASE} Setup..."
    ExecWait "$TEMP\${NETInstaller}"
    Return
 
  UpdateNotNeeded:
    DetailPrint "Microsoft .NET Framework is already up to date!"

SectionEnd

Section "Chime Helper (required)" SecChimeHelper
  SectionIn RO
  
  DetailPrint "Debug statements are intentional! :)"

  ; Ask Chime Helper to shut down
  !define SHUT_DOWN "Shutting down Chime Helper..."
  DetailPrint "${SHUT_DOWN}"

  DetailPrint '${SHUT_DOWN} [DEBUG] opening handle: "Global\nachmore.ChimeHelper.IsRunning"'

  !define EVENT_MODIFY_STATE 0x0002

  System::Call 'kernel32::OpenEventW(i ${EVENT_MODIFY_STATE}, b 0, w "Global\nachmore.ChimeHelper.IsRunning") i .R0 ? e'  
  Pop $0

  DetailPrint "${SHUT_DOWN} [DEBUG] OpenEventW: $R0 GetLastError: $0"

  System::Call 'kernel32::SetEvent(i $R0) b .R1 ? e'
  Pop $0

  DetailPrint "${SHUT_DOWN} [DEBUG] Handle: $R0 SetEvent: $R1 GetLastError: $0"

  ; if the return value is 0 then we failed to set the event (Chime Helper is likely not running)
  ; so don't waste time sleeping
  IntCmp $R0 0 ContinueInstall

  ; Let the process shutdown
  DetailPrint "Sleeping to allow Chime Helper to shutdown..."
  Sleep 5000

  ContinueInstall:
    
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File "..\ChimeHelperUX\bin\Release\ChimeHelper.exe"	
  File "..\ChimeHelperUX\bin\Release\ChimeHelper.exe.config"
  File "..\ChimeHelperUX\bin\Release\*.dll"
  File "..\ChimeHelperUX\bin\Release\*.xml"
  File "..\..\LICENSE"
  File "..\..\OTHER_LICENSES"

  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "DisplayName" "Chime Helper"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "DisplayIcon" "$INSTDIR\ChimeHelper.exe,0"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "Publisher" "Oren Nachman"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "Version" "1.0"  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "DisplayVersion" "1.0"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
SectionEnd

Section "Start Menu icon" SecStartMenuIcon
  # Start Menu
  CreateShortCut "$SMPROGRAMS\Chime Helper.lnk" "$INSTDIR\ChimeHelper.exe" "" "$INSTDIR\ChimeHelper.exe" 0
  
SectionEnd

Section "Autostart" SecAutoStart
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "Chime Helper" '"$INSTDIR\ChimeHelper.exe"'
SectionEnd

;--------------------------------
; Descriptions

  LangString DESC_SecDotNet ${LANG_ENGLISH} "Updates your .NET installation if needed"
  LangString DESC_SecChimeHelper ${LANG_ENGLISH} "The actual Chime Helper application"
  LangString DESC_SecStartMenuIcon ${LANG_ENGLISH} "Option to add a Start menu icon for Chime Helper"
  LangString DESC_SecAutoStart ${LANG_ENGLISH} "Automatically to launch Chime Helper with Windows (no UI)"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDotNet} $(DESC_SecDotNet)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecChimeHelper} $(DESC_SecChimeHelper)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecStartMenuIcon} $(DESC_SecStartMenuIcon)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecAutoStart} $(DESC_SecAutoStart)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  # Remove Start Menu launcher
  Delete "$SMPROGRAMS\ChimeHelper.lnk"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChimeHelper"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "ChimeHelper"

  ; Remove files and uninstaller
  Delete "$INSTDIR\*"

  ; Remove directories used
  RMDir "$INSTDIR"
SectionEnd

Function .onInit
FunctionEnd