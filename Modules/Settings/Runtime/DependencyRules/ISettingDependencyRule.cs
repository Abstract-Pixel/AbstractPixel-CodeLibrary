namespace AbstractPixel.Settings
{
    /// <summary>
    /// This Interface is to be used for scripts that needs to evaluate different rules that can be applied to any setting 
    /// inheriting from BaseSetting<T>
    /// </summary>
    public interface ISettingDependencyRule
    {
        bool Evaluate();
    }
}