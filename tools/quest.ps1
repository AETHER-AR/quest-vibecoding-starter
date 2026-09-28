param(
    [ValidateSet('Preflight','Prepare','BuildStarter','BuildFinished','InstallStarter','InstallFinished')]
    [string]$Action = 'Preflight',
    [string]$UnityPath,
    [string]$Serial
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$requiredVersion = '2022.3.62f3'
if (-not $UnityPath) {
    $candidates = @(
        "$env:LOCALAPPDATA/Unity/Hub/Editor/$requiredVersion/Editor/Unity.exe",
        "$env:ProgramFiles/Unity/Hub/Editor/$requiredVersion/Editor/Unity.exe"
    )
    $UnityPath = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $UnityPath -or -not (Test-Path -LiteralPath $UnityPath)) {
    throw "Install Unity $requiredVersion in Unity Hub, including Android Build Support, SDK/NDK and OpenJDK. For a custom install location use -UnityPath."
}
$editorVersion = (Get-Item -LiteralPath $UnityPath).VersionInfo.ProductVersion
if ($editorVersion -notlike "$requiredVersion*") { throw "Wrong Unity editor: $editorVersion. Required: $requiredVersion." }
$androidRoot = Join-Path (Split-Path -Parent $UnityPath) 'Data/PlaybackEngines/AndroidPlayer'
foreach ($relative in @('SDK/platform-tools/adb.exe','NDK/source.properties','OpenJDK/bin/java.exe','SDK/platforms/android-34/android.jar')) {
    if (-not (Test-Path -LiteralPath (Join-Path $androidRoot $relative))) {
        throw "Missing Android tool: $relative. Add Android Build Support, SDK/NDK and OpenJDK through Unity Hub; install Android SDK platform 34 if absent."
    }
}
$adb = Join-Path $androidRoot 'SDK/platform-tools/adb.exe'
function Invoke-Adb([string[]]$Arguments) {
    # Windows PowerShell treats native stderr as ErrorRecord, even for daemon startup notices.
    $previousPreference = $ErrorActionPreference
    try { $ErrorActionPreference = 'Continue'; $output = & $adb @Arguments 2>&1; $nativeExit = $LASTEXITCODE }
    finally { $ErrorActionPreference = $previousPreference }
    if ($nativeExit -ne 0) { throw "ADB failed: $($output -join [Environment]::NewLine)" }
    return $output
}
function Source-Hash {
    $lines = foreach ($directory in @('Assets','Packages','ProjectSettings')) {
        Get-ChildItem -LiteralPath (Join-Path $projectRoot $directory) -File -Recurse | Sort-Object FullName | ForEach-Object {
            ($_.FullName.Substring($projectRoot.Length).Replace('\','/')) + ':' + (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    }
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))))).Replace('-','').ToLowerInvariant() }
    finally { $sha.Dispose() }
}
$devices = @(Invoke-Adb @('devices','-l'))
Write-Host "Unity $requiredVersion and the Android build tools are available."
$devices | ForEach-Object { Write-Host $_ }
if ($Action -eq 'Preflight') {
    Write-Host 'This checks installed tools, not Unity licensing, compilation, or headset visuals.'
    return
}
if ($Action.StartsWith('Install')) {
    $variant = $Action.Substring(7)
    $apk = Join-Path $projectRoot "Builds/QuestWorkshop-$variant.apk"
    $receiptPath = "$apk.receipt.json"
    if (-not (Test-Path -LiteralPath $receiptPath)) { throw "Build $variant first; no verified build receipt exists." }
    $receipt = Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
    if ($receipt.sourceSha256 -ne (Source-Hash)) { throw 'Source changed since the last build. Rebuild before installing.' }
    if ($receipt.apkSha256 -ne (Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash) { throw 'APK differs from the build receipt. Rebuild first.' }
    # adb -d explicitly selects USB. Windows does not always print usb: in devices -l.
    try {
        $usbState = ((Invoke-Adb @('-d','get-state')) -join '').Trim()
        if ($usbState -ne 'device') { throw "USB state is $usbState" }
        $deviceId = ((Invoke-Adb @('-d','get-serialno')) -join '').Trim()
    } catch { throw 'Connect exactly one authorized Quest over USB. Unlock it and confirm Allow USB debugging. Disconnect other USB Android devices; this script does not switch to Wi-Fi.' }
    if ($Serial -and $Serial -ne $deviceId) { throw 'The connected USB device does not match -Serial.' }
    $model = (Invoke-Adb @('-s',$deviceId,'shell','getprop','ro.product.model')) -join ''
    if ($model.Trim() -ne 'Quest 3') { throw "This tutorial targets Quest 3; connected device reports: $model" }
    Invoke-Adb @('-s',$deviceId,'install','-r',$apk) | Write-Host
    Invoke-Adb @('-s',$deviceId,'shell','am','force-stop','com.aether.questworkshop') | Out-Null
    Invoke-Adb @('-s',$deviceId,'shell','am','start','-n','com.aether.questworkshop/com.unity3d.player.UnityPlayerActivity') | Write-Host
    Write-Host 'Installed the recorded APK. Now verify passthrough, both eyes, controls and comfort in the headset.'
    return
}
$lockPath = Join-Path $projectRoot 'Temp/UnityLockfile'
if (Test-Path -LiteralPath $lockPath) {
    try { $lockProbe = [IO.File]::Open($lockPath, 'Open', 'ReadWrite', 'None'); $lockProbe.Dispose() }
    catch { throw 'Close this project in Unity before using the command-line builder. Alternatively use the Workshop menu in Unity.' }
}
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'artifacts') | Out-Null
$log = Join-Path $projectRoot "artifacts/$Action.log"
$method = "QuestWorkshop.Editor.WorkshopBuild.$Action"
$argsForUnity = @('-batchmode','-nographics','-quit','-projectPath',('"'+$projectRoot+'"'),'-buildTarget','Android','-executeMethod',$method,'-logFile',('"'+$log+'"'))
$run = Start-Process -FilePath $UnityPath -ArgumentList $argsForUnity -WindowStyle Hidden -PassThru -Wait
if ($run.ExitCode -ne 0) { throw "Unity failed with exit code $($run.ExitCode). Read $log. An older APK is not a successful build." }
if ($Action.StartsWith('Build')) {
    $variant = $Action.Substring(5)
    $apk = Join-Path $projectRoot "Builds/QuestWorkshop-$variant.apk"
    if (-not (Test-Path -LiteralPath $apk)) { throw "Unity exited without producing $apk." }
    if (-not (Select-String -LiteralPath $log -SimpleMatch "QUEST_WORKSHOP: $variant APK built successfully" -Quiet)) {
        throw 'Unity did not report completing this build. No install receipt will be created.'
    }
    & (Join-Path $PSScriptRoot 'verify-apk.ps1') -Apk $apk -AndroidRoot $androidRoot
    @{
        variant=$variant; utc=[DateTime]::UtcNow.ToString('o'); unity=$requiredVersion
        sourceSha256=(Source-Hash); apkSha256=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash
    } | ConvertTo-Json | Set-Content -LiteralPath "$apk.receipt.json" -Encoding UTF8
    Write-Host "Built $apk. Use Install$variant to install this exact source state."
} else { Write-Host "Prepared the project. Details: $log" }
