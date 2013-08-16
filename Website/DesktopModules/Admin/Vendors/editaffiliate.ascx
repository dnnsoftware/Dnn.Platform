<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Vendors.EditAffiliate" CodeFile="EditAffiliate.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnEditAffiliate dnnClear" id="dnnEditAffiliate">
    <fieldset>        
        <div class="dnnFormItem">
            <dnn:label id="plStartDate" runat="server" controlname="txtStartDate" />
            <dnn:DnnDatePicker ID="StartDatePicker" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plEndDate" runat="server" controlname="txtEndDate" />
			<dnn:DnnDatePicker ID="EndDatePicker" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plCPC" runat="server" controlname="txtCPC" cssclass="dnnFormRequired" />
			<asp:textbox id="txtCPC" runat="server" maxlength="7" columns="30"  />
			<asp:requiredfieldvalidator id="valCPC1" resourcekey="CPC.ErrorMessage" runat="server" controltovalidate="txtCPC" display="Dynamic" cssclass="dnnFormMessage dnnFormError" />
			<asp:comparevalidator id="valCPC2" resourcekey="CPC.ErrorMessage" runat="server" controltovalidate="txtCPC" display="Dynamic" type="Double" operator="DataTypeCheck" cssclass="dnnFormMessage dnnFormError" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plCPA" runat="server" controlname="txtCPA" cssclass="dnnFormRequired" />
			<asp:textbox id="txtCPA" runat="server" maxlength="7" columns="30"  />
			<asp:requiredfieldvalidator id="valCPA1" resourcekey="CPA.ErrorMessage" runat="server" controltovalidate="txtCPA" display="Dynamic" cssclass="dnnFormMessage dnnFormError" />
			<asp:comparevalidator id="valCPA2" resourcekey="CPA.ErrorMessage" runat="server" controltovalidate="txtCPA" display="Dynamic" type="Double" operator="DataTypeCheck" cssclass="dnnFormMessage dnnFormError" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdDelete" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="cmdSend" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdSend" Causesvalidation="False" /></li>
    </ul>
</div>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnEditAffiliate() {
        var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
        var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
        var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
        $('#<%= cmdDelete.ClientID %>').dnnConfirm({
            text: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
            yesText: yesText,
            noText: noText,
            title: titleText
        });
    }
    $(document).ready(function () {
        setUpDnnEditAffiliate();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnEditAffiliate();
        });
    });
} (jQuery, window.Sys));
</script>