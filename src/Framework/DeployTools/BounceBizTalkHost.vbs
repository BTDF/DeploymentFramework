' Deployment Framework for BizTalk 5.0
' Copyright (C) 2004-2012 Thomas F. Abraham and Scott Colestock
'   
' This source file is subject to the Microsoft Public License (Ms-PL).

On Error Resume Next
Err.Clear

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
    if objItem.IsDisabled=TRUE then
        Wscript.Echo "Skipping disabled instance: " & objItem.HostName & " on " & objItem.RunningServer
    else
        Wscript.Echo "Stopping and starting: " & objItem.HostName & " on " & objItem.RunningServer

        objItem.Stop
        if CheckError = 0 then
            objItem.Start
        end if
        for i = 1 to 4
            if CheckError = -1 then
                Err.Clear
                WScript.Echo "Retrying: " + Cstr(i)
                objItem.Stop
                objItem.Start   
            else
                Exit For                 
            end if
        next
        
        ' We exhausted retries so exit with a -1.
        if(i = 5) then
            Wscript.Quit -1
        end if        
    end if
Next


Function CheckError()

    If Err = 0 Then 
        CheckError = 0
        Exit Function
    End if
    
    Dim strMessage        
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description
    WScript.Echo strMessage
    CheckError = -1

End Function
    
Sub PrintUsage()
	WScript.Echo "Usage:" + Chr(10) + Chr(10) + _
		     "cscript BounceBizTalkHost.vbs <HostName>" + _	
		     Chr(10) + Chr(10) + "Where: " + Chr(10) + _
		     "<HostName> = The name of the BizTalkHost you wish to restart, or ALL" +  Chr(10) + Chr(10)
End Sub
    