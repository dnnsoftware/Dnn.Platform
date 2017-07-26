using System;

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    [Serializable]
    public class Command
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string NameSpace { get; set; }
        public string Description { get; set; }
        public string AssemblyName { get; set; }
        public string Version { get; set; }
        public Type CommandType { get; set; }
        //public ConsoleCommandAttribute CommandAttribute { get; set; }
    }
}