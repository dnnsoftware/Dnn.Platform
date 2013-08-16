<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Authentication.Login" CodeFile="Login.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Profile" Src="~/DesktopModules/Admin/Security/Profile.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Password" Src="~/DesktopModules/Admin/Security/Password.ascx" %>
<%@ Register TagPrefix="dnn" TagName="User" Src="~/DesktopModules/Admin/Security/User.ascx" %>
<div class="dnnForm dnnLogin dnnClear">
    <asp:panel id="pnlLogin" runat="server" Visible="false">
        <div class="loginContent">
            <dnn:DNNTabStrip ID="tsLogin" runat="server" TabRenderMode="All" CssTabContainer="LoginTabGroup" CssContentContainer="LoginContainerGroup" DefaultContainerCssClass="LoginContainer" DefaultLabel-CssClass="LoginTab" DefaultLabel-CssClassHover="LoginTabHover" DefaultLabel-CssClassSelected="LoginTabSelected" visible="false" />
            <asp:Panel ID="pnlLoginContainer" runat="server" CssClass="LoginPanel" Visible="false" />
            <div class="dnnSocialRegistration">
                <div id="socialControls">
                    <ul class="buttonList">
                        <asp:PlaceHolder ID="socialLoginControls" runat="server"/>
                    </ul>
                </div>
            </div>
        </div>
    </asp:panel>
    <asp:Panel ID="pnlAssociate" runat="Server" Visible="false">
        <div class="associateContent">
            <h2><asp:label id="lblAuthenticatedTitle" runat="server" resourcekey="AuthenticatedTitle" /></h2>
            <div>
                <asp:label id="lblAuthenticatedHelp" runat="server" resourcekey="AuthenticatedHelp" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plType" controlname="lblType" runat="server" />
                <asp:label id="lblType" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plToken" controlname="lblToken" runat="server" />
                <asp:label id="lblToken" runat="server" />
            </div>
            <div class="dnnFormItem">
                <asp:label id="lblAssociateTitle" runat="server" resourcekey="AssociateTitle" />
                <asp:label id="lblAssociateHelp" runat="server" resourcekey="AssociateHelp" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plUsername" controlname="txtUsername" runat="server" resourcekey="Username" />
                <asp:textbox id="txtUsername" runat="server" />
            </div>
            <div class="dnnFormItem" id="divCaptcha" runat="server">
                <dnn:label id="plCaptcha" controlname="ctlCaptcha" runat="server" resourcekey="Captcha" />
                <dnn:captchacontrol id="ctlCaptcha" captchawidth="120" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plPassword" controlname="txtPassword" runat="server" resourcekey="Password" />
                <asp:textbox id="txtPassword" textmode="password" runat="server" />
            </div>
            <ul class="dnnActions dnnClear">
                <li><asp:LinkButton id="cmdAssociate" resourcekey="cmdAssociate" cssclass="dnnSecondaryAction" runat="server" CausesValidation="false" /></li>
            </ul>
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlRegister" runat="Server" Visible="false">
        <div class="registerContent">
            <h2><asp:label id="lblRegisterTitle" runat="server" resourcekey="RegisterTitle" /></h2>
            <div class="dnnFormMessage dnnFormHelpContent">
                <asp:label id="lblRegisterHelp" runat="server" />
            </div>
            <div>
                <dnn:user id="ctlUser" runat="Server" />
            </div>
            <ul class="dnnActions dnnClear">
                <li><asp:LinkButton id="cmdCreateUser" runat="server" CssClass="dnnSecondaryAction" /></li>
            </ul>
        </div>
    </asp:Panel>
    <asp:panel id="pnlPassword" runat="server" visible="false">
        <dnn:password id="ctlPassword" runat="server" />
        <asp:panel ID="pnlProceed" runat="Server" Visible="false">
            <dnn:commandbutton cssClass="dnnSecondaryAction" id="cmdProceed" runat="server" resourcekey="cmdProceed"  IconKey="Rt" />
        </asp:panel>
    </asp:panel>
    <asp:panel id="pnlProfile" runat="server" visible="false">
        <dnn:profile id="ctlProfile" runat="server" />
    </asp:panel>
</div>