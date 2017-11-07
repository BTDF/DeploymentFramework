' Deployment Framework for BizTalk
' Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
'   
' This source file is subject to the Microsoft Public License (Ms-PL).

Option Explicit
On Error Resume Next

Dim objArgs
Set objArgs = WScript.Arguments
If (objArgs.Count < 2) Then
	WScript.Echo "WriteRegValue.vbs registry_path value" + Chr(10) + Chr(10) + _
		"Example: " + Chr(10) + _
		"  cscript.exe /NoLogo WriteRegValue.vbs " + Chr(34) + "HKLM\SOFTWARE\MyCompany\Foo bar" + Chr(34) + Chr(10)
	WScript.Quit exitCode
End If

Dim wshShell
Set wshShell = WScript.CreateObject("WScript.Shell")
call wshShell.RegWrite(objArgs(0),objArgs(1))

If Err <> 0 Then
	wscript.echo "Error: " + Err.Description
	Wscript.quit 1
Else
    wscript.echo "Wrote to key " & objArgs(0) & "."
End If

Wscript.quit 0
