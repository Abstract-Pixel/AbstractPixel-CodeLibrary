using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    [ExecuteInEditMode]
    public class TextLayoutController : MonoBehaviour
    {

        [SerializeField] private TMP_Text textField;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private float CharacterLimitBeforeWrapping = 10f;

        private void Update()
        {
            if(textField == null || layoutElement == null)
            {
                return;
            }

            int textFieldLength = textField.text.Length;
            if (textFieldLength > CharacterLimitBeforeWrapping)
            {
                layoutElement.enabled = true;
            }
            else
            {
                layoutElement.enabled = false;
            }
        }


    }
}
