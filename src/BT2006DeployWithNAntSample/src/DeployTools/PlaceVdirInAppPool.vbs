'V1.0
On Error Resume Next

Set objArgs = WScript.Arguments

if (objArgs.Count <> 2) Then
	PrintUsage()
	WScript.Quit -1
End If

set appPools = GetObject("IIS://localhost/w3svc/AppPools")
set appPool = GetObject("IIS://localhost/w3svc/AppPools/" & objArgs(1))

if not IsObject(appPool) then
    WScript.echo "Application pool " & objArgs(1) & " does not exist..."
	WScript.quit -1
end if

set vRoot = GetObject("IIS://localhost/w3svc/1/Root/" & objArgs(0))
vRoot.AppPoolId = objArgs(1)
vRoot.SetInfo

CheckError

Wscript.Echo "Placed " & objArgs(0) & " in app pool " & objArgs(1)

Sub PrintUsage()
	WScript.Echo "Usage:"
	WScript.Echo
	WScript.Echo "cscript PlaceVDirInAppPool.vbs <Virtual Directory Name> <AppPoolName>"
	WScript.Echo
	WScript.Echo " Where: "
	WScript.Echo "  <Virtual Directory Name> = The name of the virtual directory to create."
	WScript.Echo "       Example: 'MyBusinessVirtualDirectory'"
	WScript.Echo
	WScript.Echo "  <AppPoolName> = The app pool name to create or retrieve.  Identity will be set to VDIR_UserName and VDIR_UserPass process environment variable values."
	WScript.Echo "       Example: 'MyAppPool'"
	WScript.Echo
End Sub

Sub CheckError
    Dim strMessage

    If Err = 0 Then Exit Sub
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description

    WScript.Echo strMessage
    WScript.Quit 1
End Sub
	