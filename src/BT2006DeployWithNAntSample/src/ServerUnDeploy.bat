DeployTools\EnvironmentSettingsExporter.exe EnvironmentSettings\SettingsFileGenerator.xml EnvironmentSettings
DeployTools\NAntSubset\nant.exe -D:deployBizTalkMgmtDB=%BT_DEPLOY_MGMT_DB% -l:DeployResults\DeployResults.txt -logger:"NAnt.ColorConsoleLogger.ColorConsoleLogger,NAnt.ColorConsoleLogger" serverUndeploy
DeployTools\NAntSubset\nant.exe -buildfile:CopyDeployResults.nant >nul
@pause
