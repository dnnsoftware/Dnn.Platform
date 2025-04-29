// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers;

using System.Collections;
using System.Xml;

using DotNetNuke.Common.Utilities;

public class ProviderConfiguration
{
    private readonly Hashtable providers = new Hashtable();
    private string defaultProvider;

    public string DefaultProvider
    {
        get
        {
            return this.defaultProvider;
        }
    }

    public Hashtable Providers
    {
        get
        {
            return this.providers;
        }
    }

    public static ProviderConfiguration GetProviderConfiguration(string strProvider)
    {
        return (ProviderConfiguration)Config.GetSection("dotnetnuke/" + strProvider);
    }

    public Provider GetDefaultProvider()
    {
        return (Provider)this.providers[this.defaultProvider];
    }

    internal void LoadValuesFromConfigurationXml(XmlNode node)
    {
        XmlAttributeCollection attributeCollection = node.Attributes;

        // Get the default provider
        this.defaultProvider = attributeCollection["defaultProvider"].Value;

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
        foreach (XmlNode provider in node.ChildNodes)
        {
            switch (provider.Name)
            {
                case "add":
                    this.Providers.Add(provider.Attributes["name"].Value, new Provider(provider.Attributes));
                    break;
                case "remove":
                    this.Providers.Remove(provider.Attributes["name"].Value);
                    break;
                case "clear":
                    this.Providers.Clear();
                    break;
            }
        }
    }
}
