// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Collections;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    public class ProviderConfiguration
    {
        private readonly Hashtable _Providers = new Hashtable();
        private string _DefaultProvider;

        public string DefaultProvider
        {
            get
            {
                return this._DefaultProvider;
            }
        }

        public Hashtable Providers
        {
            get
            {
                return this._Providers;
            }
        }

        public static ProviderConfiguration GetProviderConfiguration(string strProvider)
        {
            return (ProviderConfiguration)Config.GetSection("dotnetnuke/" + strProvider);
        }

        internal void LoadValuesFromConfigurationXml(XmlNode node)
        {
            XmlAttributeCollection attributeCollection = node.Attributes;

            // Get the default provider
            this._DefaultProvider = attributeCollection["defaultProvider"].Value;

            // Read child nodes
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "providers")
                {
                    this.GetProviders(child);
                }
            }
        }

        internal void GetProviders(XmlNode node)
        {
            foreach (XmlNode Provider in node.ChildNodes)
            {
                switch (Provider.Name)
                {
                    case "add":
                        this.Providers.Add(Provider.Attributes["name"].Value, new Provider(Provider.Attributes));
                        break;
                    case "remove":
                        this.Providers.Remove(Provider.Attributes["name"].Value);
                        break;
                    case "clear":
                        this.Providers.Clear();
                        break;
                }
            }
        }
    }
}
