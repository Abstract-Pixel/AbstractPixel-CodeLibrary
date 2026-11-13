using System.Runtime.CompilerServices;

// The test assemblies check internal parts of the save system directly.
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.Tests")]
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.PlayModeTests")]
// The Settings tests start a save system on memory storage, like the save system's own tests.
[assembly: InternalsVisibleTo("AbstractPixel.Settings.PlayModeTests")]
// The editor menu's Load All re-reads save files through the internal session.
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.Editor")]
