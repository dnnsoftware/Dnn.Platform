<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Security.PasswordReset" AutoEventWireup="false" Codebehind="PasswordReset.ascx.cs" %>
<div class="dnnForm dnnPasswordReset dnnClear">
    <div class="dnnFormMessage dnnFormInfo" runat="server" Visible="False" id="resetMessages">
        <span><asp:Label ID="lblInfo" runat="Server" /></span>
        <span class="error"><asp:Label ID="lblHelp" runat="Server" /></span>
    </div>
    <asp:Panel id="divPassword" runat="server" class="dnnPasswordResetContent" DefaultButton="cmdChangePassword">
        <div class="dnnFormItem">
            <dnn:Label ID="lblUsername" runat="server" ControlName="txtUsername" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtUsername" runat="server" TextMode="SingleLine" />
            <asp:RequiredFieldValidator ID="valUsername" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" Display="Dynamic" ControlToValidate="txtPassword" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblPassword" runat="server" ControlName="txtPassword" ResourceKey="Password" CssClass="dnnFormRequired" />
            <asp:Panel ID="passwordContainer" runat="server" CssClass="password-strength-container">
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" size="12" MaxLength="20" AutoCompleteType="Disabled" />
            </asp:Panel>
            <asp:RequiredFieldValidator ID="valPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Password.Required" Display="Dynamic" ControlToValidate="txtPassword" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblConfirmPassword" runat="server" ControlName="txtConfirmPassword" ResourceKey="Confirm" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" />
            <asp:RequiredFieldValidator ID="valConfirmPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Confirm.Required" Display="Dynamic" ControlToValidate="txtConfirmPassword" />
        </div>
		<div id="divQA" runat="server" visible="false">
			<div class="dnnFormItem">
				<dnn:label id="plAnswer" runat="server" controlname="txtAnswer" CssClass="dnnFormRequired" />
				<asp:textbox id="txtAnswer" runat="server" size="25" maxlength="20" />
			</div>
		</div>
        <ul class="dnnActions dnnClear">
            <li>
                <asp:LinkButton ID="cmdChangePassword" CssClass="dnnPrimaryAction" runat="server" resourcekey="cmdChangePassword" /></li>
            <li id="liLogin" runat="server">
                <asp:HyperLink ID="hlCancel" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdCancel" /></li>
        </ul>
    </asp:Panel>
</div>
