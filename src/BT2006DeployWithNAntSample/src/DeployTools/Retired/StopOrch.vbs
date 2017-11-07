'--------------------------------------------------------------------------
'
' WMI script to stop a specific orchestraiton.
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

StopOrch
Sub StopOrch()
	'Get the command line arguments entered for the script
	Dim objArgs: Set objArgs = WScript.Arguments

	'error handling is done by explicity checking the err object rather than using
	'the VB ON ERROR construct, so set to resume next on error.
	on error resume next
	
	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (objArgs.Count < 2) Or (objArgs.Count > 3) Then
		PrintUsage()
		wscript.quit 0
	End If

	Dim InstSet, Inst, Query, OrchestrationName, AssemblyName, Unenlist
	
	OrchestrationName = objArgs(0)
	AssemblyName = objArgs(1)
			
	'Check if orchestration is to be started
	If (objArgs.Count = 3) Then
		If ("Unenlist" = objArgs(2)) Then
			Unenlist = True
		Else
			wscript.echo "Wrong optional flag."
			PrintUsage()
			wscript.quit 0
		End If
	End If
	
	'set up a WMI query to acquire a list of orchestrations with the given Name and 
	'AssemblyName key values.  This should be a list of zero or one Orchestrations.

	' First terminate instances!
	Query = "SELECT * FROM MSBTS_ServiceInstance WHERE AssemblyName = """ & AssemblyName & """"
	Set InstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(Query)
	if InstSet.Count > 0 then
		For Each Inst in InstSet
			Inst.Terminate
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
		next
	end if

	Query = "SELECT * FROM MSBTS_Orchestration WHERE Name =""" & OrchestrationName & """ AND AssemblyName = """ & AssemblyName & """"
	Set InstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(Query)
	
	'Check for error condition before continuing.
	If Err <> 0	Then
		PrintWMIErrorThenExit Err.Description, Err.Number
	End If

	'If orchestration found, enlist the orchestration, otherwise print error and end.
	If InstSet.Count > 0 then
		For Each Inst in InstSet
			Inst.Stop 2, 1
			If Err <> 0	Then
				wscript.echo "Warning: Orchestration not stopped: " + Err.Description
            else
                wscript.echo "The Orchestration was successfully stopped."
			End If
			
			
			If Unenlist Then
				Inst.Unenlist
				If Err <> 0	Then
					PrintWMIErrorThenExit Err.Description, Err.Number
				End If
				wscript.echo "The Orchestration was successfully unenlisted."
			End If
		Next
	Else
		wscript.echo "No orchestration was found matching that Name and AssemblyName."
	End If
			
End Sub 

'This subroutine deals with all errors using the WbemScripting object.  Error descriptions
'are returned to the user by printing to the console.
Sub	PrintWMIErrorThenExit(strErrDesc, ErrNum)
	On Error Resume	Next
	Dim	objWMIError	: Set objWMIError =	CreateObject("WbemScripting.SwbemLastError")

	If ( TypeName(objWMIError) = "Empty" ) Then
		wscript.echo strErrDesc & " (HRESULT: "	& Hex(ErrNum) & ")."
	Else
		wscript.echo objWMIError.Description & "(HRESULT: "	& Hex(ErrNum) & ")."
		Set objWMIError	= nothing
	End	If
	
	'bail out
	wscript.quit -1
End Sub 

Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
		     "cscript StopOrch.vbs <Orchestration Name> <Assembly Name> [Unenlist]" + _	
		     Chr(10) + Chr(10) + "Where: " + Chr(10) + _
		     "<Orchestration Name> = The name of the orchestration you wish to enlist." + _
		     Chr(10) + "       Example: 'MyBusinessOrchestration'" + Chr(10) + Chr(10) + _
		     "<Assembly Name>      = The name of the assembly in which the orchestration was deployed." + _
		     Chr(10) + "       Example: 'MyBusinessAssembly'" + Chr(10) + Chr(10) + _
		     "[Unenlist]              = An optional parameter to indecate that the orchestration should be unenlisted." + _
		     Chr(10) + "       Example: 'Unenlist'" + Chr(10) + Chr(10)
End Sub
