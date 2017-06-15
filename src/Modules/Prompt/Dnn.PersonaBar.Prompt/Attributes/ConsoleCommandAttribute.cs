using System;

namespace Dnn.PersonaBar.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {

        public string Name { get; set; }
        public string NameSpace { get; set; }
        public string Description { get; set; }
        public string[] Options;

        public ConsoleCommandAttribute(string name, string description, string[] options)
        {
            this.Name = name;
            this.NameSpace = "";
            this.Description = description;
            this.Options = options;
        }

        public ConsoleCommandAttribute(string name, string nameSpace, string description, string[] options)
        {
            this.Name = name;
            this.NameSpace = nameSpace;
            this.Description = description;
            this.Options = options;
        }
    }
}