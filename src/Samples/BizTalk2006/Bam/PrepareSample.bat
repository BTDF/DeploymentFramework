@echo Copying sample files from BizTalk2009\BAM...
@echo -------------------------------------------------
xcopy ..\..\BizTalk2009\Bam\*.* /EXCLUDE:PrepareSampleExclude.txt /E /I /R /Y
@echo -------------------
@echo Done copying files.
@pause
