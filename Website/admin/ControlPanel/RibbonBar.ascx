<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanels.RibbonBar" CodeFile="RibbonBar.ascx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="AddModule" Src="~/admin/ControlPanel/AddModule.ascx" %>
<%@ Register TagPrefix="dnn" TagName="AddPage" Src="~/admin/ControlPanel/AddPage.ascx" %>
<%@ Register TagPrefix="dnn" TagName="UpdatePage" Src="~/admin/ControlPanel/UpdatePage.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SwitchSite" Src="~/admin/ControlPanel/SwitchSite.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.DDRMenu.TemplateEngine" Assembly="DotNetNuke.Web.DDRMenu" %>
<%@ Register TagPrefix="dnn" TagName="MENU" src="~/DesktopModules/DDRMenu/Menu.ascx" %>
<!--SEO NOINDEX-->
<asp:Panel id="ControlPanel" runat="server" CssClass="dnnForm dnnControlPanel dnnClear">
    <div class="dnnCPHeader dnnClear">
        <div class="dnnCPHMode dnnLeft"><dnn:MENU ID="adminMenus" MenuStyle="admin/Menus/DNNAdmin" IncludeHidden="True" runat="server" OnInit="DetermineNodesToInclude" /></div>
        <div class="dnnCPHNav dnnRight">
			<asp:Label ID="lblUILanguage" runat="server" ResourceKey="lblUILanguage" Visible="false" />
            <dnn:DnnComboBox ID="ddlUICulture" runat="server" AutoPostBack="true" Visible="false" />
            <asp:Label id="lblMode" runat="server" ResourceKey="Mode" />
            <dnn:DnnComboBox ID="ddlMode" runat="server" AutoPostBack="true" OnClientSelectedIndexChanged="ddlModeClientSelectedIndexChanged">
                <Items>
                    <dnn:DnnComboBoxItem value="VIEW" ResourceKey="ModeView" />
                    <dnn:DnnComboBoxItem value="EDIT" ResourceKey="ModeEdit" />
                    <dnn:DnnComboBoxItem value="LAYOUT" ResourceKey="ModeLayout" />
                    <dnn:DnnComboBoxItem value="PREVIEW" ResourceKey="ModeMobilePreview" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <asp:HyperLink ID="hypMessage" runat="server" Target="_new" CssClass="dnnCPHMessage" />
    </div>
    <div id="BodyPanel" runat="server" class="dnnCPContent" style="display: none">
        <asp:Panel ID="CommonTasksPanel" runat="server" CssClass="cpcbCommonTasks dnnClear">
            <div class="cbctAddModule">
                <dnn:AddModule ID="AddMod" runat="server" />
            </div>
        </asp:Panel>
        <asp:Panel ID="CurrentPagePanel" runat="server" CssClass="cpcbCurrentPage dnnClear">
        	<div class="cbcpFiCol">
                <div class="cbcpPageActions dnnClear">
                    <h5><dnn:DnnLiteral ID="CurrentTabActions" runat="server" Text="CurrentTabActions" /></h5>
                    <dnn:DnnRibbonBarTool ID="EditCurrentSettings" runat="server" ToolName="PageSettings" ToolCssClass="cpEditCurrentPage" />
                    <dnn:DnnRibbonBarTool id="NewPage" runat="server" ToolName="NewPage" ToolCssClass="cpAddNewPage" />
                    <dnn:DnnRibbonBarTool id="CopyPage" runat="server" ToolName="CopyPage" ToolCssClass="cpCopyPage" />
                    <dnn:DnnRibbonBarTool id="DeletePage" runat="server" ToolName="DeletePage" ToolCssClass="cpDeletePage" />
                    <dnn:DnnRibbonBarTool id="ImportPage" runat="server" ToolName="ImportPage" ToolCssClass="cpImportPage" />
                    <dnn:DnnRibbonBarTool id="ExportPage" runat="server" ToolName="ExportPage" ToolCssClass="cpExportPage" />
                </div>
                <div class="cbcpPageCopy dnnClear">
                    <h5><dnn:DnnLiteral ID="CurrentTabCopyToChildren" runat="server" Text="CurrentTabCopyToChildren" /></h5>
                    <dnn:DnnRibbonBarTool ID="CopyPermissionsToChildren" runat="server" ToolName="CopyPermissionsToChildren" ToolCssClass="cpCopyPermissions" />
                    <dnn:DnnRibbonBarTool ID="CopyDesignToChildren" runat="server" ToolName="CopyDesignToChildren" ToolCssClass="cpCopyDesign" />
                </div>
            </div>
            <div class="cbctAddPage dnnLeft">
                <h5><dnn:DnnLiteral ID="CurrentTabAddPage" runat="server" Text="CurrentTabAddPage" /></h5>
                <dnn:AddPage id="AddPage" runat="server" />
            </div>
            <div class="cbcpPageEdit dnnRight">
                <h5><dnn:DnnLiteral ID="CurrentTabEditPage" runat="server" Text="CurrentTabEditPage" /></h5>
                <dnn:UpdatePage id="EditPage" runat="server" />
            </div>
        </asp:Panel>
        <asp:Panel ID="AdminPanel" runat="server" CssClass="cpcbAdmin">
            <div class="cbaManage dnnClear">
				<h5><dnn:DnnLiteral id="SiteTabManage" runat="server" Text="SiteTabManage" /></h5>
                <dnn:DnnRibbonBarTool id="NewUser" runat="server" ToolName="NewUser" ToolCssClass="cpNewUser" />
                <dnn:DnnRibbonBarTool id="NewRole" runat="server" ToolName="NewRole" ToolCssClass="cpNewRole" />
                <dnn:DnnRibbonBarTool id="UploadFile" runat="server" ToolName="UploadFile" ToolCssClass="cpUploadFile" />
                <dnn:DnnRibbonBarTool ID="ClearCache" runat="server" ToolName="ClearCache" ToolCssClass="cpClearCache" />
                <dnn:DnnRibbonBarTool ID="RecycleApp" runat="server" ToolName="RecycleApp" ToolCssClass="cpRecycleApp" />
            </div>
            <asp:Panel runat="server" ID="AdvancedToolsPanel" CssClass="cbhTools dnnClear">
                <h5><dnn:DnnLiteral id="SystemTabTools" runat="server" Text="SystemTabTools" /></h5>
                <dnn:DnnRibbonBarTool id="WebServerManager" runat="server" ToolInfo-ToolName="WebServerManager" ToolInfo-IsHostTool="True" ToolInfo-ModuleFriendlyName="WebServerManager" ToolCssClass="cpWebServerManager" />
                <dnn:DnnRibbonBarTool id="SupportTickets" runat="server" ToolInfo-ToolName="SupportTickets" ToolInfo-IsHostTool="True" ToolInfo-LinkWindowTarget="_Blank" NavigateUrl="http://customers.dotnetnuke.com/Main/frmTickets.aspx" ToolCssClass="cpSupportTickets" />
                <dnn:DnnRibbonBarTool id="ImpersonateUser" runat="server" ToolInfo-ToolName="ImpersonateUser" ToolInfo-IsHostTool="False" ToolInfo-ModuleFriendlyName="UserSwitcher" ToolCssClass="cpImpersonateUser" />
                <dnn:DnnRibbonBarTool id="IntegrityChecker" runat="server" ToolInfo-ToolName="IntegrityChecker" ToolInfo-IsHostTool="True" ToolInfo-ModuleFriendlyName="IntegrityChecker" ToolCssClass="cpIntegrityChecker" />
            </asp:Panel>
            <div class="cbhSwitchSite dnnClear">
                <h5><dnn:DnnLiteral id="SystemTabSwitchSite" runat="server" Text="SystemTabSwitchSite" /></h5>
                <dnn:SwitchSite id="SwitchSite" runat="server" />
            </div>
            <div class="cbcpPageHelp dnnClear">
                <h5><dnn:DnnLiteral ID="CurrentTabHelp" runat="server" Text="CurrentTabHelp" /></h5>
				<dnn:DnnRibbonBarTool id="Help" runat="server" ToolName="Help" ToolCssClass="cpPageHelp" />
            </div>            
        </asp:Panel>
    </div>
    
	<script type="text/javascript">
		jQuery(document).ready(function ($) {
			if (!$(".dnnControlPanel").data("loaded")) {
				var yesText = '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("Yes.Text", Localization.SharedResourceFile)) %>';
				var noText = '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("No.Text", Localization.SharedResourceFile)) %>';
				var titleText = '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(Localization.GetString("Confirm.Text", Localization.SharedResourceFile)) %>';

				// Client IDs for the following three have _CPCommandBtn appended as a rule
				$('#<%= DeletePage.ClientID %>_CPCommandBtn').dnnConfirm({
					text: '<%= GetButtonConfirmMessage("DeletePage") %>',
					yesText: yesText,
					noText: noText,
					title: titleText
				});
				$('#<%= CopyPermissionsToChildren.ClientID %>_CPCommandBtn').dnnConfirm({
					text: '<%= GetButtonConfirmMessage("CopyPermissionsToChildren") %>',
					yesText: yesText,
					noText: noText,
					title: titleText
				});
				$('#<%= CopyDesignToChildren.ClientID %>_CPCommandBtn').dnnConfirm({
					text: '<%= GetButtonConfirmMessage("CopyDesignToChildren") %>',
					yesText: yesText,
					noText: noText,
					title: titleText
				});

				$(".dnnControlPanel").data("loaded", true);
			};
		});

        function ddlModeClientSelectedIndexChanged(sender, e){
            var selectedItem = e.get_item();
            if(selectedItem != null && selectedItem.get_value() === "PREVIEW"){
                <%=PreviewPopup() %>;
                e.stopPropagation();
            }
        }
	</script>
</asp:Panel>
<!--END SEO-->