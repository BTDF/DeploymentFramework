'--------------------------------------------------------------------------
'
' WMI script to enable a receive location.
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

EnableReceiveLocation
Sub EnableReceiveLocation()
	'Get the command line arguments entered for the script
	Dim objArgs: Set objArgs = WScript.Arguments

	'error handling is done by explicity checking the err object rather than using
	'the VB ON ERROR construct, so set to resume next on error.
	On Error Resume Next
	
	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (objArgs.Count < 2) Or (objArgs.Count > 3) Then
		PrintUsage()
		WScript.Quit 0
	End If

	Dim objInstSet, objInst, strQuery
	Dim strReceivePortName, strReceiveLocationName, strTransportURL
	
	strReceivePortName = objArgs(0)
	strReceiveLocationName = objArgs(1)
			
	'Check if TransportURL is to be set
	If (objArgs.Count = 3) Then
		'TransportURI for these samples are being set reletive to install location
		' NOTE: This is assuming that this is a FILE transport type
		' and we want to update it with the current directory
		Dim WshShell
		Set WshShell = WScript.CreateObject("WScript.Shell")
		
		'Check for error condition before continuing.
		If Err <> 0	Then
			PrintWMIErrorThenExit Err.Description, Err.Number
		End If
		
		strTransportURL = WshShell.CurrentDirectory & objArgs(2)
	Else
		strTransportURL = ""
	End If

	'set up a WMI query to acquire a list of receive locations with the given Name and 
	'ReceivePortName key values.  This should be a list of zero or one Receive Locations.
	strQuery = "SELECT * FROM MSBTS_ReceiveLocation WHERE  ReceivePortName =""" & strReceivePortName & """AND Name =""" & strReceiveLocationName & """"
	Set objInstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(strQuery)
	
	'Check for error condition before continuing.
	If Err <> 0	Then
		PrintWMIErrorThenExit Err.Description, Err.Number
	End If

	'If Receive Location found, enable it, otherwise print error and end.
	If objInstSet.Count > 0 then
		For Each objInst in objInstSet
			'If the TransportURL is to be set, change it now
			If "" <> strTransportURL Then
				objInst.InboundTransportURL = strTransportURL
				If Err <> 0	Then
					PrintWMIErrorThenExit Err.Description, Err.Number
				End If
				WScript.Echo "Inbound Transport URL was set to:"
				WScript.Echo strTransportURL
			End If
			
			'Now commit the change
			objInst.Put_(1)
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			WScript.Echo "Changes were successfully commited."
			
			'Now enable to receive location
			objInst.Enable
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			WScript.Echo "The Receive Location was successfully enabled."
		Next
	Else
		WScript.Echo "No Receive Location was found matching that Name."
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
				 "cscript EnableRecLoc.vbs <Receive Port Name> <Receive Location Name> [<Inbound Transport URI>]" + _
				 Chr(10) + Chr(10) + "Where: " + Chr(10) + _
				 "  <Receive Port Name>     = The name of the receive port that contains the receive location." + _
				 Chr(10) + "       Example: 'MyBusinessReceivePort'" + Chr(10) + Chr(10) + _
				 "  <Receive Location Name> = The name of the receive location to enable." + _
				 Chr(10) + "       Example: 'MyBusinessReceiveLocation'" + Chr(10) + Chr(10) + _
				 "  <Inbound Transport URI> = An optional parameter to change the Inbound Transport URI reletive to the currect directory." + _
				 Chr(10) + "       Example: '\In\*.xml'" + Chr(10) + Chr(10)
End Sub
