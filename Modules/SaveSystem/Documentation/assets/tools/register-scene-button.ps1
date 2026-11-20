# Optional, one-time, per machine. Without this, a scene button still pings the scene in Unity when
# the Editor is open, and a Types page's "Open in Visual Studio" button still copies the file's path;
# this adds the fallbacks for when Unity is closed, by registering two URL protocols so Windows hands
# a click to open-scene.ps1 (File Explorer) or open-file.ps1 (your default .cs editor). Registers for
# the CURRENT USER only (HKEY_CURRENT_USER); no admin rights needed.
#
# Usage, from this folder, with Unity closed or open either way:
#   .\register-scene-button.ps1 -ProjectRoot "C:\path\to\your\Unity\project"
# Re-run it (with the new path) if you move the project, or start using these docs from a second
# project: the protocols are machine-wide, so only the most recently registered project's buttons
# use them. Every project's scene button still works for the "ping a running Unity" case.

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectRoot
)

$ProjectRoot = (Resolve-Path $ProjectRoot).Path

function Register-Protocol($Name, $Description, $HelperScript) {
    $helper = Join-Path $PSScriptRoot $HelperScript
    $command = "powershell.exe -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$helper`" -ProjectRoot `"$ProjectRoot`" -Url `"%1`""
    New-Item -Path "HKCU:\Software\Classes\$Name" -Force | Out-Null
    Set-ItemProperty -Path "HKCU:\Software\Classes\$Name" -Name "(default)" -Value $Description
    Set-ItemProperty -Path "HKCU:\Software\Classes\$Name" -Name "URL Protocol" -Value ""
    New-Item -Path "HKCU:\Software\Classes\$Name\shell\open\command" -Force | Out-Null
    Set-ItemProperty -Path "HKCU:\Software\Classes\$Name\shell\open\command" -Name "(default)" -Value $command
}

Register-Protocol "apscene" "URL:Abstract Pixel scene opener" "open-scene.ps1"
Register-Protocol "apfile" "URL:Abstract Pixel file opener" "open-file.ps1"

Write-Host "Registered apscene:// and apfile:// for $ProjectRoot."
Write-Host "Scene buttons now open File Explorer there, and Types page file buttons now open the file in your default .cs editor, when Unity isn't running."
