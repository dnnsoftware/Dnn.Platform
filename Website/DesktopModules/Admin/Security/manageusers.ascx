<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.ManageUsers" Codebehind="ManageUsers.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Membership" Src="~/DesktopModules/Admin/Security/Membership.ascx" %>
<%@ Register TagPrefix="dnn" TagName="User" Src="~/DesktopModules/Admin/Security/User.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Roles" Src="~/DesktopModules/Admin/Security/SecurityRoles.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Password" Src="~/DesktopModules/Admin/Security/Password.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Profile" Src="~/DesktopModules/Admin/Security/Profile.ascx" %>
<%@ Register TagPrefix="dnn" TagName="MemberServices" Src="~/DesktopModules/Admin/Security/MemberServices.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls.Internal" %>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnManageUsers() {
        if ($('#<%=adminTabNav.ClientID%>').length) {
            $('#dnnManageUsers').dnnTabs().dnnPanels();
        }
    }

    $(document).ready(function () {
        setUpDnnManageUsers();
        var pageNo = <%=PageNo %>;
        if(pageNo > 0) {
            $('#dnnManageUsers > ul > li:nth-child(' + pageNo + ')').find('a').click();
        }
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnManageUsers();
        });
    });
} (jQuery, window.Sys));
</script>
<div class="dnnForm dnnManageUsers dnnClear" id="dnnManageUsers" runat="server" ClientIDMode="Static">
    <ul class="dnnAdminTabNav dnnClear" id="adminTabNav" runat="server">
        <li><a href="#<%=dnnUserDetails.ClientID %>"><%=LocalizeString("cmdUser")%></a></li>
        <li id="rolesTab" runat="server"><a href="#<%=dnnRoleDetails.ClientID%>"><%=LocalizeString("cmdRoles")%></a></li>
        <li id="passwordTab" runat="server"><a href="#<%=dnnPasswordDetails.ClientID%>"><%=LocalizeString("cmdPassword")%></a></li>
        <li><a href="#<%=dnnProfileDetails.ClientID%>"><%=LocalizeString("cmdProfile")%></a></li>
    </ul>
    <div id="dnnUserDetails" runat="server" class="dnnUserDetails dnnClear">
        <div class="udContent dnnClear">
            <fieldset>
                <div id="titleRow" runat="server" class="dnnFormItem">
				    <h2 class="dnnFormSectionHead"><asp:label id="lblTitle" runat="server" /></h2>
				    <dnn:DnnImage id="imgLockedOut" IconKey="SecurityRoles" runat="server" visible="False" />
				    <dnn:DnnImage id="imgOnline" IconKey="UserOnline" runat="server" visible="False" />
                </div>
                <div id="helpRow" runat="server" visible="false" class="dnnFormItem">
                    <asp:label id="lblUserHelp" runat="server" />
                </div>
                <div id="userRow" runat="server" class="dnnFormItem">
                    <asp:Panel id="userContainer" cssClass="dnnUser" runat="server">
                        <dnn:user id="ctlUser" runat="Server" />
                    </asp:Panel>
                    <div id="membershipRow" runat="server" class="dnnMembership">
                        <dnn:membership id="ctlMembership" runat="Server" />
                    </div>
                </div>
            </fieldset>
        </div>
    </div>
    <div id="dnnRoleDetails" runat="server" class="dnnRoleDetails dnnClear">
 	    <dnn:roles id="ctlRoles" runat="server"></dnn:roles>
    </div>
    <div id="dnnPasswordDetails" runat="server" class="dnnPasswordDetails dnnClear">
    	<dnn:Password id="ctlPassword" runat="server"></dnn:Password>
    </div>
    <asp:Panel id="dnnProfileDetails" runat="server" class="dnnProfileDetails dnnClear">
    	<dnn:Profile id="ctlProfile" runat="server"></dnn:Profile>
    </asp:Panel>
</div>
<div class="dnnForm">
    <ul id="actionsRow" runat="server" visible="false" class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdAdd" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" /></li>
        <li><asp:Hyperlink id="loginLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdLogin" Visible="false" /></li>
    </ul>
</div>