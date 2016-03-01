<%@ Control Inherits="DotNetNuke.Modules.Admin.Users.UserAccounts" Language="C#" AutoEventWireup="false" Codebehind="Users.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web.Deprecated" Namespace="DotNetNuke.Web.UI.WebControls"%>
<div class="dnnForm dnnUsers dnnClear" id="dnnUsers">
    <div class="dnnFormItem">
        <asp:textbox id="txtSearch" Runat="server" CssClass="dnnUserSearchInput dnnFixedSizeComboBox" />
        <dnn:DnnComboBox id="ddlSearchType" Runat="server" CssClass="dnnUsersSearchFilter dnnFixedSizeComboBox" />
        <asp:LinkButton ID="cmdSearch" runat="server" resourcekey="Search" CssClass="dnnSecondaryAction dnnUsersSearchFilter" />        
    </div>
    <div class="dnnClear" ></div>
    <ul class="uLetterSearch dnnClear">
        <asp:Repeater id="rptLetterSearch" Runat="server">
            <ItemTemplate>
                <li><asp:HyperLink ID="hlLetter" runat="server" NavigateUrl='<%# FilterURL((string)Container.DataItem) %>' Text='<%# Container.DataItem %>'></asp:HyperLink></li>
            </ItemTemplate>
        </asp:Repeater>
    </ul>
    <div>
        <dnn:DNNGrid id="grdUsers" AutoGenerateColumns="false" CssClass="dnnGrid dnnSecurityRolesGrid" Runat="server" AllowPaging="True" AllowCustomPaging="True" EnableViewState="True" OnNeedDataSource="NeedDataSource">
            <MasterTableView>
                <Columns>
                    <dnn:DnnGridImageCommandColumn CommandName="Edit" IconKey="Edit" EditMode="URL" KeyField="UserID" UniqueName="EditButton" />
                    <dnn:DnnGridTemplateColumn UniqueName="DeleteActions" >
                        <ItemTemplate>
                            <dnn:DnnImageButton runat="server" ID="Delete" CommandName="Delete" IconKey="ActionDelete" />
                            <dnn:DnnImageButton runat="server" ID="Restore" CommandName="Restore" IconKey="Rollback" />
                            <dnn:DnnImageButton runat="server" ID="Remove" CommandName="Remove" IconKey="Cancel" />
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridImageCommandColumn CommandName="UserRoles" IconKey="SecurityRoles" EditMode="URL" KeyField="UserID" UniqueName="RolesButton" />
                    <dnn:DnnGridTemplateColumn UniqueName="UsersOnline" Visible="False">
                        <ItemTemplate>
                            <dnn:DnnImage id="imgOnline" runat="Server" IconKey="userOnline" />		
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridBoundColumn datafield="UserName" headertext="Username"/>
                    <dnn:DnnGridBoundColumn datafield="FirstName" headertext="FirstName"/>
                    <dnn:DnnGridBoundColumn datafield="LastName" headertext="LastName"/>
                    <dnn:DnnGridBoundColumn datafield="DisplayName" headertext="DisplayName"/>
                    <dnn:DnnGridTemplateColumn HeaderText="Address">
                        <ItemTemplate>
                            <asp:Label ID="lblAddress" Runat="server" Text='<%# DisplayAddress(((UserInfo)Container.DataItem).Profile.Unit,((UserInfo)Container.DataItem).Profile.Street, ((UserInfo)Container.DataItem).Profile.City, ((UserInfo)Container.DataItem).Profile.Region, ((UserInfo)Container.DataItem).Profile.Country, ((UserInfo)Container.DataItem).Profile.PostalCode) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridTemplateColumn HeaderText="Telephone">
                        <ItemTemplate>
                            <asp:Label ID="Label4" Runat="server" Text='<%# DisplayEmail(((UserInfo)Container.DataItem).Profile.Telephone) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridTemplateColumn HeaderText="Email">
                        <ItemTemplate>
                            <asp:Label ID="lblEmail" Runat="server" Text='<%# DisplayEmail(((UserInfo)Container.DataItem).Email) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridTemplateColumn HeaderText="CreatedDate">
                        <ItemTemplate>
                            <asp:Label ID="lblCreateDate" Runat="server" Text='<%# DisplayDate(((UserInfo)Container.DataItem).Membership.CreatedDate) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridTemplateColumn HeaderText="LastLogin">
                        <ItemTemplate>
                            <asp:Label ID="lblLastLogin" Runat="server" Text='<%# DisplayDate(((UserInfo)Container.DataItem).Membership.LastLoginDate) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridTemplateColumn HeaderText="Authorized"  HeaderStyle-HorizontalAlign="Center"  ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <dnn:DnnImage Runat="server" ID="imgApproved" IconKey="Checked" Visible="False" />
                            <dnn:DnnImage Runat="server" ID="imgNotApproved" IconKey="Unchecked" Visible="False" />
                            <dnn:DnnImage Runat="server" ID="imgApprovedDeleted" IconKey="CheckedDisabled" Visible="False" />
                            <dnn:DnnImage Runat="server" ID="imgNotApprovedDeleted" IconKey="UncheckedDisabled" Visible="False" />
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                </Columns>
                <PagerStyle AlwaysVisible="true" PageButtonCount="6" Mode="NextPrevAndNumeric"/>

            </MasterTableView>
        </dnn:DNNGrid>
    </div>
        <ul class="dnnActions dnnClear">
        <li><asp:HyperLink id="addUserButton" runat="Server" CssClass="dnnPrimaryAction" resourcekey ="AddContent.Action" Visible="false" /></li>
		<li><asp:LinkButton id="removeDeletedButton" runat="server" CssClass="dnnSecondaryAction" resourcekey="RemoveDeleted.Action" Visible="false" /></li>
		<li><asp:LinkButton id="deleteUnAuthorizedButton" runat="server" CssClass="dnnSecondaryAction" resourcekey="DeleteUnAuthorized.Action" Visible="false" /></li>
	</ul>
</div>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setUpDnnUsers() {
            $('#<%= deleteUnAuthorizedButton.ClientID %>').dnnConfirm({
                text: '<%= Localization.GetSafeJSString("DeleteItems.Text", Localization.SharedResourceFile) %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });
            $('#<%= removeDeletedButton.ClientID %>').dnnConfirm({
                text: '<%= Localization.GetSafeJSString("RemoveItems.Confirm", Localization.SharedResourceFile) %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });
	        $('.dnnSecurityRolesGrid td input[type="image"]').each(function(index, item) {
		        var $this = $(this);
		        var name = $this.attr('name');
		        var text = '<%= Localization.GetSafeJSString("RemoveItems.Confirm", Localization.SharedResourceFile) %>';
		        if (name.indexOf('Delete') > 0) {
			        text = '<%= Localization.GetSafeJSString("Delete.Confirm", LocalResourceFile) %>';
		        } else if (name.indexOf('Restore') > 0) {
			        text = '<%= Localization.GetSafeJSString("Restore.Confirm", LocalResourceFile) %>';
		        } else if (name.indexOf('Remove') > 0) {
			        text = '<%= Localization.GetSafeJSString("Remove.Confirm", LocalResourceFile) %>';
		        }

				$this.dnnConfirm({
					text: text,
					yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
					noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
					title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
					isButton: true
				});
	        });
        }

        $(document).ready(function () {
            setUpDnnUsers();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUpDnnUsers();
            });
        });
    } (jQuery, window.Sys));
</script>