<%@ Control Inherits="DotNetNuke.Modules.Admin.AdvancedSettings.AdvancedSettings" Language="C#" AutoEventWireup="false" EnableViewState="True" CodeFile="AdvancedSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="SmtpServerSettings" Src="~/DesktopModules/admin/AdvancedSettings/SmtpServerSettings.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LanguagePacks" Src="~/DesktopModules/admin/AdvancedSettings/LanguagePacks.ascx" %>
<%@ Register TagPrefix="dnn" tagName="SkinEdit" src="~/DesktopModules/admin/Skins/editskins.ascx"%>
<%@ Register TagPrefix="dnn" tagName="SkinAttributes" src="~/DesktopModules/admin/SkinDesigner/Attributes.ascx"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<div class="dnnForm dnnAdvancedSettings dnnClear" id="dnnAdvancedSettings">
    <ul class="dnnAdminTabNav dnnClear">
        <li><a href="#asSkinsAndContainers">
            <%=LocalizeString("SkinsAndContainers") %></a></li>
        <% if (IsHost) { %>
        <li><a href="#asSmtpServer">
            <%= LocalizeString("SmtpServer") %></a></li>
        <li><a href="#asLanguagePacks">
            <%=LocalizeString("LanguagePacks")%></a></li>
        <li><a href="#asAuthenticationSystems">
            <%=LocalizeString("AuthenticationSystems")%></a></li>
        <li><a href="#asProviders">
            <%=LocalizeString("Providers")%></a></li>
        <li><a href="#asOptionalModules">
            <%=LocalizeString("OptionalModules")%></a></li>
        <% } %>
    </ul>
    <div class="asSkinsAndContainers dnnClear" id="asSkinsAndContainers">
        <h2><span id="skinEditorTitle" class="Head"><%=LocalizeString("SkinEditor.Title")%></span></h2>                
        <dnn:SkinEdit runat="server" id="EditSkins" />
        <h2><span id="skinDesignerTitle" class="Head"><%=LocalizeString("SkinDesigner.Title")%></span></h2>                
        <dnn:SkinAttributes runat="server" ID="Attributes"/>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="cmdSkinsUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>   
            <li><asp:LinkButton ID="cmdSkinsCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <% if (IsHost) { %>
    <div class="asSmtpServer dnnClear" id="asSmtpServer">
        <dnn:SmtpServerSettings runat="server" ID="smtpServerSettings"/>        
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="cmdSmtpUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
            <li><asp:LinkButton ID="cmdSmtpCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <div class="asLanguagePacks dnnClear" id="asLanguagePacks">
        <dnn:LanguagePacks runat="server" ID="languagePacks" ShowDescription="True"/>
        <ul class="dnnActions dnnClear">            
            <li><asp:LinkButton ID="cmdLangCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <div class="asAuthenticationSystems dnnClear" id="asAuthenticationSystems">
        <div class="dnnFormMessage dnnFormInfo">
            <%=LocalizeString("AuthSystemsDetail")%>
        </div>            
        <asp:DataGrid ID="authSystemsGrid" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" 
            runat="server" GridLines="None" Width="100%" CssClass="dnnGrid">
            <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
            <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
            <AlternatingItemStyle CssClass="dnnGridAltItem" />
            <Columns>
		        <asp:TemplateColumn>
                    <ItemStyle HorizontalAlign="Center" Width="32px" Height="32px"/>
                    <ItemTemplate>
                        <asp:Image ID="imgIcon" runat="server" Width="32px" Height="32px" ImageUrl='<%# GetIconUrl("icon_authentication.png") %>' ToolTip='<%# GetPackageDescription(Container.DataItem) %>' />
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
                <asp:TemplateColumn headerStyle-width="75px">
                    <ItemStyle HorizontalAlign="Center" />
                    <ItemTemplate>
                        <asp:HyperLink ID="hlInstall" runat="server" CssClass="dnnSecondaryAction installAction" ResourceKey="installExtension" />                        
                    </ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
        <div id="divNoAuthSystems" runat="server" class="dnnFormMessage dnnFormWarning" Visible="False">
            <%= LocalizeString("NoAuthSystems")%>	
        </div>
        <ul class="dnnActions dnnClear">            
            <li><asp:LinkButton ID="cmdAuthCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <div class="asProviders dnnClear" id="asProviders">
        <div class="dnnFormMessage dnnFormInfo">
            <%=LocalizeString("ProvidersDetail")%>
        </div>                    
        <asp:DataGrid ID="providersGrid" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" 
            runat="server" GridLines="None" Width="100%" CssClass="dnnGrid">
            <HeaderStyle Wrap="False" CssClass="dnnGridHeader" />
            <ItemStyle CssClass="dnnGridItem" VerticalAlign="Top" />
            <AlternatingItemStyle CssClass="dnnGridAltItem" />
            <Columns>
		        <asp:TemplateColumn>
                    <ItemStyle HorizontalAlign="Center" Width="32px" Height="32px"/>
                    <ItemTemplate>
                        <asp:Image ID="imgIcon" runat="server" Width="32px" Height="32px" ImageUrl='<%# GetIconUrl("icon_provider.gif") %>' ToolTip='<%# GetPackageDescription(Container.DataItem) %>' />
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
                <asp:TemplateColumn headerStyle-width="75px">
                    <ItemStyle HorizontalAlign="Center" />
                    <ItemTemplate>
                        <asp:HyperLink ID="hlInstall" runat="server" CssClass="dnnSecondaryAction installAction" ResourceKey="installExtension" />                        
                    </ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
        <div id="divNoProviders" runat="server" class="dnnFormMessage dnnFormWarning" Visible="False">
            <%= LocalizeString("NoProviders")%>	
        </div>
        <ul class="dnnActions dnnClear">            
            <li><asp:LinkButton ID="cmdProvidersCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <div class="asOptionalModules dnnClear" id="asOptionalModules">
        <div class="dnnFormMessage dnnFormInfo">
            <%=LocalizeString("ModulesDetail")%>
        </div>            
        <asp:DataGrid ID="modulesGrid" CellPadding="0" CellSpacing="0" AutoGenerateColumns="false" 
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
                <asp:TemplateColumn headerStyle-width="75px">
                    <ItemStyle HorizontalAlign="Center" />
                    <ItemTemplate>
                        <asp:HyperLink ID="hlInstall" runat="server" CssClass="dnnSecondaryAction installAction" ResourceKey="installExtension" />                        
                    </ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
        <div id="divNoModules" runat="server" class="dnnFormMessage dnnFormWarning" Visible="False">
            <%= LocalizeString("NoModules")%>	
        </div>
        <ul class="dnnActions dnnClear">            
            <li><asp:LinkButton ID="cmdModulesCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
        </ul>
    </div>
    <% } %>
