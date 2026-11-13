using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// An empty MonoBehaviour that only runs coroutines. Give one group of coroutines (for example one animation
    /// chain) its own host, and StopAllCoroutines on the host stops exactly that group, leaving the owner's other
    /// coroutines running. Unlike CoroutineWorkHorse it is not a singleton: add one wherever a separate group is needed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoroutineHost : MonoBehaviour
    {
        /// <summary>
        /// Creates a host on a new child of _parent, so it lives and dies with that object.
        /// </summary>
        public static CoroutineHost CreateChildHost(Transform _parent, string _name)
        {
            GameObject hostObject = new GameObject(_name);
            hostObject.transform.SetParent(_parent, false);
            return hostObject.AddComponent<CoroutineHost>();
        }
    }
}
