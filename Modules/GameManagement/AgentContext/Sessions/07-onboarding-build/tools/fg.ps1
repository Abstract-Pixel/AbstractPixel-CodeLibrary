param([string]$Title = "Abstract_Pixel_Lab")
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class Fg { [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h); [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c); }
"@
$p = Get-Process Unity | Where-Object { $_.MainWindowTitle -like "*$Title*" } | Select-Object -First 1
[Fg]::ShowWindow($p.MainWindowHandle, 9) | Out-Null
[Fg]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
