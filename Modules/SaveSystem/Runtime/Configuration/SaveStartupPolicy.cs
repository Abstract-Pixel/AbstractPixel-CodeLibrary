
namespace AbstractPixel.SaveSystem
{
    public enum SaveStartupPolicy
    {
        Manual =0, // Save system will not automatically load or save data. You must call the appropriate methods to load and save data.
        AutoSetUp = 1, // Creates a new profile if no profile exists, and  loads the most recently used profile if it exists.
    }
}
