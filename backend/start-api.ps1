# Start Backend API
Write-Host "Starting Attendance Management Backend API..." -ForegroundColor Green
Write-Host ""

# Navigate to the API project directory
$projectPath = "E:\Attendance\backend\AttendanceAPI"

if (Test-Path $projectPath) {
    Set-Location $projectPath
    Write-Host "✓ Project found at: $projectPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Starting API server..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan
    Write-Host ""
    
    # Run the API
    dotnet run
} else {
    Write-Host "✗ Error: Project not found at $projectPath" -ForegroundColor Red
    Write-Host "Please ensure the project exists and the path is correct." -ForegroundColor Yellow
}
