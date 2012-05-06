@ECHO OFF

SET SSFS=..\tools\ssfs
SET KASM=..\tools\kasm --little-8-bit
SET KDIS=..\tools\kdis --little-8-bit
SET OUT=bin
SET RELOC=--relocatable

%KASM% bootrom.dasm -o %OUT%\boot.rom
IF ERRORLEVEL 1 GOTO STOP

%KDIS% %OUT%\boot.rom -o %OUT%\boot.lst

GOTO EOF

:STOP
PAUSE

:EOF
