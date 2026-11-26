using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Shows a menu while its state is active: on enter or once effects apply, as chosen; hides it when the state leaves, and while a sub-state (Settings over Pause) is on top.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Menu")]
    public class StateMenuReactor : MonoBehaviour
    {
        [SerializeField] private StateSO state;
        [SerializeField] private GameObject content;
        [SerializeField] private MenuRevealTiming reveal = MenuRevealTiming.OnConfigurationsApplied;
        [Tooltip("Hide this menu while another state is on top of its state, and show it again when that state leaves.")]
        [SerializeField] private bool hideWhileCovered = true;

        private StateWatch watch;

        private void Awake()
        {
            watch = new StateWatch(state);
            watch.Entered += HandleEntered;
            watch.Applied += HandleApplied;
            watch.Left += Hide;
            watch.Covered += HandleCovered;
            watch.Uncovered += HandleUncovered;
            Hide();
        }

        private void OnEnable()
        {
            watch.Bind();
        }

        private void OnDisable()
        {
            watch.Unbind();
        }

        private void HandleEntered()
        {
            if (reveal == MenuRevealTiming.OnStateRegistered)
            {
                Show();
            }
        }

        private void HandleApplied()
        {
            Show();
        }

        private void HandleCovered()
        {
            if (hideWhileCovered)
            {
                Hide();
            }
        }

        private void HandleUncovered()
        {
            if (hideWhileCovered)
            {
                Show();
            }
        }

        private void Show()
        {
            if (content != null)
            {
                content.SetActive(true);
            }
        }

        private void Hide()
        {
            if (content != null)
            {
                content.SetActive(false);
            }
        }
    }
}
