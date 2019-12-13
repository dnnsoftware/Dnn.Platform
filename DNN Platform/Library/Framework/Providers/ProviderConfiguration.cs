#region Usings

using System.Collections;
using System.Xml;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Framework.Providers
{
    public class ProviderConfiguration
    {
        private readonly Hashtable _Providers = new Hashtable();
        private string _DefaultProvider;

        public string DefaultProvider
        {
            get
            {
                return _DefaultProvider;
            }
        }

        public Hashtable Providers
        {
            get
            {
                return _Providers;
            }
        }

        public static ProviderConfiguration GetProviderConfiguration(string strProvider)
        {
            return (ProviderConfiguration) Config.GetSection("dotnetnuke/" + strProvider);
        }

        internal void LoadValuesFromConfigurationXml(XmlNode node)
        {
            XmlAttributeCollection attributeCollection = node.Attributes;

			//Get the default provider
            _DefaultProvider = attributeCollection["defaultProvider"].Value;

			//Read child nodes
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "providers")
                {
                    GetProviders(child);
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
                        Providers.Add(Provider.Attributes["name"].Value, new Provider(Provider.Attributes));
                        break;
                    case "remove":
                        Providers.Remove(Provider.Attributes["name"].Value);
                        break;
                    case "clear":
                        Providers.Clear();
                        break;
                }
            }
        }
    }
}
