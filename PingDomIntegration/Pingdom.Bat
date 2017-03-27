@ECHO OFF
@Echo INFO BEGIN PINGDOM %JOB_NAME% %TIME% 

SET OUPUTPATH=bin\Debug\output.md

Echo Export All Monitors
SET PINDOMPATHEXE=PingDomIntegration\bin\Debug\PingDomIntegration.exe
REM SET PINGDOMOPTIONS= --e --o %OUPUTPATH%
SET PINGDOMOPTIONS= --p month

Echo "Processing: %PINDOMPATHEXE% %PINGDOMOPTIONS%
%PINDOMPATHEXE% %PINGDOMOPTIONS%

REM ECHO Getting Monthly Uptime Report
REM "%PINDOMPATHEXE%" --p month


