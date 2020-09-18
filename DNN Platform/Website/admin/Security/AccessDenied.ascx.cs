// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Web;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    public partial class AccessDeniedPage : PortalModuleBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string message = null;
            Guid messageGuid;
            var guidText = this.Request.QueryString["message"];
            if (!string.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out messageGuid))
            {
                message = HttpUtility.HtmlEncode(DataProvider.Instance().GetRedirectMessage(messageGuid));
            }

            UI.Skins.Skin.AddModuleMessage(
                this,
                !string.IsNullOrEmpty(message) ? message : Localization.GetString("AccessDenied", this.LocalResourceFile),
                ModuleMessage.ModuleMessageType.YellowWarning);
        }
    }
}
