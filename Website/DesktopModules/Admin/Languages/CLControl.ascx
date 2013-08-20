<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CLControl.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Languages.CLControl" Debug="true" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div id="DnnPages">
    <div id="mainContainer" runat="server" class="container RadGrid RadGrid_Default">
        <asp:Repeater ID="rDnnModules" runat="server"
            OnItemDataBound="rDnnModules_ItemDataBound">
            <HeaderTemplate>
                <table class="rgMasterTable">
                    <thead>
                        <tr class="pageHeaderRow">
                            <asp:Repeater ID="rHeader" runat="server">
                                <ItemTemplate>
                                    <th class="pageHeaderCol<%# ((Container.ItemIndex == 0) && (Data.Pages.Count>1)) ? " headcol" : "" %>" title='<%# CultureName(Eval("CultureCode"))  + " " + Eval("LanguageStatus")  %>'>
                                        <span class="Language">
                                            <asp:Image ID="imgFlag" runat="server" ImageUrl='<%# "~/images/Flags/"+(Eval("CultureCode") == "" ? "none" : Eval("CultureCode") )+".gif" %>' ImageAlign="Middle" />
                                            <asp:Literal ID="lblCulture" runat="server" Text='<%# (string)Eval("CultureCode")%>' />
                                        </span>
                                        <div class="dnnRight pageActions">
                                            <span id="viewPage" runat="server" visible="<%# enablePageEdit %>">
                                                <a href='<%# DotNetNuke.Common.Globals.NavigateURL(Convert.ToInt32(Eval("TabId")), Null.NullBoolean, PortalSettings, "", Eval("CultureCode").ToString(), new string[]{}) %>' target="_blank" title='<%# LocalizeString("View.Header")%>'><dnn:DnnImage ID="imgView" runat="server" ResourceKey="View.Header" IconKey="Eye" IconStyle="Gray" /></a>
                                            </span>
                                            <span id="editPage" runat="server" visible="<%# enablePageEdit %>">
                                                <a href="<%# BuildSettingsURL(Convert.ToInt32(Eval("TabId")))%>" title='<%# LocalizeString("Settings.Header")%>'><dnn:DnnImage ID="imgSettings" runat="server" ResourceKey="Settings.Header" IconKey="Cog" IconStyle="Gray" /></a>
                                            </span>
                                            <asp:LinkButton ID="cmdDeleteTranslation" runat="server" 
                                                    CommandArgument='<%# Eval("TabId").ToString() + "|" + Eval("CultureCode").ToString() %>' 
                                                    name='<%# Eval("CultureCode").ToString() %>'
                                                    OnClick="cmdDeleteTranslatedPage" 
                                                    CausesValidation="False" Visible='<%# (!Convert.ToBoolean(Eval("HasChildren"))) && Convert.ToBoolean(Eval("NotDefault")) %>' 
                                                    ToolTip='<%# LocalizeString("Delete.Header") %>'>
                                                <dnn:DnnImage ID="imgDeleteTranslation" runat="server" ResourceKey="cmdDeleteTranslation" IconKey="Delete" IconStyle="Gray" />
                                            </asp:LinkButton>
                                        </div>
                                    </th>

                                </ItemTemplate>
                            </asp:Repeater>
                        </tr>
                    </thead>
                    <tr class="rgRow firstRow">
                        <asp:Repeater ID="rDnnPage" runat="server">
                            <ItemTemplate>
                                <td class="<%# ((Container.ItemIndex == 0) && (Data.Pages.Count>1)) ? " headcol" : "" %>">
                                    <div class="pageInfo dnnFormItem">
                                        <asp:HiddenField ID="hfTabID" runat="server" Value='<%# Eval("TabID") %>' />
                                        <asp:TextBox ID="tbTabName" runat="server" Text='<%# Eval("TabName") %>' ToolTip='<%# LocalizeString("PageName.Text")%>' Visible='<%# Eval("TabID") != null%>' Enabled='<%# Eval("CanAdminPage") %>'></asp:TextBox>
                                        <asp:TextBox ID="tbTitle" runat="server" Text='<%# Eval("Title") %>' ToolTip='<%# LocalizeString("PageTitle.Text")%>' Visible='<%# Eval("TabID") != null%>' Enabled='<%# Eval("CanAdminPage") %>'></asp:TextBox>
                                        <asp:TextBox ID="tbDescription" runat="server" Text='<%# Eval("Description") %>' ToolTip='<%# LocalizeString("PageDescription.Text")%>' Visible='<%# Eval("TabID") != null%>' TextMode="MultiLine" Rows="3" Enabled='<%# Eval("CanAdminPage") %>'></asp:TextBox>
                                    </div>
                                </td>

                            </ItemTemplate>
                        </asp:Repeater>
                    </tr>
                    <tr class="moduleHeaderRow">
                        <asp:Repeater ID="rColHeader" runat="server">
                            <ItemTemplate>
                                <td class="moduleHeaderCol<%# ((Container.ItemIndex == 0) && (Data.Pages.Count>1)) ? " headcol" : "" %>">
                                    <div class="moduleHeaderOverlay">
                                        <div class="moduleHeaderColLabel" ><asp:literal runat="server" ID="litModuleColHeader" Text='<%# LocalizeString("ModuleColHeader.Text")%>' /></div>
                                        <div class="infoCheckBoxes" runat="server" visible='<%# Eval("NotDefault") %>'>
                                            <div class="infoCheckBox">
                                                <a onclick="ToggleAll('.detached-<%# Eval("CultureCode") + "-" + Eval("TabId") %>')" title='<%#LocalizeString("Detached.Header") %>'>
                                                    <dnn:DnnImage ID="imgDetached" runat="server" IconKey="Link" IconStyle="Gray" Visible='<%# Eval("NotDefault") %>' />
                                                </a>
                                            </div>
                                            <div class="infoCheckBox">
                                                <a onclick="ToggleAll('.translated-<%# Eval("CultureCode") + "-" + Eval("TabId") %>')" title='<%#LocalizeString("Translated.Header") %>'>
                                                    <dnn:DnnImage ID="imgTranslated" runat="server" IconKey="Checklist" IconStyle="Gray" Visible='<%# Eval("NotDefault") %>' />
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class="<%# Container.ItemIndex % 2 == 0 ? "rgAltRow" : "rgRow" %>">
                    <asp:Repeater ID="rDnnModule" runat="server">
                        <ItemTemplate>
                            <td class="moduleInfo<%# ((Container.ItemIndex == 0) && (Data.Pages.Count>1)) ? " headcol" : "" %>">
                                <asp:Panel ID="pAddModule" runat="server" Visible='<%# Eval("NotExist") %>' class="dnnFormItem">
                                    <asp:CheckBox ID="cbAddModule" runat="server" Visible='<%# Eval("NotExist") %>' Text='<%# LocalizeString("CopyModule.Text")%>' />
                                </asp:Panel>
                                <asp:Panel ID="pDnnModule" runat="server" Visible='<%# Convert.ToBoolean(Eval("Exist")) && Convert.ToBoolean(Eval("CanViewModule")) %>' class="dnnFormItem">
                                    <asp:HiddenField ID="hfTabModuleID" runat="server" Value='<%# Eval("TabModuleID") %>' />
                                    <dnn:Label ID="plInfo" runat="server" HelpText='<%# GetModuleInfo(Eval("ModuleID")) %>'></dnn:Label>
                                    <asp:TextBox ID="tbModuleTitle" runat="server" Text='<%# Eval("ModuleTitle") %>' Visible='<%# Eval("Exist") %>' ToolTip='<%# GetModuleTitleHint(Convert.ToBoolean(Eval("IsDeleted")))%>' Enabled='<%# Convert.ToBoolean(Eval("CanAdminModule")) && !Convert.ToBoolean(Eval("IsDeleted")) %>' CssClass='<%# DeletedClass(Convert.ToBoolean(Eval("IsDeleted")))%>'></asp:TextBox>
                                    <div class="infoCheckBoxes" runat="server" visible='<%# ( Convert.ToBoolean(Eval("LocalizedVisible")) || Convert.ToBoolean(Eval("TranslatedVisible")) ) && !Convert.ToBoolean(Eval("IsDeleted")) %>'>
                                        <div class='detached-<%# Eval("CultureCode") + "-" + Eval("TabId") %> infoCheckBox'>
                                            <asp:CheckBox ID="cbLocalized" runat="server" Checked='<%# Eval("IsLocalized")  %>' Visible='<%# Eval("LocalizedVisible") %>' ToolTip='<%# Eval("LocalizedTooltip") %>' CssClass="detached" />
                                        </div>
                                        <div class='translated-<%# Eval("CultureCode") + "-" + Eval("TabId") %> infoCheckBox'>
                                            <asp:CheckBox ID="cbTranslated" runat="server" Checked='<%# Eval("IsTranslated")  %>' Visible='<%# Eval("TranslatedVisible")  %>' ToolTip='<%# Eval("TranslatedTooltip")  %>' />
                                        </div>
                                    </div>
                                    <div id="DeletedModuleActions" class="infoActions" runat="server" visible='<%# Convert.ToBoolean(Eval("IsDeleted")) %>'>
                                        <div class="infoCheckBox">
                                            <asp:LinkButton ID="cmdRestoreModule" runat="server" 
                                                    CommandArgument='<%# Eval("TabModuleID") %>' 
                                                    OnClick="cmdRestoreModule" 
                                                    CausesValidation="False"  
                                                    ToolTip='<%# LocalizeString("cmdRestoreModule") %>'>
                                                <dnn:DnnImage ID="DnnImage1" runat="server" ResourceKey="cmdRestoreModule" IconKey="FolderRefreshSync" IconStyle="Gray" />
                                            </asp:LinkButton>
                                        </div>
                                        <div class="infoCheckBox">
                                            <asp:LinkButton ID="cmdDeleteModule" runat="server" 
                                                    CommandArgument='<%# Eval("TabModuleID") %>' 
                                                    OnClick="cmdDeleteModule" 
                                                    CausesValidation="False"  
                                                    ToolTip='<%# LocalizeString("cmdDeleteModule") %>'>
                                                <dnn:DnnImage ID="imgDeleteTranslation" runat="server" ResourceKey="cmdDeleteModule" IconKey="Delete" IconStyle="Gray" />
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                 </asp:Panel>
                                <asp:Panel ID="Panel1" runat="server" Visible='<%# Convert.ToBoolean(Eval("Exist")) && !Convert.ToBoolean(Eval("CanViewModule")) %>' class="dnnFormItem" />

                            </td>
                        </ItemTemplate>
                    </asp:Repeater>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <tr class="lastrow <%# Container.ItemIndex % 2 == 0 ? "rgAltRow" : "rgRow" %>">
                    <asp:Repeater ID="rFooter" runat="server">
                        <ItemTemplate>
                            <td class="lastrow<%# ((Container.ItemIndex == 0) && (Data.Pages.Count>1)) ? " headcol" : "" %>">
                                <div class="PageStatus">
                                    <asp:HiddenField ID="hfTabID" runat="server" Value='<%# Eval("TabID") %>' />
                                    <asp:CheckBox ID="cbTranslated" runat="server" Text="Translated" Checked='<%# Eval("IsTranslated")  %>' Visible='<%# Eval("TranslatedVisible")  %>' ToolTip='<%# LocalizeString("TranslatedPageTooltip")  %>' />
                                    <asp:CheckBox ID="cbPublish" runat="server" Text="Published" Visible='<%# PublishVisible(Eval("CultureCode").ToString() )  %>' Checked='<%# Eval("IsPublished") %>' ToolTip='<%# LocalizeString("PublishedPageTooltip")  %>'/>
                                </div>
                            </td>
                        </ItemTemplate>
                    </asp:Repeater>
                </tr>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>
