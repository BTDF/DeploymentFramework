@echo on
@echo ---------------------------------
@echo Copying DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll  to current user's ESB Toolkit 2.1 Itinerary Designer Extensions folder...
@echo ---------------------------------
@echo off
IF EXIST "%ProgramFiles(x86)%\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll". (
  copy "%ProgramFiles(x86)%\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll" "%USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Microsoft\BizTalk ESB Toolkit Itinerary Designer\2.1.0.0\Lib".
)
IF EXIST "%ProgramFiles%\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll". (
  copy "%ProgramFiles%\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll" "%USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Microsoft\BizTalk ESB Toolkit Itinerary Designer\2.1.0.0\Lib".
)
IF EXIST "D:\Program Files (x86)\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll". (
  copy "D:\Program Files (x86)\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll" "%USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Microsoft\BizTalk ESB Toolkit Itinerary Designer\2.1.0.0\Lib".
)
IF EXIST "D:\Program Files\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll". (
  copy "D:\Program Files\Microsoft BizTalk ESB Toolkit 2.1\Tools\Itinerary Designer\Lib\DeploymentFrameworkForBizTalk.Services.Extenders.Resolvers.Sso.2.1.dll" "%USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Microsoft\BizTalk ESB Toolkit Itinerary Designer\2.1.0.0\Lib".
)
@echo on
@echo ---------------------------------
@echo Please restart Visual Studio 2010 if it is currently running.
@echo ---------------------------------
@echo off
pause
