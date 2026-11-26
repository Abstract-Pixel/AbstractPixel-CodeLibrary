namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>Which of the two scene opening preferences decides what opens next to a scene opened on its own.</summary>
    public enum SceneOpeningPreference
    {
        /// <summary>No preference applies: the launch scene, or nothing start-up could do. Nothing opens.</summary>
        None,

        /// <summary>"Open a main scene with its scene group": a main scene, a manager or shared scene, a sample scene.</summary>
        MainSceneWithGroup,

        /// <summary>"Open a scene in no group with the default scene set": a scene in no group.</summary>
        NoGroupSceneWithDefaultSet
    }
}
