<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Portals.Signup" CodeFile="Signup.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<div class="dnnForm dnnPortalSignup dnnClear" id="dnnPortalSignup">
    <fieldset>
        <asp:label id="lblInstructions" runat="server" Visible="false" />
        <div id="validationPanel" runat="Server" Visible="false" class="dnnFormItem">
			<asp:label id="lblMessage" runat="server"></asp:label>
			<asp:datalist id="lstResults" runat="server" cellspacing="0" borderwidth="0" visible="False" width="100%">
				<headertemplate><asp:label id="lblValidationResults" runat="server" resourcekey="ValidationResults"/></headertemplate>
				<itemtemplate><%# Container.DataItem %></itemtemplate>
			</asp:datalist>
        </div>
        <div id="rowType" runat="server" class="dnnFormItem">
            <dnn:label id="plType" runat="server" controlname="optType"/>
            <asp:radiobuttonlist id="optType" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True">
				<asp:listitem resourcekey="Parent" value="P" />
				<asp:listitem resourcekey="Child" value="C" />
			</asp:radiobuttonlist>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plPortalAlias" runat="server" controlname="txtPortalAlias" CssClass="dnnFormRequired" />
            <asp:textbox id="txtPortalAlias" runat="server" maxlength="128" />
			<asp:requiredfieldvalidator id="valPortalAlias" resourcekey="valPortalAlias.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtPortalAlias" display="Dynamic" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plHomeDirectory" runat="server" controlname="txtHomeDirectory" />
            <asp:textbox id="txtHomeDirectory" runat="server"  maxlength="100" />
			<asp:LinkButton CausesValidation="False" ID="btnCustomizeHomeDir" Runat="server" resourcekey="Customize" CssClass="dnnSecondaryAction" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plTitle" runat="server" controlname="txtPortalName" CssClass="dnnFormRequired" />
            <asp:textbox id="txtPortalName" runat="server" maxlength="128" />
			<asp:requiredfieldvalidator id="valPortalName" resourcekey="valPortalName.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtPortalName" display="Dynamic" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plDescription" runat="server" controlname="txtDescription" />
            <asp:textbox id="txtDescription" runat="server" maxlength="500" textmode="MultiLine" rows="2" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plKeyWords" runat="server" controlname="txtKeyWords" />
            <asp:textbox id="txtKeyWords" runat="server"  maxlength="500" textmode="MultiLine" rows="2" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plTemplate" runat="server" controlname="cboTemplate" />
            <dnn:DnnComboBox id="cboTemplate"  runat="server" AutoPostBack="true" CausesValidation="False" />
        </div>
        <div id="rowTemplateDescription" class="dnnFormItem" runat="server" Visible="False">
            <div class="dnnLabel">&nbsp;</div>
            <div class="dnnFormMessage dnnFormInfo suTemplateInfo">
			<asp:Label id="lblTemplateDescription" runat="server" /></div>
        </div>
        <div class="dnnFormItem" id="pnlSiteGroups" runat="server" Visible="false">
            <dnn:label id="plSiteGroups" runat="server" controlname="cboSiteGroups" />
            <dnn:DnnComboBox id="cboSiteGroups"  runat="server" CausesValidation="False" />
        </div>
        <div id="useCurrentPanel" runat="server" class="dnnFormItem">
            <dnn:label id="useCurrentLabel" runat="server" controlname="useCurrent" />
            <asp:CheckBox ID="useCurrent" runat="server" AutoPostBack="true" />
        </div>
        <asp:Panel ID="adminUserPanel" runat="server">
            <div class="dnnFormMessage dnnFormWarning"><asp:label id="lblNote" resourcekey="Note" runat="server" /></div>
            <div class="dnnFormItem">
                <dnn:label id="plUsername" runat="server" controlname="txtUsername" CssClass="dnnFormRequired" />
                <asp:textbox id="txtUsername" runat="server" maxlength="100" />
			    <asp:requiredfieldvalidator id="valUsername" resourcekey="valUsername.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtUsername" display="Dynamic" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plFirstName" runat="server" controlname="txtFirstName" CssClass="dnnFormRequired" />
                <asp:textbox id="txtFirstName" runat="server" maxlength="100" />
	            <asp:requiredfieldvalidator id="valFirstName" resourcekey="valFirstName.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtFirstName" display="Dynamic" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plLastName" runat="server" controlname="txtLastName" CssClass="dnnFormRequired" />
                <asp:textbox id="txtLastName" runat="server" maxlength="100" />
		        <asp:requiredfieldvalidator id="valLastName" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtLastName" resourcekey="valLastName.ErrorMessage" display="Dynamic" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEmail" runat="server" controlname="txtEmail" CssClass="dnnFormRequired"></dnn:label>
                <asp:textbox id="txtEmail" runat="server" maxlength="100" />
			    <asp:requiredfieldvalidator id="valEmail" resourcekey="valEmail.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtEmail" display="Dynamic" />
			    <asp:RegularExpressionValidator ID="valEmail2" runat="server" resourcekey="valEmail2.ErrorMessage" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtEmail" Display="Dynamic" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plPassword" runat="server" controlname="txtPassword" CssClass="dnnFormRequired" />
                <asp:textbox id="txtPassword" runat="server" maxlength="20" textmode="password" />
		        <asp:requiredfieldvalidator id="valPassword" resourcekey="valPassword.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtPassword" display="Dynamic" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plConfirm" runat="server" controlname="txtConfirm" CssClass="dnnFormRequired" />
                <asp:textbox id="txtConfirm" runat="server" maxlength="20" textmode="password" />
	            <asp:requiredfieldvalidator id="valConfirm" resourcekey="valConfirm.ErrorMessage" CssClass="dnnFormMessage dnnFormError" runat="server" controltovalidate="txtConfirm" display="Dynamic" />
            </div>
            <div id="questionRow" runat="server" CssClass="dnnFormItem" visible="false">
                <dnn:label id="plQuestion" runat="server" controlname="lblQuestion" />
                <asp:textbox id="txtQuestion" runat="server" maxlength="100" />
            </div>
            <div id="answerRow" runat="server" CssClass="dnnFormItem" visible="false">
                <dnn:label id="plAnswer" runat="server" controlname="txtAnswer" />
                <asp:textbox id="txtAnswer" runat="server" />
            </div>
        </asp:Panel>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" Causesvalidation="False" /></li>
    </ul>
</div>