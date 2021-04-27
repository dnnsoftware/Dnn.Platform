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
            <asp:TextBox ID="txtUsername" runat="server" TextMode="SingleLine" />
            <asp:RequiredFieldValidator ID="valUsername" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" Display="Dynamic" ControlToValidate="txtUsername" />
        </div>
        <div class="dnnFormItem">
            <asp:Panel ID="passwordContainer" runat="server">
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" size="12" MaxLength="39" AutoCompleteType="Disabled" />
            </asp:Panel>
            <asp:RegularExpressionValidator ID="valPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Password.Required" Display="Dynamic" ControlToValidate="txtPassword" ValidationExpression="[\w\W]{7,}" />
        </div>
        <div class="dnnFormItem">
            <asp:TextBox ID="txtConfirmPassword" runat="server" MaxLength="39" TextMode="Password" CssClass="password-confirm" />
            <asp:RequiredFieldValidator ID="valConfirmPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Confirm.Required" Display="Dynamic" ControlToValidate="txtConfirmPassword" />
        </div>
		<div id="divQA" runat="server" visible="false">
		    <div class="dnnFormItem">
				<asp:label id = "lblQuestion" runat="server" />
			</div>
			<div class="dnnFormItem">
				<asp:textbox id="txtAnswer" runat="server" />
                <asp:RequiredFieldValidator ID="valAnswer" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" Display="Dynamic" resourcekey="Answer.Required" ControlToValidate="txtAnswer" />
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
