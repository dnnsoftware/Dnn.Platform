<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Security.PasswordReset" AutoEventWireup="false" CodeFile="PasswordReset.ascx.cs" %>
<div class="dnnForm dnnPasswordReset dnnClear">
    <div class="dnnFormMessage dnnFormInfo" runat="server" Visible="False" id="resetMessages"><asp:Label ID="lblHelp" runat="Server" /></div>
	<div id="divPassword" runat="server" class="dnnPasswordResetContent">
        <div class="dnnFormItem">
                            <dnn:Label ID="lblUsername" runat="server" ControlName="txtUsername" ResourceKey="Username" CssClass="dnnFormRequired"/>
                            <asp:TextBox ID="txtUsername" runat="server" TextMode="SingleLine"/>
                            <asp:RequiredFieldValidator ID="valUsername" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Username.Required" Display="Dynamic" ControlToValidate="txtPassword" />
                        </div>
		<div class="dnnFormItem">
                            <dnn:Label ID="lblPassword" runat="server" ControlName="txtPassword" ResourceKey="Password" CssClass="dnnFormRequired"/>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"/>
                            <asp:RequiredFieldValidator ID="valPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Password.Required" Display="Dynamic" ControlToValidate="txtPassword" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblConfirmPassword" runat="server" ControlName="txtConfirmPassword" ResourceKey="Confirm" CssClass="dnnFormRequired" />
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="valConfirmPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Confirm.Required" Display="Dynamic" ControlToValidate="txtConfirmPassword" />
                        </div>
		<ul class="dnnActions dnnClear">
			<li><asp:LinkButton id="cmdChangePassword" cssclass="dnnPrimaryAction" runat="server" resourcekey="cmdChangePassword" /></li>
			<li id="liLogin" runat="server"><asp:HyperLink ID="hlCancel" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdCancel" /></li>
		</ul>
	</div>
</div>
