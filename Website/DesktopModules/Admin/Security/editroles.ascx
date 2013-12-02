<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Security.EditRoles" CodeFile="EditRoles.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Url" Src="~/controls/UrlControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnEditRole dnnClear" id="dnnEditRole">
    <ul class="dnnAdminTabNav dnnClear">
		<li><a href="#erBasicSettings"><%=LocalizeString("BasicSettings")%></a></li>
		<li><a href="#erAdvancedSettings"><%=LocalizeString("AdvancedSettings")%></a></li>
	</ul>
    <div class="erBasicSettings" id="erBasicSettings">
        <div class="erbsContent dnnClear">
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:Label ID="plRoleName" runat="server" ResourceKey="RoleName" ControlName="txtRoleName" CssClass="dnnFormRequired" />
                    <asp:TextBox ID="txtRoleName" runat="server" MaxLength="50" />
                    <asp:Label ID="lblRoleName" Visible="False" runat="server" />
                    <asp:RequiredFieldValidator ID="valRoleName" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valRoleName" ControlToValidate="txtRoleName" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="valRoleName2" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valRoleName2" ControlToValidate="txtRoleName" Display="Dynamic" ValidationExpression="[A-Za-z0-9\.\s_-]*" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plDescription" runat="server" ResourceKey="Description" ControlName="txtDescription" />
                    <asp:TextBox ID="txtDescription" runat="server" MaxLength="1000" Columns="50" TextMode="MultiLine" Rows="4" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plRoleGroups" runat="server" Suffix="" ControlName="cboRoleGroups" />
                   <%-- <asp:DropDownList ID="cboRoleGroups" runat="server" />--%>
                    <dnn:DnnComboBox ID="cboRoleGroups" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plIsPublic" runat="server" ResourceKey="PublicRole" ControlName="chkIsPublic" />
                    <asp:CheckBox ID="chkIsPublic" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plAutoAssignment" runat="server" ResourceKey="AutoAssignment" ControlName="chkAutoAssignment" />
                    <asp:CheckBox ID="chkAutoAssignment" runat="server" />
                </div>
				<div class="dnnFormItem" id="assignToExistUsers">
                    <dnn:Label ID="plAssignToExistUsers" runat="server" ResourceKey="AssignToExistUsers" ControlName="chkAssignToExistUsers" />
                    <asp:CheckBox ID="chkAssignToExistUsers" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="securityModeListLabel" runat="server" Suffix="" ControlName="securityModeList" />
                    <%--<asp:DropDownList ID="securityModeList" runat="server"/>--%>
                    <dnn:DnnComboBox ID="securityModeList" runat="server"/>
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="statusListLabel" runat="server" Suffix="" ControlName="statusList" />
                   <%-- <asp:DropDownList ID="statusList" runat="server"/>--%>
                   <dnn:DnnComboBox ID="statusList" runat="server"/>
                </div>
            </fieldset>
        </div>
    </div>
    <div class="erAdvancedSettings" id="erAdvancedSettings">
        <div class="erasContent dnnClear">
            <fieldset>
                <div class="dnnFormItem dnnClear"><asp:Label ID="lblProcessorWarning" visible="false" runat="server" resourcekey="ProcessorWarning" EnableViewState="False" CssClass="dnnFormMessage dnnFormWarning" /></div>
                <div class="dnnFormItem" id="divServiceFee" runat="server">
                    <dnn:Label ID="plServiceFee" runat="server" ResourceKey="ServiceFee" Suffix=":" ControlName="txtServiceFee" />
                    <asp:TextBox ID="txtServiceFee" runat="server" MaxLength="50" Columns="30" />
                    <asp:CompareValidator ID="valServiceFee1" CssClass="dnnFormError" runat="server" resourcekey="valServiceFee1" ControlToValidate="txtServiceFee" Display="Dynamic" Type="Currency" Operator="DataTypeCheck" />
                    <asp:CompareValidator ID="valServiceFee2" CssClass="dnnFormError" runat="server" resourcekey="valServiceFee2" ControlToValidate="txtServiceFee" Display="Dynamic" Operator="GreaterThanEqual" ValueToCompare="0" />
                </div>
                <div class="dnnFormItem" id="divBillingPeriod" runat="server">
                    <dnn:Label ID="plBillingPeriod" runat="server" ResourceKey="BillingPeriod" Suffix=":" ControlName="txtBillingPeriod" />
                    <asp:TextBox ID="txtBillingPeriod" runat="server" MaxLength="50" Columns="30" CssClass="dnnFixedSizeComboBox" />
                    <dnn:DnnComboBox ID="cboBillingFrequency" runat="server" DataValueField="value" DataTextField="text" AutoPostBack="true" CssClass="dnnFixedSizeComboBox" />
                    <asp:CompareValidator ID="valBillingPeriod1" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valBillingPeriod1" ControlToValidate="txtBillingPeriod" Display="Dynamic" Type="Integer" Operator="DataTypeCheck" />
                    <asp:CompareValidator ID="valBillingPeriod2" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valBillingPeriod2" ControlToValidate="txtBillingPeriod" Display="Dynamic" Operator="GreaterThan" ValueToCompare="0" />
                </div>
                <div class="dnnFormItem" id="divTrialFee" runat="server">
                    <dnn:Label ID="plTrialFee" runat="server" ResourceKey="TrialFee" Suffix=":" ControlName="txtTrialFee" />
                    <asp:TextBox ID="txtTrialFee" runat="server" MaxLength="50" Columns="30" />
                    <asp:CompareValidator ID="valTrialFee1" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valTrialFee1" ControlToValidate="txtTrialFee" Display="Dynamic" Type="Currency" Operator="DataTypeCheck" />
                    <asp:CompareValidator ID="valTrialFee2" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valTrialFee2" ControlToValidate="txtTrialFee" Display="Dynamic" Operator="GreaterThanEqual" ValueToCompare="0" />
                </div>
                <div class="dnnFormItem" id="divTrialPeriod" runat="server">
                    <dnn:Label ID="plTrialPeriod" runat="server" ResourceKey="TrialPeriod" Suffix=":" ControlName="txtTrialPeriod" />
                    <asp:TextBox ID="txtTrialPeriod" runat="server" MaxLength="50" Columns="30" CssClass="dnnFixedSizeComboBox" />
                    <dnn:DnnComboBox ID="cboTrialFrequency" runat="server" DataValueField="value" DataTextField="text" AutoPostBack="true" CssClass="dnnFixedSizeComboBox" />
                    <asp:CompareValidator ID="valTrialPeriod1" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valTrialPeriod1" ControlToValidate="txtTrialPeriod" Display="Dynamic" Type="Integer" Operator="DataTypeCheck" />
                    <asp:CompareValidator ID="valTrialPeriod2" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valTrialPeriod2" ControlToValidate="txtTrialPeriod" Display="Dynamic" Operator="GreaterThan" ValueToCompare="0" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plRSVPCode" runat="server" ControlName="txtRSVPCode" />
                    <asp:TextBox ID="txtRSVPCode" runat="server" MaxLength="50" Columns="30" AutoPostBack="true" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plRSVPLink" runat="server" ControlName="txtRSVPLink" />
                    <asp:Label ID="lblRSVPLink" runat="server" />
                </div>
                 <div class="dnnFormItem">
                    <dnn:Label ID="plIcon" Text="Icon:" runat="server" ControlName="ctlIcon" />
                    <div class="dnnLeft"><dnn:Url ID="ctlIcon" runat="server" ShowUrls="False" ShowTabs="False" ShowLog="False" ShowTrack="False" Required="False" /></div>
                </div>
            </fieldset>
        </div>
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdUpdate" resourcekey="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li><asp:LinkButton ID="cmdDelete" resourcekey="cmdDelete" runat="server" CssClass="dnnSecondaryAction dnnDeleteRole" CausesValidation="False" /></li>
        <li><asp:LinkButton ID="cmdManage" resourcekey="cmdManage" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
        <li><asp:HyperLink ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
    </ul>
