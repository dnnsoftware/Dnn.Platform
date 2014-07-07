<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileProperties.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.FileProperties" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<asp:Panel ID="ScopeWrapper" runat="server">
    
    <asp:Panel runat="server" ID="FolderContentPanel" CssClass="dnnModuleDigitalAssetsFilePropertiesContent">
        <ul class="dnnAdminTabNav dnnClear" runat="server" ID="Tabs">
            <li id="dnnModuleDigitalAssetsGeneralTab">
                <asp:HyperLink href="#dnnModuleDigitalAssetsGeneral" runat="server" ID="GeneralTabLink" resourcekey="GeneralTab" />
            </li>
        </ul>    
        <asp:Panel runat="server" ID="TabsPanel" class="dnnClear" >
            <div class="dnnClear" id="dnnModuleDigitalAssetsGeneral">
                <div class="dnnModuleDigitalAssetsPreviewInfo" id="dnnModuleDigitalAssetsFilePreview">
                    <asp:Panel runat="server" ID="PreviewPanelContainer"></asp:Panel>                    
                </div>
                <div class="dnnModuleDigitalAssetsGeneralProperties dnnForm" id="dnnModuleDigitalAssetsFileProperties">
                    <asp:Panel runat="server" ID="FileFieldsContainer"></asp:Panel>                  
                </div>
            </div>
        </asp:Panel>
    </asp:Panel>
    <div id="dnnModuleDigitalAssetsButtonPane">
        <ul class="dnnActions dnnClear">
            <li>
                <asp:LinkButton ID="SaveButton" runat="server" class="dnnPrimaryAction" resourcekey="SaveButton" /></li>
            <li>
                <asp:LinkButton ID="CancelButton" CausesValidation="False" runat="server" class="dnnSecondaryAction" resourcekey="CancelButton" /></li>
        </ul>
    </div>
</asp:Panel>
<script type="text/javascript">
    // IE8 doesn't like using var dnnModule = dnnModule || {}
    if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

    dnnModule.digitalAssets = dnnModule.digitalAssets || {};
    dnnModule.digitalAssets.fileProperties = function ($) {
        function init(controls, settings) {
            parent.$("#iPopUp").dialog('option', 'title', settings.dialogTitle);
            setupDnnTabs(controls, settings);
            setupDnnCheckboxes(controls);
            
            var permissionTabId = controls.permissionTabId;
            var canAdminPerms = (settings.canAdminPermissions === 'true');
            if (!canAdminPerms) {
                hideTab(permissionTabId);
            } else {
                showTab(permissionTabId);
            }

            //set active tab
            var activeTab = '<%= ActiveTab %>';
            if (activeTab) {
                $('#' + activeTab + ' a').click();
            }
        }
        
        function setupDnnTabs(controls, settings) {
            var options = {};
            var selectedTab = parseInt(settings.selectedTab);
            if (selectedTab != NaN && selectedTab != null) options.selected = selectedTab;
            $('#' + controls.scopeWrapperId).dnnTabs(options).dnnPanels();
        }
        
        function setupDnnCheckboxes(controls) {
            $('#' + controls.scopeWrapperId + ".dnnModuleDigitalAssetsGeneralPropertiesCheckBoxGroup input[type='checkbox']").dnnCheckbox();
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

    dnnModule.digitalAssets.fileProperties.init(
        {
            scopeWrapperId: '<%=ScopeWrapper.ClientID %> ',
            permissionTabId: 'dnnModuleDigitalAssetsPermissionsTab',
            dialogTitleContainerId: 'ui-dialog-title-iPopUp'
        },
        {
            selectedTab: '<%=(!IsPostBack ? "0" : "-1")%>',            
            canAdminPermissions: '<%=Localization.GetSafeJSString(CanManageFolder.ToString().ToLowerInvariant()) %>',
            dialogTitle: '<%=Localization.GetSafeJSString(DialogTitle)%>'
        }
    );
</script>