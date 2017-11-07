'V1.0
On Error Resume Next

Set objArgs = WScript.Arguments
Set WshShell = WScript.CreateObject("WScript.Shell")
Set WshSysEnv = WshShell.Environment("PROCESS")

if (objArgs.Count <> 1) Then
	PrintUsage()
	WScript.Quit -1
End If

set appPools = GetObject("IIS://localhost/w3svc/AppPools")
set appPool = appPools.Create("IIsApplicationPool",objArgs(0))

if not IsObject(appPool) then
	set appPool = GetObject("IIS://localhost/w3svc/AppPools/" & objArgs(0))
    Wscript.echo "Unable to create app pool, but retrieved existing one: " & objArgs(0)
	Err.Clear
else
    Wscript.echo "Created app pool: " & objArgs(0)
end if

appPool.AppPoolIdentityType = 3 ' distinct account

' Expectation is that these are in defined environment variables.
userName = WshSysEnv("VDIR_UserName")
userPass = WshSysEnv("VDIR_UserPass")

appPool.WAMUserName = userName
appPool.WAMUserPass = userPass
appPool.SetInfo

CheckError

Wscript.Echo "Identity for app pool " & objArgs(0) & " has been set."

Sub PrintUsage()
	WScript.Echo "Usage:"
	WScript.Echo
	WScript.Echo "cscript SetAppPoolInfo.vbs <AppPoolName>"
	WScript.Echo
	WScript.Echo " Where: "
	WScript.Echo "  <AppPoolName> = The app pool name to create or retrieve.  Identity will be set to VDIR_UserName and VDIR_UserPass process environment variable values."
	WScript.Echo "       Example: 'MyAppPool'"
	WScript.Echo
End Sub

Sub CheckError
    Dim strMessage

    If Err = 0 Then Exit Sub
    strMessage = Err.Source & " " & Hex(Err) & ": " & Err.Description

    WScript.Echo strMessage
    WScript.Quit -1
End Sub
	