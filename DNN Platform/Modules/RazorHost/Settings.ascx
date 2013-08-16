<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Modules.RazorHost.Settings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnFormItem">
	<dnn:label id="scriptListLabel" controlname="scriptList" runat="server" />
    <asp:DropDownList ID="scriptList" runat="server" />
</div>