// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.Url.FriendlyUrl

// ReSharper restore CheckNamespace
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.HttpModules.UrlRewrite;

    public class DNNFriendlyUrlProvider : FriendlyUrlProvider
    {
        internal const string ProviderName = "DNNFriendlyUrl";
        internal const string ProviderType = "friendlyUrl";

        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);

        private readonly FriendlyUrlProviderBase _providerInstance;
        private readonly UrlFormatType _urlFormat = UrlFormatType.SearchFriendly;
        private readonly string[] _validExtensions;

        public DNNFriendlyUrlProvider()
        {
            // Read the configuration specific information for this provider
            var objProvider = (Provider)this._providerConfiguration.Providers[ProviderName];

            if (!string.IsNullOrEmpty(objProvider.Attributes["urlFormat"]))
            {
                switch (objProvider.Attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        this._urlFormat = UrlFormatType.SearchFriendly;
                        break;
                    case "humanfriendly":
                        this._urlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        this._urlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        this._urlFormat = UrlFormatType.SearchFriendly;
                        break;
                }
            }

            // instance the correct provider implementation
            switch (this._urlFormat)
            {
                case UrlFormatType.Advanced:
                    this._providerInstance = new AdvancedFriendlyUrlProvider(objProvider.Attributes);
                    break;
                case UrlFormatType.HumanFriendly:
                case UrlFormatType.SearchFriendly:
                    this._providerInstance = new BasicFriendlyUrlProvider(objProvider.Attributes);
                    break;
            }

            string extensions = !string.IsNullOrEmpty(objProvider.Attributes["validExtensions"]) ? objProvider.Attributes["validExtensions"] : ".aspx";
            this._validExtensions = extensions.Split(',');
        }

        public string[] ValidExtensions
        {
            get { return this._validExtensions; }
        }

        public UrlFormatType UrlFormat
        {
            get { return this._urlFormat; }
        }

        public override string FriendlyUrl(TabInfo tab, string path)
        {
            return this._providerInstance.FriendlyUrl(tab, path);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return this._providerInstance.FriendlyUrl(tab, path, pageName);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings)
        {
            return this._providerInstance.FriendlyUrl(tab, path, pageName, settings);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return this._providerInstance.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
