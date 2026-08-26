<#
.SYNOPSIS
    Build and publish SIISWebApp (Windows/PowerShell)
    Run from: C:\Users\User\Desktop\SIISWebApp
#>

$ErrorActionPreference = 'Stop'

function Write-Status($message) {
    Write-Host "`n[~] $message..." -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "[✓] $message" -ForegroundColor Green
}

$root = Split-Path -Parent $PSScriptRoot
$backend = Join-Path $root "SIISMinimalAPI"

Write-Status "Building .NET backend (Vue build will run automatically)"
Set-Location $backend
dotnet build

Write-Status "Publishing backend to ./publish"
$publishDir = Join-Path $root "publish"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish -c Release -o $publishDir

Write-Success "Build completed. Output: $publishDir"
