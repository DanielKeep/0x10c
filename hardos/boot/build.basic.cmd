@ECHO OFF

SET SSFS=..\..\tools\ssfs
SET KASM=..\..\tools\kasm
SET KDIS=..\..\tools\kdis
SET OUT=bin

%KASM% basicloader.dasm screen.dasm utility.dasm ssfs_ro.dasm -o %OUT%\bootload.bin
%KDIS% %OUT%\bootload.bin -o %OUT%\bootload.lst
