<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.Password" Codebehind="Password.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<div class="dnnForm dnnPassword dnnClear" id="dnnPassword">
    <fieldset>
		<asp:Panel runat="server" ID="CannotChangePasswordMessage" CssClass="dnnFormMessage dnnFormWarning" Visible="False"><%=LocalizeString("CannotChangePassword") %></asp:Panel>
		<asp:panel id="pnlChange" runat="server">
		    <h2 class="dnnFormSectionHead"><asp:label id="lblChangePasswordHeading" runat="server" resourceKey="ChangePassword" /></h2>
            <div class="dnnFormItem"><p><asp:label id="lblChangeHelp" runat="server" ViewStateMode="Disabled" /></p></div>
            <div id="oldPasswordRow" runat="server" class="dnnFormItem">
                <dnn:label id="plOldPassword" runat="server" controlname="txtOldPassword" />
                <asp:textbox id="txtOldPassword" runat="server" textmode="Password" size="25" maxlength="39" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plNewPassword" runat="server" controlname="txtNewPassword" />
                <asp:Panel ID="passwordContainer" runat="server" ViewStateMode="Disabled">
                    <asp:textbox id="txtNewPassword" runat="server" textmode="Password" size="25" maxlength="39" />
                </asp:Panel>
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plNewConfirm" runat="server" controlname="txtNewConfirm" />
                <asp:textbox id="txtNewConfirm" runat="server" textmode="Password" size="25" maxlength="39" CssClass="password-confirm" />
            </div>
            <div id="captchaRow" runat="server" visible="false" class="dnnFormItem dnnCaptcha">
                <dnn:label id="captchaLabel" controlname="ctlCaptcha" runat="server" />
                <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" ErrorStyle-CssClass="dnnFormMessage dnnFormError dnnCaptcha" runat="server" />
            </div>
            <div class="dnnClear"></div>
            <ul class="dnnActions dnnClear">
                 <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="ChangePassword" /></li>
            </ul>       
		</asp:panel>
       	<asp:panel id="pnlReset" runat="server" ViewStateMode="Disabled">
            <h2 class="dnnFormSectionHead"><asp:label id="lblResetHeading" runat="server" resourceKey="ResetPassword" /></h2>
            <div class="dnnFormItem"><asp:label id="lblResetHelp" runat="server"></asp:label></div>
            <div id="questionRow" runat="server" class="dnnFormItem">
                <dnn:label id="plQuestion" runat="server" controlname="lblQuestion" />
                <asp:label id = "lblQuestion" runat="server" />
            </div>
            <div id="answerRow" runat="server" class="dnnFormItem">
                <dnn:label id="plAnswer" runat="server" controlname="txtAnswer" />
                <asp:textbox id="txtAnswer" runat="server" size="25" maxlength="20" />
            </div>
		</asp:panel>
		<asp:panel id="pnlQA" runat="server" ViewStateMode="Disabled">
            <div class="dnnFormItem"><asp:label id="lblChangeQA" runat="server" resourceKey="ChangeQA" /></div>
            <div class="dnnFormItem"><asp:label id="lblQAHelp" resourcekey="QAHelp" cssclass="Normal" runat="server" /></div>
            <div class="dnnFormItem">
                <dnn:label id="plQAPassword" runat="server" controlname="txtQAPassword" />
                <asp:textbox id="txtQAPassword" runat="server" textmode="Password" size="25" maxlength="20" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEditQuestion" runat="server" controlname="txtEditQuestion" />
                <asp:textbox id="txtEditQuestion" runat="server" size="25" maxlength="20" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEditAnswer" runat="server" controlname="txtEditAnswer" />
                <asp:textbox id="txtEditAnswer" runat="server" size="25" maxlength="20" />
            </div>
		</asp:panel>
        <div class="dnnFormItem">
            <dnn:label id="plLastChanged" runat="server" controlname="lblLastChanged" />
            <asp:label id = "lblLastChanged" runat="server" ViewStateMode="Disabled" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plExpires" runat="server" controlname="lblExpires" />
            <asp:label id = "lblExpires" runat="server" ViewStateMode="Disabled"/>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdReset" runat="server" CssClass="dnnPrimaryAction" resourcekey="ResetPassword" ViewStateMode="Disabled" /></li>
        <li><asp:LinkButton id="cmdUserReset" runat="server" CssClass="dnnPrimaryAction" resourcekey="ResetPassword" Visible="False" ViewStateMode="Disabled" /></li>
        <li><asp:LinkButton id="cmdUpdateQA" runat="server" CssClass="dnnSecondaryAction" resourcekey="SaveQA" ViewStateMode="Disabled"/></li>
    </ul>  
</div>