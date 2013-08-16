<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.AuthenticationEditor" CodeFile="AuthenticationEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("AuthenticationSettings")%></a></h2>
<fieldset>
    <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server" /></div>
    <dnn:DnnFormEditor id="authenticationForm" runat="Server" FormMode="Short">
        <Items>
            <dnn:DnnFormTextBoxItem ID="authenticationType" runat="server" DataField = "AuthenticationType" />
            <dnn:DnnFormTextBoxItem ID="loginControlSrc" runat="server" DataField = "LoginControlSrc" />
            <dnn:DnnFormTextBoxItem ID="logoffControlSrc" runat="server" DataField = "LogoffControlSrc"  />
            <dnn:DnnFormTextBoxItem ID="settingsControlSrc" runat="server" DataField = "SettingsControlSrc" />
            <dnn:DnnFormToggleButtonItem ID="isEnabled" runat="server" DataField = "IsEnabled"/>
        </Items>
    </dnn:DnnFormEditor>
    <dnn:DnnFormEditor id="authenticationFormReadOnly" runat="Server" FormMode="Short">
        <Items>
            <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem1" runat="server" DataField = "AuthenticationType" />
            <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem2" runat="server" DataField = "LoginControlSrc" />
            <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem3" runat="server" DataField = "LogoffControlSrc"  />
            <dnn:DnnFormLiteralItem ID="DnnFormTextBoxItem4" runat="server" DataField = "SettingsControlSrc" />
            <dnn:DnnFormLiteralItem ID="DnnFormToggleButtonItem1" runat="server" DataField = "IsEnabled"/>
        </Items>
    </dnn:DnnFormEditor>
    <asp:Panel ID="pnlSettings" runat="server" Visible="false" CssClass="dnnAuthenticationSettings">
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        </ul>
    </asp:Panel>
</fieldset>