@ECHO OFF

SET SSFS=..\..\tools\ssfs
SET KASM=..\..\tools\kasm
SET HEADERS=loader.i
SET OUT=bin

%KASM% %HEADERS% header.dasm -o %OUT%\header.bin
%KASM% %HEADERS% ..\syscall.i loader.dasm -o %OUT%\loader.o
%KASM% %HEADERS% hmd2043.dasm -o %OUT%\hmd2043.o
%KASM% %HEADERS% ssfs.dasm -o %OUT%\ssfs.o

COPY /b %OUT%\hmd2043.o + %OUT%\loader.o + %OUT%\ssfs.o %OUT%\bootload.bin
