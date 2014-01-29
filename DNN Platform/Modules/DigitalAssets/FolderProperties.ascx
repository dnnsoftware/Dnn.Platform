<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FolderProperties.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.FolderProperties" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dam" tagName="PreviewPanelControl" src="~/DesktopModules/DigitalAssets/PreviewPanelControl.ascx"%>

<asp:Panel ID="ScopeWrapper" runat="server">
    <asp:Panel runat="server" ID="FolderContentPanel">
        <ul class="dnnAdminTabNav dnnClear">
            <li id="dnnModuleDigitalAssetsGeneralTab"><a href="#dnnModuleDigitalAssetsGeneral">
                <%=LocalizeString("GeneralTab")%></a>
            </li>
            <li id="dnnModuleDigitalAssetsPermissionsTab"><a href="#dnnModuleDigitalAssetsPermissions">
                <%=LocalizeString("PermissionsTab")%></a>
            </li>
        </ul>    
        <div class="dnnClear" id="dnnModuleDigitalAssetsPropertiesContent" >
            <div class="dnnClear" id="dnnModuleDigitalAssetsGeneral">
                <div class="dnnModuleDigitalAssetsPreviewInfo" id="dnnModuleDigitalAssetsFolderPreview">                    
                    <dam:PreviewPanelControl runat="server" ID="FolderInfoPreviewPanel"></dam:PreviewPanelControl>
                </div>
                <div class="dnnModuleDigitalAssetsGeneralProperties dnnForm" id="dnnModuleDigitalAssetsFolderProperties">
                    <div class="dnnFormItem">
                        <dnn:Label ID="FolderNameLabel" ControlName="FolderNameInput" CssClass="dnnFormRequired" ResourceKey="FolderNameLabel" runat="server" Suffix=":" />
                        <asp:TextBox type="text" ID="FolderNameInput" runat="server"/>
                        <asp:RequiredFieldValidator ID="FolderNameValidator" CssClass="dnnFormMessage dnnFormError"
                            runat="server" resourcekey="FolderNameRequired.ErrorMessage" Display="Dynamic" ControlToValidate="FolderNameInput" />
                        <asp:RegularExpressionValidator runat="server" Display="Dynamic" ControlToValidate="FolderNameInput" CssClass="dnnFormMessage dnnFormError" 
                            ID="FolderNameInvalidCharactersValidator"/>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:Label ID="FolderTypeLabel" ControlName="FolderTypeLiteral" ResourceKey="FolderTypeLabel" runat="server" Suffix=":" />
                        <asp:Label ID="FolderTypeLiteral" runat="server" CssClass="dnnModuleDigitalAssetsGeneralPropertiesSingleField"/>
                    </div>
                    <asp:Panel runat="server" ID="FolderDynamicFieldsContainer"></asp:Panel>
                </div>
            </div>
            <div id="dnnModuleDigitalAssetsPermissions">            
                <dnn:folderpermissionsgrid id="PermissionsGrid" runat="server"/>
                <div id="copyPermissionRow" runat="server">
					<div class="dnnFormItem"><dnn:Label ID="lblCopyPerm" runat="server" ResourceKey="lblCopyPerm" /></div>
					<asp:LinkButton ID="cmdCopyPerm" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCopyPerm" />
				</div>
            </div>
        </div>
    </asp:Panel>
    <div id="dnnModuleDigitalAssetsButtonPane">
        <ul class="dnnActions dnnClear">
            <li>
                <asp:LinkButton ID="SaveButton" runat="server" class="dnnPrimaryAction dnnModuleDigitalAssetsSaveFolderPropertiesButton" resourcekey="SaveButton" /></li>
            <li>
                <asp:LinkButton ID="CancelButton" CausesValidation="False" runat="server" class="dnnSecondaryAction" resourcekey="CancelButton" /></li>
        </ul>
    </div>
</asp:Panel>
<script type="text/javascript">
    // IE8 doesn't like using var dnnModule = dnnModule || {}
    if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

    dnnModule.digitalAssets = dnnModule.digitalAssets || {};
    dnnModule.digitalAssets.folderProperties = function ($) {
        function init(controls, settings) {
            parent.$("#iPopUp").dialog('option', 'title', settings.dialogTitle);
            setupDnnTabs(controls, settings);            
        }
        
        function setupDnnTabs(controls, settings) {
            var options = {};
            var selectedTab = parseInt(settings.selectedTab);
            if (selectedTab != NaN && selectedTab != null) options.selected = selectedTab;
            $('#' + controls.scopeWrapperId).dnnTabs(options).dnnPanels();

            var permissionTabId = controls.permissionTabId;
            var canAdminPerms = (settings.canAdminPermissions === 'true');
            if (!canAdminPerms) {
                hideTab(permissionTabId);
            } else {
                showTab(permissionTabId);
            }
        }
        
        function hideTab(tabId) {
            $('#' + tabId).hide();
        }
        
        function showTab(tabId) {
            $('#' + tabId).css("display", "");            
        }

        return {
            init: init
        };
    }(jQuery);

    dnnModule.digitalAssets.folderProperties.init(
        {
            scopeWrapperId: '<%=ScopeWrapper.ClientID %> ',
            permissionTabId: 'dnnModuleDigitalAssetsPermissionsTab',
            dialogTitleContainerId : 'ui-dialog-title-iPopUp'
        }, 
        {
            selectedTab: '<%=(!IsPostBack ? "0" : "-1")%>',
            canAdminPermissions: '<%=Localization.GetSafeJSString((HasFullControl && !IsHostPortal).ToString().ToLowerInvariant()) %>',
            dialogTitle: '<%=Localization.GetSafeJSString(DialogTitle)%>'
        }
    );
</script>
