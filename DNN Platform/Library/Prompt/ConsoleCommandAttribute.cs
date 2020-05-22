using System;

namespace DotNetNuke.Prompt
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Name { get; set; }
        /// <summary>
        /// Resource key for the category of the command
        /// </summary>
        public string CategoryKey { get; set; }
        /// <summary>
        /// Resource key for the description of the command
        /// </summary>
        public string DescriptionKey { get; set; }

        public ConsoleCommandAttribute(string name, string category, string description)
        {
            Name = name;
            CategoryKey = category;
            DescriptionKey = description;
        }
    }
}
