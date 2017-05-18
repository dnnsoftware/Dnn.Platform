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