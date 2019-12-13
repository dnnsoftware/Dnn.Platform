// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.UserControls;

#endregion

namespace DotNetNuke.Modules.Admin.Authentication
{

	/// <summary>
	/// Manages the Authentication settings
	/// </summary>
	/// <remarks>
	/// </remarks>
	public partial class Authentication : PortalModuleBase
	{

		private readonly List<AuthenticationSettingsBase> _settingControls = new List<AuthenticationSettingsBase>();

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			cmdUpdate.Click += OnUpdateClick;

			var authSystems = AuthenticationController.GetEnabledAuthenticationServices();

			foreach (var authSystem in authSystems)
			{
				//Add a Section Header
				var sectionHeadControl = (SectionHeadControl) LoadControl("~/controls/SectionHeadControl.ascx");
				sectionHeadControl.IncludeRule = true;
				sectionHeadControl.CssClass = "Head";

				//Create a <div> to hold the control
				var container = new HtmlGenericControl();
				container.ID = authSystem.AuthenticationType;

				var authSettingsControl = (AuthenticationSettingsBase) LoadControl("~/" + authSystem.SettingsControlSrc);

				//set the control ID to the resource file name ( ie. controlname.ascx = controlname )
				//this is necessary for the Localization in PageBase
				authSettingsControl.ID = Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc) + "_" + authSystem.AuthenticationType;

				//Add Settings Control to Container
				container.Controls.Add(authSettingsControl);
				_settingControls.Add(authSettingsControl);

				//Add Section Head Control to Container
				pnlSettings.Controls.Add(sectionHeadControl);

				//Add Container to Controls
				pnlSettings.Controls.Add(container);

				//Attach Settings Control's container to Section Head Control
				sectionHeadControl.Section = container.ID;

				//Get Section Head Text from the setting controls LocalResourceFile
				authSettingsControl.LocalResourceFile = authSettingsControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
														Path.GetFileNameWithoutExtension(authSystem.SettingsControlSrc);
				sectionHeadControl.Text = Localization.GetString("Title", authSettingsControl.LocalResourceFile);
				pnlSettings.Controls.Add(new LiteralControl("<br/>"));
				cmdUpdate.Visible = IsEditable;
			}
		}

		protected void OnUpdateClick(object sender, EventArgs e)
		{
			foreach (var settingControl in _settingControls)
			{
				settingControl.UpdateSettings();
			}
			
			//Validate Enabled
			var enabled = false;
			var authSystems = AuthenticationController.GetEnabledAuthenticationServices();
			foreach (var authSystem in authSystems)
			{
				var authLoginControl = (AuthenticationLoginBase) LoadControl("~/" + authSystem.LoginControlSrc);

				//Check if AuthSystem is Enabled
				if (authLoginControl.Enabled)
				{
					enabled = true;
					break;
				}
			}
			if (!enabled)
			{
				//Display warning
				UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoProvidersEnabled", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
			}
		}

	}
}
