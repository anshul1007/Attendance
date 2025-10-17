# Start Frontend Application
Write-Host "Starting Attendance Management Frontend..." -ForegroundColor Green
Write-Host ""

# Navigate to the frontend directory
$frontendPath = "E:\Attendance\frontend"

if (Test-Path $frontendPath) {
    Set-Location $frontendPath
    Write-Host "✓ Frontend found at: $frontendPath" -ForegroundColor Green
    Write-Host ""
    
    # Check if node_modules exists
    if (!(Test-Path "node_modules")) {
        Write-Host "⚠ node_modules not found. Installing dependencies..." -ForegroundColor Yellow
        npm install
        Write-Host ""
    }
    
    Write-Host "Starting development server..." -ForegroundColor Yellow
    Write-Host "Application will be available at: http://localhost:4200" -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan
    Write-Host ""
    
    # Start the Angular app
    npm start
} else {
    Write-Host "✗ Error: Frontend not found at $frontendPath" -ForegroundColor Red
    Write-Host "Please ensure the frontend directory exists." -ForegroundColor Yellow
}
