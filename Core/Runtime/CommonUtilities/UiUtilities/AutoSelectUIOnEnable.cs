using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    public class AutoSelectUIOnEnable : MonoBehaviour
    {
        [SerializeField] private Selectable selectableToSelect;

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(selectableToSelect?.gameObject);
        }
    }
}
