@echo off
IF EXIST EnvironmentSettings\SettingsFileGenerator.xml (
Framework\DeployTools\EnvironmentSettingsExporter.exe EnvironmentSettings\SettingsFileGenerator.xml EnvironmentSettings )
Framework\DeployTools\SetEnvUI.exe UnInstallWizard.xml Framework\ServerUnDeploy.bat %1