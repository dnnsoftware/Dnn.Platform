// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Controls
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;

    public class PrivacyControl : RazorModuleControlBase
    {
        public override object ViewModel()
        {
            return Localization.GetSystemMessage(PortalSettings.Current, "MESSAGE_PORTAL_PRIVACY");
        }

        public override string ViewName
        {
            get
            {
                return "Terms";
            }
        }
    }
}
