@ECHO OFF

SET SSFS=..\..\tools\ssfs
SET KASM=..\..\tools\kasm
SET OUT=bin

%KASM% hmd2043.dasm -o %OUT%\hmd2043.o
%KASM% loader.dasm -o %OUT%\loader.o
%KASM% ssfs.dasm -o %OUT%\ssfs.o

COPY /b %OUT%\hmd2043.o + %OUT%\loader.o + %OUT%\ssfs.o %OUT%\bootload.bin
