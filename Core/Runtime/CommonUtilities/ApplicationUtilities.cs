using UnityEngine;

namespace AbstractPixel.Core
{
    public class ApplicationUtilities : MonoBehaviour
    {
        [SerializeField] string gameLink;
        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenGameSteamPage()
        {
            Application.OpenURL(gameLink);
        }
    }
}
