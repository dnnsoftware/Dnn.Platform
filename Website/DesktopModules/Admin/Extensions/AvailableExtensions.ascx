<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.AvailableExtensions" CodeFile="AvailableExtensions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="LanguagePacks" Src="~/DesktopModules/admin/AdvancedSettings/LanguagePacks.ascx" %>

<div class="dnnForm exieContent dnnClear">
    <h2 id="Panel-Language" class="dnnFormSectionHead"><a href="" class=""><%= Localization.GetString("Language.Type", LocalResourceFile) %></a></h2>
    <dnn:LanguagePacks runat="server" ID="languagePacks" ShowDescription="False" />                   
</div>

<asp:Repeater ID="extensionTypeRepeater" runat="server">
    <ItemTemplate>
        <div class="dnnForm exieContent dnnClear">
            <h2 id="Panel-<%# GetPackageType(Container.DataItem) %>" class="dnnFormSectionHead"><a href="" class=""><%# GetPackageType(Container.DataItem) %></a></h2>
            <fieldset>             
                <asp:DataGrid ID="extensionsGrid" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" 
                    runat="server" GridLines="None" Width="100%" CssClass="dnnGrid">
                    <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
                    <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
                    <AlternatingItemStyle CssClass="dnnGridAltItem" />
                    <Columns>
		                <asp:TemplateColumn>
                            <ItemStyle HorizontalAlign="Center" Width="32px" Height="32px"/>
                            <ItemTemplate>
                                <asp:Image ID="imgIcon" runat="server" Width="32px" Height="32px" ImageUrl='<%# GetPackageIcon(Container.DataItem) %>' ToolTip='<%# GetPackageDescription(Container.DataItem) %>' />
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <dnn:textcolumn headerStyle-width="150px" DataField="FriendlyName" HeaderText="Name">
                            <itemstyle Font-Bold="true" />
                        </dnn:textcolumn>
                        <dnn:textcolumn headerStyle-width="275px" ItemStyle-HorizontalAlign="Left" DataField="Description" HeaderText="Description" />
                        <asp:TemplateColumn HeaderText="Version" >
                            <HeaderStyle HorizontalAlign="Left" Wrap="False" Width="70px"/>
                            <ItemStyle HorizontalAlign="Left"/>
                            <ItemTemplate>
                                <asp:Label ID="lblVersion" runat="server" Text='<%# FormatVersion(Container.DataItem) %>' />
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn headerStyle-width="160px">
                            <ItemTemplate>
                                    <asp:HyperLink id="cmdInstall" runat="server" CssClass="dnnSecondaryAction" ResourceKey="installExtension" />
                                    <asp:HyperLink id="cmdDownload" runat="server" CssClass="dnnSecondaryAction" ResourceKey="download" />
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
            </fieldset>
        </div>
    </ItemTemplate>
</asp:Repeater>