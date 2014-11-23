#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Globalization;
using System.Linq;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI.WebControls;

using DataCache = DotNetNuke.UI.Utilities.DataCache;

#endregion

namespace DesktopModules.Admin.Security
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserSettings PortalModuleBase is used to manage User Settings for the portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class UserSettings : ModuleSettingsBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

			if (PortalController.IsMemberOfPortalGroup(PortalId) && PortalController.GetEffectivePortalId(PortalId) != PortalId)
			{
				dnnUserSettings.Visible = false;
				CannotChangeSettingsMessage.Visible = true;
			}

            manageServiceItem.Visible = usersControl.Visible = !IsHostMenu;
        }

        public override void LoadSettings()
        {
            displayMode.EnumType = "DotNetNuke.Entities.Modules.DisplayMode, DotNetNuke";
            usersControl.EnumType = "DotNetNuke.Entities.Modules.UsersControl, DotNetNuke";

            settingsEditor.DataSource = UserController.GetUserSettings(PortalId);
            settingsEditor.DataBind();

        }

        public override void UpdateSettings()
        {
            foreach (DnnFormItemBase item in settingsEditor.Items)
            {
                PortalController.UpdatePortalSetting(PortalId, item.DataField,
                                                        item.Value.GetType().IsEnum
                                                            ? Convert.ToInt32(item.Value).ToString(CultureInfo.InvariantCulture)
                                                            : item.Value.ToString()
                                                        );
            }

            //Clear the UserSettings Cache
            DataCache.RemoveCache(UserController.SettingsKey(PortalId));
        }
    }
}