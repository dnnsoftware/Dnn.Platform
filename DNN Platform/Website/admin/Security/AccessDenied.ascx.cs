// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Security
{
    public partial class AccessDeniedPage : PortalModuleBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string message = null;
            Guid messageGuid;
            var guidText = Request.QueryString["message"];
            if (!string.IsNullOrEmpty(guidText) && Guid.TryParse(guidText, out messageGuid))
            {
                message = HttpUtility.HtmlEncode(DataProvider.Instance().GetRedirectMessage(messageGuid));
            }

            UI.Skins.Skin.AddModuleMessage(this,
                !string.IsNullOrEmpty(message) ? message : Localization.GetString("AccessDenied", LocalResourceFile),
                ModuleMessage.ModuleMessageType.YellowWarning);
        }
    }
}
