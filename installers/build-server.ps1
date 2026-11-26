#requires -Version 5.1
<#
.SYNOPSIS
  Publishes the deployment server + console and compiles the Inno Setup installer.

.DESCRIPTION
  1. dotnet publish of the server (framework-dependent, win-x64) into staging\server
  2. dotnet publish of the console into the same folder (must sit next to the server)
  3. Compiles installers\server\SoftwareDeployment.Server.iss with ISCC

.PARAMETER SelfContained
  Publish a self-contained build (bundles the .NET runtime, no prerequisite install).

.PARAMETER Configuration
  Build configuration (default: Release).
#>
param(
    [switch]$SelfContained,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root     = Split-Path -Parent $PSScriptRoot
$staging  = Join-Path $root "staging\server"
$server   = Join-Path $root "src\Server\SoftwareDeployment.Server\SoftwareDeployment.Server.csproj"
$console  = Join-Path $root "src\Server\SoftwareDeployment.Server.Console\SoftwareDeployment.Server.Console.csproj"
$iss      = Join-Path $PSScriptRoot "server\SoftwareDeployment.Server.iss"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

$scArgs = @("-r", "win-x64", "--self-contained", $(if ($SelfContained) { "true" } else { "false" }))

Write-Host "==> Publishing server..." -ForegroundColor Cyan
dotnet publish $server -c $Configuration @scArgs -o $staging

Write-Host "==> Publishing console (into same folder)..." -ForegroundColor Cyan
dotnet publish $console -c $Configuration @scArgs -o $staging

# Find ISCC (Inno Setup 6).
$iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
if (-not $iscc) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    $iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $iscc) {
    Write-Warning "ISCC.exe (Inno Setup 6) not found. Binaries are staged at: $staging"
    Write-Warning "Install Inno Setup 6 and run: ISCC.exe /DSourceDir=`"$staging`" `"$iss`""
    return
}

Write-Host "==> Compiling installer..." -ForegroundColor Cyan
& $iscc "/DSourceDir=$staging" $iss

Write-Host "==> Done. Installer in installers\server\Output." -ForegroundColor Green
