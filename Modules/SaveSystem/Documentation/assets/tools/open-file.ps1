# Run by Windows when a Types page's "Open in Visual Studio" button's "apfile://" link fires.
# register-scene-button.ps1 bakes -ProjectRoot into the command that invokes this script, so you don't
# run this one directly.
param(
    [string]$ProjectRoot,
    [string]$Url
)

$encoded = $Url -replace '^apfile://', '' -replace '/$', ''
$decoded = [System.Uri]::UnescapeDataString($encoded)
$full = Join-Path $ProjectRoot $decoded

if (Test-Path $full) {
    # Hands the file to whatever Windows has registered for .cs — Visual Studio, if that's your
    # default editor, without hardcoding devenv.exe's path (which varies by VS edition and version).
    Start-Process -FilePath $full
} else {
    # The button's path didn't resolve under $ProjectRoot (a different project, or it moved).
    # Copy it so you can paste it into Explorer or your editor's "Open File" dialog yourself.
    Set-Clipboard -Value $decoded
    Start-Process explorer.exe $ProjectRoot
}
