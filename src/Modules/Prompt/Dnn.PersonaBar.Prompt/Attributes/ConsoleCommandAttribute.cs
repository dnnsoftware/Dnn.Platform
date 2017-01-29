using System;

namespace Dnn.PersonaBar.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {

        public string Name;
        public string Description;

        public string[] Options;
        public ConsoleCommandAttribute(string name, string description, string[] options)
        {
            this.Name = name;
            this.Description = description;
            this.Options = options;
        }
    }
}