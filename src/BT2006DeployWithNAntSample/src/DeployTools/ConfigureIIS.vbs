'V1.0

'--------------------------------------------------------------------------
' File: ConfigureIIS.vbs
'
' Summary: This file is used by several samples to configure an IIS
'          virtual directory.
'
' (modifed by scott colestock to improve error codes/handling)
'
'--------------------------------------------------------------------------
' This file is part of the Microsoft BizTalk Server 2004 SDK
'
' Copyright (c) Microsoft Corporation. All rights reserved.
'
' This source code is intended only as a supplement to Microsoft BizTalk
' Server 2004 release and/or on-line documentation. See these other
' materials for detailed information regarding Microsoft code samples.
'
' THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
' KIND, WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
' IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
' PURPOSE.
'--------------------------------------------------------------------------

Option Explicit

ConfigureIISVirtualDirectory
Sub ConfigureIISVirtualDirectory()
    On Error Resume Next

	'Get the command line arguments entered for the script
	Dim objArgs: Set objArgs = WScript.Arguments

	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (objArgs.Count <> 2) Then
		PrintUsage()
		WScript.Quit -1
	End If

	Dim objVirtualRoot, objVirtualDirectory
	Dim strDirectoryName, strDirectoryPath
	Dim WshShell
	Set WshShell = WScript.CreateObject("WScript.Shell")

	strDirectoryName = objArgs(0)
	strDirectoryPath = WshShell.CurrentDirectory & objArgs(1)

    ' Check for existence of virtual directory - this portion added by scott colestock
    Set objVirtualDirectory = GetObject("IIS://localhost/w3svc/1/Root/" & strDirectoryName)
    if IsObject(objVirtualDirectory) then
        WScript.Echo "Virtual Directory " & strDirectoryName & " already exists."
        Wscript.Quit
    else
        Err.Clear
    end if

	'Get IIS virtual root object
	Set objVirtualRoot = GetObject("IIS://localhost/w3svc/1/Root") : CheckError

	'Create new virtual directory
	Set objVirtualDirectory = objVirtualRoot.Create("IIsWebVirtualDir", strDirectoryName)

	'Set properties on new virtual directory
	objVirtualDirectory.AccessRead = True
	objVirtualDirectory.AccessExecute = True
	objVirtualDirectory.AppFriendlyName = strDirectoryName
	objVirtualDirectory.AuthFlags = 5   ' 1 for anonymous only
	objVirtualDirectory.Path = strDirectoryPath
	objVirtualDirectory.KeyType = "IIsWebVirtualDir"
	
	'IMPORTANT SECURITY NOTE
	'This virtual directory is being created in high isolation mode
	'This will cause a COM+ App to be created for this virtual directory
	'The idenetity used by this COM+ App must have access the the BTS DB
	'It is NOT recommend that you use the IWAM_MachineName acount for this
	objVirtualDirectory.AppIsolated = 1
	objVirtualDirectory.AppCreate False
	
	'Save the changes to the IIS metabase
	objVirtualDirectory.SetInfo
	
    CheckError
    
	WScript.Echo "Virtual Directory " & strDirectoryName & " created with path:"
	WScript.Echo strDirectoryPath
End Sub

Sub PrintUsage()
	WScript.Echo "Usage:"
	WScript.Echo
	WScript.Echo "cscript ConfigIIS.vbs <Virtual Directory Name> <Virtual Directory Path>"
	WScript.Echo
	WScript.Echo " Where: "
	WScript.Echo "  <Virtual Directory Name> = The name of the virtual directory to create."
	WScript.Echo "       Example: 'MyBusinessVirtualDirectory'"
	WScript.Echo
	WScript.Echo "  <Virtual Directory Path> = The path to the directory to be made available relative to the current directory."
	WScript.Echo "       Example: 'MyVirtualDirectory'"
	WScript.Echo
End Sub



Sub CheckError
    Dim strMessage

    If Err = 0 Then Exit Sub
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description

    WScript.Echo strMessage
    WScript.Quit 1
End Sub

