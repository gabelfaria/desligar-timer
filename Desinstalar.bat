@echo off
setlocal
set "DESTINO=%LOCALAPPDATA%\Programs\DesligarTimer"

echo Este procedimento remove o Desligar Timer deste usuario.
choice /M "Deseja continuar"
if errorlevel 2 exit /b 0

reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v DesligarTimer /f >nul 2>&1
taskkill /IM DesligarTimer.exe /F >nul 2>&1
if exist "%DESTINO%" rmdir /s /q "%DESTINO%"

echo.
echo Desligar Timer removido com sucesso.
pause
