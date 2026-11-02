param(
    [string]$RimWorldManaged = 'C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed',
    [string]$BuildTools = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools'
)
$ErrorActionPreference = 'Stop'
if (Get-Process RimWorldWin64 -ErrorAction SilentlyContinue) {
    throw 'Close RimWorld before rebuilding.'
}
$HarmonyDll = Join-Path $RimWorldManaged '..\..\Mods\Harmony\1.6\Assemblies\0Harmony.dll'
$MultiplayerApiDll = Join-Path $RimWorldManaged '..\..\Mods\RimworldMP\Assemblies\0MultiplayerAPI.dll'
$compilerPath = Join-Path $BuildTools 'MSBuild\Current\Bin\Roslyn\csc.exe'
$modPath = Split-Path $PSScriptRoot -Parent
$outputPath = Join-Path $modPath 'Assemblies\SleepSchedulePresets.dll'
New-Item -ItemType Directory -Force -Path (Split-Path $outputPath -Parent) | Out-Null
[xml]$project = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'SleepSchedulePresets.csproj')
$compilerArgs = @('/noconfig', '/target:library', '/nostdlib', '/langversion:9.0', '/deterministic', "/out:$outputPath")
foreach ($reference in $project.Project.ItemGroup.Reference) {
    $referencePath = ([string]$reference.HintPath).Replace('$(RimWorldManaged)', $RimWorldManaged).Replace('$(HarmonyDll)', $HarmonyDll).Replace('$(MultiplayerApiDll)', $MultiplayerApiDll)
    if (!(Test-Path -LiteralPath $referencePath)) { throw "Missing reference: $referencePath" }
    $compilerArgs += "/reference:$referencePath"
}
$compilerArgs += @(Get-ChildItem -LiteralPath $PSScriptRoot -Recurse -Filter '*.cs' -File |
    Where-Object { $_.FullName -notmatch '[\\/]obj[\\/]' } |
    Sort-Object FullName | ForEach-Object { $_.FullName })
# Print a complete, directly reusable PowerShell command for compile evidence.
$displayArgs = $compilerArgs | ForEach-Object { "'" + $_.Replace("'", "''") + "'" }
Write-Output ("& '" + $compilerPath.Replace("'", "''") + "' " + ($displayArgs -join ' '))
& $compilerPath @compilerArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Output "BUILD PASS: $outputPath"
