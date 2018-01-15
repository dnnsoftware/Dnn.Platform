#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Urls
{
    [Serializable]
    public class ExtensionUrlProviderInfo : IHydratable
    {
        public ExtensionUrlProviderInfo()
        {
            ExtensionUrlProviderId = -1;
            Settings = new Dictionary<string, string>();
            TabIds = new List<int>();
        }

        /// <summary>
        /// When true, the module provider will be used for all tabs in the current portal.  Including a specific tabid switches value to false.
        /// </summary>
        public bool AllTabs { get { return TabIds.Count == 0; } }

        /// <summary>
        /// The DesktopModuleId is used to associate a particular Extension Url Provider with a specific DotNetNuke extension.
        /// </summary>
        /// <remarks>
        /// If the Extension provider is not associated with any particular DotNetNuke extension, return null.
        /// </remarks>
        public int DesktopModuleId { get; set; }

        public int ExtensionUrlProviderId { get; set; }

        /// <summary>
        /// When true, provider is active
        /// </summary>
        public bool IsActive { get; set; }

        public int PortalId { get; set; }

        public string ProviderName { get; set; }

        public string ProviderType { get; set; }

        /// <summary>
        /// When true, TransformFriendlyUrl is called for every Url in the portal
        /// When false, TransformFriendlyUrl is called only for tabs in the TabIds list
        /// </summary>
        public bool RewriteAllUrls { get; set; }

        /// <summary>
        /// When true, CheckForRedirect is called for every Url in the portal
        /// When false, CheckForRedirect is called only for tabs in the TabIds list
        /// </summary>
        public bool RedirectAllUrls { get; set; }

        /// <summary>
        /// When true, ChangeFriendlyUrl is called for every generated Url called through the NavigateUrl API
        /// When false, ChangeFriendlyUrl is called only for tabs in the TabIds list
        /// </summary>
        public bool ReplaceAllUrls { get; set; }

        public string SettingsControlSrc { get; set; }

        public Dictionary<string, string> Settings { get; private set; }

        /// <summary>
        /// Returns a list of TabIds where the module provider should be called when generating friendly urls
        /// </summary>
        public List<int> TabIds { get; private set; }

        public int KeyID { get; set; }

        public void Fill(IDataReader dr)
        {
            ExtensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderId"]);
            PortalId = Null.SetNullInteger(dr["PortalId"]);
            DesktopModuleId = Null.SetNullInteger(dr["DesktopModuleId"]);
            ProviderName = Null.SetNullString(dr["ProviderName"]);
            ProviderType = Null.SetNullString(dr["ProviderType"]);
            SettingsControlSrc = Null.SetNullString(dr["SettingsControlSrc"]);
            IsActive = Null.SetNullBoolean(dr["IsActive"]);
            RewriteAllUrls = Null.SetNullBoolean(dr["RewriteAllUrls"]);
            RedirectAllUrls = Null.SetNullBoolean(dr["RedirectAllUrls"]);
            ReplaceAllUrls = Null.SetNullBoolean(dr["ReplaceAllUrls"]);
        }

    }
}
