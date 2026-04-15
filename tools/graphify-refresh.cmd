@echo off
setlocal

where py >nul 2>&1
if %errorlevel%==0 (
  set PY=py -3
) else (
  set PY=python
)

%PY% "%~dp0graphify-refresh.py" %*
exit /b %errorlevel%
