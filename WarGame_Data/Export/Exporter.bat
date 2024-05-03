@echo off
cd /d %~dp0

set PYTHON_EXE=py
set SCRIPT_NAME=Exporter.py

%PYTHON_EXE% %SCRIPT_NAME%

pause
