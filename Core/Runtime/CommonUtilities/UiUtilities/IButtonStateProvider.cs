using System;

namespace AbstractPixelCore
{
    public interface IButtonStateProvider
    {
        bool IsHovered { get; }
        bool IsSelected { get; }
        event Action<bool> OnHoverStateChanged;
        event Action<bool> OnSelectStateChanged;
        event Action OnClicked;
        event Action OnSubmitted;
    }
}