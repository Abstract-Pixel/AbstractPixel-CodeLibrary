namespace AbstractPixel.InputRebinding
{
    public readonly struct RebindOverlayPayload
    {
        public readonly bool IsOpen;
        public readonly string DeviceFamilyName;
        public readonly string ActionName;
        public readonly string StatusPrompt;
        public readonly string CurrentPressedInput;
        public readonly bool HasDuplicateConflict;
        public readonly string DuplicateHeader;
        public readonly string DuplicateDetails;

        public RebindOverlayPayload(
            bool _isOpen,
            string _deviceFamilyName,
            string _actionName,
            string _statusPrompt,
            string _currentPressedInput,
            bool _hasDuplicateConflict,
            string _duplicateHeader,
            string _duplicateDetails)
        {
            IsOpen = _isOpen;
            DeviceFamilyName = _deviceFamilyName;
            ActionName = _actionName;
            StatusPrompt = _statusPrompt;
            CurrentPressedInput = _currentPressedInput;
            HasDuplicateConflict = _hasDuplicateConflict;
            DuplicateHeader = _duplicateHeader;
            DuplicateDetails = _duplicateDetails;
        }
    }
}