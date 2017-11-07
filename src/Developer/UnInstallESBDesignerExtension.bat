@echo on
@echo ---------------------------------
@echo Deleting DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll from current user's ESB Toolkit 2.1 Itinerary Designer Extensions folder...
@echo Please close Visual Studio 2010 if it is currently running.
@echo ---------------------------------
@echo off
del "%USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Microsoft\BizTalk ESB Toolkit Itinerary Designer\2.1.0.0\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll"
pause
