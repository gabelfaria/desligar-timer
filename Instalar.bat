@echo off
setlocal
set "DESTINO=%LOCALAPPDATA%\Programs\DesligarTimer"
if not exist "%DESTINO%" mkdir "%DESTINO%"
if errorlevel 1 (
  echo Nao foi possivel criar a pasta de instalacao.
  pause
  exit /b 1
)
copy /Y "%~dp0DesligarTimer.exe" "%DESTINO%\DesligarTimer.exe" >nul
if errorlevel 1 (
  echo Nao foi possivel copiar o aplicativo.
  pause
  exit /b 1
)
start "" "%DESTINO%\DesligarTimer.exe"
echo Instalado em "%DESTINO%".
echo A inicializacao automatica sera configurada na primeira execucao.
