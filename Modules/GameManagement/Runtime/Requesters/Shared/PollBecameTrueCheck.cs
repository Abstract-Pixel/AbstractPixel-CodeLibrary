namespace AbstractPixel.GameManagement
{
    /// <summary>True once when a condition turns from false to true, so a poll does not send a request every frame.</summary>
    public class PollBecameTrueCheck
    {
        private bool wasMet;

        public bool Check(bool _isMet)
        {
            bool isNewlyMet = _isMet && !wasMet;
            wasMet = _isMet;
            return isNewlyMet;
        }
    }
}
