using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AbstractPixelCore
{
    public class ImageSwapperOnHandlerEvents : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
    {
        [Header("Target Image")]
        [Tooltip("The Image component whose sprite will be swapped. If left empty, it will search this GameObject.")]
        [SerializeField] Image targetImage;

        [Header("Pointer Event Sprites")]
        [SerializeField] Sprite onPointerEnterSprite;
        [SerializeField] Sprite onPointerExitSprite;
        [SerializeField] Sprite onPointerDownSprite;
        [SerializeField] Sprite onPointerUpSprite;
        [SerializeField] Sprite onPointerClickSprite;

        [Header("Selection Event Sprites (Gamepad / Keyboard)")]
        [SerializeField] Sprite onSelectSprite;
        [SerializeField] Sprite onDeselectSprite;
        [SerializeField] Sprite onSubmitSprite;

        [Header("Settings")]
        [Tooltip("If an event's sprite is unassigned (null), fallback to the original default sprite.")]
        [SerializeField] bool fallbackToOriginalIfNull = false;

        [Tooltip("Reverts the image back to its original startup sprite when disabled.")]
        [SerializeField] bool restoreOriginalOnDisable = true;

        [Tooltip("Prevents Pointer Exit from swapping the sprite if this object is still selected via keyboard/gamepad.")]
        [SerializeField] bool keepSelectionOnPointerExit = true;

        Sprite originalSprite;
        bool isInitialized = false;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            // bad code remove later or change later 
            if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                SwapSprite(onSelectSprite);
            }
        }

        private void OnDisable()
        {
            if (restoreOriginalOnDisable && targetImage != null && originalSprite != null)
            {
                targetImage.sprite = originalSprite;
            }
        }

        private void Initialize()
        {
            if (isInitialized) return;

            // Fallback to Image on this GameObject if not assigned in Inspector
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage != null)
            {
                originalSprite = targetImage.sprite;
                isInitialized = true;
            }
        }

        private void SwapSprite(Sprite newSprite)
        {
            if (targetImage == null) return;

            if (newSprite != null)
            {
                targetImage.sprite = newSprite;
            }
            else if (fallbackToOriginalIfNull && originalSprite != null)
            {
                targetImage.sprite = originalSprite;
            }
        }

        // --- Pointer Event Handlers ---

        public void OnPointerEnter(PointerEventData eventData)
        {
            SwapSprite(onPointerEnterSprite);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (keepSelectionOnPointerExit && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                return;
            }

            SwapSprite(onPointerExitSprite);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SwapSprite(onPointerDownSprite);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SwapSprite(onPointerUpSprite);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SwapSprite(onPointerClickSprite);
        }

        // --- Selection Event Handlers (Gamepad / Keyboard) ---

        public void OnSelect(BaseEventData eventData)
        {
            SwapSprite(onSelectSprite);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SwapSprite(onDeselectSprite);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            SwapSprite(onSubmitSprite);
        }
    }
}