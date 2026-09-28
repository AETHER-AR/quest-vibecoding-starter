param([Parameter(Mandatory=$true)][string]$Apk, [Parameter(Mandatory=$true)][string]$AndroidRoot)
$ErrorActionPreference = 'Stop'
$aapt = Get-ChildItem "$AndroidRoot/SDK/build-tools/*/aapt.exe" | Sort-Object FullName -Descending | Select-Object -First 1
if (-not $aapt) { throw 'Android build-tools aapt is missing; APK manifest cannot be checked.' }
$badging = (& $aapt.FullName dump badging $Apk) -join "`n"
if ($LASTEXITCODE -ne 0) { throw 'Could not inspect the built APK.' }
foreach ($required in @("package: name='com.aether.questworkshop'", "native-code: 'arm64-v8a'", "uses-feature: name='com.oculus.feature.PASSTHROUGH'", "uses-feature: name='android.hardware.vr.headtracking'")) {
    if (-not $badging.Contains($required)) { throw "Built APK lacks required configuration: $required" }
}
foreach ($unwanted in @('EYE_TRACKING','eye_tracking','android.permission.CAMERA','horizonos.permission.HEADSET_CAMERA','android.permission.RECORD_AUDIO','com.oculus.permission.USE_SCENE')) {
    if ($badging.Contains($unwanted)) { throw "Built APK requests an unused capability: $unwanted" }
}
Write-Host 'APK manifest passed: standalone ARM64, correct package, required passthrough, no eye/raw-camera/microphone/scene permission.'
