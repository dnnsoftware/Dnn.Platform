<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JavaScriptLibraryEditor.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Extensions.JavaScriptLibraryEditor" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls"%>

<h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("Title")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="LibraryNameLabel" runat="server" ControlName="LibraryName" />
        <asp:Label runat="server" ID="LibraryName" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="VersionLabel" runat="server" ControlName="Version" />
        <asp:Label ID="Version" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="FileNameLabel" runat="server" ControlName="FileName" />
        <asp:Label ID="FileName" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="ObjectNameLabel" runat="server" ControlName="ObjectName" />
        <asp:Label ID="ObjectName" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="LocationLabel" runat="server" ControlName="Location" />
        <asp:Label ID="Location" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="CDNLabel" runat="server" ControlName="CDN" />
        <asp:Label ID="CDN" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="CustomCDNLabel" runat="server" ControlName="CustomCDN" />
        <asp:TextBox ID="CustomCDN" runat="server"></asp:TextBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="DependOnPackagesLabel" runat="server" ControlName="DependOnPackages" />
        <asp:DataGrid ID="DependOnPackages" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" runat="server" GridLines="None" 
            Width="400px" CssClass="dnnGrid">
            <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
            <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
            <AlternatingItemStyle CssClass="dnnGridAltItem" />
            <Columns>
                <asp:BoundColumn headerStyle-width="250px" DataField="PackageName" HeaderText="Name" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" />                      
                <asp:TemplateColumn HeaderText="Version">
                    <HeaderStyle HorizontalAlign="Left" Wrap="False" Width="150px"/>
                    <ItemStyle HorizontalAlign="Left" />
                    <ItemTemplate><asp:Label ID="lblVersion" runat="server" Text='<%# FormatVersion(Container.DataItem) %>' /></ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="UsedByPackagesLabel" runat="server" ControlName="UsedByPackages" />
        <asp:DataGrid ID="UsedByPackages" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" runat="server" GridLines="None" 
            Width="400px" CssClass="dnnGrid">
            <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
            <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
            <AlternatingItemStyle CssClass="dnnGridAltItem" />
            <Columns>
                <asp:BoundColumn headerStyle-width="250px" DataField="Name" HeaderText="Name" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" />                      
                <asp:TemplateColumn HeaderText="Version">
                    <HeaderStyle HorizontalAlign="Left" Wrap="False" Width="150px"/>
                    <ItemStyle HorizontalAlign="Left" />
                    <ItemTemplate><asp:Label ID="lblVersion" runat="server" Text='<%# FormatVersion(Container.DataItem) %>' /></ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
    </div>
</fieldset>
