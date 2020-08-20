// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Collections.Specialized;
    using System.Xml;

    public class Provider
    {
        private readonly NameValueCollection providerAttributes = new NameValueCollection();
        private readonly string providerName;
        private readonly string providerType;

        public Provider(XmlAttributeCollection attributes)
        {
            // Set the name of the provider
            this.providerName = attributes["name"].Value;

            // Set the type of the provider
            this.providerType = attributes["type"].Value;

            // Store all the attributes in the attributes bucket
            foreach (XmlAttribute attribute in attributes)
            {
                if (attribute.Name != "name" && attribute.Name != "type")
                {
                    this.providerAttributes.Add(attribute.Name, attribute.Value);
                }
            }
        }

        public string Name
        {
            get
            {
                return this.providerName;
            }
        }

        public string Type
        {
            get
            {
                return this.providerType;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return this.providerAttributes;
            }
        }
    }
}
