using System;

namespace DotNetNuke.Prompt
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandParameterAttribute: Attribute
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public ConsoleCommandParameterAttribute(string name, string description, bool required, string defaultValue)
        {
            Name = name;
            Required = required;
            DefaultValue = defaultValue;
            Description = description;
        }
        public ConsoleCommandParameterAttribute(string name, string description, bool required) : this(name, description, required, string.Empty) { }
        public ConsoleCommandParameterAttribute(string name, string description, string defaultValue) : this(name, description, false, defaultValue) { }
        public ConsoleCommandParameterAttribute(string name, string description) : this(name, description, false, string.Empty) { }
    }
}
