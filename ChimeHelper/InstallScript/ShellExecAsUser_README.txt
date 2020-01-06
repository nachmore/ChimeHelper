Version included in ChimeHelper is x86-unicode

https://nsis.sourceforge.io/ShellExecAsUser_plug-in

ShellExecAsUser plug-in
-----------------------
Execute the specified program using ShellExecute in non-admin context. 
The ShellExecAsUser plug-in is intended for installers that run with admin rights, but need to execute something as a normal non-elevated process.
This is accomplished by having Explorer launch the application on the installer’s behalf (http://brandonlive.com/2008/04/27/getting-the-shell-to-run-an-application-for-you-part-2-how/).
As such it's a light alternative to UAC plugin.

Usage
-----
Syntax is exactly the same as NSIS built in ExecShell command:
ShellExecAsUser::ShellExecAsUser action command [parameters] [SW_SHOWDEFAULT | SW_SHOWNORMAL | SW_SHOWMAXIMIZED | SW_SHOWMINIMIZED | SW_HIDE]

Note that action is usually "open", "print", etc, but can be an empty string to use the default action. Parameters and the show type are optional. $OUTDIR is used for the working directory.
When installer process is non-elevated (e.g. executed on Windows XP, or UAC is disabled, etc), defaults to ShellExecute method.
Example:
  ShellExecAsUser::ShellExecAsUser "open" 'http://www.google.com/' 

Release notes
---------------------------
- Updated to build Ansi and Unicode versions, using Visual Studio 2010. To reduce the dependencies, dll is built with "Multi-threaded" (/MT) runtime library option. Unfortunately the size grew up to 44K (comparing to original 7K with Visual Studio 6). If you install Visual Studio redistributables with your installer, recompile the dll with "Multi-threaded dll" (/MD) option. It will reduce the size to 11K.
