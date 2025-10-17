# Start Both Frontend and Backend# Start Both Frontend and Backend

Write-Host "========================================" -ForegroundColor CyanWrite-Host "========================================" -ForegroundColor Cyan

Write-Host " Attendance Management System" -ForegroundColor CyanWrite-Host " Attendance Management System Launcher" -ForegroundColor Cyan

Write-Host "========================================" -ForegroundColor CyanWrite-Host "========================================" -ForegroundColor Cyan

Write-Host ""Write-Host ""



# Start Backend# Function to start backend in new window

Write-Host "Starting Backend API..." -ForegroundColor Greenfunction Start-Backend {

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\backend; .\start-api.ps1"    Write-Host "Starting Backend API..." -ForegroundColor Green

    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\backend; .\start-api.ps1"

Start-Sleep -Seconds 2}



# Start Frontend# Function to start frontend in new window

Write-Host "Starting Frontend..." -ForegroundColor Greenfunction Start-Frontend {

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\frontend; npm start"    Write-Host "Starting Frontend Application..." -ForegroundColor Green

    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd E:\Attendance\frontend; .\start-frontend.ps1"

Write-Host ""}

Write-Host "✓ Applications launching in separate windows" -ForegroundColor Green

Write-Host ""# Start both

Write-Host "Backend:  http://localhost:5146" -ForegroundColor CyanWrite-Host "Launching applications..." -ForegroundColor Yellow

Write-Host "Swagger:  http://localhost:5146/swagger" -ForegroundColor CyanWrite-Host ""

Write-Host "Frontend: http://localhost:4200" -ForegroundColor Cyan

Write-Host ""Start-Backend

Write-Host "Press any key to close..." -ForegroundColor YellowStart-Sleep -Seconds 2

$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")Start-Frontend


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
