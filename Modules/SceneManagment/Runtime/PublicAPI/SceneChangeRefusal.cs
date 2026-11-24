namespace AbstractPixel.SceneManagement
{
    /// <summary>Why the scene system refused a scene change request.</summary>
    public enum SceneChangeRefusal
    {
        /// <summary>Not refused. The value when the outcome is Done or Failed.</summary>
        None,
        /// <summary>Another scene change is running. Only one runs at a time.</summary>
        Busy,
        /// <summary>Start-up is still opening the first scene group. Ask again after SceneEventBus.OnGameStarted.</summary>
        StartUpRunning,
        /// <summary>The scene group asked for is already open. Use Retry to start it again.</summary>
        AlreadyOpen,
        /// <summary>No scene group was given.</summary>
        NoGroupGiven,
        /// <summary>Retry was asked for while no scene group is open.</summary>
        NothingOpenToRetry,
    }
}
