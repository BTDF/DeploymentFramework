' Deployment Framework for BizTalk
' Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
'   
' This source file is subject to the Microsoft Public License (Ms-PL).

Option Explicit       'force variable declarations before use


Dim objArgs: Set objArgs = WScript.Arguments
Dim WshShell:Set WshShell = WScript.CreateObject("WScript.Shell")
Dim WshSysEnv:Set WshSysEnv = WshShell.Environment("PROCESS")


Dim comApp, errorString
Dim userName, userPass
errorString = ""

If (objArgs.Count <> 1) Then
	PrintUsage()
	WScript.Quit 0
End If

set comApp = New ComApplication
call comApp.ConnectToHost("localhost",objArgs(0),errorString)

' Expectation is that these are in defined environment variables.
userName = WshSysEnv("VDIR_UserName")
userPass = WshSysEnv("VDIR_UserPass")


call comApp.SetApplicationIdentity(userName,userPass,errorString)
if errorString <> "" then
	Wscript.echo errorString
	Wscript.quit -1
end if



'23-Dec-2000  rnielsen@radiantsystems.com
'  Defines the following classes for automated 
'  administration of COM+ objects.  These classes
'  correspond roughly to like-named objects in the 
'  COM+ administration object model.
'	Application - COM+ application object
'	Component - Defined COM component within an Application
'	Interface - Published interface for a COM Component
'  The original design was used to provide object classes
'  for the Wave build automation.  It is used by the build
'  scripts to manipulate the COM+ applications created as
'  part of the Wave build.

