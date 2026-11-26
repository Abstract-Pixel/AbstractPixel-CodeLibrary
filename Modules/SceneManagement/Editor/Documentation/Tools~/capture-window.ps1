# Saves a PNG of one Unity window without bringing it to the front (PrintWindow, so the owner's other windows stay on top).
# Usage:
#   powershell -NoProfile -File capture-window.ps1 -Title "Scene documentation" -Out C:\temp\shot.png
#   powershell -NoProfile -File capture-window.ps1 -Title "ProjectRallyShift" -Out C:\temp\shot.png -X 300 -Y 60 -W 1000 -H 900
# -Title matches the start of a visible top-level window title of any Unity process ("Scene documentation" when the docs
# window floats; the project name when it is docked in the main window). -X/-Y/-W/-H crop in screen pixels (optional).
param([string]$Title = "Scene documentation", [string]$Out, [int]$X = -1, [int]$Y = -1, [int]$W = 0, [int]$H = 0)
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Text; using System.Collections.Generic; using System.Runtime.InteropServices;
public struct DocsRect { public int Left, Top, Right, Bottom; }
public static class DocsCapture {
  public delegate bool EnumProc(IntPtr h, IntPtr p);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr p);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out DocsRect r);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  public static IntPtr Find(HashSet<uint> pids, string title) {
    IntPtr found = IntPtr.Zero;
    EnumWindows((h, p) => { uint pid; GetWindowThreadProcessId(h, out pid);
      if (pids.Contains(pid) && IsWindowVisible(h)) { var s = new StringBuilder(512); GetWindowText(h, s, 512);
        if (s.ToString().StartsWith(title)) { found = h; return false; } }
      return true; }, IntPtr.Zero);
    return found;
  }
}
"@
[DocsCapture]::SetProcessDPIAware() | Out-Null
$pids = New-Object 'System.Collections.Generic.HashSet[uint32]'
foreach ($process in Get-Process Unity -ErrorAction SilentlyContinue) { $pids.Add([uint32]$process.Id) | Out-Null }
$hwnd = [DocsCapture]::Find($pids, $Title)
if ($hwnd -eq [IntPtr]::Zero) { Write-Error "No visible Unity window whose title starts with '$Title'"; exit 1 }
$rect = New-Object DocsRect
[DocsCapture]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
$fullWidth = $rect.Right - $rect.Left; $fullHeight = $rect.Bottom - $rect.Top
$full = New-Object System.Drawing.Bitmap $fullWidth, $fullHeight
$graphics = [System.Drawing.Graphics]::FromImage($full)
$dc = $graphics.GetHdc(); [DocsCapture]::PrintWindow($hwnd, $dc, 2) | Out-Null; $graphics.ReleaseHdc($dc); $graphics.Dispose()
if ($W -le 0 -or $H -le 0) { $full.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png); $full.Dispose(); "saved $Out ($fullWidth x $fullHeight)"; exit 0 }
$crop = New-Object System.Drawing.Bitmap $W, $H
$cropGraphics = [System.Drawing.Graphics]::FromImage($crop)
$source = [System.Drawing.Rectangle]::new([int]($X - $rect.Left), [int]($Y - $rect.Top), $W, $H)
$cropGraphics.DrawImage($full, [System.Drawing.Rectangle]::new(0, 0, $W, $H), $source, [System.Drawing.GraphicsUnit]::Pixel)
$cropGraphics.Dispose(); $crop.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png); $crop.Dispose(); $full.Dispose()
"saved $Out ($W x $H of $fullWidth x $fullHeight)"
