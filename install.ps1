$ErrorActionPreference = 'Stop'

$package = Get-ChildItem -Path (Split-Path -Parent $MyInvocation.MyCommand.Path) -Filter '*.msix' -Recurse |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $package) {
    throw 'No MSIX package was found. Run build.ps1 first.'
}

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run PowerShell as Administrator, then run .\install.ps1 again.'
}

Add-AppxPackage -Path $package.FullName -AllowUnsigned
Write-Host "Installed: $($package.FullName)"
Write-Host 'Launch Remote PC On from the Start menu and allow notification access.'
