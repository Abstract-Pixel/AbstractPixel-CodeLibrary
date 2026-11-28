namespace AbstractPixel.GameManagement.Editor
{
    /// <summary>One menu the developer still has to finish by hand, with what to do.</summary>
    public class OnboardingToDo
    {
        public string MenuName;
        public string Text;

        public OnboardingToDo(string _menuName, string _text)
        {
            MenuName = _menuName;
            Text = _text;
        }
    }
}
