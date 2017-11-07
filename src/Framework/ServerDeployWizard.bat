@echo off
IF EXIST EnvironmentSettings\SettingsFileGenerator.xml (
Framework\DeployTools\EnvironmentSettingsExporter.exe EnvironmentSettings\SettingsFileGenerator.xml EnvironmentSettings )
Framework\DeployTools\SetEnvUI.exe InstallWizard.xml Framework\ServerDeploy.bat %1
