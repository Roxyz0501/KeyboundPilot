$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
dotnet restore (Join-Path $root 'KeyboundPilot') --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
dotnet build (Join-Path $root 'KeyboundPilot') -c Release --no-restore --no-incremental
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
dotnet run --project (Join-Path $root 'Tests') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
python (Join-Path $PSScriptRoot 'Package.py')
if ($LASTEXITCODE -ne 0) { throw 'Packaging failed' }
