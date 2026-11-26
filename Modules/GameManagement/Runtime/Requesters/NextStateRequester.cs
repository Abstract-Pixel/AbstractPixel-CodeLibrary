using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Component form of <see cref="NextStateRule"/>: when one state leaves, request another.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/Next State Requester")]
    public class NextStateRequester : MonoBehaviour
    {
        [SerializeField] private StateSO whenLeaves;
        [SerializeField] private StateSO goTo;

        private NextStateRule rule;

        private void OnEnable()
        {
            rule = new NextStateRule(whenLeaves, goTo, name);
            rule.Bind();
        }

        private void OnDisable()
        {
            rule?.Unbind();
        }
    }
}
