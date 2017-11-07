'--------------------------------------------------------------------------
'
' WMI script to start a send port group.
'

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
	If (objArgs.Count < 1) Or (objArgs.Count > 1) Then
		PrintUsage()
		WScript.Quit 0
	End If

	Dim objInstSet, objInst, strQuery
	Dim strSendPortName
	
	strSendPortName = objArgs(0)
			

	'set up a WMI query to acquire a list of send port groups with the given Name key value.
	'This should be a list of zero or one Send Ports.
	strQuery = "SELECT * FROM MSBTS_SendPortGroup WHERE  Name =""" & strSendPortName & """"
	Set objInstSet = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(strQuery)
	
	'Check for error condition before continuing.
	If Err <> 0	Then
		PrintWMIErrorThenExit Err.Description, Err.Number
	End If

	'If Send Port found, Start it, otherwise print error and end.
	If objInstSet.Count > 0 then
		For Each objInst in objInstSet
			
			
			'Now start the send port group
			objInst.Start
			If Err <> 0	Then
				PrintWMIErrorThenExit Err.Description, Err.Number
			End If
			WScript.Echo "The Send Port Group was successfully started."
		Next
	Else
		WScript.Echo "No Send Port Group was found matching that Name."
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
	WScript.Quit 0
End Sub 

Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
				 "cscript StartSendPortGroup.vbs <Send Port Group Name> " + _
				 Chr(10) + Chr(10) + "Where: " + Chr(10) + _
				 "  <Send Port Group Name> = The name of the send port group to start." + _
				 Chr(10) + "       Example: 'MyBusinessSendPortGroup'" + Chr(10) + Chr(10) 
End Sub
