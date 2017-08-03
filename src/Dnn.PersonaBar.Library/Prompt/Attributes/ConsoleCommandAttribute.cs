using System;

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    public class ConsoleCommandAttribute : Attribute
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        public ConsoleCommandAttribute(string name, string category, string description)
        {
            Name = name;
            Category = category;
            Description = description;
        }
    }
}