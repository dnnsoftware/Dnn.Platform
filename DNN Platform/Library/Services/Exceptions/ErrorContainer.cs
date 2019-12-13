#region Usings

using System;
using System.Web;
using System.Web.UI;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class ErrorContainer : Control
    {
        public ErrorContainer(string strError)
        {
            Container = FormatException(strError);
        }

        public ErrorContainer(string strError, Exception exc)
        {
            Container = FormatException(strError, exc);
        }

        public ErrorContainer(PortalSettings _PortalSettings, string strError, Exception exc)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            if (objUserInfo.IsSuperUser)
            {
                Container = FormatException(strError, exc);
            }
            else
            {
                Container = FormatException(strError);
            }
        }

        public ModuleMessage Container { get; set; }

        private ModuleMessage FormatException(string strError)
        {
            ModuleMessage m;
            m = UI.Skins.Skin.GetModuleMessageControl(Localization.Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
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
                m = UI.Skins.Skin.GetModuleMessageControl(Localization.Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
            }
            return m;
        }
    }
}
