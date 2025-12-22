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
    /// Control that displays the portal's privacy policy message.
    /// </summary>
    public class PrivacyControl : RazorModuleControlBase
    {
        /// <summary>
        /// Gets the name of the control.
        /// </summary>
        public override string ControlName => "Privacy";

        /// <summary>
        /// Gets the path where the control is located.
        /// </summary>
        public override string ControlPath => "admin/Portal";

        /// <summary>
        /// Invokes the control and returns the privacy policy message view.
        /// </summary>
        /// <returns>A razor module result containing the localized privacy policy message.</returns>
        public override IRazorModuleResult Invoke()
        {
            return this.View(Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_PORTAL_PRIVACY"));
        }
    }
}
