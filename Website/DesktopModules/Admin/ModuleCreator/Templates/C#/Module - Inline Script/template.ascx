<%@ Control Language="C#" ClassName="[OWNER].[MODULE].[CONTROL]" Inherits="DotNetNuke.Entities.Modules.PortalModuleBase" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="DotNetNuke.Entities.Modules" %>

<script runat="server">


	#region Copyright

	// 
	// Copyright (c) [YEAR]
	// by [OWNER]
	// 

	#endregion

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

</script>


<div class="dnnForm dnnEdit dnnClear" id="dnnEdit">

    <fieldset>

        <div class="dnnFormItem">

            <dnn:label id="plField" runat="server" text="Field" helptext="Enter a value" controlname="txtField" />

            <asp:textbox id="txtField" runat="server" maxlength="255" />

        </div>

   </fieldset>

    <ul class="dnnActions dnnClear">

        <li><asp:linkbutton id="cmdSave" text="Save" runat="server" cssclass="dnnPrimaryAction" /></li>

        <li><asp:linkbutton id="cmdCancel" text="Cancel" runat="server" cssclass="dnnSecondaryAction" /></li>

    </ul>

</div>

