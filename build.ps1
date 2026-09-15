$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $repo 'RemotePCon.sln'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) {
    throw 'Visual Studio Installer / vswhere.exe was not found. Install Visual Studio with the Universal Windows Platform development workload.'
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
if (-not $msbuild -or -not (Test-Path $msbuild)) {
    throw 'MSBuild was not found. Install the Visual Studio MSBuild component.'
}

Write-Host "Using MSBuild: $msbuild"
& $msbuild $solution /m /p:Configuration=Release /p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$packages = Get-ChildItem -Path $repo -Filter '*.msix' -Recurse | Sort-Object LastWriteTime -Descending
if ($packages.Count -eq 0) {
    Write-Warning 'Build completed but no MSIX was found. Open the packaging project in Visual Studio and use Build -> Create App Packages if your installed packaging targets do not emit an MSIX during a normal build.'
    exit 0
}

Write-Host "Built package: $($packages[0].FullName)"
Write-Host 'Install for local testing with an elevated PowerShell prompt:'
Write-Host "Add-AppxPackage -Path `"$($packages[0].FullName)`" -AllowUnsigned"
