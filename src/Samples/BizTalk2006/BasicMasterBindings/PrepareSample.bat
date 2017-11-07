@echo Copying sample files from BizTalk2009\BasicMasterBindings...
@echo -------------------------------------------------
xcopy ..\..\BizTalk2009\BasicMasterBindings\*.* /EXCLUDE:PrepareSampleExclude.txt /E /I /R /Y
@echo -------------------
@echo Done copying files.
@pause
