@echo Copying sample files from BizTalk2009\HelloWorld...
@echo -------------------------------------------------
xcopy ..\..\BizTalk2009\HelloWorld\*.* /EXCLUDE:PrepareSampleExclude.txt /E /I /R /Y
@echo -------------------
@echo Done copying files.
@pause
