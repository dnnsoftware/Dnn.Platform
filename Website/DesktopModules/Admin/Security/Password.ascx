<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.Password" CodeFile="Password.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnPassword dnnClear" id="dnnPassword">
    <fieldset>
		<asp:Panel runat="server" ID="CannotChangePasswordMessage" CssClass="dnnFormMessage dnnFormWarning" Visible="False"><%=LocalizeString("CannotChangePassword") %></asp:Panel>
		<asp:panel id="pnlChange" runat="server">
		    <h2 class="dnnFormSectionHead"><asp:label id="lblChangePasswordHeading" runat="server" resourceKey="ChangePassword" /></h2>
            <div class="dnnFormItem"><p><asp:label id="lblChangeHelp" runat="server" /></p></div>
            <div id="oldPasswordRow" runat="server" class="dnnFormItem">
                <dnn:label id="plOldPassword" runat="server" controlname="txtOldPassword" />
                <asp:textbox id="txtOldPassword" runat="server" textmode="Password" size="25" maxlength="128" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plNewPassword" runat="server" controlname="txtNewPassword" />
                <asp:Panel ID="passwordContainer" runat="server">
                    <asp:textbox id="txtNewPassword" runat="server" textmode="Password" size="25" maxlength="20" />
                </asp:Panel>
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plNewConfirm" runat="server" controlname="txtNewConfirm" />
                <asp:textbox id="txtNewConfirm" runat="server" textmode="Password" size="25" maxlength="128" CssClass="password-confirm" />
            </div>
            <ul class="dnnActions dnnClear">
                 <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="ChangePassword" /></li>
                </ul>
		</asp:panel>
       	<asp:panel id="pnlReset" runat="server">
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
		<asp:panel id="pnlQA" runat="server">
            <div class="dnnFormItem"><asp:label id="lblChangeQA" runat="server" resourceKey="ChangeQA" /></div>
            <div class="dnnFormItem"><asp:label id="lblQAHelp" resourcekey="QAHelp" cssclass="Normal" runat="server" /></div>
            <div class="dnnFormItem">
                <dnn:label id="plQAPassword" runat="server" controlname="txtQAPassword" />
                <asp:textbox id="txtQAPassword" runat="server" textmode="Password" size="25" maxlength="20" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEditQuestion" runat="server" controlname="lblQuetxtEditQuestionstion" />
                <asp:textbox id="txtEditQuestion" runat="server" size="25" maxlength="20" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEditAnswer" runat="server" controlname="txtEditAnswer" />
                <asp:textbox id="txtEditAnswer" runat="server" size="25" maxlength="20" />
            </div>
		</asp:panel>
        <div class="dnnFormItem">
            <dnn:label id="plLastChanged" runat="server" controlname="lblLastChanged" />
            <asp:label id = "lblLastChanged" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plExpires" runat="server" controlname="lblExpires" />
            <asp:label id = "lblExpires" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdReset" runat="server" CssClass="dnnPrimaryAction" resourcekey="ResetPassword" /></li>
        <li><asp:LinkButton id="cmdUserReset" runat="server" CssClass="dnnPrimaryAction" resourcekey="ResetPassword" Visible="False" /></li>
        <li><asp:LinkButton id="cmdUpdateQA" runat="server" CssClass="dnnSecondaryAction" resourcekey="SaveQA" /></li>
    </ul>  
</div>