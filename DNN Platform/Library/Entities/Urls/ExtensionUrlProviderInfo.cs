﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.ExtensionUrlProviderId = -1;
            this.Settings = new Dictionary<string, string>();
            this.TabIds = new List<int>();
        }

        /// <summary>
        /// When true, the module provider will be used for all tabs in the current portal.  Including a specific tabid switches value to false.
        /// </summary>
        public bool AllTabs { get { return this.TabIds.Count == 0; } }

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
            this.ExtensionUrlProviderId = Null.SetNullInteger(dr["ExtensionUrlProviderId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.DesktopModuleId = Null.SetNullInteger(dr["DesktopModuleId"]);
            this.ProviderName = Null.SetNullString(dr["ProviderName"]);
            this.ProviderType = Null.SetNullString(dr["ProviderType"]);
            this.SettingsControlSrc = Null.SetNullString(dr["SettingsControlSrc"]);
            this.IsActive = Null.SetNullBoolean(dr["IsActive"]);
            this.RewriteAllUrls = Null.SetNullBoolean(dr["RewriteAllUrls"]);
            this.RedirectAllUrls = Null.SetNullBoolean(dr["RedirectAllUrls"]);
            this.ReplaceAllUrls = Null.SetNullBoolean(dr["ReplaceAllUrls"]);
        }

    }
}
