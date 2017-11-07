' V1.0
On Error Resume Next

Dim objArgs: Set objArgs = WScript.Arguments
If (objArgs.Count <> 1) Then
	PrintUsage()
	wscript.quit 0
End If

if ucase(objArgs(0)) <> "ALL" then
   Query = "Select * from MSBTS_HostInstance where HostType=1 and HostName=" & chr(34) & objArgs(0) & chr(34)
else
   Query = "Select * from MSBTS_HostInstance where HostType=1"
end if   

Set colItems = GetObject("Winmgmts:!root\MicrosoftBizTalkServer").ExecQuery(Query)
For Each objItem in colItems
    Wscript.Echo "Stopping: " & objItem.HostName

    objItem.Stop
    CheckError
Next


Sub CheckError
    Dim strMessage

    If Err = 0 Then Exit Sub
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description

    WScript.Echo strMessage
    WScript.Quit -1
End Sub
    
Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
		     "cscript StopBizTalkHost.vbs <HostName>" + _	
		     Chr(10) + Chr(10) + "Where: " + Chr(10) + _
		     "<HostName> = The name of the BizTalkHost you wish to stop, or ALL" +  Chr(10) + Chr(10)
End Sub
    