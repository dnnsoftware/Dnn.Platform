<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FolderMappings.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.FolderMappings" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<div class="dnnForm dnnFolderMappings dnnClear">
    <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="DescriptionLabel" runat="server" resourcekey="Description.Text" /></div>
    <div>
        <h2><asp:Label ID="TableHeaderLabel" runat="server" resourcekey="TableHeader.Text" /></h2>
        <dnn:dnngrid id="MappingsGrid" runat="server" autogeneratecolumns="false" Width="98%" OnNeedDataSource="MappingsGrid_OnNeedDataSource" AllowAutomaticUpdates="false" AllowAutomaticDeletes="false" OnItemCommand="MappingsGrid_OnItemCommand" OnItemDataBound="MappingsGrid_OnItemDataBound">
            <MasterTableView DataKeyNames="FolderMappingID">
                <Columns>
                    <dnn:DnnGridTemplateColumn HeaderStyle-Width="60px">
                        <ItemTemplate>
                            <dnn:commandbutton id="EditMappingButton" runat="server" IconKey="Edit" commandname="Edit" commandargument='<%# Eval("FolderMappingID") %>' causesvalidation="false" visible='<%# Eval("IsEditable") %>' />
                            <dnn:commandbutton id="DeleteMappingButton" runat="server" IconKey="Delete" commandname="Delete" commandargument='<%# Eval("FolderMappingID") %>' causesvalidation="false" visible='<%# Eval("IsEditable") %>' />
                        </ItemTemplate>
                    </dnn:DnnGridTemplateColumn>
                    <dnn:DnnGridBoundColumn DataField="MappingName" HeaderText="Name" />
                    <dnn:DnnGridBoundColumn DataField="FolderProviderType" HeaderText="Type" />
                </Columns>
            </MasterTableView>
        </dnn:dnngrid>
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="NewMappingButton" resourcekey="cmdNewMapping" runat="server" cssclass="dnnPrimaryAction" causesvalidation="False" /></li>
        <li><asp:HyperLink id="CancelButton" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" /></li>
    </ul>
</div>