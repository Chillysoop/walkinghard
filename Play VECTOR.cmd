@echo off
if exist "%~dp0Build\Windows\Vector.exe" (
  start "" "%~dp0Build\Windows\Vector.exe"
) else (
  echo The Windows build is not available yet. Open the project in Unity and choose VECTOR - Build Windows prototype.
  pause
)
