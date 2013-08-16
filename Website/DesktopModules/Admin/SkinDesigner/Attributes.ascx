<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Skins.Attributes" CodeFile="Attributes.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnEditSkins dnnClear" id="dnnEditSkins">
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">
            <dnn:label id="plSkin" runat="server" controlname="cboSkins" />
             <dnn:DnnComboBox ID="cboSkins" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plContainer" runat="server" controlname="cboContainers" />
            <dnn:DnnComboBox ID="cboContainers" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plFile" runat="server" controlname="cboFiles" />
		    <dnn:DnnComboBox ID="cboFiles" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plToken" runat="server" controlname="cboTokens" />
		    <dnn:DnnComboBox ID="cboTokens" runat="server" DataTextField="ControlKey" DataValueField="ControlSrc" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plSetting" runat="server" controlname="cboSettings" />
		    <dnn:DnnComboBox ID="cboSettings" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plValue" runat="server" controlname="cboValue" />
		    <asp:TextBox ID="txtValue" runat="server" Visible="false" />
		    <dnn:DnnComboBox ID="cboValue" runat="server" Visible="false" />
		    <div style="margin-left: 30%;>
                <asp:Label ID="lblHelp" runat="server" />
            </div>
        </div>

    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    </ul>
</div>