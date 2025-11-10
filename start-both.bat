@echo off
echo 🚀 Starting RagApi System...
echo ========================================

REM Check if we're in the right directory
if not exist "RagApi" (
    echo ❌ RagApi folder not found!
    pause
  exit /b 1
)

if not exist "RagMaui" (
    echo ❌ RagMaui folder not found!
    pause
    exit /b 1
)

echo ✅ Project folders found

REM Start RagApi (Backend)
echo 🔄 Starting RagApi Backend...
start "RagApi Backend" cmd /k "cd RagApi && dotnet run"

REM Wait a bit for API to start
timeout /t 3 /nobreak > nul

REM Start RagMaui (Frontend)
echo 🔄 Starting RagMaui Frontend...
start "RagMaui Frontend" cmd /k "cd RagMaui && dotnet run -f net9.0-windows10.0.19041.0"

echo.
echo 🎉 Applications started!
echo ========================================
echo 🌐 RagApi Backend: https://localhost:7296
echo 📱 RagMaui Frontend: Windows Application
echo 📖 Swagger UI: https://localhost:7296/swagger
echo.
echo Both applications will open in separate command windows
echo Close those windows to stop the applications
echo.
pause