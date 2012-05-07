@ECHO OFF

SET SSFS=..\..\tools\ssfs
SET KASM=..\..\tools\kasm
SET OUT=bin

%KASM% basicloader.dasm ssfs_ro.dasm screen.dasm utility.dasm -o %OUT%\bootload.bin