'++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
'	Application Class
'++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Class COMApplication
  '/////////////////// Public Interface ///////////////////
	'////////////  Public Properties ////////////

  Public Property Get ComponentsCount
    Set components = applications.GetCollection("Components",app.Value("ID"))
    components.Populate
    ComponentsCount = components.count
    Set components = Nothing
  End Property

  Public Property Get Description
    Description = App.Value("Description")
  End Property

  Public Property Let Description(DescriptionString)
	  GetApp(App.Name)
    App.Value("Description") = DescriptionString
    applications.SaveChanges
  End Property

  Public Property Get RunForever
	RunForever = App.Value("RunForever")
  End Property

  Public Property Let RunForever(RunForeverValue)
    If RunForeverValue = True Or RunForeverValue = False Then
      GetApp(App.Name)
      App.Value("RunForever") = RunForeverValue
      applications.SaveChanges
    End If
  End Property

  Public Property Get Activation
    Activation = App.Value("Activation")
  End Property

  Public Property Let Activation(ActivationValue)
    If ActivationValue = 0 Or ActivationValue = 1 Then
      GetApp(App.Name)
      App.Value("Activation") = ActivationValue
      applications.SaveChanges
    End If
  End Property

  Public Property Get QueuingEnabled
    QueuingEnabled = App.Value("QueuingEnabled")
  End Property
  
  Public Property Let QueuingEnabled(QueuingEnabledValue)
    If QueuingEnabledValue = True or QueuingEnabledValue = False Then
    	GetApp(App.Name)
    	App.Value("QueuingEnabled") = QueuingEnabledValue
    	applications.SaveChanges
    End If
  End Property
  
  Public Property Get QueueListenerEnabled
    QueueListenerEnabled = App.Value("QueueListenerEnabled")
  End Property
  
  Public Property Let QueueListenerEnabled(QueueListenerEnabledValue)
    If QueueListenerEnabledValue = True or QueueListenerEnabledValue = False Then
    	GetApp(App.Name)
    	App.Value("QueueListenerEnabled") = QueueListenerEnabledValue
    	applications.SaveChanges
    End If
  End Property
  
  Public Property Get Authentication
    Authentication = App.Value("Authentication")
  End Property
  
  Public Property Let Authentication(AuthenticationValue)
    If AuthenticationValue >= 0 or AuthenticationValue <= 6 Then
    	GetApp(App.Name)
    	App.Value("Authentication") = AuthenticationValue
    	applications.SaveChanges
    End If
  End Property
  
	'////////////  Public Methods ////////////
  Public Function ConnectToHost(HostName,AppName,ErrorString)
	' attempts connection to a COM+ host catalog
	' returns true if connection succeeds
	' note this version contains no authentication
	' information, connection must be made in the 
	' context of the executing user
	' on exit the applications collection is populated

    On Error Resume Next	'disable error trapping

    Const FunctionName = "COMApplication.ConnectToHost"

    Set applications = catalog.GetCollection("Applications")
    applications.populate
    If applications Is Nothing Then
      ErrorString = "Failure to connect to host " & HostName & ".  Failed in " & FunctionName & _
		    ", probably due to an invalid host name or inaccessible host."
      ConnectToHost = False
    Else
      ConnectToHost = True
    End If

    	'if the application exists, assign it to this object
    Dim Status 'temporary function return value
		    Status = GetApp(AppName) 
		    
  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function CreateApplication(AppName,ErrorString)
	'create a new COM+ application
	'give the new application the parameter as a name
	'repopulate the applications collection so the new
	'app will show up in the collection
	'return true if the add is successful
    
    On Error Resume Next

    Const FunctionName = "COMApplication.CreateApplication"

    Set app = applications.Add
    If app Is Nothing Then
      ErrorString = "Failed to create a new COM+ application.  Failure in " & FunctionName & "."
      CreateApplication = False
    Else
      CreateApplication = True
    End If
    app.Value("Name") = AppName
    applications.SaveChanges
    If Err.Number <> 0 Then
      ErrorString = "Failed to save changes to the applications collection.  Failure in " & FunctionName & "."
      CreateApplication = False      
    End If
    
    		'refresh the applications collection to reflect the new addition
    applications.Populate

  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function DeleteApplication(AppName,ErrorString)
	'delete an existing COM+ application
	'assumes the applications collection is already 
	'populated
	'takes no action and returns success if application
	'is not found

    On Error Resume Next

    Const FunctionName = "COMApplication.DeleteApplication"
    
    Dim AppObject	'COM+ application object
    Dim AppIndex	'index into the applications collection

	'initialize function return value to true, handles absent app
    DeleteApplication = True

    applications.Populate
    
    AppIndex = 0
    
    For Each AppObject In applications 
      If appObject.Name = AppName Then
				applications.Remove(AppIndex)
				If Err.Number <> 0 Then
          ErrorString = "Failed to delete the " & AppName & " application.  Failure in " & FunctionName & "."
          DeleteApplication = False      
				Else
	  			applications.SaveChanges
 	  			If Err.Number <> 0 Then
            ErrorString = "Failed to save changes on delete of the " & AppName & " application.  Failure in " & FunctionName & "."
            DeleteApplication = False      
	  			End If
        End If
					Exit For
      End If
      AppIndex = AppIndex + 1
    Next

  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function AddComponents(AppName,DLLPath,ErrorString)
	'add components from on-disk DLL's to an application

    On Error Resume Next

    Const FunctionName = "COMApplication.AddComponents"

    Dim DLLFolder
    Dim DLLFiles
    Dim DLLFile

	'initialize the function return value
    AddComponents = False
    If Not FileSystem.FolderExists(DLLPath) Then
      ErrorString = "The folder supplied as the path to DLL's does not exist."
      AddComponents = False
      Exit Function     
    End If

    Set DLLFolder = FileSystem.GetFolder(DLLPath)
    If DLLFolder Is Nothing Then
      ErrorString = "Failure to attach to DLL folder.  Failed in " & FunctionName & "."
      AddComponents = False
      Exit Function     
    End If

    Set DLLFiles = DLLFolder.Files
    If DLLFiles Is Nothing Then
      ErrorString = "Failure to attach to DLL folder files.  Failed in " & FunctionName & "."
      AddComponents = False
      Exit Function     
    End If

    For Each DLLFile in DLLFiles
      catalog.InstallComponent AppName,DLLFile.Path,"",""
      If Err.Number Then
        ErrorString = "Failed to add the " & DLLFile.Path & " component.  Failure in " & FunctionName & "."
        AddComponents = False     
        Exit Function
      End If
    Next

    applications.SaveChanges
    If Err.Number Then
      ErrorString = "Failed to save changes on add of the " & AppName & " components.  Failure in " & FunctionName & "."
      AddComponents = False      
      Exit Function
    End If

    AddComponents = True

  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function SetComponentProperty(compName,propertyName,PropertySetting,OriginalSetting,ErrorString)
  	'sets selected properties of a component
		'returns original setting of the property in the "OriginalSetting" parameter
		
  Const FunctionName = "COMApplication.SetComponentProperty"

  Dim compObject

  On Error Resume Next
  
  	'initialize the function return value
  SetComponentProperty = True
  
  Set components = applications.GetCollection("Components",app.Value("ID"))
  If components Is Nothing Then
    ErrorString = "Failed to acquire the components collection in " & FunctionName & "."
    SetComponentProperty = False
    Exit Function
  End If
  
  components.Populate
  
  For Each compObject In components
    If LCase(compObject.Value("ProgID")) = compName Then
      OriginalSetting = compObject.Value(propertyName)
      compObject.Value(propertyName) = PropertySetting
      If Err.Number <> 0 Then
        ErrorString = "Failed to save property updates on the " & compName & " component.  Failure in " & FunctionName & "."
        SetComponentProperty = False      
        Exit Function
      End If 
   End If
  Next

  components.SaveChanges
  If Err.Number <> 0 Then
      ErrorString = "Failed to save changes on a set property of the " & compName & " component.  Failure in " & FunctionName & "."
      SetComponentProperty = False 
      Exit Function
  End If
  
  Set components = Nothing
  
  Exit Function
  End Function
   '-----------------------------------------------------------
  Public Function SetInterfaceProperty(compName,interfaceName,propertyName,PropertySetting,ErrorString)
  	'sets selected properties of a interface

  Const FunctionName = "COMApplication.SetInterfaceProperty"

  Dim compObject
  Dim interfaceObject
  Dim interfaces		'COM+ collection of interfaces for a specific component
  Dim SaveUpdates		'flag indicates changes were made and should be saved
  
  On Error Resume Next
  
  	'initialize the function return value
  SetInterfaceProperty = True
  
  Set components = applications.GetCollection("Components",app.Value("ID"))
  If components Is Nothing Then
    ErrorString = "Failed to acquire the components collection in " & FunctionName & "."
    SetComponentProperty = False
    Exit Function
  End If
  
  components.Populate
  
  For Each compObject In components
    If LCase(compObject.Value("ProgID")) = compName Then
      Set interfaces = components.GetCollection("InterfacesForComponent",compObject.Key)
      If interfaces is Nothing Then
        ErrorString = "Failed to access interfaces on component " & compName & ".  Failure in " & FunctionName & "."
        Set components = Nothing
        SetInterfaceProperty = False      
        Exit Function
      Else
        interfaces.Populate
        For Each interfaceObject In interfaces
          If LCase(interfaceObject.Name) = LCase(InterfaceName) Then
            interfaceObject.Value(propertyName) = PropertySetting
            If Err.Number <> 0 Then
              ErrorString = "Failed to save property updates on the " & interfaceName & " interface of " & compName & " component.  Failure in " & FunctionName & "."
              Set components = Nothing
              SetInterfaceProperty = False      
              Exit Function
            Else
              SaveUpdates = True
            End If 
          End If
        Next
      End If 
    End If
  Next

  Set components = Nothing
  If SaveUpdates Then 
    interfaces.SaveChanges
    If Err.Number <> 0 Then
      ErrorString = "Failed to save changes on a set property of the " & interfaceName & " component interface.  Failure in " & FunctionName & "."
      SetInterfaceProperty = False      
      Exit Function
    End If
  End If
  
  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function StartApplication(AppName,ErrorString)
  		'starts a COM+ application
		'use this on Server applications set as queued
  		
  	Const FunctionName = "COMApplication.StartApplication"
  	
  	On Error Resume Next
  	
    'GetApp(App.Name)
     catalog.StartApplication AppName
	  If Err.Number	Then
	    ErrorString = "Failed to start the application.  Failed in function " & FunctionName & "."
	    StartApplication = False
	  Else
	  	StartApplication = True
	  End If 
  	
  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function Export(PackagePath,ErrorString)
  		'exports the application to a reloadable on-disk package
  
  Const FunctionName = "Export"

  Const COMAdminExportForceOverwriteOfFiles = 4  'COM+ constant
  
  On Error Resume Next
  
    catalog.ExportApplication app.Name,PackagePath,COMAdminExportForceOverwriteOfFiles
  	If Err.Number Then
  	  ErrorString = "Failed to successfully export application " & app.Name & _
  	                " to path " & PackagePath & ".  Failed in function: " & FunctionName & "."
  	  Export = False  	
  	Else
  	  Export = True
  	End If
  	
  Exit Function
  End Function
  '-----------------------------------------------------------
  Public Function SetApplicationIdentity(UserName,Password,ErrorString)
  		'sets the username and password for a Server application
  		'identity is not used for Library applcations
  		
  	Const FunctionName = "SetApplicationIdentity"
  	
  	On Error Resume Next
  	
    GetApp(App.Name)
	  App.Value("Identity") = UserName
	  App.Value("Password") = Password
	  applications.SaveChanges	
	  If Err.Number	Then
	    ErrorString = "Failed to set the application identity/password.  Failed in function " & FunctionName & "."
	    SetApplicationIdentity = False
	  Else
	  	SetApplicationIdentity = True
	  End If 
  	
  Exit Function
  End Function
  '/////////////////// Private Section ///////////////////

  Private catalog			'COMAdmincatalog object, top level of object model
  Private hostcatalog	'object with capabilities to connect to a remote COM+ host
  Private applications	'COMAdminCollections object, contains all application on target system
  Private app					'COMAdminCatalogObject, application
  Private components	'COMAdminCollections object, contains all components for an application

  Private Sub Class_Initialize
    'initialize class variables and set the COMAdmin objects
    Set catalog = CreateObject("COMAdmin.COMAdminCatalog")
  Exit Sub
  End Sub

  Private Sub Class_Terminate
    'cleanup
    applications.SaveChanges
    Set applications = Nothing
    Set hostcatalog = Nothing
    Set catalog = Nothing
  Exit Sub
  End Sub
  '-----------------------------------------------------------
  Private Function GetApp(AppName)
			'iterates through the catalog of COM+ applications for the
			'provided application name.   Associates the app object
			'with it if found
			
    Dim AppObject

    GetApp = False

    applications.populate

    For Each AppObject In applications
      If AppObject.Name = AppName Then
        Set app = AppObject
        GetApp = True
        Exit For
      End If
    Next

  Exit Function
  End Function
  '-----------------------------------------------------------
End Class


Sub PrintUsage()
	WScript.Echo "Usage:"
	WScript.Echo
	WScript.Echo "cscript SetPackageIdentity.vbs <COMPlusPackageName>"
	WScript.Echo
	WScript.Echo
End Sub