</div>

<dnn:DnnGrid ID="dummyGrid" runat="server" />

<script type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupCLTools() {
            var confirmText = '<%= Localization.GetSafeJSString("DeleteTranslation.Confirm", LocalResourceFile) %>';
            
            $('a[id*="rHeader_cmdDeleteTranslation"]').each(function (index) {
                var deleteButton = this;
                var culture = deleteButton.name;
                var customText = confirmText.replace("{0}", culture);
                
                $(this).dnnConfirm({
                    text: customText,
                    yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                    noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                    title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
                });
            });
            
            $('a[id*="cmdDeleteModule"]').each(function (index) {
                $(this).dnnConfirm({
                    text: '<%= Localization.GetSafeJSString("DeleteModule.Confirm", Localization.SharedResourceFile) %>',
                    yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                    noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                    title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
                });
            });

     }
        $(document).ready(function () {

            setupCLTools();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupCLTools();
            });
        });
    }(jQuery, window.Sys));

    jQuery('#DnnPages input[type=text]').focus(function () {
        jQuery(this).parent().parent().parent().addClass('highlightedRow');
    });

    jQuery('#DnnPages textarea').focus(function () {
        jQuery(this).parent().parent().parent().addClass('highlightedRow');
    });


    jQuery('#DnnPages input[type=text]').blur(function () {
        jQuery(this).parent().parent().parent().removeClass('highlightedRow');
    });

    jQuery('#DnnPages textarea').blur(function () {
        jQuery(this).parent().parent().parent().removeClass('highlightedRow');
    });

    function ToggleAll(checkBoxClass) {
        var checkBoxes = jQuery(checkBoxClass + " input[type=checkbox]");
        checkBoxes.prop("checked", !checkBoxes.prop("checked"));
    }
</script>
