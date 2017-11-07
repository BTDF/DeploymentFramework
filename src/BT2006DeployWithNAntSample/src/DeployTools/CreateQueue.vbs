Dim objArgs
Dim queueName
Dim queueLabel

Set objArgs = WScript.Arguments
queueName = objArgs(0)
queueLabel = objArgs(1)
argTransactional = objArgs(2)

CreateQueue queueName, queueLabel, argTransactional


Sub CreateQueue(ByVal queueName, ByVal queueLabel, ByVal transactional)
On Error Resume Next

	Dim isTransactional
	If transactional = "Transactional" then
		isTransactional = True
	Else
		isTransactional = False
	End If

	Dim qinfo 
	Set qinfo = CreateObject ("MSMQ.MSMQQueueInfo")
	qinfo.PathName = ".\PRIVATE$\" & queueName
	qinfo.Label = queueLabel 
	qinfo.Create isTransactional,True

	If Err.Number = 0 Then
		WScript.Echo "Queue: " & qinfo.PathName & " created"
	Else
		WScript.Echo "Error " & CStr(Err.Number) & ": " & Err.Description
	End If

End Sub
