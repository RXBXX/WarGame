@echo off
cd /d %~dp0

set PYTHON_EXE=py
set SCRIPT_NAME=Translate.py

%PYTHON_EXE% %SCRIPT_NAME%

pause
