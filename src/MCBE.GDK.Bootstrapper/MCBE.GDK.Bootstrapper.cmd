@echo off
cd "%~dp0"

rd /q /s "bin"
md "bin"

rd /q /s "obj"
md "obj"

windres.exe -i "Resources\Application.rc" -o "obj\Application.o"
gcc.exe -mwindows -Oz -nostdlib -s "Program.c" "obj\Application.o" -lKernel32 -lUser32 -o "bin\GameLaunchHelper.exe"