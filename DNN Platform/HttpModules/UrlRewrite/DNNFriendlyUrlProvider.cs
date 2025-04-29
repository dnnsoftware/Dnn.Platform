// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Url.FriendlyUrl;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework.Providers;

public class DNNFriendlyUrlProvider : FriendlyUrlProvider
{
    internal const string ProviderName = "DNNFriendlyUrl";
    internal const string ProviderType = "friendlyUrl";

    private readonly ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);

    private readonly FriendlyUrlProviderBase providerInstance;
    private readonly UrlFormatType urlFormat = UrlFormatType.SearchFriendly;
    private readonly string[] validExtensions;

    public DNNFriendlyUrlProvider()
    {
        // Read the configuration specific information for this provider
        var objProvider = (Provider)this.providerConfiguration.Providers[ProviderName];

        if (!string.IsNullOrEmpty(objProvider.Attributes["urlFormat"]))
        {
            switch (objProvider.Attributes["urlFormat"].ToLowerInvariant())
            {
                case "searchfriendly":
                    this.urlFormat = UrlFormatType.SearchFriendly;
                    break;
                case "humanfriendly":
                    this.urlFormat = UrlFormatType.HumanFriendly;
                    break;
                case "advanced":
                case "customonly":
                    this.urlFormat = UrlFormatType.Advanced;
                    break;
                default:
                    this.urlFormat = UrlFormatType.SearchFriendly;
                    break;
            }
        }

        // instance the correct provider implementation
        switch (this.urlFormat)
        {
            case UrlFormatType.Advanced:
                this.providerInstance = new AdvancedFriendlyUrlProvider(objProvider.Attributes);
                break;
            case UrlFormatType.HumanFriendly:
            case UrlFormatType.SearchFriendly:
                this.providerInstance = new BasicFriendlyUrlProvider(objProvider.Attributes);
                break;
        }

        string extensions = !string.IsNullOrEmpty(objProvider.Attributes["validExtensions"]) ? objProvider.Attributes["validExtensions"] : ".aspx";
        this.validExtensions = extensions.Split(',');
    }

    public string[] ValidExtensions
    {
        get { return this.validExtensions; }
    }

    public UrlFormatType UrlFormat
    {
        get { return this.urlFormat; }
    }

    /// <inheritdoc/>
    public override string FriendlyUrl(TabInfo tab, string path)
    {
        return this.providerInstance.FriendlyUrl(tab, path);
    }

    /// <inheritdoc/>
    public override string FriendlyUrl(TabInfo tab, string path, string pageName)
    {
        return this.providerInstance.FriendlyUrl(tab, path, pageName);
    }

    /// <inheritdoc/>
    public override string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings)
    {
        return this.providerInstance.FriendlyUrl(tab, path, pageName, settings);
    }

    /// <inheritdoc/>
    public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
    {
        return this.providerInstance.FriendlyUrl(tab, path, pageName, portalAlias);
    }
}
