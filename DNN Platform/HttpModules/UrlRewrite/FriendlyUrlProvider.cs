// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework.Providers;
using DotNetNuke.HttpModules.UrlRewrite;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.Url.FriendlyUrl
// ReSharper restore CheckNamespace
{
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
            //Read the configuration specific information for this provider
            var objProvider = (Provider)_providerConfiguration.Providers[ProviderName];

            if (!String.IsNullOrEmpty(objProvider.Attributes["urlFormat"]))
            {
                switch (objProvider.Attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        _urlFormat = UrlFormatType.SearchFriendly;
                        break;
                    case "humanfriendly":
                        _urlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        _urlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        _urlFormat = UrlFormatType.SearchFriendly;
                        break;
                }
            }
            //instance the correct provider implementation
            switch (_urlFormat)
            {
                case UrlFormatType.Advanced:
                    _providerInstance = new AdvancedFriendlyUrlProvider(objProvider.Attributes);
                    break;
                case UrlFormatType.HumanFriendly:
                case UrlFormatType.SearchFriendly:
                    _providerInstance = new BasicFriendlyUrlProvider(objProvider.Attributes);
                    break;
            }

            string extensions = !String.IsNullOrEmpty(objProvider.Attributes["validExtensions"]) ? objProvider.Attributes["validExtensions"] : ".aspx";
            _validExtensions = extensions.Split(',');
        }

        public string[] ValidExtensions
        {
            get { return _validExtensions; }
        }

        public UrlFormatType UrlFormat
        {
            get { return _urlFormat; }
        }

        public override string FriendlyUrl(TabInfo tab, string path)
        {
            return _providerInstance.FriendlyUrl(tab, path);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return _providerInstance.FriendlyUrl(tab, path, pageName);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings)
        {
            return _providerInstance.FriendlyUrl(tab, path, pageName, settings);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return _providerInstance.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}
