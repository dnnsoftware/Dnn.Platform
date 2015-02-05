<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Dashboard.Dashboard" CodeFile="Dashboard.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDashboardTabs() {
        $('#dnnDashBoard').dnnTabs();
    }

    $(document).ready(function () {
        // We are waiting modules tab is ready for runs dnnTabs() method
        setTimeout(setUpDashboardTabs, 500);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setTimeout(setUpDashboardTabs, 500);
        });
    });
} (jQuery, window.Sys));
</script>
<div class="dnnForm dnnDashBoard dnnClear" id="dnnDashBoard">
    <asp:Repeater ID="rptTabs" runat="server">
        <HeaderTemplate>
            <ul class="dnnAdminTabNav dnnClear" id="">
        </HeaderTemplate>
        <ItemTemplate>
            <li><a href='#<%# Eval("DashboardControlKey") %>-tab'><%# Eval("LocalizedTitle")%></a></li>
        </ItemTemplate>
        <FooterTemplate>
            </ul>
        </FooterTemplate>        
    </asp:Repeater>
    <asp:Repeater ID="rptControls" runat="server">
        <ItemTemplate>
            <div id='<%# Eval("DashboardControlKey") %>-tab' class="dnnDashboardPanel">
                <div class="dashboardHeader">
                </div>
                <asp:PlaceHolder ID="phControl" runat="server" />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<ul class="dnnActions dnnClear">
	<li><asp:Hyperlink id="cmdInstall" runat="server" CssClass="dnnPrimaryAction" resourcekey="Install.Action"  /></li>
	<li><asp:Hyperlink id="cmdManage" runat="server" CssClass="dnnSecondaryAction" resourcekey="Manage.Action" /></li>
	<li><asp:Hyperlink id="cmdExport" runat="server" CssClass="dnnSecondaryAction" resourcekey="Export.Action" /></li>
</ul>