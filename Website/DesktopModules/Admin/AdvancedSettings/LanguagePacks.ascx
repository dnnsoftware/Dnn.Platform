<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LanguagePacks.ascx.cs" Inherits="DotNetNuke.Modules.Admin.AdvancedSettings.LanguagePacks" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<asp:Repeater ID="extensionTypeRepeater" runat="server">
    <ItemTemplate>
        <% if (ShowDescription)
           { %>
            <div class="dnnFormMessage dnnFormInfo">
                <%= LocalizeString("LanguagePacksDetail") %>
            </div>
        <% } %>                  
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
                        <ItemStyle HorizontalAlign="Center"/>
                        <ItemTemplate>
                            <asp:HyperLink id="cmdInstall"  runat="server" CssClass="dnnSecondaryAction installAction" ResourceKey="installExtension" />
                            <asp:LinkButton runat="server" OnClick="DeployLanguage" ID="downloadLanguage" CssClass="dnnSecondaryAction installAction" Text="Deploy" Visible="False"></asp:LinkButton>
                            <asp:LinkButton id="cmdDownload"  runat="server" CssClass="dnnSecondaryAction" ResourceKey="Download" OnClick="DownloadLanguage" />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </fieldset>
    </ItemTemplate>
</asp:Repeater>


<script language="javascript" type="text/javascript">
	/*globals jQuery, window, Sys */
    (function ($, Sys) {
        var LangPacksNS = {};
        LangPacksNS.deleteBeforeCloseEvent = function() {
            var dialog = parent.$('.ui-dialog:visible'); //this object remains shown when the confirm dialog appears

            if (dialog != null) {
                dialog.unbind('dialogbeforeclose');
            }
        };
        function checkPopupLink() {
            $("a[popupUrl]").each(function (index, item) {
                if (LangPacksNS) {
                    LangPacksNS.deleteBeforeCloseEvent();
                }
            	//reset page scroll top to 0
	            $("#ScrollTop").val(0);
                location.href = $(this).attr("popupUrl");
            });
        }

        $(document).ready(function () {
            checkPopupLink();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                checkPopupLink();
            });

        });

    } (jQuery, window.Sys));
</script>

 