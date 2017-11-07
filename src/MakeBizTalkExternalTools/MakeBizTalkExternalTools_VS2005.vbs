' This script can be used to automate the creation of Visual Studio .NET 2005 External Tools.
' Scott Colestock - www.traceofthought.net

Option Explicit
On Error Resume Next

Dim wshShell
Set wshShell = WScript.CreateObject("WScript.Shell")

dim nantLocation,buildFile
nantLocation = "%ProgramFiles%\nant\bin\nant.exe"
buildFile = "/f:""$(SolutionDir)$(SolutionFileName).deploy.build"" "

Dim toolKey

toolKey = "HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\8.0\External Tools\"

call Common
call AddTool(wshShell,"BT - &HAT","%ProgramFiles%\Microsoft BizTalk Server 2006\BTSHatApp.exe","",17)
call AddTool(wshShell,"&Gac This","%ProgramFiles%\Microsoft Visual Studio 8\SDK\v2.0\Bin\gacutil.exe","/i $(TargetPath) /f",26)


MsgBox "External tools have been added for BizTalk.  Enjoy!"

sub Common()

call AddTool(wshShell,"BT - B&izTalk Deploy Debug Build",nantLocation,buildFile+"debugDeploy",26)
call AddTool(wshShell,"BT - BizTalk &UnDeploy Debug Build",nantLocation,buildFile+"debugUndeploy",26)
call AddTool(wshShell,"BT - B&izTalk Deploy Release Build",nantLocation,buildFile+"releaseDeploy",26)
call AddTool(wshShell,"BT - BizTalk &UnDeploy Release Build",nantLocation,buildFile+"releaseUndeploy",26)
call AddTool(wshShell,"BT - Update Orchs/Comps/SSO Debug",nantLocation,buildFile+"updateOrchestration",26)
call AddTool(wshShell,"BT - Update SSO",nantLocation,"-D:debugDeploy=true "+buildFile+"callDeploySSO",26)
call AddTool(wshShell,"BT - Export Settings",nantLocation,"-D:debugDeploy=true "+buildFile+"exportSettings",26)
call AddTool(wshShell,"BT - Preprocess Bindings",nantLocation,"-D:debugDeploy=true "+buildFile+"callPreprocessBindings",26)
call AddTool(wshShell,"BT - Import Bindings",nantLocation,"-D:debugDeploy=true "+buildFile+"callDeployAndStartPorts",26)
call AddTool(wshShell,"BT - &NAnt Current Target",nantLocation,"-buildfile:$(ItemPath) $(CurText)",26)
call AddTool(wshShell,"BT - Bounce BizTalk",nantLocation,buildFile+"bounceBizTalk",26)

end sub

Sub AddTool(wshShell, toolTitle, toolCmd, toolArg, toolOpt)
	
	
	Dim toolCount
	toolCount = wshShell.RegRead(toolKey + "ToolNumKeys")
	call wshShell.RegWrite(toolKey + "ToolNumKeys",toolCount+1,"REG_DWORD")

	call wshShell.RegWrite(toolKey + "ToolTitle" + CStr(toolCount),toolTitle)
	call wshShell.RegWrite(toolKey + "ToolCmd" + CStr(toolCount),toolCmd)
	call wshShell.RegWrite(toolKey + "ToolArg" + CStr(toolCount),toolArg)
	call wshShell.RegWrite(toolKey + "ToolDir" + CStr(toolCount),"$(SolutionDir)")
	call wshShell.RegWrite(toolKey + "ToolSourceKey" + CStr(toolCount),"")
	call wshShell.RegWrite(toolKey + "ToolOpt" + CStr(toolCount),toolOpt,"REG_DWORD")
	
End Sub


If Err <> 0 Then
	wscript.echo "Error: " + Err.Description
	Wscript.quit 1
End If

Wscript.quit 0
