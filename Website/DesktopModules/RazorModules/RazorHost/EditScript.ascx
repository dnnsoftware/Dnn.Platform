<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="EditScript.ascx.cs" Inherits="DotNetNuke.Modules.RazorHost.EditScript" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register Assembly="DotnetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<dnn:DnnToolTipManager ID="toolTipManager" runat="server" Position="Center" RelativeTo="BrowserWindow" Width="500px" Height="200px" HideEvent="ManualClose" ShowEvent="OnClick" Modal="true" Skin="Default" RenderInPageRoot="true" AnimationDuration="200" ManualClose="true"
	ManualCloseButtonText="Close" />
<div class="dnnForm dnnRazorHostEditScript dnnClear" id="dnnEditScript">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label id="scriptsLabel" runat="Server" controlname="scriptList" />
            <asp:DropDownList ID="scriptList" runat="server" AutoPostBack="true" />
			<asp:linkbutton ID="cmdAdd" runat="server" ResourceKey="AddNew" cssclass="dnnSecondaryAction" />
        </div>
		<div class="dnnFormItem razorHostModSource"><asp:Label ID="lblSourceFile" runat="server" /></div>        
        <div class="dnnFormItem">
            <dnn:label id="plSource" controlname="txtSource" runat="server" />
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="16" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label id="currentScriptLabel" runat="Server" controlname="isCurrentScript" />
            <asp:CheckBox ID="isCurrentScript" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:linkbutton id="cmdSave" resourcekey="cmdSave" runat="server" cssclass="dnnPrimaryAction"/></li>
        <li><asp:linkbutton id="cmdSaveClose" resourcekey="cmdSaveClose" runat="server" cssclass="dnnSecondaryAction" /></li>
        <li><asp:linkbutton id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>
    </ul>
</div>
