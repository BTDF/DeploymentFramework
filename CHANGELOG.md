### 5.8.95 [Release Candidate 1] (07/?/2020)

- Add support for BizTalk 2020
- Remove support for BizTalk 2010 and 2013 (R1)
- Move BTDF ESB Resolver into a separate, dedicated MSI
- Upgrade to XmlPreprocess v3.0
- Upgrade to Environment Settings Manager v1.7 (eliminates .NET 2.x/3.x dependency)
- Upgrade to BizTalk BAM Definition XML Exporter v2.3 (eliminates .NET 2.x/3.x dependency)
- Minor updates to documentation

### 5.7.100 [Release] (08/13/2017)
* Visual Studio Addin (#11160)
    * New icons; more commands on toolbar; enable/disable commands when appropriate; add icons in VS 2015
    * **Remove Visual Studio Addin from MSI; move to Visual Studio Marketplace**
* Remove PDF version of docs from MSI; make available as separate download
* Remove MigrateToProjectFolderStructure.cmd

### 5.7.96 [Release Candidate 2] (03/20/2017)
* Rewrite Visual Studio Addin to target VS 2010 and up (#11160)
* **Known Issue - toolbar buttons display as text in VS 2015**
* Rewrite IIS deployment functionality, as in v6.0 Beta, maintaining backward compatibility; add IIS sample app (#11164, #10937)
* Port BTDF v6.0 custom MSBuild tasks to v5.x line (#11161)
* Remove remaining dependencies on SDC.Tasks (#11163)

### 5.7.95 [Release Candidate 1] (02/05/2017)
* Add support for BizTalk 2016 **not yet including the Visual Studio addin** (#11152)
* Fix: On a clean machine, SSO deploys before required DLL is deployed (#11074)
* Remove support for BizTalk 2006 and 2009; raise minimum .NET version to v4.0 (#11155)
* Internal: Simplify build process for ESB Toolkit extensions; automate more of the overall build process (#11157)

### 5.6.101 [Release] (3/12/2016)
* Allow full control of product name in MSI (#11004)
* Fix the order of operations when DeployBizTalkMgmtDB is false (#11016)

### 5.6.100 [Release Candidate] (08/31/2015)
* Add support for BizTalk 2013 R2 (#10975)
* Provide enhanced control over AppDomains within BizTalk (#10986)
* Change default location of settings spreadsheet to main project folder (#10976)
* Merge default install path into MSI vs. generated batch file (#10980)
* Optimize XML in SettingsFileGenerator.xml (#10979)
* Make host instance restart output less verbose (#10982)
* Undeploy BRE policy/vocab files in reverse order (#10983)
* Change DeployPDBsToGac default to False to avoid host instance restart (#10978)
* Add WCF-WebHttp XPaths to AdapterXPaths.txt (#10984)
* Fix error during restart of clustered host instances (#10120)
* Add retry logic to TerminateServiceInstances (#10985)
* Rename MSBuild property FrameworkDir to avoid conflict with VS Command Prompt env vars (#10991)
* Reduce dependency on deprecated SDC MSBuild Tasks (#10992)
* Fix issue with WixProjectFile with non-default filename (#10994)
* Make PackageComments property optional (#10995)
* Start app after restarting host instances; deploy SSO much earlier (#10996)

### 6.0.100 [Beta 1] (09/10/2014)
* Modernize code for MSBuild 4.0; enable support for 64-bit MSBuild (#10544, #10545)
* Allow use of properties from settings spreadsheet in any PropertyGroup or ItemGroup, eliminating MSBuild order-of-evaluation problems (#10541)
* Start app after restarting host instances; enable granular control over app start/stop (#10565; #10562)
* Add a Visual Studio menu item to decode port bindings file (ElementTunnel decode) (#9331)
* Break MSI Redist target into multiple targets
* Install BizTalkDeploymentFramework.ServerExecute.targets to same location as other targets (#10585)

**The following changes are also included in v5.7:**

* Remove support for BizTalk 2006-2009 (#10535)
* Rewrite IIS deployment functionality (#10546)
* Add IIS sample application (#10546)
* Remove dependency on SDC.Tasks DLL (#10559)

**The following changes are also included in v5.6:**

* Add support for BizTalk 2013 R2 (#10635)
* Fix error during restart of clustered host instances (#10120)
* Add retry logic to TerminateServiceInstances (#10597)
* Make host instance restart output less verbose (#10574)
* Undeploy BRE policy/vocab files in reverse order (#10717)
* Add WCF-WebHttp XPaths to AdapterXPaths.txt (#10698)
* Fix issue with WixProjectFile with non-default filename (#10572)
* Change DeployPDBsToGac default to False to avoid host instance restart (#10577)
* Merge default install path into MSI vs. generated batch file (#10573)
* Fail MSI build if DefaultInstallDir value ends in backslash (#10573)
* Optimize XML in SettingsFileGenerator.xml (#10571)
* Change default location of settings spreadsheet to main project folder (#10564)
* Provide enhanced control over AppDomains within BizTalk (#10542)
* Make PackageComments property optional; remove ProductVersion from MSI product name (#10578)

### 5.5.100 [Release] (02/26/2014)
* Refactor SSO code, add retry logic, add unit tests, fix random credential error (#10520)
* Validate bindings XML syntax on deploy and MSI build (#6068)
* Fix issue with TerminateServiceInstances and routing failure reports (#10519)
* Update to WiX v3.8 (#10531)
* Update to XmlPreprocess 2.0.18 (#10515)
* Update to NUnit 2.6.3 (#10517)
* Get Quick Deploy Start menu command to pause at end of process (#10516)
* Get MSBuild props from settings spreadsheet to act like typical MSBuild properties when values incl. semicolon (#10266)
* Fix issue that could cause a deployment process to incorrectly indicate failure (#10406)
* Add send port EndpointBehaviorConfiguration to ElementTunnel AdapterXPaths (#10387)
* Add property ConfigureBizTalkDebuggingFeatures to enable/disable BTSNTSvc.exe.config updates for BizTalk debug options (#10226)
* Add PublishWcfServiceArtifacts MSBuild task to automate the WCF Service Publishing Wizard (#9634)
* Don't indicate expected behavior as MSBuild warnings during BRE deployment (#10227)
* Restore original behavior in SSOSettingsFileReader.Read() and add new overload to enable SSO access from remote machine (#9551; #10137)
* Convert Bounce and StopBizTalkHost VBScripts to MSBuild task; handle clustered host instances  (#10120; #10212)
* Add /f option to GacUtil to force overwrite of existing assemblies in GAC (#10146)

### 5.1.2 [Stable Beta] (05/10/2013)
* Add AutoTerminateInstances property to auto-terminate instances on deploy, undeploy, quick deploy (#10019)
* Enhance and extend WiX MSI generation; update to WiX 3.7, replace JavaScript WXS generator with WiX Heat, more (#8159, 9688, 10029)
* Update server deployment process to 100% MSBuild without batch files (#10036)
* Add BTS 2013 RTM ESB Toolkit support for BTDF SSO resolver and designer extension; update ESB Toolkit install dir reg key (#9921)
* Fix SSO tooling issue due to DLL change in BizTalk 2013 RTM (#9921)
* Update SSOSettingsFileReader to support use from non-BizTalk machine (#9551)
* Add support for IIS 8 and pre-configured AppPools; drop legacy VDirList.txt; refactor DeployVDirs target (#10067, 10082)
* Stop script execution early on undeploy when BizTalk app does not exist (#10020)
* Stop host instances as late as possible when DeployPdbsToGac is enabled (#10021)
* Remove IncludeCompsAndVDirsAsResources feature (#10022)
* Add Start menu shortcuts for Quick Deploy, Preprocess and Import Bindings, Terminate Service Instances; remember selected settings file from deploy for use in undeploy (#3858, 7635, 10095)
* Add option to disable all Start menu shortcuts on server install (#10096)
* Split custom MSBuild tasks DLL to isolate dependencies on BizTalk DLLs (#10032)
* Add MSBuild task to pause for a keypress (#10033)
* Update SetEnvUIConfig.xsd with missing radio button elements; update and enhance SetEnvUI documentation (#10025, 10026)
* Add workaround for ESB Toolkit 2.1 Itinerary Designer extension install when ESB Toolkit installed on D: (#9838)
* Update server deploy MSI to create InstallPath registry key (#7178)
* Docs: remove unnecessary step in scripted deployment; add OutputFilename element for FilesToXmlPreprocess; update software requirements; add documentation on WiX MSI customization (#10025)
* Minor updates to Advanced and BasicMasterBindings sample apps (#10102)

### 5.1.1 [Stable Beta] (04/08/2013)
* Add support for BizTalk 2013 Beta (#9921)
* Allow the Framework to be referenced and used without installing MSI, such as on a build server (#9961)
* Allow suppression of WiX MSI validation (#9962)
* ElementTunnel enhancements incl. whitespace preservation; fix for occasional data loss during decode; set ApplyXmlEscape true for new projects (#10011, 10012)

### 5.0.100 [Final Release] (07/26/2012)
* Completely new, comprehensive documentation in CHM and PDF format (110+ pages)
* Change default project configuration and internal properties to directly support Team Build (#8847)
* Add MsiName MSBuild property to allow override of generated MSI filename (#7672)
* Fix parsing of BAM view names to allow spaces and periods within names (#8119)
* Fix error when multiple BamDefinition elements are specified (#8252)
* Prevent deploy from failing if a PDB file is missing and DeployPDBsToGac is true (#8539)
* Add guard condition to ensure 32-bit MSBuild.exe since 64-bit MSBuild.exe is unsupported (#8185)
* Fix issue with settings exporter when spreadsheet XML contained Index attributes on cells that held no data (#8385)
* Prevent deploy from failing if a PDB file is missing and DeployPDBsToGac is true (#8539)
* Fail the MSI build if the OutputPath property is undefined (#8748)
* Added overridable target named CustomPostInstaller that runs after an MSI is built (#9203)
* Add new overridable targets CustomFinalDeploy and CustomFinalUndeploy at very end of deploy/undeploy process (#9392)
* Add property XmlEscapeXPathsFile to enable override of default AdapterXPaths.txt file path (#9369)
* Implement retry logic in stop host instances script (#8318)
* Allow BAM view names to contain periods in settings spreadsheet (#8688)
* Add new BAM sample app and simplify Advanced sample (#8580)
* Add /c switch to XmlPreprocess for Log4net & FilesToXmlPreprocess to strip preproc comments from output file (#8639)
* Added optional element AppPoolNetVersion to VDirList to configure .NET version on AppPool; IIS7+ only (#7628)
* Add log4net registry key to both 32 and 64-bit registry views; remove cscript64.exe from DeployTools (#4788)
* Always use BTDF copy of gacutil.exe for Visual Studio GAC Output command (#8114)
* Change ElementTunnel.exe to decode only XML special chars (#8856)
* Integrate ExportBamDefinitionXml.exe V2.2 to fix XML truncation with large BAM models (#7969)
* Skip NTFS permissions when setting up FILE adapter paths on network/UNC (#8452)
* Add slightly modified version of Team Build 2010 DefaultTemplate.xaml for BTDF solutions (#8847)
* Set ToolsVersion="4.0" on Project element in Add Project Wizard when BizTalk 2010 (#9399)
* Run DeployBTRules.exe with .NET 4.0 only when BizTalk 2010 (#9460)
* Enable VS add-in to locate a project file at Deployment\<solutionNameNoExtension>.Deployment.btdfproj (#9491)
* Improve new project setup experience by adding a default PortBindingsMaster.xml (#9492)
* Default new projects to simple XMLPreprocess syntax that doesn't require ifdef block (#9501)
* In BTDF installer, display destination folder on Custom page and allow it to be changed (#6066)
* In BTDF installer, add status messages during VS add-in install/uninstall (#9196)

### 5.0.26 [Release Candidate 2] (03/10/2011)
* Fix for rules deployment failure on BT2010 when an FX4.0 DLL is referenced by a policy (#7588)
* Fix for error dialog after Add New Project wizard when project path contains spaces (#7587)
* Add quotes around paths in light.exe command line to prevent issues with spaces in paths (#7840)
* Add additional extensibility points via more overridable MSBuild targets (#7854)
* Add BizTalkAppDescription property to specify app description displayed in BizTalk Admin (#7555)
* Fix to use SettingsSpreadsheetPath property when building MSI; fix server deploy when a custom path is present (#7956)
* Add ModifyNTFSPermissionsOnVDirPaths property to allow bypass of NTFS permission changes to IIS vdir physical folders (#7994)
* Modify FilesToXmlPreprocess to support an output filename instead of modifying the source file (#7663)
* Upgrade WiX to 3.5 RTM; used to build server MSI's (#8004)

### 5.0.25 [Release Candidate] (11/16/2010)
* Full support for BizTalk 2010 RTM and ESB Toolkit 2.1 (#7507)
* Improve default configuration of template project and auto open generated BTDFPROJ for editing (Add New Project wizard) (#6930; #6919)
* Integrate Environment Settings Manager exporter V1.6.0. Fixes issue with incorrect exported data after copying and pasting data in the XML Excel spreadsheet. (#6959)
* Integrate XmlPreprocess V2.0.13 (#6976)
* Improve Visual Studio add-in registration to remember if BTDF toolbar is hidden or visible and maintain keyboard shortcut mappings after VS restart (#7422)
* Enable automatic version upgrade of server deploy MSI's (after undeploy is complete; avoids need to uninstall old version before installing new version) (#7099)
* Add RequireXmlPreprocessDirectives property to allow global macro replacement in XML files by XMLPreprocess without #ifdef directives (#7028)
* Add SkipHostInstancesRestart property to allow bypass of host instances restart (#7035)
* Add property SettingsSpreadsheetPath to allow override of path to settings spreadsheet (#7348)
* Include missing SSOSettingsEditor.exe in BTDF installer so that the Edit SSO Settings Start menu shortcut appears (#7008)
* Fixed bug where SSOSettingsEditor throws an exception when a setting value is empty/null (#7010)
* Fix to deploy BTSNTSvc.exe.config changes on all servers, not just the last server in the group (#7009)
* Fix for issue when side-by-side is true and UseMasterBindings is false and port names are prepended again on each redeploy (#7077)
* Fix for error while applying NTFS permissions during vdir deploy when physical directory contains spaces (#7461)
* BizTalk 2010 fix: always use GacUtil 4.0 w/ 2010 and fix detection of GAC path under CLR 4.0 (#7246)
* Fix for Visual Studio GAC Output of Selected Project command when path contains spaces (#7335)
* During BTDF install, add prompt to close Visual Studio (#7365)
* Allow spaces in the BizTalk application name (#7175)
* Upgrade BTDF installer to WiX 3.5 RC (#7542)
* Add sample solution that demonstrates use of BTDF SSO Resolver for ESB Toolkit (#7507)
* Add source code for previously customized SDCTasks AppPool Recycle task (#7107)
* Documentation updates

### 5.0.19 (06/30/2010)
* Fix BTDF installer issue that always (incorrectly) detects BTS 2010 and always installs BTS 2010-specific files (#6923)
* Fix for Gac Output of Selected Project menu item in BTS 2010 (#6916)

### 5.0.18 (06/28/2010)
* Add full support for BizTalk Server 2010 Beta 1/Visual Studio 2010 (#6796)
* Enable Add New Project template in Visual Studio 2005 - tested with 2006 R2 (#6572)
* Relax Visual Studio add-in's restrictions on .btdfproj file and directory names (#6885)
* Create a GUI for editing live runtime settings stored in an SSO affiliate app and a Start menu shortcut on server deploy (#6895)
* Add x64 support for isolated app domain config in BTSNTSvc64.exe.config (#5660)
* Enable auto-configuration of BizTalk debugging options in the BTSNTSvc.exe.config (#6902)
* Added IISMetabasePath property that allows deployment to a web site other than Default Web Site (#4015)
* Converted old VBScripts for IIS configuration into MSBuild tasks using WMI, and tested IIS configuration with IIS 7 (#5869)
* Added UndeployIISArtifacts property that enables undeploy of IIS artifacts on dev machine. Default is true only for server deploy. (#5871)
* Fix resizing issue with Add New Project options dialog (#6861)
* Fix to skip auto config of FILE adapter paths when IncludeMessagingBindings is false (#6884)
* Fix for MSI creation failure when IncludeMessagingBindings is false and no PortBindings.xml exists (#6886)
* Eliminate legacy *.deploy.include XmlPreprocess code in ServerDeploy.bat and ServerReDeploy.bat (#6888)

## 5.0.17 (05/21/2010)
* Create Add New Project wizard in Visual Studio to add a Deployment Framework project to a solution (#6572)
* Fix UAC issue on Win2008/7 where the "deploy now" checkbox at the end of the server install wizard doesn't do anything (#6681)
* Modify server deploy to use MSBuild 3.5 if it is present, otherwise 2.0 (#6702)
* Fix incorrect references to old target name DeployAndStartPorts in Start menu shortcut and Tools menu (#6706)
* Add GetRegistryValue to BuildTasks and switch registry reads from SDC task to the new task (#6316)
* Add optional property to bypass starting referenced applications during deploy (#6430)
* Retain ability to auto-undeploy BAM model even if the definition file has changed (#6457)
* Fix issue where test for existing service instances was skipped (#6500)
* Update to ExportBamDefinitionXml 2.1 to fix issue due to missing OLE DB provider on x64 (#6552)
* Updates to documentation

### 5.0.16 (04/14/2010)
* Fix NTFS permission assignment on FILE adapter path setup so assigned perms exactly match the Windows Security dialog box (#6255)
* Rename DeployAndStartPorts target to ImportBindings (#6300)
* Modify behavior of EnableXmlPreprocess property to not affect anything besides calls to XmlPreprocess.exe (#6301)
* Change DeploySSO target so that it also exports from settings spreadsheet on developer machines (#6302)
* Add a SetRegistryValue MSBuild task to DeploymentFramework.BuildTasks.dll (#6303)
* On undeploy, check for existing service instances and stop the BizTalk app earlier in the process (#6308)
* Reverse order of undeploy operations - stop BizTalk app first, then check for existing service instances (#6308)
* Add optional boolean property EnableAllReceiveLocationsOnDeploy to support BizTalk app start without enabling receive locations (#5836)
* Fix for issue where an unattended server install results in broken Start menu shortcuts; fix submitted by giuliov (#6278)
* Files in ExternalAssemblies and AdditionalAssemblies item groups will now be deployed even when IncludeComponents is false (#6312)
* Add more properties to IntelliSense
* Add ESB Toolkit 2.0 Integration to documentation and a couple other minor doc updates

### 5.0.15 (03/25/2010)
* Fix incorrect path to BAM model on server deploy (#6238)
* Fix BizTalk host restart when a host is disabled (#6173)
* Change default mode for BAM undeploy to undeploy on developer machines and skip undeploy on servers -- previous mode was to always skip (#6239) 

### 5.0.14 (03/24/2010)
* Enhanced ESB Toolkit integration: custom Resolver component that can pull values from SSO at runtime based on data from the SettingsFileGenerator.xml spreadsheet (#6235)
* Add support for BAM tracking profiles (#6236)
* Update IntelliSense definition file to include additional ItemGroups
* Fix issue with BAM file paths during BAM undeploy

### 5.0.13 (03/19/2010)
* BRE fix for two issues related to server MSI build and deploy/undeploy if vocabs are present without policies or vice versa (#6202, 6205)
* Add optional <IisAppPools> item group to specify AppPools to restart vs IISReset
* Fix for IntelliSense not always working due to MSBuild schemas not loading (#6211)
* Add informational messages during components deployment
* Undeploy rules and vocabs before deploying rules and vocabs (consistent with behavior of other artifacts)

### 5.0.12 (03/15/2010)
* POTENTIAL BREAKING CHANGE: See [issue #6134](http://biztalkdeployment.codeplex.com/WorkItem/View.aspx?WorkItemId=6134).  Comma-separated file lists in PropertyGroups have been converted to ItemGroups; the UseCustomDirs option has been removed (#6134)
* Complete overhaul of support for BRE (see [issue #6084](http://biztalkdeployment.codeplex.com/WorkItem/View.aspx?WorkItemId=6084))
* Fix issue with ElementTunnel writing XML file as UTF-16 without byte order mark (#6112)
* On the last page of the server MSI installer, add a checkbox to immediately launch the deployment script and a warning note that deployment is not complete
* Automatically check the "accept license" checkbox in the server MSI installer (#6085) 
* Fix the Verify Deployment Start menu item so that it automatically launches the test assembly in NUnit (#6116)
* Add an option to automatically include the settings spreadsheet in the server MSI
* Add a new optional ItemGroup that can hold additional files that should be automatically packaged into the server MSI
* Add initial support for deployment of ESB Toolkit 2.0 itineraries (#6167)
* Add support for IntelliSense while editing .BTDFPROJ files in the Visual Studio XML editor (#6170)
* Move less-important Start menu items on server install into a Deployment Tools sub-menu
* Reorganize items in Visual Studio add-in menu to group similar commands together and add separators

### 5.0.11
* Documentation updates
* Add new BasicMasterBindings sample; 5756
* Integrate latest version of XmlPreprocess.exe (adds features and fixes handling of explicit value 'false in settings spreadsheet); 4933
* Define <DeveloperPreProcessSettings> property by default; 5757
* Upgrade all Tools project/sln files to VS 2008 (keep target at .NET 2.0) and update binaries in DeployTools; 6007
* Fix: use ProductUpgradeCode value passed from the .btdfproj in the Windows Installer Upgrade table vs. hardcoded GUID; 6008
* Fix: Add binding file to BizTalk app resources using name <BizTalkAppName>.PortBindings.xml to keep filename unique within the BizTalk group; 4771
* Integrate EnvironmentSettingsManager Exporter V1.5.1 (bug fixes and new functionality not used by the Framework)
* Integrate ExportBamDefinitionXml V2.0 (no longer uses Excel Automation so Excel need not be installed; for XLSX need Office 2007 Data Connectivity Components)

### 5.0.10
* Update and reformat documentation
* Bug fix for BounceBizTalk target
* Add two new server deploy Start menu shortcuts to bounce BizTalk and to import the project's bindings file
* Eliminate duplication of files between BT2006 and BT2009 samples
* Generate a batch file next to the MSI that can be used to set command-line properties for MSIEXEC.exe
* Make the DefaultInstallDir property in the btdfproj optional
* Fix issue where changing the destination folder at install time caused some files to be installed to default folder and others to specified folder
* Move MSI properties to their own PropertyGroup, 5685
* Modify MSI generator and add UacElevate.exe in order to support UAC elevation on Vista and Server 2008, 5694
* Update xDeployWizard.bat files to skip settings export if SettingsFileGenerator.xml does not exist
* Fix: include PortBindings.xml in MSI when UsingMasterBindings is set to false
* Drive IIS configuration steps by IIS version instead of OS version
* Set Is64bitOS property value with GetOsVersion task
* Enhance GetOsVersion task to return Is64BitOperatingSystem and IisMajorVersion
* Add property to enable/disable XmlPreprocess
* Create new folder structure for samples
* Add new HelloWorld sample
* Add support for auto-configuration of FILE adapter physical paths and permissions, remove ApplyFilePerms target from samples
* By default, disable less-common deployment types (deploy test, custom pipeline, rules, etc.) unless explicitly enabled in .btdfproj
* Add an XPath to the binding file encoding list to support send port ReceivePipelineData, 4187
* Add an XPath to the binding file encoding list to support receive location SendPipelineData
* Add an XPath to the binding file encoding list to support SAP Adapter 3.0 receive
* In the Advanced sample, remove extra quote from end of DeveloperPreProcessSettings file path and add comments

### 5.0.1 - 5.0.9
* **NEW: Support for BizTalk Server 2009**
    * Now includes support for BizTalk Server 2006, 2006 R2 and 2009
* **NEW: Complete conversion from NAnt to MSBuild**
    * MSBuild project file structure is modeled after standard .csproj/.vbproj project files
    * Property settings can now be different per configuration (Debug/Release/Server)
    * Establishes the foundation for a +future+ Visual Studio add-in (project file, properties pages, etc.)
* **NEW: Completely reorganized folder structure**
    * Consolidated deployment files under a <projectname>.Deployment folder, leaving the solution root clean
    * Integrated server deployment MSI generation into the main project file; no more <projectname>.WiXSetup folder
    * Solution deployment project folder includes only a handful of user-editable files
* **NEW: Visual Studio 2005/2008 Add-in**
    * Menu items moved to new 'Deployment Framework for BizTalk' menu under Tools menu
    * Deployment Framework commands are available to Visual Studio for toolbars, keyboard accelerators, etc.
    * Commands are aware of the current solution configuration
    * Selected commands are added to a default toolbar
    * Added menu items for exporting environment settings, preprocessing bindings and importing bindings
    * Added menu item/toolbar command to build MSI for server deployment
    * Added menu item/toolbar command to terminate all service instances for the current BizTalk application
* **NEW: Windows Installer MSI for developer workstations**
    * Install options include core files, developer tools, Visual Studio 2005/2008 integration and tools source code
    * Can automatically install and configure the Visual Studio 2005/2008 add-in
    * Includes repair and change options
* **NEW: Core Framework enhancements**
    * Automated export and deployment of BAM XML from a BAM XLS file to avoid needing Excel on the server
    * Property 'FilesToXmlPreprocess' can hold a list of files to be run through XmlPreprocess.exe
    * Property 'DisableAutomaticPortNameVersioning' to disable side-by-side related changes to port names
    * Properties to hold a list of referenced assemblies that will be auto-GAC'd and un-GAC'd during deploy/undeploy
    * Eliminated VDirList.txt (by default, still available for backward compatibility) by merging IIS config data into the main project file
    * Added many new binding XML file XPath's for automatic encoding of nested XML when ApplyXmlEscape is enabled
    * Support for Windows Vista and Windows Server 2008 and 2008 R2 (including UAC elevation)
    * Added a simple BAM definition XLS to the Advanced sample
* **Other changes and improvements**
    * Fixed a side-by-side issue when a binding file contains a port name defined in a different BizTalk application
    * Modified BizTalk application stop script so that the states of referenced apps are not affected
    * PortBindingsMaster.xml and PortBindings.xml no longer have the project name prepended
    * **_Discontinued support for BizTalk 2004_** (BizTalk 2004 users, please use Version 4.0)
