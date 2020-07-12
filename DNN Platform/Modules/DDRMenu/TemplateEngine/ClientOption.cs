// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(ClientBoolean))]
    [XmlInclude(typeof(ClientNumber))]
    [XmlInclude(typeof(ClientString))]
    public class ClientOption
    {
        public ClientOption()
        {
        }

        public ClientOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class ClientString : ClientOption
    {
        public ClientString()
        {
        }

        public ClientString(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class ClientNumber : ClientOption
    {
        public ClientNumber()
        {
        }

        public ClientNumber(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class ClientBoolean : ClientOption
    {
        public ClientBoolean()
        {
        }

        public ClientBoolean(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
