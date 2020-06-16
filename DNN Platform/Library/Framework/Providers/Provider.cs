// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Collections.Specialized;
    using System.Xml;

    public class Provider
    {
        private readonly NameValueCollection _ProviderAttributes = new NameValueCollection();
        private readonly string _ProviderName;
        private readonly string _ProviderType;

        public Provider(XmlAttributeCollection Attributes)
        {
            // Set the name of the provider
            this._ProviderName = Attributes["name"].Value;

            // Set the type of the provider
            this._ProviderType = Attributes["type"].Value;

            // Store all the attributes in the attributes bucket
            foreach (XmlAttribute Attribute in Attributes)
            {
                if (Attribute.Name != "name" && Attribute.Name != "type")
                {
                    this._ProviderAttributes.Add(Attribute.Name, Attribute.Value);
                }
            }
        }

        public string Name
        {
            get
            {
                return this._ProviderName;
            }
        }

        public string Type
        {
            get
            {
                return this._ProviderType;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return this._ProviderAttributes;
            }
        }
    }
}
