// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Specialized;
using System.Xml;

#endregion

namespace DotNetNuke.Framework.Providers
{
    public class Provider
    {
        private readonly NameValueCollection _ProviderAttributes = new NameValueCollection();
        private readonly string _ProviderName;
        private readonly string _ProviderType;

        public Provider(XmlAttributeCollection Attributes)
        {
            //Set the name of the provider
            _ProviderName = Attributes["name"].Value;

            //Set the type of the provider
            _ProviderType = Attributes["type"].Value;

            //Store all the attributes in the attributes bucket
            foreach (XmlAttribute Attribute in Attributes)
            {
                if (Attribute.Name != "name" && Attribute.Name != "type")
                {
                    _ProviderAttributes.Add(Attribute.Name, Attribute.Value);
                }
            }
        }

        public string Name
        {
            get
            {
                return _ProviderName;
            }
        }

        public string Type
        {
            get
            {
                return _ProviderType;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return _ProviderAttributes;
            }
        }
    }
}
