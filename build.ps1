<#
.SYNOPSIS
    Build script for SIISWebApp (Windows/PowerShell)
#>

$ErrorActionPreference = 'Stop'

function Write-Status($message) {
    Write-Host "`n[~] $message..." -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "[✓] $message" -ForegroundColor Green
}

Write-Status "Building .NET backend (skipping Vue build)"
Set-Location "C:\Users\User\Desktop\SIISWebApp\SIISMinimalAPI"
dotnet build -p:SKIP_VUE_BUILD=true

Write-Status "Publishing backend to ./publish"
dotnet publish -c Release -o "C:\Users\User\Desktop\SIISWebApp\publish" -p:SKIP_VUE_BUILD=true

Write-Success "Build and publish completed successfully!"
