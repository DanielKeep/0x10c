@ECHO OFF

SET SSFS=..\tools\ssfs
SET KASM=..\tools\kasm
SET KDIS=..\tools\kdis
SET OUT=bin
SET RELOC=--relocatable

%KASM% bootrom.dasm -o %OUT%\boot.rom
IF ERRORLEVEL 1 GOTO STOP

%KDIS% %OUT%\boot.rom -o %OUT%\boot.lst

GOTO EOF

:STOP
PAUSE

:EOF
