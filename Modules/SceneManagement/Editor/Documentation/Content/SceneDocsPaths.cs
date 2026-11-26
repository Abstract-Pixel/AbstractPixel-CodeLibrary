using AbstractPixel.EditorModules.Documentation;
namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>Where the scene documentation's files are, as asset paths.</summary>
    public static class SceneDocsPaths
    {
        public const string Library = "Packages/com.abstractpixel.library/";
        public const string Module = Library + "Modules/SceneManagement/";
        public const string Pages = Module + "Editor/Documentation/Pages/";
        public const string Images = Module + "Editor/Documentation/Images/";
        public const string Samples = Module + "Samples/";
        public const string Adr = Module + "docs/adr/";
        public const string Glossary = Module + "GLOSSARY.md";
        public const string Contents = Pages + "Contents.md";
    }
}
