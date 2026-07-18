using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(LayoutElement), typeof(ContentSizeFitter))]
    public class DynamicPanelWidthController : MonoBehaviour
    {
        [Tooltip("The maximum horizontal width (in pixels) this panel can expand to before child texts begin to wrap.\"")]
        [SerializeField] private float maxContainerWidth = 800f;

        private LayoutElement rootLayoutElement;
        private TMP_Text[] cachedTextFields;

        string lastCombinedTextContent = string.Empty;

        private void OnValidate()
        {
            if (rootLayoutElement == null)
            {
                rootLayoutElement = GetComponent<LayoutElement>();
                rootLayoutElement.preferredWidth = maxContainerWidth;
            }
            if (cachedTextFields == null)
            {
                cachedTextFields = GetComponentsInChildren<TMP_Text>(true);
            }
        }

        private void OnEnable()
        {
            if (rootLayoutElement == null)
            {
                rootLayoutElement = GetComponent<LayoutElement>();
                rootLayoutElement.preferredWidth = maxContainerWidth;
            }
            cachedTextFields = GetComponentsInChildren<TMP_Text>(true);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                AdjustsLayout();
            }
            else
            {
                if (HasTextContentChanged())
                {
                    AdjustsLayout();
                }
            }
        }

        public void AdjustsLayout()
        {
            if (cachedTextFields == null || cachedTextFields.Length == 0)
            {
                return;
            }

            float currentMaxPreferredWidth = 0f;

            foreach (TMP_Text textField in cachedTextFields)
            {
                if (textField == null || !textField.gameObject.activeInHierarchy)
                {
                    continue;
                }
                float textWidth = textField.preferredWidth;
                if (textWidth > currentMaxPreferredWidth)
                {
                    currentMaxPreferredWidth = textWidth;
                }
            }

            if (currentMaxPreferredWidth > maxContainerWidth)
            {
                rootLayoutElement.enabled = true;
            }
            else
            {
                rootLayoutElement.enabled = false;
            }
        }

        private bool HasTextContentChanged()
        {
            string combinedTextContent = string.Empty;
            foreach (TMP_Text textField in cachedTextFields)
            {
                if (textField != null)
                {
                    combinedTextContent += textField.text;
                }
            }

            bool hasChanged = combinedTextContent != lastCombinedTextContent;
            lastCombinedTextContent = combinedTextContent;
            return hasChanged;
        }
    }
}
