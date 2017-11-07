'v1.0

'--------------------------------------------------------------------------
' File: RemoveIISVDir.vbs
'
' Summary: This file is used by several samples to remove an IIS
'          virtual directory.
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

RemoveIISVirtualDirectory
Sub RemoveIISVirtualDirectory()
	'Get the command line arguments entered for the script
	Dim objArgs: Set objArgs = WScript.Arguments

	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (objArgs.Count <> 1) Then
		PrintUsage()
		WScript.Quit 0
	End If

	Dim objVirtualRoot, objVirtualDirectory
	Dim strDirectoryName

	strDirectoryName = objArgs(0)

	'Get IIS virtual directory object
    On Error Resume Next
	Set objVirtualDirectory = GetObject("IIS://localhost/w3svc/1/Root/" & strDirectoryName)
    if not IsObject(objVirtualDirectory) then
        WScript.Echo "Virtual Directory " & strDirectoryName & " does not exist."
        Wscript.Quit
    end if
    On Error Goto 0
    
	objVirtualDirectory.AppDeleteRecursive
	
	'Save the changes to the IIS metabase
	objVirtualDirectory.SetInfo
	
	Set objVirtualDirectory = nothing

	'Get IIS virtual directory object
	Set objVirtualRoot = GetObject("IIS://localhost/w3svc/1/Root")

	'Remove virtual directory
	objVirtualRoot.Delete "IIsWebVirtualDir", strDirectoryName
    
    WScript.Echo "Virtual Directory " & strDirectoryName & " removed."
End Sub

Sub PrintUsage()
	WScript.Echo "Usage:"
	WScript.Echo
	WScript.Echo "cscript RemoveIISVDir.vbs <Virtual Directory Name>"
	WScript.Echo
	WScript.Echo " Where: "
	WScript.Echo "  <Virtual Directory Name> = The name of the virtual directory to create."
	WScript.Echo "       Example: 'MyBusinessVirtualDirectory'"
	WScript.Echo
End Sub
