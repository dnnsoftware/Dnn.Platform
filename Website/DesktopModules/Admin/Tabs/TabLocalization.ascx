<%@ Control Language="C#" AutoEventWireup="false" Explicit="True" CodeFile="TabLocalization.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Tabs.TabLocalization" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnTabLocalization dnnClear">
    <dnnweb:DnnGrid ID="localizedTabsGrid" runat="server" AutoGenerateColumns="false" AllowMultiRowSelection="true" Width="100%" 
        CssClass="dnnTabLocalizationGrid">
        <ClientSettings >
            <Selecting AllowRowSelect="true" />
        </ClientSettings>
        <MasterTableView DataKeyNames="CultureCode">
            <Columns>
                <dnnweb:DnnGridClientSelectColumn HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="40px" />
                <dnnweb:DnnGridTemplateColumn UniqueName="Language" HeaderText="Language" ItemStyle-VerticalAlign="Middle" ItemStyle-Width="200px" >
                    <ItemTemplate>
                        <dnnweb:DnnLanguageLabel ID="languageLanguageLabel" runat="server" Language='<%# Eval("CultureCode") %>'  />
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridBoundColumn HeaderText="TabName" DataField="TabName" ItemStyle-Width="200px"  />
                <dnnweb:DnnGridTemplateColumn UniqueName="View" HeaderText="View" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" >
                    <ItemTemplate>
                        <asp:PlaceHolder ID="viewPlaceHolder" runat="server" Visible='<%# CanView(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString()) %>'>
                            <a href='<%# DotNetNuke.Common.Globals.NavigateURL(Convert.ToInt32(Eval("TabId")), Null.NullBoolean, PortalSettings, "", Eval("CultureCode").ToString(), new string[]{}) %>' >
                                <dnnweb:DnnImage ID="viewCultureImage" runat="server" ResourceKey="view" IconKey="View" />
                            </a>
                        </asp:PlaceHolder>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn UniqueName="Edit" HeaderText="Edit" ItemStyle-VerticalAlign="Middle" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:PlaceHolder ID="editPlaceHolder" runat="server" Visible='<%# CanEdit(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString()) %>'>
                            <a href='<%# DotNetNuke.Common.Globals.NavigateURL(Convert.ToInt32(Eval("TabId")), Null.NullBoolean, PortalSettings, "Tab", Eval("CultureCode").ToString(), new []{"action=edit"}) %>' >
                                <dnnweb:DnnImage ID="editCultureImage" runat="server" ResourceKey="edit" IconKey="Edit" />
                            </a>
                        </asp:PlaceHolder>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn UniqueName="IsTranslated" HeaderText="Translated">
                    <ItemStyle VerticalAlign="Middle" Width="60px" HorizontalAlign="Center"/>
                    <ItemTemplate>
                        <dnnweb:DnnImage ID="translatedImage" runat="server" IconKey="Grant" Visible='<%# Eval("IsTranslated")%>' />
                        <dnnweb:DnnImage ID="notTranslatedImage" runat="server" IconKey="Deny" Visible='<%# !Convert.ToBoolean(Eval("IsTranslated"))%>' />
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn  HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
                    <HeaderTemplate>
                        <dnnweb:DnnImage ID="totalModulesImage" runat="server" IconKey="Total" resourceKey="TotalModules" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <span><%# GetTotalModules(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn  HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
                    <HeaderTemplate>
                        <dnnweb:DnnImage ID="sharedModulesImage" runat="server" IconKey="Shared" resourceKey="SharedModules" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <span><%# GetSharedModules(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn  HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
                    <HeaderTemplate>
                        <dnnweb:DnnImage ID="localizedModulesImage" runat="server" IconKey="ModuleUnbind" resourceKey="LocalizedModules" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <span><%# GetLocalizedModules(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                        <br />
                        <span><%# GetLocalizedStatus(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
                <dnnweb:DnnGridTemplateColumn HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
                    <HeaderTemplate>
                        <dnnweb:DnnImage ID="translatedModulesImage" runat="server" IconKey="Translated" resourceKey="TranslatedModules" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <span><%# GetTranslatedModules(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                        <br />
                        <span><%# GetTranslatedStatus(Convert.ToInt32(Eval("TabId")), Eval("CultureCode").ToString())%></span>
                    </ItemTemplate>
                </dnnweb:DnnGridTemplateColumn>
            </Columns>
        </MasterTableView>
    </dnnweb:DnnGrid>
    <asp:PlaceHolder ID="footerPlaceHolder" runat="server">
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="markTabTranslatedButton" resourcekey="markTabTranslated" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
            <li><asp:LinkButton ID="markTabUnTranslatedButton" resourcekey="markTabUnTranslated" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
        </ul>
    </asp:PlaceHolder>
</div>