using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Shows a menu while its state is active: on enter or once effects apply, as chosen; hides it when the state leaves.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/State Menu")]
    public class StateMenuReactor : MonoBehaviour
    {
        [SerializeField] private StateSO state;
        [SerializeField] private GameObject content;
        [SerializeField] private MenuRevealTiming reveal = MenuRevealTiming.OnConfigurationsApplied;

        private StateWatch watch;

        private void Awake()
        {
            watch = new StateWatch(state);
            watch.Entered += HandleEntered;
            watch.Applied += HandleApplied;
            watch.Left += Hide;
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
