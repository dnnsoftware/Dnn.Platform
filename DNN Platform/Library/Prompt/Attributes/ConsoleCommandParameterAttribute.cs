using System;

namespace DotNetNuke.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandParameterAttribute : Attribute
    {
        /// <summary>
        /// The name used in commands to access this parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether the parameter is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// The default value serialized as string
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// The resource key for the description of this parameter
        /// </summary>
        public string DescriptionKey { get; set; }
        public ConsoleCommandParameterAttribute(string name, string description, bool required, string defaultValue)
        {
            Name = name;
            Required = required;
            DefaultValue = defaultValue;
            DescriptionKey = description;
        }
        public ConsoleCommandParameterAttribute(string name, string description, bool required) : this(name, description, required, string.Empty) { }
        public ConsoleCommandParameterAttribute(string name, string description, string defaultValue) : this(name, description, false, defaultValue) { }
        public ConsoleCommandParameterAttribute(string name, string description) : this(name, description, false, string.Empty) { }
    }
}
