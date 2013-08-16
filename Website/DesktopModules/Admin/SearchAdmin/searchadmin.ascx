<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Search.SearchAdmin" CodeFile="SearchAdmin.ascx.cs" %>
<%@ Register TagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSearchAdmin dnnClear" id="dnnSearchAdmin">
    <dnnext:EditPageTabExtensionControl runat="server" ID="SearchAdminTabExtensionControl"
        Module="SearchAdmin" Group="SearchAdminTabExtensions"
        TabControlId="searchAdminTabs" PanelControlId="searchAdminPanes" />
    <ul id="searchAdminTabs" runat="server" class="dnnAdminTabNav dnnClear">
        <li><a href="#saGeneral">
            <asp:Label runat="server" ID="generalLink" resourceKey="General"></asp:Label></a></li>
        <li><a href="#saSynonyms">
            <asp:Label runat="server" ID="synonymsLink" resourceKey="Synonyms"></asp:Label></a></li>
        <li><a href="#saStopWords">
            <asp:Label runat="server" ID="stopWordsLink" resourcekey="StopWords"></asp:Label></a></li>
    </ul>
    <div class="dnnClear" id="saGeneral">
        <div class="dnnFormItem">
            <div class="dnnFormMessage dnnFormWarning">
                <%= Localization.GetString("MessageReIndexWarning", LocalResourceFile) %>
            </div>
        </div>
        <div class="dnnFormItem">
            <asp:LinkButton runat="server" ID="btnReIndex" CssClass="dnnSecondaryAction" resourcekey="btnReIndex" OnClick="ReIndex" />
        </div>

    </div>
    <div class="dnnClear" id="saSynonyms">
        <div class="dnnFormItem">
            <p><%= Localization.GetString("introSynonyms", Localization.GetResourceFile(this, MyFileName)) %></p>
        </div>
        <div class="dnnFormItem">
            <div class="dnnTableHeader">
                <dnn:Label ID="plSynonymsCultureList" runat="server" ResourceKey="lblSynonymsCultureList" />
                <div class="dnnLeft dnnLeftCultureList">
                    <asp:Repeater runat="server" ID="rptSynonymsCultureList">
                        <ItemTemplate>
                            <asp:ImageButton runat="server" ID="imgBtnStopWordsCulture" CausesValidation="False" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <a href="javascript:void(0)" class="dnnSecondaryAction dnnRight" id="btnAddSynonymsGroup"><%= Localization.GetString("AddSynonymsGroup", Localization.GetResourceFile(this, MyFileName)) %></a>
                <div class="dnnClear"></div>
            </div>
            <table class="dnnTableDisplay" width="100%" id="synonymsGroupTable">
                <tr class="dnnGridHeader">
                    <th>
                        <span><%= Localization.GetString("SynonymsTags", Localization.GetResourceFile(this, MyFileName)) %></span>
                    </th>
                    <th width="15%">
                        <span><%= Localization.GetString("Actions", Localization.GetResourceFile(this, MyFileName)) %></span>
                    </th>
                </tr>

                <% foreach (var synonymGroup in CurrentPortalSynonymsGroups)
                   { %>
                <tr data-synonymsid="<%= synonymGroup.SynonymsGroupId %>">
                    <td>
                        <span class="synonymsGroupTags"><%= synonymGroup.SynonymsTags.Replace(",", ", ") %></span>
                    </td>
                    <td>
                        <a href="javascript:void(0)" class="btnEditSynonymsGroup"></a>
                        <a href="javascript:void(0)" class="btnDeleteSynonymsGroup"></a>
                    </td>
                </tr>
                <% } %>
            </table>
        </div>
    </div>
    <div class="dnnClear" id="saStopWords">
        <div class="dnnFormItem">
            <p><%= Localization.GetString("introIgnoreWords", Localization.GetResourceFile(this, MyFileName)) %></p>
        </div>
        <div class="dnnFormItem">
            <div class="dnnTableHeader">
                <dnn:Label ID="plCultureList" runat="server" ResourceKey="lblStopwordsCultureList" />
                <div class="dnnLeft dnnLeftCultureList">
                    <asp:Repeater runat="server" ID="rptStopWordsCultureList">
                        <ItemTemplate>
                            <asp:ImageButton runat="server" ID="imgBtnStopWordsCulture" CausesValidation="False" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <a class='dnnSecondaryAction dnnRight' id="btnAddStopwords"><%= Localization.GetString("AddStopWords", Localization.GetResourceFile(this, MyFileName)) %></a>
                <div class="dnnClear"></div>
            </div>
            <table class="dnnTableDisplay" width="100%" id="stopwordsTable">
                <tr class="dnnGridHeader">
                    <th>
                        <span><%= Localization.GetString("StopWords", Localization.GetResourceFile(this, MyFileName)) %></span>
                    </th>
                    <th width="15%">
                        <span><%= Localization.GetString("Actions", Localization.GetResourceFile(this, MyFileName)) %></span>
                    </th>
                </tr>
                <% if (CurrentSearchStopWords != null)
                   { %>
                <tr data-stopwordsid="<%= CurrentSearchStopWords.StopWordsId %>">
                    <td>
                        <span class="stopwordsTags"><%= CurrentSearchStopWords.StopWords.Replace(",", ", ") %></span>
                    </td>
                    <td>
                        <a href="javascript:void(0)" class="btnEditStopwords"></a>
                        <a href="javascript:void(0)" class="btnDeleteStopwords"></a>
                    </td>
                </tr>
                <% } %>
            </table>

        </div>

    </div>
    <asp:PlaceHolder runat="server" ID="searchAdminPanes"></asp:PlaceHolder>
