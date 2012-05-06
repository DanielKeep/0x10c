@ECHO OFF

SET SSFS=..\tools\ssfs
SET KASM=..\tools\kasm --little-8-bit
SET KDIS=..\tools\kdis --little-8-bit
SET OUT=bin
SET RELOC=--relocatable

%KASM% hello.dasm -o %OUT%\hello.o
IF ERRORLEVEL 1 GOTO STOP

COPY /B %OUT%\hello.o + padding.bin %OUT%\hello.disk

%KDIS% %OUT%\hello.o -o %OUT%\hello.lst

GOTO EOF

:STOP
PAUSE

:EOF
