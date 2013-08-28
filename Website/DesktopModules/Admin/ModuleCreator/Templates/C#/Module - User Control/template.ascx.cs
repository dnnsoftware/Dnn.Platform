#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;

#endregion

namespace [OWNER].[MODULE]
{

	public partial class [CONTROL] : PortalModuleBase
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
                    ModuleController controller = new ModuleController();
                    controller.UpdateModuleSetting(ModuleId, "field", txtField.Text);
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Update Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
		}


		protected void cmdCancel_Click(object sender, EventArgs e)
		{
		}

		#endregion

	}
}
