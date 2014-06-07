using System.Xml.Serialization;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    [XmlInclude(typeof(ClientBoolean))]
    [XmlInclude(typeof(ClientNumber))]
    [XmlInclude(typeof(ClientString))]
    public class ClientOption
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ClientOption()
        {
        }

        public ClientOption(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ClientString : ClientOption
    {
        public ClientString()
        {
        }

        public ClientString(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ClientNumber : ClientOption
    {
        public ClientNumber()
        {
        }

        public ClientNumber(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ClientBoolean : ClientOption
    {
        public ClientBoolean()
        {
        }

        public ClientBoolean(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}