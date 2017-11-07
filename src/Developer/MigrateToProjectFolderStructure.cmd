@echo Usage - MigrateTo50FolderStructure [SolutionRootName]
@echo BizTalk Solution Root Name: %1

@echo This batch file must be executed from a command-prompt with the current directory as the BizTalk solution root.
@echo DO NOT CONTINUE OTHERWISE.
@pause

@md %1.Deployment
@attrib -r ServerDeploy.bat
@del ServerDeploy.bat
@attrib -r ServerDeployWizard.bat
@del ServerDeployWizard.bat
@attrib -r ServerRedeploy.bat
@del ServerRedeploy.bat
@attrib -r ServerRedeployWizard.bat
@del ServerRedeployWizard.bat
@attrib -r ServerUnDeploy.bat
@del ServerUnDeploy.bat
@attrib -r ServerUnDeployWizard.bat
@del ServerUnDeployWizard.bat
@attrib -r BizTalkDeploymentInclude.nant
@del BizTalkDeploymentInclude.nant
@attrib -r CopyDeployResults.nant
@del CopyDeployResults.nant
@rd /s /q DeployTools

@move %1.PortBindings.xml %1.Deployment\PortBindings.xml
@move %1.PortBindingsMaster.xml %1.Deployment\PortBindingsMaster.xml
@move InstallWizard.xml %1.Deployment\InstallWizard.xml
@move UnInstallWizard.xml %1.Deployment\UnInstallWizard.xml
@move %1.sln.deploy.build %1.Deployment\%1.Deployment.btdfproj.ToBeMigrated
@move %1.VDirList.txt %1.Deployment\VDirList.txt

@md %1.Deployment\EnvironmentSettings
@move EnvironmentSettings\SettingsFileGenerator.xml %1.Deployment\EnvironmentSettings\SettingsFileGenerator.xml
@rd /s /q EnvironmentSettings

@move %1.WiXSetup\Setup\License.rtf %1.Deployment\License.rtf
@move %1.WiXSetup\Setup\%1.WiXSetup.build %1.Deployment\%1.WiXSetup.ToBeMigrated
@rd /s /q %1.WiXSetup

@echo Done
