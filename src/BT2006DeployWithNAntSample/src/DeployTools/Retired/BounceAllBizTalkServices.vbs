On Error Resume Next
strComputer = "."
Set objWMIService = GetObject("winmgmts:\\" & strComputer & "\root\cimv2")
Set colItems = objWMIService.ExecQuery("Select * from Win32_Service Where DisplayName LIKE ""BizTalk Service%""",,48)
For Each objItem in colItems
    Wscript.Echo "Stopping and starting: " & objItem.DisplayName

    objItem.StopService
    objItem.StartService
    CheckError
Next


Sub CheckError
    Dim strMessage

    If Err = 0 Then Exit Sub
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description

    WScript.Echo strMessage
    WScript.Quit -1
End Sub
    