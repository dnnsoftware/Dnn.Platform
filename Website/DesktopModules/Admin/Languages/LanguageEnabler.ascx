<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Languages.LanguageEnabler" AutoEventWireup="false" Explicit="True" CodeFile="LanguageEnabler.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Src="~/DesktopModules/Admin/Languages/CLControl.ascx" TagName="CLControl" %>

<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnLanguages dnnClear" id="dnnLanguages">
    <ul class="dnnAdminTabNav" id="TabStrips" runat="server">
        <li id="tabLanguages" runat="server"><a href="#Languages"><%= LocalizeString("Languages.Tab")%></a></li>
        <li id="tabContentLocalization" runat="server"><a href="#ContentLocalization"><%= LocalizeString("ContentLocalization.Tab")%></a></li>
        <li id="tabSettings" runat="server"><a href="#Settings"><%= LocalizeString("Settings.Tab")%></a></li>
    </ul>
    <div id="Languages" class="ssLanguages dnnClear">
        <div id="PanelLanguages" runat="server">
            <fieldset>
                <ul class="dnnActions dnnClear" style="float: right">
                    <li>
                        <asp:HyperLink ID="cmdEnableLocalizedContent" runat="server" CssClass="dnnSecondaryAction enableLocalization" ResourceKey="EnableLocalization" /></li>
                    <li>
                        <asp:LinkButton ID="cmdDisableLocalization" runat="server" CssClass="dnnSecondaryAction enableLocalization" ResourceKey="DisableLocalization" CausesValidation="False" />
                    </li>
                </ul>

                <div class="dnnFormItem">
                    <dnn:DnnGrid ID="languagesGrid" runat="server" AutoGenerateColumns="false" EnableViewState="True" OnNeedDataSource="LanguagesGrid_NeedDataSource">
                        <MasterTableView>
                            <ItemStyle VerticalAlign="Top" HorizontalAlign="Center" />
                            <AlternatingItemStyle VerticalAlign="Top" HorizontalAlign="Center" />
                            <HeaderStyle VerticalAlign="Bottom" HorizontalAlign="Center" Wrap="false" />
                            <Columns>
                                <dnn:DnnGridTemplateColumn ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left">
                                    <HeaderTemplate>
                                        <%# LocalizeString("Culture.Header")%>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <dnn:DnnLanguageLabel ID="translationStatusLabel" runat="server" Language='<%# Eval("Code") %>' />
                                        <asp:Label ID="defaultLanguageMessageLabel" runat="server" CssClass="NormalRed" Text="**"
                                            Visible='<%# IsDefaultLanguage(Eval("Code").ToString()) %>' />
                                    </ItemTemplate>
                                </dnn:DnnGridTemplateColumn>
                                <dnn:DnnGridTemplateColumn ItemStyle-Width="80px">
                                    <HeaderTemplate>
                                        <%# LocalizeString("Enabled.Header")%>
                                        <asp:Label ID="enabledMessageLabel" runat="server" CssClass="NormalRed" Text="*"
                                            Visible='<%# PortalSettings.ContentLocalizationEnabled %>' />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="enabledCheckbox" runat="server" AutoPostBack="True" CommandArgument='<%# Eval("LanguageId") %>'
                                            Enabled='<%# CanEnableDisable(Eval("Code").ToString()) %>' 
                                            OnCheckedChanged="enabledCheckbox_CheckChanged" CssClass="normalCheckBox" />
                                    </ItemTemplate>
                                </dnn:DnnGridTemplateColumn>
                                <dnn:DnnGridTemplateColumn ItemStyle-Width="80px">
                                    <HeaderTemplate>
                                        <%# LocalizeString("Settings")%>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:HyperLink ID="editLink" runat="server" NavigateUrl='<%# GetEditUrl(Eval("LanguageId").ToString()) %>'>
                                            <dnn:DnnImage ID="editImage" runat="server" IconKey="Edit" resourcekey="Edit" />
                                        </asp:HyperLink>
                                    </ItemTemplate>
                                </dnn:DnnGridTemplateColumn>
                                <dnn:DnnGridTemplateColumn HeaderStyle-Width="194px">
                                    <HeaderTemplate>
                                        <table class="DnnGridNestedTable" style="width: 180px;">
                                            <caption>
                                                <%# LocalizeString("Static.Header") %></caption>
                                            <tbody>
                                                <tr>
                                                    <%
                                                        if (UserInfo.IsSuperUser)
                                                        {%>
                                                    <td id="Td2" style="width: 60px;" runat="server">
                                                        <%# LocalizeString("System")%>
                                                    </td>
                                                    <td id="Td1" style="width: 60px;" runat="server">
                                                        <%# LocalizeString("Host")%>
                                                    </td>
                                                    <td style="width: 60px;">
                                                        <%# LocalizeString("Portal")%>
                                                    </td>
                                                    <%
                                                        }
                                                        else
                                                        {%>
                                                    <td style="width: 180px;">
                                                        <%# LocalizeString("Portal")%>
                                                    </td>
                                                    <%
                                                        }%>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <%
                                            if (UserInfo.IsSuperUser)
                                            {%>
                                        <table class="DnnGridNestedTable" style="width: 180px;">
                                            <%
                                            }
                                            else
                                            {%>
                                            <table class="DnnGridNestedTable" style="width: 60px;">
                                                <%
                                            }%>
                                                <tbody>
                                                    <tr>
                                                        <%
                                                            if (UserInfo.IsSuperUser)
                                                            {
                                                        %>
                                                        <td id="Td4" style="width: 60px; border-width: 0">
                                                            <asp:HyperLink ID="editSystemLink" runat="server" NavigateUrl='<%# GetEditKeysUrl(Eval("Code").ToString(), "System") %>'>
                                                                <dnn:DnnImage ID="editSystemImage" runat="server" IconKey="Edit" resourcekey="System.Help" />
                                                            </asp:HyperLink>
                                                        </td>
                                                        <td id="Td3" style="width: 60px;" runat="server">
                                                            <asp:HyperLink ID="editHostLink" runat="server" NavigateUrl='<%# GetEditKeysUrl(Eval("Code").ToString(), "Host") %>'>
                                                                <dnn:DnnImage ID="editHostImage" runat="server" IconKey="Edit" resourcekey="Host.Help" />
                                                            </asp:HyperLink>
                                                        </td>
                                                        <%
                                                            }%>
                                                        <td style="width: 60px;">
                                                            <asp:HyperLink ID="editPortalLink" runat="server" NavigateUrl='<%# GetEditKeysUrl(Eval("Code").ToString(), "Portal") %>'>
                                                                <dnn:DnnImage ID="editPortalImage" runat="server" IconKey="Edit" resourcekey="Portal.Help" />
                                                            </asp:HyperLink>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                    </ItemTemplate>
                                </dnn:DnnGridTemplateColumn>
                                <dnn:DnnGridTemplateColumn UniqueName="ContentLocalization" HeaderStyle-Width="204px">
                                    <HeaderTemplate>
                                        <table class="DnnGridNestedTable" style="width: 400px;">
                                            <caption>
                                                <%# LocalizeString("Content.Header")%></caption>
                                            <tr>
                                                <td style="width: 50px;">
                                                    <%# LocalizeString("Pages.Header")%>
                                                </td>
                                                <td style="width: 50px;">
                                                    <%# LocalizeString("Translated.Header")%>
                                                </td>
                                                <td style="width: 50px;">
                                                    <%# LocalizeString("Active.Header")%>
                                                    <asp:Label ID="publishedMessageLabel" runat="server" CssClass="NormalRed" Text="*"
                                                        Visible='<%# PortalSettings.ContentLocalizationEnabled %>' />
                                                </td>
                                                <td style="width: 50px;">
                                                    <%# LocalizeString("Publish.Header")%>
                                                    <asp:Label ID="Label1" runat="server" CssClass="NormalRed" Text="*"
                                                        Visible='<%# PortalSettings.ContentLocalizationEnabled %>' />
                                                </td>
                                                <td style="width: 50px;">
                                                     <%# LocalizeString("Delete.Header")%>
                                                </td>
                                            </tr>
                                        </table>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <table class="DnnGridNestedTable" style="width: 400px;">
                                            <tr>
                                                <td style="width: 50px; border-width: 0;vertical-align:bottom">
                                                    <asp:PlaceHolder ID="PlaceHolder1" runat="server" Visible='<%# IsDefaultLanguage(Eval("Code").ToString()) %>'>
                                                        <span title="<%# LocalizeString("LocalizablePages")%>"><%# GetLocalizablePages(Eval("Code").ToString())%></span>
                                                    </asp:PlaceHolder>
                                                    <asp:PlaceHolder ID="localizationStatus" runat="server" Visible='<%# IsLocalized(Eval("Code").ToString()) %>'>
                                                        <div>
                                                            <div style="display: inline-block; ">
                                                                <span><%# GetLocalizedPages(Eval("Code").ToString())%></span>
                                                                <span style="font-size: 0.8em">(<%# GetLocalizedStatus(Eval("Code").ToString()) %>)</span><br /><br />
                                                            </div>
                                                            <div style="display: inline-block; float:right;">
                                                                <asp:HyperLink ID="localizeLinkAlt" runat="server" Visible='<%# CanLocalize(Eval("Code").ToString()) %>'>
                                                                    <asp:Image ID="localizeImageAlt" runat="server" ImageAlign="Middle" IconKey="Languages" ResourceKey="CreateLocalizedPages" />
                                                                </asp:HyperLink>
                                                            </div>
                                                        </div>
                                                    </asp:PlaceHolder>
                                                    <asp:HyperLink ID="localizeLink" runat="server" Visible='<%# !IsLocalized(Eval("Code").ToString()) && CanLocalize(Eval("Code").ToString()) %>'>
                                                        <asp:Image ID="localizeImage" runat="server" ImageAlign="Middle" IconKey="Languages" ResourceKey="CreateLocalizedPages" />
                                                    </asp:HyperLink>
                                                </td>
                                                <td style="width: 50px;vertical-align:bottom">
                                                    <span><%# GetTranslatedPages(Eval("Code").ToString())%></span>
                                                    <span style="font-size: 0.8em">(<%# GetTranslatedStatus(Eval("Code").ToString())%>)</span><br />
                                                    <asp:HyperLink ID="hyp" runat="server" NavigateUrl='<%# "javascript:getNonTranslatedPages(\"" + Eval("Code").ToString() + "\",\"" + Eval("NativeName").ToString() + "\");"%>'
                                                        Visible='<%# !IsDefaultLanguage(Eval("Code").ToString()) && IsLocalized(Eval("Code").ToString()) %>' 
                                                        ToolTip='<%# LocalizeString("GetNonTranslatedPages") %>'>
                                                        <dnn:DnnImage ID="DnnImage1" runat="server" ResourceKey="GetNonTranslatedPages" IconKey="Translate" />
                                                    </asp:HyperLink>
                                                    &nbsp;&nbsp;
                                                   <asp:LinkButton ID="cmdTranslateAll" runat="server" CommandArgument='<%# Eval("LanguageId") %>' Visible='<%# !IsDefaultLanguage(Eval("Code").ToString()) && IsLocalized(Eval("Code").ToString()) %>' OnClick="MarkAllPagesTranslated" CausesValidation="False" ToolTip='<%# LocalizeString("MarkAllPagesTranslated") %>'>
                                                        <dnn:DnnImage ID="DnnImage2" runat="server" ResourceKey="MarkAllPagesTranslated" IconKey="Grant" />
                                                    </asp:LinkButton>
                                                </td>
                                                <td style="width: 50px; text-align: center;vertical-align:bottom">
                                                    <asp:CheckBox ID="publishedCheckbox" runat="server" AutoPostBack="True" 
                                                        Enabled='<%# IsLanguageEnabled(Eval("Code").ToString()) && !IsDefaultLanguage(Eval("Code").ToString()) %>'
                                                        OnCheckedChanged="publishedCheckbox_CheckChanged"
                                                        Visible='<%# IsLocalized(Eval("Code").ToString()) %>' CssClass="normalCheckBox" />
                                                </td>
                                                <td style="width: 50px;vertical-align:bottom">
                                                    <asp:LinkButton ID="publishButton" runat="server" CommandArgument='<%# Eval("LanguageId") %>' Visible='<%# IsLanguagePublished(Eval("Code").ToString()) && !IsDefaultLanguage(Eval("Code").ToString()) && IsLocalized(Eval("Code").ToString()) %>' OnClick="PublishPages" CausesValidation="False" ToolTip='<%# LocalizeString("PublishTranslatedPages") %>'>
                                                        <dnn:DnnImage ID="imgPublish" runat="server" ResourceKey="PublishTranslatedPages" IconKey="PublishLanguage" />
                                                    </asp:LinkButton>
                                                </td>
                                                <td style="width: 50px;vertical-align:bottom">
                                                    <asp:LinkButton ID="cmdDeleteTranslation" runat="server" CommandArgument='<%# Eval("LanguageId") %>' OnClick="cmdDeleteTranslation_Click" CausesValidation="False" Visible='<%# IsLocalized(Eval("Code").ToString()) && !IsDefaultLanguage(Eval("Code").ToString()) %>' ToolTip='<%# LocalizeString("cmdDeleteTranslation") %>'>
                                                        <dnn:DnnImage ID="imgDeleteTranslation" runat="server" ResourceKey="cmdDeleteTranslation" IconKey="Delete" />
                                                    </asp:LinkButton>
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>
                                </dnn:DnnGridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </dnn:DnnGrid>
                    <div class="dnnFormItem">
                        <asp:PlaceHolder runat="server" ID="enabledPublishedPlaceHolder">
                            <asp:Label ID="enabledPublishedLabel" runat="server" CssClass="NormalRed" Text="*" />
                            <asp:Label ID="enabledPublishedMessage" runat="server" CssClass="Normal" ResourceKey="EnabledPublishedMessage" />
                        </asp:PlaceHolder>
                        <div>
                            <asp:Label ID="defaultPortalLabel" runat="server" CssClass="NormalRed" Text="**" />
                            <asp:Label ID="defaultPortalMessage" runat="server" CssClass="Normal" />
                        </div>
                    </div>
                </div>
            </fieldset>

            <ul class="dnnActions dnnClear">
                <li>
                    <asp:HyperLink ID="addLanguageLink" runat="server" CssClass="dnnPrimaryAction" resourcekey="AddLanguage" /></li>
                <li>
                    <asp:HyperLink ID="installLanguagePackLink" runat="server" CssClass="dnnSecondaryAction" ResourceKey="InstallLanguage" /></li>
                <li>
                    <asp:HyperLink ID="installAvailableLanguagePackLink" runat="server" CssClass="dnnSecondaryAction" ResourceKey="InstallAvailabeLanguage" /></li>
                <li>
                    <asp:HyperLink ID="createLanguagePackLink" runat="server" CssClass="dnnSecondaryAction" ResourceKey="CreateLanguage" /></li>
                <li>
                    <asp:HyperLink ID="verifyLanguageResourcesLink" runat="server" CssClass="dnnSecondaryAction" ResourceKey="Verify" /></li>
            </ul>
        </div>
    </div>
    <div id="ContentLocalization" class="ssContentLocalization dnnClear">
        <div id="panelContentLocalization" runat="server">
            <div class="clControl">
                <div class="dnnFormItem">
                    <dnn:Label ID="plPage" runat="server" ResourceKey="Page" ControlName="ddlPages" />
                    <dnn:DnnPageDropDownList ID="ddlPages" runat="server" OnSelectionChanged="ddlPages_SelectedIndexChanged" AutoPostBack="True" />
                </div>
            </div>
            <dnn:CLControl ID="CLControl1" runat="server" />
            <div id="NeutralMessage" runat="server" class="dnnFormMessage dnnFormInfo">
                <asp:Label runat="server" ID="litNeutralMessage" resourcekey="NeutralMessage.Text"></asp:Label>
            </div>
            <div class="dnnFormItem dnnActions">
                <asp:LinkButton runat="server" ID="cmdUpdate" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" OnClick="cmdUpdate_Click" />
                <asp:LinkButton runat="server" ID="MakeTranslatable" CssClass="dnnSecondaryAction" resourcekey="MakeTranslatable" OnClick="MakeTranslatable_Click" />
                <asp:LinkButton runat="server" ID="MakeNeutral" CssClass="dnnSecondaryAction" resourcekey="MakeNeutral" OnClick="MakeNeutral_Click" />
                <asp:LinkButton runat="server" ID="AddMissing" CssClass="dnnSecondaryAction" resourcekey="AddMissingLanguages" OnClick="AddMissing_Click" />

            </div>
        </div>
    </div>
    <div id="Settings" class="ssSettings dnnClear">
        <div id="panelSettings" runat="server">
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:Label ID="systemDefaultLabel" runat="server" />
                    <dnn:DnnLanguageLabel ID="systemDefaultLanguageLabel" runat="server" Width="300" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="siteDefaultLabel" runat="server" />
                    <dnn:DnnLanguageLabel ID="defaultLanguageLabel" runat="server" />
                    <dnn:DnnLanguageComboBox ID="languagesComboBox" runat="server" LanguagesListType="Supported" CssClass="dnnLanguageCombo" />
                </div>
                <div id="urlRow" runat="server" class="dnnFormItem">
                    <dnn:Label ID="plUrl" runat="server" ControlName="chkUrl" />
                    <asp:CheckBox ID="chkUrl" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="detectBrowserLable" runat="server" />
                    <asp:CheckBox ID="chkBrowser" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="allowUserCulture" runat="server" />
                    <asp:CheckBox ID="chkUserCulture" runat="server" />
                </div>
                <ul class="dnnActions dnnClear">
                    <li>
                        <asp:LinkButton ID="updateButton" runat="server" CssClass="dnnPrimaryAction" resourcekey="Update" />
                    </li>
                </ul>
            </fieldset>
        </div>
    </div>
</div>

<script type="text/javascript">
    var serviceFramework;
    var baseServicepath;
    var closeText = '<%= LocalizeString("Close")%>';
    var titleText = '<%= LocalizeString("NonLocalizedPagesTitle")%> ';
    var view = '<%= LocalizeString("ViewPage")%> ';
    var edit = '<%= LocalizeString("EditPage")%> ';

    function getNonTranslatedPages(code, lang) {
        $.ajax({
            url: baseServicepath + 'GetNonTranslatedPages',
            type: 'GET',
            data: 'languageCode=' + code,
            beforeSend: serviceFramework.setModuleHeaders
        }).done(function (result) {
            debugger;
            var pages ="";
            $.each(result, function () {
                pages = pages + "<li>" + this.Name + " <a href='" + this.ViewUrl + "'>"+ view + "</a> - <a href='" + this.EditUrl + "'>" + edit + "</a></li>"
            });
            $.dnnAlert({
                okText: closeText,
                title: titleText + lang,
                text: "<div class=\"pages\"><ul>" + pages + "</ul></div>"
            });
        }).fail(function (xhr, status, error) {
            $.dnnAlert({
                okText: closeText,
                title: titleText + lang,
                text: error
            });
        });
    }

    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupLanguageEnabler() {
            $('#dnnLanguages').dnnTabs();

            $('#<%= cmdDisableLocalization.ClientID %>').dnnConfirm({ text: '<%= Localization.GetSafeJSString("DisableLocalization.Confirm", LocalResourceFile) %>', yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>', noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>', title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>' });
            $('#<%= MakeNeutral.ClientID %>').dnnConfirm({ text: '<%= Localization.GetSafeJSString("MakeNeutral.Confirm", LocalResourceFile) %>', yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>', noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>', title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>' });

            <%= BuildConfirmationJS("cmdDeleteTranslation", "DeleteTranslations.Confirm") %>
            
                <%= BuildConfirmationJS("publishButton", "Publish.Confirm") %>
        }


        $(document).ready(function () {

            serviceFramework = $.ServicesFramework(<%=ModuleContext.ModuleId %>);
            baseServicepath = serviceFramework.getServiceRoot('InternalServices') + 'LanguageService/';

            setupLanguageEnabler();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupLanguageEnabler();
            });
            
        });
    }(jQuery, window.Sys));
</script>

