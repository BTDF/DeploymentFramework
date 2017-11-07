// Dynamically generates the WiX XML necessary to install a directory tree.
// Version 5.0

// This is originally the work of Loren Halvorsen, with modifications for the Deployment Framework
// (specifically, the program menu shortcuts we like to have.) It could be that WiX fragments could
// be used to create these shortcuts, and this script could be used without modification from Loren's work.
// Loren's articles appeared here: http://weblogs.asp.net/lorenh/archive/2004/10/31/250361.aspx

var g_shell = new ActiveXObject("WScript.Shell");
var g_fs = new ActiveXObject("Scripting.FileSystemObject");
if (WScript.Arguments.length != 5)
{
    WScript.Echo("Usage: cscript.exe generate-install-script.js <rootFolder> <outputXMLFile> <includeRulesShortcut> <includeSSOEditorShortcut> <bizTalkAppName>");
	WScript.Quit(1);
}
var rootDir = WScript.Arguments.Item(0);
var outFile = WScript.Arguments.Item(1);
var includeRulesShortcut = WScript.Arguments.Item(2).toLowerCase();
var includeSSOEditorShortcut = WScript.Arguments.Item(3).toLowerCase();
var bizTalkAppName = WScript.Arguments.Item(4);
var baseFolder = g_fs.GetFolder(rootDir);
var componentIds = new Array();

WScript.Echo("Generating " + outFile + "...");

var f = g_fs.CreateTextFile(outFile, true);
f.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
f.WriteLine("<Include>");
f.WriteLine("  <Directory Id=\"TARGETDIR\" Name=\"SourceDir\">");

f.WriteLine(
	"<Directory Id=\"ProgramMenuFolder\" Name=\".\" SourceName=\"USERPROG\">" +
		"<Directory Id=\"BizShortCuts\" Name=\"$(var.ProductName) $(var.ProjectVersion)\">" +
    		"<Directory Id=\"BizShortCutsTools\" Name=\"Deployment Tools\" />" +
		"</Directory>" +
	"</Directory>");
	
f.WriteLine(
	"<Component Id=\"Cleanup\" Guid=\"" + GetGuid() + "\" KeyPath=\"yes\">" +
		"<RemoveFolder Directory=\"BizShortCutsTools\" On=\"uninstall\" Id=\"_6F32C15A748C4463A8B3D506F27EF282\" />" +
		"<RemoveFolder Directory=\"BizShortCuts\" On=\"uninstall\" Id=\"_6B02C15A748C4463A8B3D506F27EF282\" />" +
	"</Component>");

f.WriteLine("  <Directory Id=\"ProgramFilesFolder\" Name=\"ProgramFiles\">");
f.WriteLine("  <Directory Id=\"ProductDir\" Name=\"$(var.ProductName)\">");
f.WriteLine("  <Directory Id=\"INSTALLDIR\" Name=\"$(var.ProjectVersion)\">");

f.Write(getDirTree(rootDir, "", 1, baseFolder, componentIds));
f.WriteLine("  </Directory>");
f.WriteLine("  </Directory>");
f.WriteLine("  </Directory>");
f.WriteLine("  </Directory>");
f.WriteLine("  <Feature Id=\"DefaultFeature\" Level=\"1\">");
for (var i=0; i<componentIds.length; i++)
{
	f.WriteLine("    <ComponentRef Id=\"C__" + componentIds[i] + "\" />");
}
f.WriteLine("    <ComponentRef Id=\"Cleanup\" />");
f.WriteLine("  </Feature>");
f.WriteLine("</Include>");
f.Close();

