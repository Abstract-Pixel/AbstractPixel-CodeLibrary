using System.Runtime.CompilerServices;

// The editor reads data game code has no use for, such as which Own values the developer picked.
[assembly: InternalsVisibleTo("AbstractPixel.GameManagement.Editor")]
// The debugger reads the applied effects and whether the journal moved.
[assembly: InternalsVisibleTo("AbstractPixel.GameManagement.Debugger")]
