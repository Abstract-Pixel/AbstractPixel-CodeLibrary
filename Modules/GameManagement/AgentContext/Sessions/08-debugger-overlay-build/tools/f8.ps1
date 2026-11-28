param([string]$Focus)
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class K {
  [DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
}
"@
& $Focus -Project Abstract_Pixel_Lab | Out-Null
Start-Sleep -Milliseconds 700
$t = New-Object System.Text.StringBuilder 300
[void][K]::GetWindowText([K]::GetForegroundWindow(), $t, 300)
if ($t.ToString() -notlike "*Abstract_Pixel_Lab*") { Write-Output "Lab not in front: $t"; exit 1 }
[K]::keybd_event(0x77, 0x42, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[K]::keybd_event(0x77, 0x42, 2, [UIntPtr]::Zero)
Write-Output "real F8 sent"
