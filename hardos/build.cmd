@ECHO OFF

SET SSFS=..\tools\ssfs
SET KASM=..\tools\kasm
SET OUT=bin\root
SET RELOC=--relocatable
SET DISK=bin\hardos01.disk

REM Make sure output directory exists.
IF NOT EXIST %OUT% MKDIR %OUT%

REM Compile kernel.
SET INFILES=kernel\kernel.dasm
FOR %%F IN (kernel\*.dasm) DO (
    IF "%%~nF"=="kernel" (
        REM Do nothing.
    ) ELSE (
        SET INFILES=%INFILES% %%F
    )
)

%KASM% %INFILES% -o %OUT%\kernel.sys
IF ERRORLEVEL 1 GOTO STOP

REM Compile drivers.
FOR %%F IN (drivers\*.dasm) DO (
    %KASM% %%F %RELOC% -o %OUT%\%%~nF.drv
    IF ERRORLEVEL 1 GOTO STOP
)

REM Compile programs.
FOR %%F IN (programs\*.dasm) DO (
    %KASM% %%F %RELOC% -o %OUT%\%%~nF.sro
    IF ERRORLEVEL 1 GOTO STOP
)

REM Copy other files.
COPY root\* bin\root\

REM Create disk image.
%SSFS% create -r -bboot\bin\bootload.bin -ppriority.txt %OUT% %DISK%
IF ERRORLEVEL 1 GOTO STOP

REM Output file list.
%SSFS% list %DISK%

GOTO EOF

:STOP
PAUSE

:EOF