</div>

<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnEditRoles() {
        $('#dnnEditRole').dnnTabs();
        var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
        var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
        var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
        $('#<%= cmdDelete.ClientID %>').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteItem")) %>',
            yesText: yesText,
            noText: noText,
            title: titleText
        });

	    var updateAssignRow = function(show) {
			if (show) {
				$("#assignToExistUsers").show();
			} else {
				$("#assignToExistUsers").hide().find("input[type=checkbox]").attr("checked", false);
				$("#assignToExistUsers").find("input[type=checkbox]").attr("checked", false);
			}
	    };
        
         var updateView = function(e) {
            var txtBillingPeriod = $("#<%=txtBillingPeriod.ClientID %>");
            var cboBillingFrequency = $find("<%=cboBillingFrequency.ClientID %>");
            var txtTrialPeriod = $("#<%=txtTrialPeriod.ClientID %>");
            var cboTrialFrequency = $find("<%=cboTrialFrequency.ClientID %>");
            
            if ($("#<%=txtServiceFee.ClientID %>").val() == "") {
                txtBillingPeriod.attr("disabled", "disabled").val("");
                cboBillingFrequency.disable();
                cboBillingFrequency.clearSelection();
            } else {
                txtBillingPeriod.removeAttr("disabled");
                cboBillingFrequency.enable();
            }

            if ($("#<%=txtTrialFee.ClientID %>").val() == "") {
                txtTrialPeriod.attr("disabled", "disabled").val("");
                cboTrialFrequency.disable();
                cboTrialFrequency.clearSelection();
            } else {
                txtTrialPeriod.removeAttr("disabled");
                cboTrialFrequency.enable();
            }

         	updateAssignRow($("#<%=chkAutoAssignment.ClientID%>").attr("checked"));
         };

        $("#<%=txtServiceFee.ClientID %>").keyup(updateView);
    	$("#<%=txtTrialFee.ClientID %>").keyup(updateView);

	    $("#<%=chkAutoAssignment.ClientID%>").click(function() {
		    updateAssignRow(this.checked);
	    });
        
        setTimeout(updateView, 0);
    }
    $(document).ready(function () {
        setUpDnnEditRoles();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnEditRoles();
        });
    });
}(jQuery, window.Sys));
</script>
