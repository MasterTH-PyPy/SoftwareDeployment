#requires -Version 5.1
<#
.SYNOPSIS
  Publishes the client agent + GUI and compiles the Inno Setup installer.

.DESCRIPTION
  1. dotnet publish of the agent service (win-x64) into staging\client
  2. dotnet publish of the GUI into the same folder (shared Core.dll)
  3. Compiles installers\client\SoftwareDeployment.Client.iss with ISCC

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
$root    = Split-Path -Parent $PSScriptRoot
$staging = Join-Path $root "staging\client"
$service = Join-Path $root "src\Client\SoftwareDeployment.Client.Service\SoftwareDeployment.Client.Service.csproj"
$gui     = Join-Path $root "src\Client\SoftwareDeployment.Client.Gui\SoftwareDeployment.Client.Gui.csproj"
$iss     = Join-Path $PSScriptRoot "client\SoftwareDeployment.Client.iss"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null

$scArgs = @("-r", "win-x64", "--self-contained", $(if ($SelfContained) { "true" } else { "false" }))

Write-Host "==> Publishing agent service..." -ForegroundColor Cyan
dotnet publish $service -c $Configuration @scArgs -o $staging

Write-Host "==> Publishing GUI (into same folder)..." -ForegroundColor Cyan
dotnet publish $gui -c $Configuration @scArgs -o $staging

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

Write-Host "==> Done. Installer in installers\client\Output." -ForegroundColor Green
