﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Entities.Portals;

    public abstract class SitemapProvider
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Enabled
        {
            get
            {
                return bool.Parse(PortalController.GetPortalSetting(this.Name + "Enabled", PortalController.Instance.GetCurrentPortalSettings().PortalId, "True"));
            }

            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, this.Name + "Enabled", value.ToString());
            }
        }

        public bool OverridePriority
        {
            get
            {
                return bool.Parse(PortalController.GetPortalSetting(this.Name + "Override", PortalController.Instance.GetCurrentPortalSettings().PortalId, "False"));
            }

            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, this.Name + "Override", value.ToString());
            }
        }

        public float Priority
        {
            get
            {
                float value = 0;
                if (this.OverridePriority)
                {
                    // stored as an integer (pr * 100) to prevent from translating errors with the decimal point
                    value = float.Parse(PortalController.GetPortalSetting(this.Name + "Value", PortalController.Instance.GetCurrentPortalSettings().PortalId, "0.5"), NumberFormatInfo.InvariantInfo);
                }

                return value;
            }

            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, this.Name + "Value", value.ToString(NumberFormatInfo.InvariantInfo));
            }
        }

        /// <summary>Includes page urls on the sitemap.</summary>
        /// <remarks>
        ///   Pages that are included:
        ///   <list type="bullet">
        ///     <item><description>are not deleted</description></item>
        ///     <item><description>are not disabled</description></item>
        ///     <item><description>are normal pages (not links, …)</description></item>
        ///     <item><description>are visible (based on date and permissions)</description></item>
        ///   </list>
        /// </remarks>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="ps">The portal settings.</param>
        /// <param name="version">The version number.</param>
        /// <returns>A <see cref="List"/> of <see cref="SitemapUrl"/> instances.</returns>
        public abstract List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version);
    }
}
