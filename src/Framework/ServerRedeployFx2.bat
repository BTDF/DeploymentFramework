@echo off

IF EXIST "%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe" (
SET BTDFMSBuildPath="%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe"
) ELSE IF EXIST "%windir%\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe" (
SET BTDFMSBuildPath="%windir%\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe"
)

%BTDFMSBuildPath% /p:DeployBizTalkMgmtDB=%BT_DEPLOY_MGMT_DB%;Configuration=Server /target:Deploy /l:FileLogger,Microsoft.Build.Engine;logfile=..\DeployResults\DeployResults.txt %1
@echo on
@echo -----
@echo off
%BTDFMSBuildPath% Framework\CopyDeployResults.msbuild /nologo
@pause