</div>
<!-- persist value from MS AJAX post back -->
<input type="hidden" id="hdnSynonymsSelectedPortalID" runat="server" />
<input type="hidden" id="hdnStopWordsSelectedPortalID" runat="server" />
<input type="hidden" id="hdnSynonymsSelectedCultureCode" runat="server" />
<input type="hidden" id="hdnStopWordsSelectedCultureCode" runat="server" />

<script type="text/javascript">
    if (typeof dnn !== 'undefined' && typeof dnn.searchAdmin !== 'undefined') {
        var moduleId = <%= ModuleContext.ModuleId %>;
        dnn.searchAdmin.init({
            // module id
            moduleId: moduleId,
            
            // synonyms config
            synonymsSelectedPortalIdCtrl: '<%= hdnSynonymsSelectedPortalID.ClientID %>',
            synonymsSelectedCultureCodeCtrl: '<%= hdnSynonymsSelectedCultureCode.ClientID %>',
            msgSynonymsTagRequired: '<%= Localization.GetSafeJSString("SynonymsTagRequired", Localization.GetResourceFile(this, MyFileName)) %>',
            msgSynonymsTagDuplicated: '<%= Localization.GetSafeJSString("SynonymsTagDuplicated", Localization.GetResourceFile(this, MyFileName)) %>',

            // stopwords config
            stopwordsSelectedPortalIdCtrl: '<%= hdnStopWordsSelectedPortalID.ClientID %>',
            stopwordsSelectedCultureCodeCtrl: '<%= hdnStopWordsSelectedCultureCode.ClientID %>',
            
            // reindex
            btnReIndex: '<%= btnReIndex.ClientID %>',
            titleReIndexConfirmation: '<%= Localization.GetSafeJSString("ReIndexConfirmationTitle", Localization.GetResourceFile(this, MyFileName)) %>',
            msgReIndexConfirmation: '<%= Localization.GetSafeJSString("ReIndexConfirmationMessage", Localization.GetResourceFile(this, MyFileName)) %>',
            reIndexConfirmationYes: '<%= Localization.GetString("ReIndexConfirmationYes", Localization.GetResourceFile(this, MyFileName)) %>',
            reIndexConfirmationCancel: '<%= Localization.GetString("ReIndexConfirmationCancel", Localization.GetResourceFile(this, MyFileName)) %>'
        });

        // extensions
        if (dnn.searchAdmin.extensions && dnn.searchAdmin.extensionsInitializeSettings) {
            for (var k in dnn.searchAdmin.extensions) {
                var extensionSettings = dnn.searchAdmin.extensionsInitializeSettings[k];
                var extensionObject = dnn.searchAdmin.extensions[k];
                if (extensionSettings && typeof extensionObject.init == 'function') {
                    extensionSettings.moduleId = moduleId;
                    dnn.searchAdmin.extensions[k].init(extensionSettings);
                }
            }
        }
        
    }
</script>
