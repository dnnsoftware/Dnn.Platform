<%@ Control Inherits="DesktopModules.Admin.Security.Roles" Language="C#" AutoEventWireup="false" CodeFile="Roles.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnSecurityRoles">
    <div runat="server" id="divGroups">
        <div class="dnnFormItem">
            <dnn:label id="plRoleGroups" runat="server" suffix="" controlname="cboRoleGroups" />
            <%--<asp:dropdownlist id="cboRoleGroups" Runat="server" AutoPostBack="True" />--%>
            <dnn:DnnComboBox id="cboRoleGroups" Runat="server" AutoPostBack="True" />
			<asp:hyperlink ID="lnkEditGroup" runat="server" CssClass="inlineButton">
				<dnn:dnnImage ID="imgEditGroup" IconKey="Edit" AlternateText="Edit" runat="server" resourcekey="Edit" />
			</asp:hyperlink>
			<dnn:DnnImagebutton ID="cmdDelete" Runat="server" IconKey="Delete" CssClass="inlineButton" />
        </div>
    </div>
	<dnn:DnnGrid id="grdRoles" AutoGenerateColumns="false" EnableViewState="false" runat="server" CssClass="dnnGrid">
	    <MasterTableView>
		    <Columns>
			    <dnn:DnnGridImageCommandColumn commandname="Edit" IconKey="Edit" editmode="URL" keyfield="RoleID" UniqueName="EditButton" />
			    <dnn:DnnGridImageCommandColumn commandname="UserRoles" IconKey="Users" editmode="URL" keyfield="RoleID" UniqueName="RolesButton" />
		        <dnn:DnnGridBoundColumn DataField="RoleName" HeaderText="Name" />
		        <dnn:DnnGridBoundColumn DataField="Description" HeaderText="Description" />
			    <dnn:DnnGridTemplateColumn HeaderText="Fee">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatPrice((float)DataBinder.Eval(Container.DataItem, "ServiceFee")) %>' ID="Label1" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Every">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatPeriod((int)DataBinder.Eval(Container.DataItem, "BillingPeriod")) %>' ID="Label2" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Period">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatFrequency((string) DataBinder.Eval(Container.DataItem, "BillingFrequency")) %>' ID="lblBillingFrequency" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Trial">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatPrice((float)DataBinder.Eval(Container.DataItem, "TrialFee")) %>' ID="Label3" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Every">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatPeriod((int)DataBinder.Eval(Container.DataItem, "TrialPeriod")) %>' ID="Label4" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Period">
				    <ItemTemplate>
					    <asp:label runat="server" Text='<%#FormatFrequency((string) DataBinder.Eval(Container.DataItem, "TrialFrequency")) %>' ID="lblTrialFrequency" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Public" ItemStyle-HorizontalAlign="Center">
				    <ItemTemplate>
					    <dnn:DnnImage Runat="server" ID="imgApproved" IconKey="Checked" Visible='<%# DataBinder.Eval(Container.DataItem,"IsPublic") %>' />
					    <dnn:DnnImage Runat="server" ID="imgNotApproved" IconKey="Unchecked" Visible='<%# !(bool)DataBinder.Eval(Container.DataItem,"IsPublic")%>' />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Auto" ItemStyle-HorizontalAlign="Center">
				    <ItemTemplate>
					    <dnn:Dnnimage Runat="server" ID="Image1" IconKey="Checked" Visible='<%# DataBinder.Eval(Container.DataItem,"AutoAssignment") %>' />
					    <dnn:Dnnimage Runat="server" ID="Image2" IconKey="Unchecked" Visible='<%# !(bool)DataBinder.Eval(Container.DataItem,"AutoAssignment") %>' />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
		        <dnn:DnnGridBoundColumn DataField="UserCount" HeaderText="UserCount" ItemStyle-HorizontalAlign="Center" />
		    </Columns>
        </MasterTableView>
	</dnn:DnnGrid>
    <ul class="dnnActions dnnClear">
		<li><asp:LinkButton id="cmdAddRole" runat="server" CssClass="dnnPrimaryAction" resourcekey="AddContent.Action"  /></li>
		<li><asp:LinkButton id="cmdAddRoleGroup" runat="server" CssClass="dnnSecondaryAction" resourcekey="AddGroup.Action" /></li>
	</ul>

</div>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setupDnnRoles() {
        $('#<%= cmdDelete.ClientID %>').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteItem")) %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
        	title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
        	isButton: true
        });
    }

    $(document).ready(function () {
        setupDnnRoles();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        	setupDnnRoles();
        });
    });
} (jQuery, window.Sys));
</script>