// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.MvcPipeline.ModuleControl;

namespace DotNetNuke.Web.MvcWebsite.Controls
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    public class TermsControl : RazorModuleControlBase
    {
        public override object ViewModel()
        {
            return Localization.GetSystemMessage(PortalSettings.Current, "MESSAGE_PORTAL_TERMS");
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
