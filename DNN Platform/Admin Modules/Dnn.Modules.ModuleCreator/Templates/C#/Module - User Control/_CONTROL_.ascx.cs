// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Using Statements

using System;
using DotNetNuke.Entities.Modules;

#endregion

namespace _OWNER_._MODULE_
{

	public partial class _CONTROL_ : PortalModuleBase
	{

		#region Event Handlers

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			cmdSave.Click += cmdSave_Click;
			cmdCancel.Click += cmdCancel_Click;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			
			if (!Page.IsPostBack)
			{
                             txtField.Text = (string)Settings["field"];
			}
		}
		
		protected void cmdSave_Click(object sender, EventArgs e)
		{
            ModuleController.Instance.UpdateModuleSetting(ModuleId, "field", txtField.Text);
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Update Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
		}


		protected void cmdCancel_Click(object sender, EventArgs e)
		{
		}

		#endregion

	}
}