// recursive method to extract information for a folder
function getDirTree(root, xml, indent, baseFolder, componentIds)
{
	var fdrFolder = null;
	try
	{
		fdrFolder = g_fs.GetFolder(root);
	}
	catch (e)
	{
		return;
	}

	// indent the xml
	var space = "";
	for (var i=0; i<indent; i++)
		space = space + "  ";

	if (fdrFolder != baseFolder)
	{
		var directoryId = "_" + FlatFormat(GetGuid());

		xml = xml + space + "<Directory Id=\"" + directoryId +"\"";
		xml = xml + " Name=\"" + fdrFolder.Name + "\"";
		xml = xml + ">\r\n";
	}

	var componentGuid = GetGuid();
	var componentId = FlatFormat(componentGuid);

	if (fdrFolder.Files.Count > 0)
	{
	    var addedPortBindingsRemove = false;
	    var enumFiles = new Enumerator(fdrFolder.Files);

		for (;!enumFiles.atEnd();enumFiles.moveNext())
		{
			componentGuid = GetGuid();
			componentId = FlatFormat(componentGuid);

			xml = xml + space + "  <Component Id=\"C__" + componentId + "\"" + " Guid=\"" + componentGuid + "\">\r\n";

			var file = enumFiles.item();
			
			var extension = g_fs.GetExtensionName(file.Name);
			extension = extension.toUpperCase();

			var fId = "_" + FlatFormat(GetGuid());

			var openFileTag = space + "    <File Id=\"" + fId +
							   "\" Name=\"" + file.Name +
							   "\" Source=\"$(var.redist_folder)" + file.Path.substring(baseFolder.Path.length) +
							   "\" Vital=\"yes" +
							   "\" KeyPath=\"yes" +
							   "\" DiskId=\"1\">\r\n";

								   
         // Special case for deployment related shortcuts								   
			if(file.Name == "UacElevate.exe")
			{
				xml = xml + openFileTag;

				xml = xml + space +
				"<Shortcut Id=\"BounceShortcut\" Directory=\"BizShortCutsTools\" Name=\"Bounce BizTalk\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C %windir%\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe /p:Configuration=Server /t:BounceBizTalk $(var.ProjectFilename)\"' Advertise=\"yes\" />";

				xml = xml + space +
				"<Shortcut Id=\"Deploy\" Directory=\"BizShortCuts\" Name=\"Deploy $(var.PackageDescription)\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C Framework\\ServerDeployWizard.bat $(var.ProjectFilename)\"' Advertise=\"yes\" />";

				xml = xml + space +
				"<Shortcut Id=\"ReDeploy\" Directory=\"BizShortCuts\" Name=\"Redeploy $(var.PackageDescription)\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C Framework\\ServerRedeployWizard.bat $(var.ProjectFilename)\"' Advertise=\"yes\" />";

				xml = xml + space +
				"<Shortcut Id=\"UnDeploy\" Directory=\"BizShortCuts\" Name=\"Undeploy $(var.PackageDescription)\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C Framework\\ServerUnDeployWizard.bat $(var.ProjectFilename)\"' Advertise=\"yes\" />";

				xml = xml + space +
				"<Shortcut Id=\"ImportBindingsShortcut\" Directory=\"BizShortCutsTools\" Name=\"Import Bindings\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C %windir%\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe /p:Configuration=Server /t:ImportBindings $(var.ProjectFilename)\"' Advertise=\"yes\" />";

				if (includeRulesShortcut == "true")
				{
				    xml = xml + space +
				    "<Shortcut Id=\"RedeployRulesShortcut\" Directory=\"BizShortCutsTools\" Name=\"Redeploy Rules Policies and Vocabularies\" WorkingDirectory=\"DEPLOYMENTDIR\" Arguments='CMD.EXE \"/C %windir%\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe /p:Configuration=Server;ExplicitlyDeployRulePoliciesOnDeploy=true /t:DeployVocabAndRules $(var.ProjectFilename)\"' Advertise=\"yes\" />";
				}

				xml = xml + space + "</File>\r\n";
			}
			// Special case for generated biztalk documentation files.
			else if(extension == "CHM")
			{
				xml = xml + openFileTag;
				xml = xml + space +
				"<Shortcut Id=\"" + file.Name.replace(/~/g,"_").replace(/-/g,"_").replace(/ /g,"_") + "\" Directory=\"BizShortCuts\" Name=\"" + file.Name + "\"  Advertise=\"yes\" />";
				xml = xml + space + "</File>\r\n";
			}
			// Special case for unit test assembly
			else if(file.Name == "nunit-gui.exe")
			{
				xml = xml + openFileTag;
				xml = xml + space +
				"<Shortcut Id=\"" + "nunitgui" + "\" Directory=\"BizShortCutsTools\" Name=\"Verify Deployment\" Arguments='\"[INSTALLDIR]\\$(var.DeploymentTest)\" /run' WorkingDirectory=\"INSTALLDIR\"  Advertise=\"yes\" />";
				xml = xml + space + "</File>\r\n";
			}
			// Special case for SSO editor
			else if (file.Name == "SSOSettingsEditor.exe" && includeSSOEditorShortcut == "true") {
			    xml = xml + openFileTag;
			    xml = xml + space +
				"<Shortcut Id=\"" + "SSOSettingsEditor" + "\" Directory=\"BizShortCutsTools\" Name=\"Edit SSO Settings\" Arguments='\"" + bizTalkAppName + "\"' WorkingDirectory=\"INSTALLDIR\"  Advertise=\"yes\" />";
			    xml = xml + space + "</File>\r\n";
			}
			else
			{
				xml = xml + space + "    <File Id=\"" + fId +
							   "\" Name=\"" + file.Name +
							   "\" Source=\"$(var.redist_folder)" + file.Path.substring(baseFolder.Path.length) +
							   "\" Vital=\"yes" +
							   "\" KeyPath=\"yes" +
							   "\" DiskId=\"1\"/>\r\n";

				if (fdrFolder.Name == "EnvironmentSettings") {
				    xml = xml + space + "    <RemoveFile Name=\"*.xml\" On=\"uninstall\" Id=\"_CD35C15A748F1133D8F3A716D27EF287\" />\r\n";
				}
				else if (fdrFolder.Name == "Deployment" && addedPortBindingsRemove == false) {
				    xml = xml + space + "    <RemoveFile Name=\"PortBindings.xml\" On=\"uninstall\" Id=\"_EC7203CA2E7E4a4eAAF544410878DCEA\" />\r\n";
				    addedPortBindingsRemove = true;
                }
			}

			xml = xml + space + "  </Component>\r\n";
			componentIds[componentIds.length] = componentId;
		}
	}
	else
	{
		componentGuid = GetGuid();
		componentId = FlatFormat(componentGuid);

		xml = xml + space + "  <Component Id=\"C__" + componentId + "\"" + " Guid=\"" + componentGuid + "\">\r\n";

		xml = xml + space + "    <CreateFolder />\r\n";
		
		if (fdrFolder.Name == "DeployResults")
		{
			xml = xml + space + "    <RemoveFile Name=\"*.txt\" On=\"uninstall\" Id=\"_CD35C15A748F6673A8B3F606F27EF287\" />\r\n";
		}

		xml = xml + space + "  </Component>\r\n";

		componentIds[componentIds.length] = componentId;
	}


	var enumSubFolders = new Enumerator(fdrFolder.SubFolders);

	var depth = indent + 1;
	for (;!enumSubFolders.atEnd();enumSubFolders.moveNext())
	{
		var subfolder = enumSubFolders.item();
		xml = getDirTree(enumSubFolders.item().Path, xml, depth, baseFolder, componentIds);
	}

	if (fdrFolder != baseFolder)
	{
		xml = xml + space + "</Directory>\r\n";
	}

	return xml;
}

// Generate a new GUID
function GetGuid()
{
	return new ActiveXObject("Scriptlet.Typelib").Guid.substr(1,36).toUpperCase();
}

// Convert a GUID from this format
//   7E70E5E5-CE19-4270-A740-223A09796433
// to this format:
//   7E70E5E5CE194270A740223A09796433
function FlatFormat(guid)
{
	return guid.replace(/-/g, "");
}