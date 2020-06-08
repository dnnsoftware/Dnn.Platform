// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Collections.Generic;
using System.Globalization;

using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.Sitemap
{
    public abstract class SitemapProvider
    {
        public string Name { get; set; }

        public string Description { get; set; }


        public bool Enabled
        {
            get
            {
                return bool.Parse(PortalController.GetPortalSetting(Name + "Enabled", PortalController.Instance.GetCurrentPortalSettings().PortalId, "True"));
            }
            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, Name + "Enabled", value.ToString());
            }
        }


        public bool OverridePriority
        {
            get
            {
                return bool.Parse(PortalController.GetPortalSetting(Name + "Override", PortalController.Instance.GetCurrentPortalSettings().PortalId, "False"));
            }
            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, Name + "Override", value.ToString());
            }
        }

        public float Priority
        {
            get
            {
                float value = 0;
                if ((OverridePriority))
                {
                    // stored as an integer (pr * 100) to prevent from translating errors with the decimal point
                    value = float.Parse(PortalController.GetPortalSetting(Name + "Value", PortalController.Instance.GetCurrentPortalSettings().PortalId, "0.5"), NumberFormatInfo.InvariantInfo);
                }
                return value;
            }

            set
            {
                PortalController.UpdatePortalSetting(PortalController.Instance.GetCurrentPortalSettings().PortalId, Name + "Value", value.ToString(NumberFormatInfo.InvariantInfo));
            }
        }


        public abstract List<SitemapUrl> GetUrls(int portalId, PortalSettings ps, string version);
    }
}
