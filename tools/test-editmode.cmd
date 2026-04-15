@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0unity-batch.ps1" -Task editmode %*