</div>

<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupDnnAdvancedSettings() {
            $('#dnnAdvancedSettings').dnnTabs(); //.dnnPanels();
        }

        var AdvSettings = {};
        AdvSettings.addBeforeCloseEvent = function () {
            var dialog = parent.$('.ui-dialog:visible'); //this object remains shown when the confirm dialog appears
            if (dialog != null) {
                if ($("a[popupUrl]").length >= 1) { //If the request is a call back of a language pack deployment, then we cannot attach de event because the page will be redirect
                    return;
                }
                dialog.bind('dialogbeforeclose', function (event, ui) {
                    AdvSettings.deleteBeforeCloseEvent();
                });
            }
        };
        AdvSettings.deleteBeforeCloseEvent = function () {
            var dialog = parent.$('.ui-dialog:visible'); //this object remains shown when the confirm dialog appears

            if (dialog != null) {
                dialog.unbind('dialogbeforeclose');
            }
        };
        AdvSettings.addClickEvent = function () {
            $('.installAction').click(function () {
                AdvSettings.deleteBeforeCloseEvent();
            });
        };

        $(document).ready(function () {
            AdvSettings.addBeforeCloseEvent();
            AdvSettings.addClickEvent();
            setupDnnAdvancedSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupDnnAdvancedSettings();
            });
        });

    } (jQuery, window.Sys));
</script>
