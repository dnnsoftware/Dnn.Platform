using System;

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    public class ConsoleCommandAttribute : Attribute
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
    {

        public string Name { get; set; }
        public string NameSpace { get; set; }
        public string Description { get; set; }
        public string[] Options;

        public ConsoleCommandAttribute(string name, string description, string[] options)
        {
            Name = name;
            NameSpace = "";
            Description = description;
            Options = options;
        }

        public ConsoleCommandAttribute(string name, string nameSpace, string description, string[] options)
        {
            Name = name;
            NameSpace = nameSpace;
            Description = description;
            Options = options;
        }
    }
}