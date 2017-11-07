'--------------------------------------------------------------------------
'
' WMI script to enlist a specific orchestraiton.
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

EnlistOrch
Sub EnlistOrch()
	'Get the command line arguments entered for the script
	Dim Args: Set Args = WScript.Arguments

	'error handling is done by explicity checking the err object rather than using
	'the VB ON ERROR construct, so set to resume next on error.
	on error resume next
	
	'Make sure the expected number of arguments were provided on the command line.
	'if not, print usage text and exit.
	If (Args.Count < 2) Or (Args.Count > 3) Then
		PrintUsage()
		wscript.quit 0
	End If

	Dim InstSet, Inst, Query, OrchestrationName, AssemblyName, HostName, Start
	Dim AutoEnableReceiveLocation: AutoEnableReceiveLocation = 2
	Dim AutoResumeOrchestrationInstance: AutoResumeOrchestrationInstance = 2
	
	OrchestrationName = Args(0)
	AssemblyName = Args(1)
			
	'Check if orchestration is to be started
	If (Args.Count = 3) Then
		If ("Start" = Args(2)) Then
			Start = True
		Else
			wscript.echo "Wrong optional flag."
			PrintUsage()
			wscript.quit 0
		End If
	End If
	
	'set up a WMI query to acquire a list of defaul inprocess hosts
	'This should be a list of zero or one host.
	Query = "SELECT * FROM MSBTS_HostSetting WHERE IsDefault =""TRUE"""
	Set InstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(Query)

	'Check for error condition before continuing.
	If Err <> 0 Then
		PrintWMIErrorthenExit Err.Description, Err.Number
	End If

	'if Default Host found, get Host Name and NT Group Name.  There is only one default host.
	If InstSet.Count > 0 Then
		For Each Inst In InstSet
			HostName = Inst.Name
			If Err <> 0	Then
				PrintWMIErrorthenExit Err.Description, Err.Number
			End If
			' (Scott Colestock) - commented this out, since we will use the host for the orchestration if it is defined
			'wscript.echo "Using default inprocess host " & HostName & "."
		Next
	End If

	'set up a WMI query to acquire a list of orchestrations with the given Name and 
	'AssemblyName key values.  This should be a list of zero or one Orchestrations.
	Query = "SELECT * FROM MSBTS_Orchestration WHERE Name =""" & OrchestrationName & """ AND AssemblyName = """ & AssemblyName & """"
	Set InstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(Query)
	
	'Check for error condition before continuing.
	If Err <> 0	Then
		PrintWMIErrorThenExit Err.Description, Err.Number
	End If

	'If orchestration found, enlist the orchestration, otherwise print error and end.
	If InstSet.Count > 0 then
		For Each Inst in InstSet
			' Use the existing host name if one is there
			if(Inst.HostName <> "" and Inst.HostName <> vbNull) then
			   HostName = Inst.HostName
			end if
			
			Inst.Enlist(HostName)
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			wscript.echo "The Orchestration was successfully enlisted in host: " & HostName
			
			If Start Then
				Inst.Start AutoEnableReceiveLocation, AutoResumeOrchestrationInstance
				If Err <> 0	Then
					PrintWMIErrorThenExit Err.Description, Err.Number
				End If
				wscript.echo "The Orchestration was successfully started."
			End If
		Next
	Else
		wscript.echo "No orchestration was found matching that Name and AssemblyName."
        wscript.quit -1
	End If
			
End Sub 

'This subroutine deals with all errors using the WbemScripting object.  Error descriptions
'are returned to the user by printing to the console.
Sub	PrintWMIErrorThenExit(strErrDesc, ErrNum)
	On Error Resume	Next
	Dim	WMIError : Set WMIError = CreateObject("WbemScripting.SwbemLastError")

	If ( TypeName(WMIError) = "Empty" ) Then
		wscript.echo strErrDesc & " (HRESULT: "	& Hex(ErrNum) & ")."
	Else
		wscript.echo WMIError.Description & "(HRESULT: " & Hex(ErrNum) & ")."
		Set WMIError = nothing
	End	If
	
	'bail out
	wscript.quit -1
End Sub 

Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
				 "cscript EnlistOrch.vbs <Orchestration Name> <Assembly Name> [Start]" + _
				 Chr(10) + Chr(10) + " Where: " + Chr(10) + _
				 "  <Orchestration Name> = The name of the orchestration you wish to enlist." + _
				 Chr(10) + "       Example: 'MyBusinessOrchestration'" + Chr(10) + Chr(10) + _
				 "  <Assembly Name>      = The name of the assembly in which the orchestration was deployed." + _
				 Chr(10) + "       Example: 'MyBusinessAssembly'" + Chr(10) + Chr(10) + _
				 "  [Start]              = An optional parameter to indecate that the orchestration should be started." + _
				 Chr(10) + "       Example: 'Start'" + Chr(10) + Chr(10)
End Sub
