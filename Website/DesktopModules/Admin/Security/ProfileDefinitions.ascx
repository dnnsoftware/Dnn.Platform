<%@ Control Inherits="DotNetNuke.Modules.Admin.Users.ProfileDefinitions" Language="C#" AutoEventWireup="false" Codebehind="ProfileDefinitions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnProfileProperties">
    <asp:datagrid id="grdProfileProperties" AutoGenerateColumns="false" 
                width="96%" CellPadding="4" GridLines="None" CssClass="dnnGrid" Runat="server">
        <headerstyle cssclass="dnnGridHeader" verticalalign="Top" horizontalalign="Left" />
        <itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
        <alternatingitemstyle cssclass="dnnGridAltItem" />
        <edititemstyle cssclass="dnnFormInput" />
        <selecteditemstyle cssclass="dnnFormError" />
        <footerstyle cssclass="dnnGridFooter" />
        <pagerstyle cssclass="dnnGridPager" />
        <columns>
            <dnn:imagecommandcolumn CommandName="Edit" Text="Edit" IconKey="Edit" KeyField="PropertyDefinitionID" EditMode="URL" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder"  HeaderStyle-Width="18px"/>
            <dnn:imagecommandcolumn CommandName="Delete" Text="Delete" IconKey="Delete" KeyField="PropertyDefinitionID" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" HeaderStyle-Width="18px"/>
            <dnn:imagecommandcolumn commandname="MoveDown" IconKey="Dn" keyfield="PropertyDefinitionID" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" HeaderStyle-Width="18px"/>
            <dnn:imagecommandcolumn commandname="MoveUp" IconKey="Up" keyfield="PropertyDefinitionID" HeaderStyle-Width="18px" />
            <dnn:textcolumn DataField="PropertyName" HeaderText="Name" Width="100px" />
            <dnn:textcolumn DataField="PropertyCategory" HeaderText="Category" Width="80px" />
            <asp:TemplateColumn HeaderText="DataType">
                <ItemStyle Width="60px"></ItemStyle>
                <ItemTemplate>
                    <asp:label id="lblDataType" runat="server" Text='<%# DisplayDataType((DotNetNuke.Entities.Profile.ProfilePropertyDefinition)Container.DataItem) %>'></asp:label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <dnn:textcolumn DataField="Length" HeaderText="Length" Width="50px" />
            <dnn:textcolumn DataField="DefaultValue" HeaderText="DefaultValue" Width="80px" />
            <dnn:textcolumn DataField="ValidationExpression" HeaderText="ValidationExpression" Width="100px" />
            <asp:TemplateColumn HeaderText="DefaultVisibility">
                <ItemStyle Width="100px"></ItemStyle>
                <ItemTemplate>
                    <asp:label id="lbDefaultVisibility" runat="server" Text='<%# DisplayDefaultVisibility((DotNetNuke.Entities.Profile.ProfilePropertyDefinition)Container.DataItem) %>'></asp:label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <dnn:checkboxcolumn DataField="Required" HeaderText="Required" AutoPostBack="True" />
            <dnn:checkboxcolumn DataField="Visible" HeaderText="Visible" AutoPostBack="True" />
        </columns>
    </asp:datagrid>
    <ul id="actionsRow" runat="server" class="dnnActions dnnClear">
        <li><asp:HyperLink id="cmdAdd" runat="server" CssClass="dnnSecondaryAction" resourcekey="AddContent.Action" /></li>
        <li><asp:LinkButton id="cmdRefresh" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRefresh" /></li>
    </ul>
</div>