using System.Runtime.CompilerServices;

// The test assemblies check internal parts of the save system directly.
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.Tests")]
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.PlayModeTests")]
// The editor menu's Load All re-reads save files through the internal session.
[assembly: InternalsVisibleTo("AbstractPixel.SaveSystem.Editor")]
