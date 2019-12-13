using System;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class Field
    {
        public Field(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string DisplayName { get; set; }

        public object Value { get; set; }

        public string StringValue { get; set; }

        public Type Type { get; set; }
    }
}
