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