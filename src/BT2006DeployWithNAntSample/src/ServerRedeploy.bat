DeployTools\EnvironmentSettingsExporter.exe EnvironmentSettings\SettingsFileGenerator.xml EnvironmentSettings
@REM For loop is just to keep this generic.
@for %%i in (*.deploy.include) DO DeployTools\XmlPreprocess.exe /o:%%i /i:%%i /s:"%ENV_SETTINGS%"
DeployTools\NAntSubset\nant.exe -D:deployBizTalkMgmtDB=%BT_DEPLOY_MGMT_DB% -l:DeployResults\DeployResults.txt -logger:"NAnt.ColorConsoleLogger.ColorConsoleLogger,NAnt.ColorConsoleLogger" serverDeploy
DeployTools\NAntSubset\nant.exe -buildfile:CopyDeployResults.nant >nul
@pause
