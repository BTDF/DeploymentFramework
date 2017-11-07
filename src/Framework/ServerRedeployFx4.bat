@echo off
SET BTDFMSBuildPath="%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

%BTDFMSBuildPath% /p:DeployBizTalkMgmtDB=%BT_DEPLOY_MGMT_DB%;Configuration=Server /target:Deploy /l:FileLogger,Microsoft.Build.Engine;logfile=..\DeployResults\DeployResults.txt %1
@echo on
@echo -----
@echo off
%BTDFMSBuildPath% Framework\CopyDeployResults.msbuild /nologo
@pause
