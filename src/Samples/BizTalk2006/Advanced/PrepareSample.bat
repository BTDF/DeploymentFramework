@echo Copying sample files from BizTalk2009\Advanced...
@echo -------------------------------------------------
xcopy ..\..\BizTalk2009\Advanced\*.* /EXCLUDE:PrepareSampleExclude.txt /E /I /R /Y
@echo -------------------
@echo Done copying files.
@pause
