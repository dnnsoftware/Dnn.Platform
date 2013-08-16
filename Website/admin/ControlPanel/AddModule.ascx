<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanel.AddModule"
    CodeFile="AddModule.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Entities.Modules" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<script type="text/javascript">   
    function popupShareableWarning() {
        // Hide the Add Module panel.
        var parentControl = $('#<%= cmdAddModule.ClientID %>').closest('.megaborder');
        if (parentControl != null) {
            parentControl.hide();
        }

        $('#shareableWarning').dialog({
            autoOpen: true,
            resizable: false,
            modal: true,
            width: '500px',
            zIndex: 1000,
            stack: false,
            title: '<%= GetString("ShareableWarningTitle") %>',
            dialogClass: 'dnnFormPopup dnnClear',
            open: function () { },
            close: function () { }
        });
    }

    function openShareableWarning() {
        var sharing = (dnn.getVar('moduleSharing') || 'false') == 'true';
        if (sharing) {
            var siteCombo = $find('<%= SiteList.ClientID %>');
            var selectedPortalId = siteCombo ? siteCombo.get_value() : '-1';
            var tabCombo = $find('<%= PageLst.ClientID %>');
            var selectedTabId = tabCombo ? tabCombo.get_value() : '-1';
            var selectedModuleId = $find('<%= ModuleLst.RadComboBoxClientId %>').get_value();

            if(selectedPortalId === '-1') {
                return true;
            }
            var parameters = {
                ModuleId: selectedModuleId,
                TabId: selectedTabId,
                PortalId: selectedPortalId
            };

            var service = $.dnnSF();
            var serviceUrl = service.getServiceRoot('internalservices') + 'ModuleService/GetModuleShareable';
            
            jQuery.ajax({
                url: serviceUrl,
                type: 'GET',
                async: false,
                data: parameters,
                success: function (m) {
                    if (typeof (m) == 'undefined') {
                        return false;
                    }

                    if (m.RequiresWarning) {
                        popupShareableWarning();
                    }
                    else {
                        __doPostBack('<%= cmdAddModule.UniqueID %>', '');
                    }
                },
                error: function () {
                }
            });

            return false;
        }
        
        return true;
    }

    function confirmAddShareable() {
        hideShareableWarning();
    }

    function cancelAddShareable(e) {
        if (window.event != null) {
            event.returnValue = false;
            event.cancel = true;
        } else {
            if (e != null) {
                e.preventDefault();
            }
        }
        hideShareableWarning();
    }

    function hideShareableWarning() {
        $('#shareableWarning').dialog('close');
    }

    $(function () {
        var cmdAddModuleClicked = function () {
            var result = openShareableWarning();
            if (result)
                __doPostBack('<%= cmdAddModule.UniqueID %>', '');
        };
        $('#cmdAddModule_Fake').unbind('click').bind('click', cmdAddModuleClicked); 
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            $('#cmdAddModule_Fake').unbind('click').bind('click', cmdAddModuleClicked);
        });
    });
</script>
<asp:UpdatePanel ID="UpdateAddModule" runat="server" ChildrenAsTriggers="true">
    <contenttemplate>
        <div class="dnnCPAddModule">
            <h5><asp:Label runat="server" ResourceKey="AddModule" /></h5>            
            <asp:RadioButton ID="AddNewModule" runat="server" ResourceKey="AddNew" GroupName="AddModule" Checked="true" AutoPostBack="true" />           
            <asp:RadioButton ID="AddExistingModule" runat="server" ResourceKey="AddExisting" GroupName="AddModule" AutoPostBack="true" />            
            <asp:HyperLink ID="hlMoreExtensions" runat="server" CssClass="dnnPrimaryAction" Visible="false" />
        </div>
        <div class="dnnCPModSelection dnnFormItem">
            <h5><asp:Label ID="Label1" runat="server" ResourceKey="SelectModule" /></h5>
            <asp:Panel ID="SiteListPanel" runat="server" Visible="True" CssClass="dnnClear">
               <asp:Label ID="SiteLbl" runat="server" ResourceKey="Site" AssociatedControlID="SiteList"></asp:Label>
               <dnn:DnnComboBox ID="SiteList" runat="server" AutoPostBack="true" />
            </asp:Panel>
            <asp:Panel ID="CategoryListPanel" runat="server" Visible="false" CssClass="dnnClear">
                <asp:Label ID="CategoryListLbl" runat="server" ResourceKey="Category" AssociatedControlID="CategoryList" />
                <dnn:DnnComboBox ID="CategoryList" runat="server" AutoPostBack="true" DataTextField="Name" DataValueField="Name" />
            </asp:Panel>
            <asp:Panel ID="PageListPanel" runat="server" Visible="false" CssClass="dnnClear">
                <asp:Label ID="PageListLbl" runat="server" ResourceKey="Page" AssociatedControlID="PageLst" />
                <dnn:DnnComboBox ID="PageLst" runat="server" AutoPostBack="true" />
            </asp:Panel>
            <div class="dnnClear">
                <asp:Label ID="ModuleLstLbl" runat="server" ResourceKey="Module" AssociatedControlID="ModuleLst" />
                <dnn:DnnModuleComboBox ID="ModuleLst" runat="server" />
            </div>
            <asp:Panel ID="TitlePanel" runat="server" Visible="true" CssClass="dnnClear">
                <asp:Label ID="TitleLbl" runat="server" ResourceKey="Title" AssociatedControlID="Title" />
                <asp:TextBox ID="Title" runat="server" />
            </asp:Panel>
            <div class="dnnClear">
                <asp:Label ID="VisibilityLstLbl" runat="server" ResourceKey="Visibility" AssociatedControlID="VisibilityLst" />
                <dnn:DnnComboBox ID="VisibilityLst" runat="server" />
            </div>
        </div>
        <div class="dnnCPModLocation dnnFormItem">
            <h5><asp:Label ID="Label2" runat="server" ResourceKey="LocateModule" /></h5>
            <div class="dnnClear">
                <asp:Label ID="PaneLstLbl" runat="server" ResourceKey="Pane" AssociatedControlID="PaneLst" />
                <dnn:DnnComboBox ID="PaneLst" runat="server" AutoPostBack="true" />
            </div>
            <div class="dnnClear">
                <asp:Label ID="PositionLstLbl" runat="server" ResourceKey="Insert" AssociatedControlID="PositionLst" />
                <dnn:DnnComboBox ID="PositionLst" runat="server" AutoPostBack="true" />
            </div>
            <div class="dnnClear">
                <asp:Label ID="PaneModulesLstLbl" runat="server" ResourceKey="Module" AssociatedControlID="PaneModulesLst" />
                <dnn:DnnComboBox ID="PaneModulesLst" runat="server" />
            </div>
            <div class="dnnFormCheckbox"><asp:CheckBox ID="chkCopyModule" runat="server" /></div>
        </div>
        <a href="javascript:void(0)" class="dnnPrimaryAction" id="cmdAddModule_Fake"><%= GetString("AddModule.Text") %></a>
        <asp:LinkButton ID="cmdAddModule" runat="server" ResourceKey="AddModule" CssClass="Hidden"  />
    </contenttemplate>
</asp:UpdatePanel>

