// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Url.FriendlyUrl
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Framework.Providers;

    /// <summary>The friendly URL provider.</summary>
    public class DNNFriendlyUrlProvider : FriendlyUrlProvider
    {
        /// <summary>The provider name.</summary>
        internal const string ProviderName = "DNNFriendlyUrl";

        /// <summary>The provider type.</summary>
        internal const string ProviderType = "friendlyUrl";

        private readonly ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly FriendlyUrlProviderBase providerInstance;

        /// <summary>Initializes a new instance of the <see cref="DNNFriendlyUrlProvider"/> class.</summary>
        public DNNFriendlyUrlProvider()
        {
            // Read the configuration specific information for this provider
            var objProvider = (Provider)this.providerConfiguration.Providers[ProviderName];

            if (!string.IsNullOrEmpty(objProvider.Attributes["urlFormat"]))
            {
                switch (objProvider.Attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        this.UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                    case "humanfriendly":
                        this.UrlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        this.UrlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        this.UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                }
            }

            // instance the correct provider implementation
            this.providerInstance = this.UrlFormat switch
            {
                UrlFormatType.Advanced => new AdvancedFriendlyUrlProvider(objProvider.Attributes),
                UrlFormatType.HumanFriendly or UrlFormatType.SearchFriendly => new BasicFriendlyUrlProvider(objProvider.Attributes),
                _ => this.providerInstance,
            };

            string extensions = !string.IsNullOrEmpty(objProvider.Attributes["validExtensions"]) ? objProvider.Attributes["validExtensions"] : ".aspx";
            this.ValidExtensions = extensions.Split(',');
        }

        /// <summary>Gets the valid friendly URL extensions.</summary>
        public string[] ValidExtensions { get; private set; }

        /// <summary>Gets the URL format.</summary>
        public UrlFormatType UrlFormat { get; private set; } = UrlFormatType.SearchFriendly;

        /// <inheritdoc/>
        public override string FriendlyUrl(TabInfo tab, string path) => this.providerInstance.FriendlyUrl(tab, path);

        /// <inheritdoc/>
        public override string FriendlyUrl(TabInfo tab, string path, string pageName) => this.providerInstance.FriendlyUrl(tab, path, pageName);

        /// <inheritdoc/>
        public override string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings) => this.providerInstance.FriendlyUrl(tab, path, pageName, settings);

        /// <inheritdoc/>
        public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias) => this.providerInstance.FriendlyUrl(tab, path, pageName, portalAlias);
    }
}
