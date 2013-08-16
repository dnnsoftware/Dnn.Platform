<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.InstalledExtensions" CodeFile="InstalledExtensions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.WebControls" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>                
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnFormMessage dnnFormInfo" id="lblUpdateRow" runat="server"><asp:Label ID="lblUpdate" runat="server" resourceKey="lblUpdate" /></div>
<div class="dnnFormItem" id="languageSelectorRow" runat="server">
    <dnn:Label ID="plLocales" runat="server" ControlName="cboLocales" />
    <asp:DropDownList ID="cboLocales" runat="server" DataTextField="Text" DataValueField="Code" AutoPostBack="true" />
</div>
<asp:Repeater ID="extensionTypeRepeater" runat="server">
    <ItemTemplate>
        <div class="dnnForm exieContent dnnClear">
            <h2 id="Panel-<%# GetPackageType(Container.DataItem) %>" class="dnnFormSectionHead"><a href="" class=""><%# GetPackageType(Container.DataItem) %></a></h2>
            <fieldset>
                <asp:DataGrid ID="extensionsGrid" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" runat="server" GridLines="None" Width="100%" CssClass="dnnGrid">
                    <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
                    <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
                    <AlternatingItemStyle CssClass="dnnGridAltItem" />
                    <Columns>                      
					    <asp:TemplateColumn>
                            <ItemStyle HorizontalAlign="Center" Width="32px" Height="32px"/>
                            <ItemTemplate><asp:Image ID="imgIcon" runat="server" Width="32px" Height="32px" ImageUrl='<%# GetPackageIcon(Container.DataItem) %>' ToolTip='<%# GetPackageDescription(Container.DataItem) %>' /></ItemTemplate>
                        </asp:TemplateColumn>
                        <dnn:textcolumn headerStyle-width="150px" DataField="FriendlyName" HeaderText="Name" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
                            <itemstyle Font-Bold="true" />
                        </dnn:textcolumn>
                        <asp:TemplateColumn FooterText="Portal">
                            <HeaderStyle HorizontalAlign="Left" Wrap="False" Width="18px" />
                            <ItemStyle HorizontalAlign="Left" />
                            <ItemTemplate>
							    <dnn:DnnImage ID="imgAbout" runat="server" ToolTip='<%# GetAboutTooltip(Container.DataItem) %>' IconKey="About" Visible='<%# ((String)(DataBinder.Eval(Container.DataItem, "PackageType")) == "Skin" || ((String)DataBinder.Eval(Container.DataItem, "PackageType")) == "Container") %>' />
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <dnn:textcolumn headerStyle-width="275px" ItemStyle-HorizontalAlign="Left" DataField="Description" HeaderText="Description"  ItemStyle-CssClass="dnnGridHeaderTD-Border" />
                        <asp:TemplateColumn HeaderText="Version">
                            <HeaderStyle HorizontalAlign="Left" Wrap="False" />
                            <ItemStyle HorizontalAlign="Left" />
                            <ItemTemplate><asp:Label ID="lblVersion" runat="server" Text='<%# FormatVersion(Container.DataItem) %>' /></ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn HeaderText="InUse">
                            <HeaderStyle HorizontalAlign="Left" Wrap="False" />
                            <ItemStyle HorizontalAlign="Left" />
                            <ItemTemplate>
                                <asp:Label ID="lblUserInfo" runat="server" Text='<%# GetIsPackageInUseInfo(Container.DataItem) %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn HeaderText="Upgrade" >
                            <HeaderStyle HorizontalAlign="Left" Wrap="False" Width="120px" />
                            <ItemStyle HorizontalAlign="Left" />
                            <ItemTemplate><asp:Label ID="lblUpgrade" runat="server" Text='<%# UpgradeService((Version)DataBinder.Eval(Container.DataItem,"Version"),DataBinder.Eval(Container.DataItem,"PackageType").ToString(),DataBinder.Eval(Container.DataItem,"Name").ToString()) %>' ></asp:Label></ItemTemplate>
                        </asp:TemplateColumn>
                        <dnn:imagecommandcolumn headerStyle-width="18px" CommandName="Edit" IconKey="Edit" EditMode="URL" KeyField="PackageID" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" />
                        <dnn:imagecommandcolumn headerStyle-width="18px" commandname="Delete" IconKey="Delete" EditMode="URL" keyfield="PackageID" />
                    </Columns>
                </asp:DataGrid>
                <asp:Label ID="noResultsLabel" runat="server" resourcekey="NoResults" Visible="false" />
            </fieldset>
        </div>
    </ItemTemplate>
</asp:Repeater>