using System;

namespace AbstractPixel.SaveSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SavableAttribute : Attribute
    {
        public SaveCategory Category { get; set; }
        public string ClassId { get; set; }
        public SavableAttribute(SaveCategory _dataCategory, string _classId = default)
        {
            Category = _dataCategory;
            ClassId = _classId;

        }

    }
}
