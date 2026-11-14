using UnityEngine;

namespace AbstractPixel.SaveSystem
{
    /// <summary>Saves this GameObject's position, rotation and local scale, with no other code.</summary>
    public class SaveTransform : MonoBehaviour, IOnRestored
    {
        [Save] Vector3 position;
        [Save] Quaternion rotation;
        [Save] Vector3 localScale;

        // The Savable Bridge captures whatever these fields hold, so they must always match the live Transform.
        void Awake()
        {
            CopyFromTransform();
        }

        void Update()
        {
            CopyFromTransform();
        }

        public void OnRestored()
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }

        void CopyFromTransform()
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }
    }
}
