# Run by Windows when a scene button's "apscene://" link fires and no Unity Editor answered the ping.
# register-scene-button.ps1 bakes -ProjectRoot into the command that invokes this script, so you don't
# run this one directly.
param(
    [string]$ProjectRoot,
    [string]$Url
)

$encoded = $Url -replace '^apscene://', '' -replace '/$', ''
$decoded = [System.Uri]::UnescapeDataString($encoded)
$full = Join-Path $ProjectRoot $decoded

if (Test-Path $full) {
    Start-Process explorer.exe "/select,`"$full`""
} else {
    # The button's path didn't resolve under $ProjectRoot (a different project, or it moved).
    # Copy it so you can paste it into Explorer or Unity's search yourself.
    Set-Clipboard -Value $decoded
    Start-Process explorer.exe $ProjectRoot
}
