// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcWebsite.Controls
{
    using DotNetNuke.Entities.Portals;    
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

    /// <summary>
    /// Control that displays the portal's terms and conditions message.
    /// </summary>
    public class TermsControl : RazorModuleControlBase
    {
        /// <summary>
        /// Gets the name of the control.
        /// </summary>
        public override string ControlName => "Terms";

        /// <summary>
        /// Gets the path where the control is located.
        /// </summary>
        public override string ControlPath => "admin/Portal";

        /// <summary>
        /// Invokes the control and returns the terms and conditions message view.
        /// </summary>
        /// <returns>A razor module result containing the localized terms and conditions message.</returns>
        public override IRazorModuleResult Invoke()
        {
            return this.View(Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_PORTAL_TERMS"));
        }
    }
}
