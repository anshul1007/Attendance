# Start Both Frontend and Backend
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Attendance Management System Launcher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to start backend in new window
function Start-Backend {
    Write-Host "Starting Backend API..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\backend; .\start-api.ps1"
}

# Function to start frontend in new window
function Start-Frontend {
    Write-Host "Starting Frontend Application..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\frontend; .\start-frontend.ps1"
}

# Start both
Write-Host "Launching applications..." -ForegroundColor Yellow
Write-Host ""

Start-Backend
Start-Sleep -Seconds 2
Start-Frontend

Write-Host ""
Write-Host "✓ Both applications are starting in separate windows" -ForegroundColor Green
Write-Host ""
Write-Host "Access the applications at:" -ForegroundColor Cyan
Write-Host "  Frontend: http://localhost:4200" -ForegroundColor White
Write-Host "  Backend:  https://localhost:7001 (check terminal for actual port)" -ForegroundColor White
Write-Host "  Swagger:  https://localhost:7001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Close each window or press Ctrl+C in each to stop the servers" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit this launcher..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
