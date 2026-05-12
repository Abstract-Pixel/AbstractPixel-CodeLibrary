
namespace AbstractPixel.GameManagement
{

    /// <summary>
    /// This is used to define the different game request events that can be raised 
    /// To trigger GameStateEvents like OnStart, OnPlay, OnPause, OnWin, OnLose etc
    /// </summary>
    public enum GameStateEvent
    {
        StartGame,
        PlayGame,
        PauseGame,
        UnPauseGame,
        WinGame,
        LoseGame,
        RestartGame,
        QuitGame,
    }
}
