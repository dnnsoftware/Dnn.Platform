<%@ Control Language="c#" AutoEventWireup="True" Codebehind="EditorConfigManager.ascx.cs" Inherits="DNNConnect.CKEditorProvider.Module.EditorConfigManager" %>
<script type="text/javascript">
    var saveGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("SaveGroupName.Text", this.ResXFile, this.LangCode) %>';
    var deleteGroup = '<%= DotNetNuke.Services.Localization.Localization.GetString("DeleteGroup.Text", this.ResXFile, this.LangCode) %>';
    var editGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("EditGroupName.Text", this.ResXFile, this.LangCode) %>';
    var newGroupName = '<%= DotNetNuke.Services.Localization.Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) %>';
    var deleteToolbar = '<%= DotNetNuke.Services.Localization.Localization.GetString("DeleteToolbarButton.Text", this.ResXFile, this.LangCode) %>';
    var newRowName = '<%= DotNetNuke.Services.Localization.Localization.GetString("RowBreak.Text", this.ResXFile, this.LangCode) %>';
</script>
<style type="text/css">
    .groupButtons li {margin-left: -15px;}
</style>

<div>
    <h3><asp:Label runat="server" ID="ModuleHeader"></asp:Label></h3>

    <div class="optionsSidebar">
    <div class="managerSettingHeader">
        <asp:Label runat="server" ID="PortalOnlyLabel"></asp:Label>
    </div>  
    <div class="managerSettingHeader">
        <div class="portalOnlyCheckBox">
           <asp:CheckBox runat="server" ID="PortalOnly" Text="The Portal Only" Checked="True" AutoPostBack="True" />
        </div>
        <hr />
    </div>
    <div class="managerSettingHeader">
        <div class="portalTabsAndModulesTree">
            <asp:TreeView runat="server" ID="PortalTabsAndModulesTree" ExpandDepth="0"></asp:TreeView>
        </div>
        <hr />
    </div>
    <div class="managerSettingContent">
        <h4><asp:Label runat="server" ID="IconLegendLabel"></asp:Label></h4>
        <div class="iconLegend">
            <ul>
                <li>
                    <asp:Image ID="HostHasSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/HostHasSetting.png" />&nbsp;
                    <asp:Label runat="server" ID="HostHasSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="HostNoSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/HostNoSetting.png"/>&nbsp;
                    <asp:Label runat="server" ID="HostNoSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="PortalHasSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/PortalHasSetting.png" />&nbsp;
                    <asp:Label runat="server" ID="PortalHasSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="PortalNoSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/PortalNoSetting.png"/>&nbsp;
                    <asp:Label runat="server" ID="PortalNoSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="PageHasSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/PageHasSetting.png"/>&nbsp;
                    <asp:Label runat="server" ID="PageHasSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="PageNoSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/PageNoSetting.png" />&nbsp;
                    <asp:Label runat="server" ID="PageNoSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="ModuleHasSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/ModuleHasSetting.png" />&nbsp;
                    <asp:Label runat="server" ID="ModuleHasSettingLabel"></asp:Label>
                </li>
                <li>
                    <asp:Image ID="ModuleNoSettingImage" runat="server" ImageUrl="~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/ModuleNoSetting.png"/>&nbsp;
                    <asp:Label runat="server" ID="ModuleNoSettingLabel"></asp:Label>
                </li>
            </ul>
        </div>
    </div>
    </div>
    <div class="optionsContainer">
        <asp:PlaceHolder id="OptionsPlaceHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="ModuleInstanceInfoPlaceHolder" Visible="False">
            <div class="ui-widget">
                <div class="ui-state-error ui-corner-all" style="padding: 0 .7em;">
                    <p><span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>
                        <asp:Label runat="server" ID="ModuleInstanceInfo"></asp:Label>
                    </p>
                </div>

            </div>
        </asp:PlaceHolder>
    </div>
</div>