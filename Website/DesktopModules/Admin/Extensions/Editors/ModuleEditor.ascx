<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.ModuleEditor" CodeFile="ModuleEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.Security.Permissions.Controls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<h2 class="dnnFormSectionHead" id="moduleSettingsHead" runat='server'><a href="" class="dnnLabelExpanded"><%=LocalizeString("ModuleSettings")%></a></h2>
<fieldset>
    <asp:Panel ID="helpPanel" runat="server" CssClass="dnnFormMessage dnnFormInfo">
        <asp:Label ID="lblHelp" runat="server" />
    </asp:Panel>
    <dnn:DnnFormEditor id="desktopModuleForm" runat="Server" FormMode="Short">
        <Items>
            <dnn:DnnFormLiteralItem ID="moduleName" runat="server" DataField = "ModuleName" />
            <dnn:DnnFormTextBoxItem ID="folderName" runat="server" DataField = "FolderName" />
            <dnn:DnnFormComboBoxItem ID="category" runat="server" DataField = "Category" ListTextField="Name" ListValueField="Name" />
            <dnn:DnnFormTextBoxItem ID="controllerClass" runat="server" DataField = "BusinessControllerClass" Columns="55" />
            <dnn:DnnFormTextBoxItem ID="dependencies" runat="server" DataField = "Dependencies" Columns="55"/>
            <dnn:DnnFormTextBoxItem ID="permissions" runat="server" DataField = "Permissions" Columns="55"/>
            <dnn:DnnFormLiteralItem ID="isPortable" runat="server" DataField="IsPortable" />
            <dnn:DnnFormLiteralItem ID="isSearchable" runat="server" DataField="IsSearchable" />
            <dnn:DnnFormLiteralItem ID="isUpgradable" runat="server" DataField="IsUpgradeable" />
            <dnn:DnnFormComboBoxItem ID="Shareable" runat="server" DataField="Shareable" ListTextField="Name" ListValueField="Name" />
            <dnn:DnnFormToggleButtonItem ID="IsPremiumm" runat="server" DataField="IsPremium" />
            <dnn:DnnFormTemplateItem ID="PremiumModules" runat="server">
                <ItemTemplate>
				    <dnn:Label ID="plPremium" runat="server" ControlName="ctlPortals" />
                    <dnn:DualListBox id="ctlPortals" runat="server" DataValueField="PortalID" DataTextField="PortalName" AddKey="AddPortal" RemoveKey="RemovePortal" AddAllKey="AddAllPortals" RemoveAllKey="RemoveAllPortals" AddImageURL="~/images/rt.gif" AddAllImageURL="~/images/ffwd.gif" RemoveImageURL="~/images/lt.gif" RemoveAllImageURL="~/images/frev.gif" ContainerStyle-HorizontalAlign="Center" >
                        <AvailableListBoxStyle Height="130px" Width="225px" />
                        <HeaderStyle />
                        <SelectedListBoxStyle Height="130px" Width="225px"  />
                    </dnn:DualListBox>                            
                </ItemTemplate>
            </dnn:DnnFormTemplateItem>
        </Items>
    </dnn:DnnFormEditor>
    <asp:Panel ID="pnlPermissions" runat="server" Visible="false">
        <div><dnn:DesktopModulePermissionsGrid ID="dgPermissions" runat="server"  /></div>
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        </ul>
    </asp:Panel>
</fieldset>
<asp:Panel ID="pnlDefinitions" runat="server" Visible="False">
    <h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("Definitions")%></a></h2>
    <fieldset>
        <div id="definitionSelectRow" class="dnnFormItem" runat="server">
            <dnn:label id="plSelectDefinition" controlname="cboDefinitions" runat="server" />
            <dnn:DnnComboBox id="cboDefinitions" runat="server" datatextfield="DefinitionName" datavaluefield="ModuleDefId" autopostback="True" CssClass="dnnFixedSizeComboBox" />
            <asp:LinkButton id="cmdAddDefinition" resourcekey="cmdAddDefinition" runat="server" CssClass="dnnSecondaryAction" CausesValidation="false" />
        </div>
        <asp:Panel ID="pnlDefinition" runat="server" Visible="false">
            <dnn:DnnFormEditor id="definitionsEditor" runat="Server" FormMode="Short">
                <Items>
                    <dnn:DnnFormLiteralItem ID="definitionNameLiteral" runat="server" DataField = "DefinitionName" />
                    <dnn:DnnFormTextBoxItem ID="definitionName" runat="server" DataField="DefinitionName" Required="True" />
                    <dnn:DnnFormTextBoxItem ID="friendlyName" runat="server" DataField = "FriendlyName" Required="true" />
                    <dnn:DnnFormTextBoxItem ID="cacheTime" runat="server" DataField = "DefaultCacheTime" />
                </Items>
            </dnn:DnnFormEditor>
            <asp:Panel ID="pnlControls" CssClass="dnnFormItem" runat="server" Visible="false">
                <dnn:label ID="lblControls" runat="server" ResourceKey="Controls" controlname="grdControls" />
		        <asp:datagrid id="grdControls" runat="server" cellspacing="0" autogeneratecolumns="false" enableviewstate="true" GridLines="None" CssClass="dnnASPGrid">
                    <HeaderStyle CssClass="dnnGridHeader" />
                    <ItemStyle CssClass="dnnGridItem" />
                    <Columns>                        
                        <dnn:textcolumn  DataField="ControlKey" HeaderText="Control" />
                        <dnn:textcolumn  DataField="ControlTitle" HeaderText="Title" />
                        <dnn:textcolumn  DataField="ControlSrc" HeaderText="Source" />
                        <dnn:imagecommandcolumn headerStyle-width="18px" CommandName="Edit" IconKey="Edit" EditMode="URL" KeyField="ModuleControlID" />
                        <dnn:imagecommandcolumn headerStyle-width="18px" commandname="Delete" IconKey="Delete" keyfield="ModuleControlID" />
                    </Columns>
                </asp:datagrid>
                <asp:Hyperlink id="cmdAddControl" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdAddControl" />
            </asp:Panel>
            <div class="dnnFormItem"><asp:Label ID="lblDefinitionError" runat="server" CssClass="dnnFormMessage dnnFormError" Visible="false" ResourceKey="DuplicateName" /> </div>
            <ul class="dnnActions dnnClear">
    	        <li><asp:LinkButton id="cmdUpdateDefinition" runat="server" CssClass="dnnPrimaryAction" /></li>
                <li><asp:LinkButton id="cmdDeleteDefinition" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdDeleteDefinition" Causesvalidation="False" /></li>
            </ul>
        </asp:Panel>
    </fieldset>
</asp:Panel>