using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

// Nothing in game code references this assembly, so a stripped player build must be told to keep it.
[assembly: AlwaysLinkAssembly]
[assembly: InternalsVisibleTo("AbstractPixel.GameManagement.Editor")]
[assembly: InternalsVisibleTo("AbstractPixel.GameManagement.Tests")]
