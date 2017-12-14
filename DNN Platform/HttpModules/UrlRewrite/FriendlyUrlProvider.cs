#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;

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
                switch (objProvider.Attributes["urlFormat"].ToLower())
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

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return _providerInstance.FriendlyUrl(tab, path, pageName, settings);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return _providerInstance.FriendlyUrl(tab, path, pageName, portalAlias);
        }
    }
}