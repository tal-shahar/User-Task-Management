# PowerShell script to run the API
Write-Host "Starting Task Management API..." -ForegroundColor Green
Write-Host ""

# Check if dotnet is available
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Error: .NET SDK not found. Please install .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}

# Navigate to project directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to restore packages." -ForegroundColor Red
    exit 1
}

# Build project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed." -ForegroundColor Red
    exit 1
}

# Run the API
Write-Host ""
Write-Host "Starting API server..." -ForegroundColor Green
Write-Host "API will be available at: http://localhost:5263" -ForegroundColor Cyan
Write-Host "Swagger UI will be available at: http://localhost:5263/swagger" -ForegroundColor Cyan
Write-Host ""
dotnet run


