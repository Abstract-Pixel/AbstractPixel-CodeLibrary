using UnityEngine;

namespace AbstractPixel.GameManagement
{
    /// <summary>Component form of <see cref="FollowUpRule"/>: when one state leaves, request another.</summary>
    [AddComponentMenu("Abstract Pixel/Game Management/When State Leaves Go To")]
    public class WhenLeavesGoTo : MonoBehaviour
    {
        [SerializeField] private StateSO whenLeaves;
        [SerializeField] private StateSO goTo;

        private FollowUpRule rule;

        private void OnEnable()
        {
            rule = new FollowUpRule(whenLeaves, goTo, name);
            rule.Bind();
        }

        private void OnDisable()
        {
            rule?.Unbind();
        }
    }
}
