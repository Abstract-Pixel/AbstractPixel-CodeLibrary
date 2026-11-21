using UnityEngine;

namespace AbstractPixel.SceneManagement.Relocation.Tests
{
    /// <summary>Counts OnHold and OnRelease calls. A runtime class, because a MonoBehaviour from an Editor assembly cannot be added to a GameObject.</summary>
    public sealed class CountingHold : MonoBehaviour, IRelocationHold
    {
        public int Holds;
        public int Releases;

        public void OnHold()
        {
            Holds++;
        }

        public void OnRelease()
        {
            Releases++;
        }
    }
}
