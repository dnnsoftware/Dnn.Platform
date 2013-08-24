<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdvancedUrlSettings.ascx.cs" Inherits="DotNetNuke.Modules.UrlManagement.AdvancedUrlSetttings" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelcontrol.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke" %>

<dnnext:EditPagePanelExtensionControl runat="server" ID="UrlSettingsExtensionControl" Module="SiteSettings" Group="UrlSettings"/>
<h2 id="dnnSitePanel-ModuleProviders" class="dnnFormSectionHead">
    <a href=""><%=LocalizeString("ModuleProviders")%></a>
</h2>
<fieldset class="ssasModuleProviders">
    <div class="dnnFormItem">
        <asp:label id="providersWarningLabel" runat="server" Visible="False" />
        <dnn:label id="moduleProvidersLabel" runat="server" controlname="moduleProvidersGrid" />
        <div class="dnnFormGroup">
            <asp:DataGrid ID="providersGrid" Runat="server" AutoGenerateColumns="false" width="100%" GridLines="None" CssClass="dnnGrid">
                <headerstyle CssClass="dnnGridHeader" />
                <itemstyle CssClass="dnnGridItem" horizontalalign="Left" />
                <alternatingitemstyle CssClass="dnnGridAltItem" />
                <edititemstyle />
                <selecteditemstyle />
                <footerstyle />
	            <Columns>
                    <asp:TemplateColumn HeaderText="Enabled" HeaderStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:CheckBox ID="isActve" runat="server"
                                Checked='<%#Eval("IsActive") %>' 
                                />
                            <asp:HiddenField ID="urlProviderId" runat="server"
                                Value='<%#Eval("ExtensionUrlProviderId") %>'/>
                        </ItemTemplate>
                    </asp:TemplateColumn>
		            <asp:TemplateColumn HeaderText="ProviderName" HeaderStyle-Width="160px" >
		                <HeaderStyle  HorizontalAlign="Left" />
		                <ItemStyle  HorizontalAlign="Left" />
		                <ItemTemplate>
                            <asp:label runat="server" Text='<%#Eval("ProviderName") %>' ID="lblProviderName" />
		                </ItemTemplate>
		            </asp:TemplateColumn>
	                <dnn:ImageCommandColumn HeaderText="Action" HeaderStyle-Width="50px" ItemStyle-HorizontalAlign="Center" EditMode="URL" 
                        IconKey="Edit" KeyField="ExtensionUrlProviderId" />
	            </Columns>
            </asp:DataGrid>
        </div>
    </div>
</fieldset>

<script>
    (function ($) {
        $(document).ready(function () {
            var sf = $.ServicesFramework(<%=ModuleContext.ModuleId %>);
            var sfUrl = sf.getServiceRoot('UrlManagement') + 'ExtensionProviderService/';
            
            $('input[name$="isActve"]').click(function () {
                var $check = $(this);
                var checked = $check[0].checked;
                var url = sfUrl + ((checked) ? "EnableUrlProvider/" : "DisableUrlProvider/");
                var $hidden = $check.parent().children('input[name$="urlProviderId"]');
                var urlProviderId = $hidden.val();

                $.ajax({
                    type: "POST",
                    cache: false,
                    url: url,
                    data: { extensionurlProviderId: urlProviderId },
                    beforeSend: sf.setModuleHeaders
                });
            });
        });

    }(jQuery));
</script>
