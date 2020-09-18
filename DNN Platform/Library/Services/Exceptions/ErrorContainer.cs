// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.Skins.Controls;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class ErrorContainer : Control
    {
        public ErrorContainer(string strError)
        {
            this.Container = this.FormatException(strError);
        }

        public ErrorContainer(string strError, Exception exc)
        {
            this.Container = this.FormatException(strError, exc);
        }

        public ErrorContainer(PortalSettings _PortalSettings, string strError, Exception exc)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            if (objUserInfo.IsSuperUser)
            {
                this.Container = this.FormatException(strError, exc);
            }
            else
            {
                this.Container = this.FormatException(strError);
            }
        }

        public ModuleMessage Container { get; set; }

        private ModuleMessage FormatException(string strError)
        {
            ModuleMessage m;
            m = UI.Skins.Skin.GetModuleMessageControl(Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
            return m;
        }

        private ModuleMessage FormatException(string strError, Exception exc)
        {
            ModuleMessage m;
            if (exc != null)
            {
                m = UI.Skins.Skin.GetModuleMessageControl(strError, HttpUtility.HtmlEncode(exc.ToString()), ModuleMessage.ModuleMessageType.RedError);
            }
            else
            {
                m = UI.Skins.Skin.GetModuleMessageControl(Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
            }

            return m;
        }
    }
}
