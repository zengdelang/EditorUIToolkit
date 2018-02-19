using System;

namespace EUTK
{
    public class NameAttribute : Attribute
    {
        public string name;
        public NameAttribute(string name)
        {
            this.name = name;
        }
    }

    public class DescriptionAttribute : Attribute
    {
        public string description;
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {
        public string category;
        public CategoryAttribute(string category)
        {
            this.category = category;
        }
    }
}