<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Scheduler.EditSchedule" CodeFile="EditSchedule.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<div class="dnnForm dnnEditSchedule dnnClear" id="dnnEditSchedule">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="plFriendlyName" runat="server" ControlName="txtFriendlyName" />
            <asp:TextBox ID="txtFriendlyName" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plType" runat="server" ControlName="txtType" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtType" runat="server" />
            <asp:RequiredFieldValidator ID="valType" runat="server" Display="Dynamic" EnableClientScript="true" ControlToValidate="txtType" CssClass="dnnFormMessage dnnFormError" resourcekey="TypeRequired" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plEnabled" runat="server" ControlName="chkEnabled" />
            <asp:CheckBox ID="chkEnabled" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
		   <dnn:Label ID="plScheduleStartDate" runat="server" ControlName="txtStartDate" />
		   <dnn:dnndatetimepicker ID="startScheduleDatePicker" runat="server"/>&nbsp;
		</div>  
        <div class="dnnFormItem timeMeasurement">
            <dnn:Label ID="plTimeLapse" runat="server" ControlName="txtTimeLapse" />
            <asp:TextBox ID="txtTimeLapse" runat="server" MaxLength="10" CssClass="dnnSmallSizeComboBox" />
            <asp:RequiredFieldValidator ID="TimeLapseRequiredValidator" CssClass="dnnFormMessage dnnFormError" EnableViewState="false" runat="server" resourcekey="TimeLapseRequired.ErrorMessage" Display="Dynamic" ControlToValidate="txtTimeLapse" />
            <asp:RangeValidator runat="server" ControlToValidate="txtTimeLapse" Display="Dynamic" ID="TimeLapseValidator" EnableViewState="false" MinimumValue="1" MaximumValue="999999" Type="Integer" CssClass="dnnFormMessage dnnFormError" resourcekey="TimeLapseValidator.ErrorMessage"></asp:RangeValidator>
            <dnn:DnnComboBox ID="ddlTimeLapseMeasurement" runat="server" CssClass="dnnSmallSizeComboBox">
                <Items>
                    <dnn:DnnComboBoxItem resourcekey="Seconds" Value="s" />
                    <dnn:DnnComboBoxItem resourcekey="Minutes" Value="m" />
                    <dnn:DnnComboBoxItem resourcekey="Hours" Value="h" />
                    <dnn:DnnComboBoxItem resourcekey="Days" Value="d" />
                    <dnn:DnnComboBoxItem resourcekey="Weeks" Value="w" />
                    <dnn:DnnComboBoxItem resourcekey="Months" Value="mo" />
                    <dnn:DnnComboBoxItem resourcekey="Years" Value="y" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <div class="dnnFormItem timeMeasurement">
            <dnn:Label ID="plRetryTimeLapse" runat="server" ControlName="txtRetryTimeLapse" />
            <asp:TextBox ID="txtRetryTimeLapse" runat="server" MaxLength="10" CssClass="dnnSmallSizeComboBox" OnTextChanged="VisibilityRetryTimeLapse" AutoPostBack="True"/>
            <asp:RequiredFieldValidator ID="RetryTimeLapseRequireValidator" CssClass="dnnFormMessage dnnFormError" EnableViewState="false" runat="server" resourcekey="RetryTimeLapseRequired.ErrorMessage" Display="Dynamic" ControlToValidate="txtRetryTimeLapse" />
            <asp:RangeValidator runat="server" ControlToValidate="txtRetryTimeLapse" Display="Dynamic" ID="RetryTimeLapseValidator" EnableViewState="false" MinimumValue="0" MaximumValue="999999" Type="Integer" CssClass="dnnFormMessage dnnFormError" resourcekey="RetryTimeLapseValidator.ErrorMessage"></asp:RangeValidator>
            <dnn:DnnComboBox ID="ddlRetryTimeLapseMeasurement" runat="server" CssClass="dnnSmallSizeComboBox">
                <Items>
                    <dnn:DnnComboBoxItem resourcekey="Seconds" Value="s" />
                    <dnn:DnnComboBoxItem resourcekey="Minutes" Value="m" />
                    <dnn:DnnComboBoxItem resourcekey="Hours" Value="h" />
                    <dnn:DnnComboBoxItem resourcekey="Days" Value="d" />
                    <dnn:DnnComboBoxItem resourcekey="Weeks" Value="w" />
                    <dnn:DnnComboBoxItem resourcekey="Months" Value="mo" />
                    <dnn:DnnComboBoxItem resourcekey="Years" Value="y" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plRetainHistoryNum" runat="server" ControlName="ddlRetainHistoryNum" />
            <dnn:DnnComboBox ID="ddlRetainHistoryNum" runat="server">
                <Items>
                    <dnn:DnnComboBoxItem Value="0" resourcekey="None" />
                    <dnn:DnnComboBoxItem Value="1" Text="1" />
                    <dnn:DnnComboBoxItem Value="5" Text="5" />
                    <dnn:DnnComboBoxItem Value="10" Text="10" />
                    <dnn:DnnComboBoxItem Value="25" Text="25" />
                    <dnn:DnnComboBoxItem Value="50" Text="50" />
                    <dnn:DnnComboBoxItem Value="100" Text="100" />
                    <dnn:DnnComboBoxItem Value="250" Text="250" />
                    <dnn:DnnComboBoxItem Value="500" Text="500" />
                    <dnn:DnnComboBoxItem Value="-1" resourcekey="All" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plAttachToEvent" runat="server" ControlName="ddlAttachToEvent" />
            <dnn:DnnComboBox ID="ddlAttachToEvent" runat="server" >
                <Items>
                    <dnn:DnnComboBoxItem resourcekey="None" Value="" />
                    <dnn:DnnComboBoxItem resourcekey="APPLICATION_START" Value="APPLICATION_START" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plCatchUpEnabled" runat="server" ControlName="chkCatchUpEnabled" />
            <asp:CheckBox ID="chkCatchUpEnabled" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plObjectDependencies" runat="server" ControlName="txtObjectDependencies" />
            <asp:TextBox ID="txtObjectDependencies" runat="server" MaxLength="150" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plServers" runat="server" ControlName="txtServers" />
            <asp:TextBox ID="txtServers" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li><asp:LinkButton id="cmdRun" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRun" Causesvalidation="False"/></li>
        <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdDelete" Causesvalidation="False" /></li>
        <li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
    </ul>
</div>
<script type="text/javascript">
/*globals jQuery */
(function ($) {
    var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
    var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
    var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
	$('#<%= cmdDelete.ClientID %>').dnnConfirm({
	    text: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
		yesText: yesText,
		noText: noText,
		title: titleText
	});
} (jQuery));
</script>