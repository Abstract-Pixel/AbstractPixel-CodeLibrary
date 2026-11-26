using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Shows or hides a HUD object as the states' HUD effect asks (<see cref="GameHud"/>).</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/HUD Toggle")]
    public class HudToggleReactor : MonoBehaviour
    {
        [SerializeField] private GameObject hud;

        private void OnEnable()
        {
            GameHud.VisibilityChanged += Apply;
            Apply(GameHud.IsVisible);
        }

        private void OnDisable()
        {
            GameHud.VisibilityChanged -= Apply;
        }

        private void Apply(bool _isVisible)
        {
            if (hud != null)
            {
                hud.SetActive(_isVisible);
            }
        }
    }
}
