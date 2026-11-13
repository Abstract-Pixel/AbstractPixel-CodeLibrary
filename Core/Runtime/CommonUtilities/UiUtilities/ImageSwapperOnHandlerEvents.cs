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
        [SerializeField] private Image targetImage;

        [Header("Pointer Event Sprites")]
        [SerializeField] private Sprite onPointerEnterSprite;
        [SerializeField] private Sprite onPointerExitSprite;
        [SerializeField] private Sprite onPointerDownSprite;
        [SerializeField] private Sprite onPointerUpSprite;
        [SerializeField] private Sprite onPointerClickSprite;

        [Header("Selection Event Sprites (Gamepad / Keyboard)")]
        [SerializeField] private Sprite onSelectSprite;
        [SerializeField] private Sprite onDeselectSprite;
        [SerializeField] private Sprite onSubmitSprite;

        [Header("Settings")]
        [Tooltip("If an event's sprite is unassigned (null), fallback to the original default sprite.")]
        [SerializeField] private bool fallbackToOriginalIfNull = false;

        [Tooltip("Reverts the image back to its original startup sprite when disabled.")]
        [SerializeField] private bool restoreOriginalOnDisable = true;

        private Sprite originalSprite;
        private bool isInitialized = false;
        private IButtonStateProvider linkedHoverStateProvider;

        private void Awake()
        {
            TryGetComponent(out linkedHoverStateProvider);
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();

            if (linkedHoverStateProvider != null)
            {
                linkedHoverStateProvider.OnHoverStateChanged += HandleHoverStateChanged;
                linkedHoverStateProvider.OnSelectStateChanged += HandleSelectStateChanged;
                linkedHoverStateProvider.OnClicked += HandleClicked;
                linkedHoverStateProvider.OnSubmitted += HandleSubmitted;
            }

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                SwapSprite(onSelectSprite);
            }
        }

        private void OnDisable()
        {
            if (linkedHoverStateProvider != null)
            {
                linkedHoverStateProvider.OnHoverStateChanged -= HandleHoverStateChanged;
                linkedHoverStateProvider.OnSelectStateChanged -= HandleSelectStateChanged;
                linkedHoverStateProvider.OnClicked -= HandleClicked;
                linkedHoverStateProvider.OnSubmitted -= HandleSubmitted;
            }

            if (restoreOriginalOnDisable && targetImage != null && originalSprite != null)
            {
                targetImage.sprite = originalSprite;
            }
        }

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

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

        private void HandleHoverStateChanged(bool _isHovered)
        {
            if (_isHovered)
            {
                SwapSprite(onPointerEnterSprite);
            }
            else
            {
                if (linkedHoverStateProvider != null && linkedHoverStateProvider.IsSelected)
                {
                    return;
                }

                SwapSprite(onPointerExitSprite);
            }
        }

        private void HandleSelectStateChanged(bool _isSelected)
        {
            if (_isSelected)
            {
                SwapSprite(onSelectSprite);
            }
            else
            {
                if (linkedHoverStateProvider != null && linkedHoverStateProvider.IsHovered)
                {
                    return;
                }

                SwapSprite(onDeselectSprite);
            }
        }

        private void HandleClicked()
        {
            SwapSprite(onPointerClickSprite);
        }

        private void HandleSubmitted()
        {
            SwapSprite(onSubmitSprite);
        }

        private void SwapSprite(Sprite _newSprite)
        {
            if (targetImage == null)
            {
                return;
            }

            if (_newSprite != null)
            {
                targetImage.sprite = _newSprite;
            }
            else if (fallbackToOriginalIfNull && originalSprite != null)
            {
                targetImage.sprite = originalSprite;
            }
        }

        // --- Standalone Fallbacks (Used only if ButtonFeedback is NOT attached) ---

        public void OnPointerEnter(PointerEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onPointerEnterSprite);
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onPointerExitSprite);
        }

        public void OnPointerDown(PointerEventData _eventData)
        {
            SwapSprite(onPointerDownSprite);
        }

        public void OnPointerUp(PointerEventData _eventData)
        {
            SwapSprite(onPointerUpSprite);
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onPointerClickSprite);
        }

        public void OnSelect(BaseEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onSelectSprite);
        }

        public void OnDeselect(BaseEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onDeselectSprite);
        }

        public void OnSubmit(BaseEventData _eventData)
        {
            if (linkedHoverStateProvider != null) return;
            SwapSprite(onSubmitSprite);
        }
    }
}