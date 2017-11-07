'--------------------------------------------------------------------------
'
' WMI script to start a send port.
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

StartSendPort
Sub StartSendPort()
	'Get the command line arguments entered for the script
	Dim objArgs: Set objArgs = WScript.Arguments

	'error handling is done by explicity checking the err object rather than using
	'the VB ON ERROR construct, so set to resume next on error.
	On Error Resume Next
	
	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (objArgs.Count < 1) Or (objArgs.Count > 2) Then
		PrintUsage()
		WScript.Quit 0
	End If

	Dim objInstSet, objInst, strQuery
	Dim strSendPortName, strPrimaryTransportAddress
	
	strSendPortName = objArgs(0)
			
	'Check if PTAddress is to be set
	If (objArgs.Count = 2) Then
		'PTAddress for these samples are being set reletive to install location
		' NOTE: This is assuming that this is a FILE transport type
		' and we want to update it with the current directory
		Dim WshShell
		Set WshShell = WScript.CreateObject("WScript.Shell")
		
		'Check for error condition before continuing.
		If Err <> 0	Then
			PrintWMIErrorThenExit Err.Description, Err.Number
		End If
		
		strPrimaryTransportAddress = WshShell.CurrentDirectory & objArgs(1)
	Else
		strPrimaryTransportAddress = ""
	End If

	'set up a WMI query to acquire a list of send ports with the given Name key value.
	'This should be a list of zero or one Send Ports.
	strQuery = "SELECT * FROM MSBTS_SendPort WHERE  Name =""" & strSendPortName & """"
	Set objInstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(strQuery)
	
	'Check for error condition before continuing.
	If Err <> 0	Then
		PrintWMIErrorThenExit Err.Description, Err.Number
	End If

	'If Send Port found, Start it, otherwise print error and end.
	If objInstSet.Count > 0 then
		For Each objInst in objInstSet
			'If the PTAddress is to be set, change it now
			If "" <> strPrimaryTransportAddress Then
				objInst.PTAddress = strPrimaryTransportAddress
				If Err <> 0	Then
					PrintWMIErrorThenExit Err.Description, Err.Number
				End If
				WScript.Echo "Primary Transport Address was set to:"
				WScript.Echo strPrimaryTransportAddress
			End If
			
			'Now commit the change
			objInst.Put_(1)
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			WScript.Echo "Changes were successfully commited."
			
			'Now start the send port
			objInst.Start
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			WScript.Echo "The Send Port was successfully started."
		Next
	Else
		WScript.Echo "No Send Port was found matching that Name."
        WScript.Quit -1
	End If
			
End Sub 

'This subroutine deals with all errors using the WbemScripting object.  Error descriptions
'are returned to the user by printing to the console.
Sub	PrintWMIErrorThenExit(strErrDesc, nErrNum)
	On Error Resume	Next
	Dim	objWMIError	: Set objWMIError =	CreateObject("WbemScripting.SwbemLastError")

	If ( TypeName(objWMIError) = "Empty" ) Then
		WScript.Echo strErrDesc & " (HRESULT: "	& Hex(nErrNum) & ")."
	Else
		WScript.Echo objWMIError.Description & "(HRESULT: "	& Hex(nErrNum) & ")."
		Set objWMIError	= Nothing
	End	If
	
	'bail out
	WScript.Quit -1
End Sub 

Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
				 "cscript StartSendPort.vbs <Send Port Name> [<Primary Transport Address>]" + _
				 Chr(10) + Chr(10) + "Where: " + Chr(10) + _
				 "  <Send Port Name> = The name of the send port tot start." + _
				 Chr(10) + "       Example: 'MyBusinessSendPort'" + Chr(10) + Chr(10) + _
				 "  <Primary Transport Address> = An optional parameter to change the PTAddress reletive to the current directory." + _
				 Chr(10) + "       Example: '\Out\%MessageID%.xml'" + Chr(10) + Chr(10)
End Sub
