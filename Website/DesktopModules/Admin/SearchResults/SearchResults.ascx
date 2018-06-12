<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.SearchResults.SearchResults" Codebehind="SearchResults.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnSearchBoxPanel">
    <a href="javascript:void(0)" class="dnnSearchResultAdvancedTip"><%= LinkAdvancedTipText %></a>
    <input type="text" id="dnnSearchResult_dnnSearchBox" value="<%= HttpUtility.HtmlAttributeEncode(SearchTerm) %>" aria-label="Search Term" />
    <div id="dnnSearchResult-advancedTipContainer">
        <%= AdvancedSearchHintText %>
    </div>
</div>

<div class="dnnSearchResultPanel">
    <div class="dnnRight">
        <ul class="dnnSearchResultSortOptions">
            <li class="active"><a href="#byRelevance"><%= RelevanceText %></a></li>
            <li><a href="#byDate"><%= DateText %></a></li>
        </ul>
    </div>

    <div class="dnnRight">
        <span class="dnnSearchResultCountPerPage"><%= ResultsPerPageText %></span>
        <dnn:DnnComboBox ID="ResultsPerPageList" runat="server" OnClientSelectedIndexChanged="dnnSearchResultPageSizeChanged" aria-label="PageSize">
            <Items>
                <asp:ListItem runat="Server" Text="15" Value="15" />
                <asp:ListItem runat="Server" Text="25" Value="25" />
                <asp:ListItem runat="Server" Text="50" Value="50" />
                <asp:ListItem runat="Server" Text="100" Value="100" />
            </Items>
        </dnn:DnnComboBox>
    </div>
    <div class="dnnClear"></div>
</div>

<div class="dnnSearchResultPager dnnSearchResultPagerTop">
    <div class="dnnLeft">
        <span></span>
    </div>
    <div class="dnnRight">
    </div>
    <div class="dnnClear"></div>
</div>

<div class="dnnSearchResultContainer">
</div>

<div class="dnnSearchResultPager">
    <div class="dnnLeft">
        <span></span>
    </div>
    <div class="dnnRight">
    </div>
</div>

<div id="dnnSearchResultAdvancedForm" class="dnnForm">
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedTags" runat="server" ResourceKey="lblAdvancedTags" ControlName="advancedTagsCtrl" />
        <input type="text" id="advancedTagsCtrl" value="<%= HttpUtility.HtmlAttributeEncode(TagsQuery) %>" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedDates" runat="server" ResourceKey="lblAdvancedDates" ControlName="AdvnacedDatesList" />
        <dnn:DnnComboBox ID="AdvnacedDatesList" runat="server" aria-label="Advanced Date">
            <Items>
                <asp:ListItem runat="Server" ResourceKey="optionAll.Text" Value="" Selected="True" />
                <asp:ListItem runat="Server" ResourceKey="optionDay.Text" Value="day" />
                <asp:ListItem runat="Server" ResourceKey="optionWeek.Text" Value="week" />
                <asp:ListItem runat="Server" ResourceKey="optionMonth.Text" Value="month" />
                <asp:ListItem runat="Server" ResourceKey="optionQuarter.Text" Value="quarter" />
                <asp:ListItem runat="Server" ResourceKey="optionYear.Text" Value="year" />
            </Items>
        </dnn:DnnComboBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedScope" runat="server" ResourceKey="lblAdvancedScope" ControlName="SearchScopeList" />
        <dnn:DnnComboBox ID="SearchScopeList" runat="server" CheckBoxes="true" Width="235px" OnClientSelectedIndexChanged="dnnSearchResultScopeItemChanged" aria-label="Advanced Scope" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedExactSearch" runat="server" ResourceKey="lblAdvancedExactSearch" ControlName="dnnSearchResultAdvancedExactSearch" />
        <input type="checkbox" id="dnnSearchResultAdvancedExactSearch" <%= CheckedExactSearch %> />
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <a id="dnnSearchResultAdvancedSearch" class="dnnPrimaryAction"><%= SearchButtonText %></a>
        </li>
        <li>
            <a id="dnnSearchResultAdvancedClear" class="dnnSecondaryAction"><%= ClearButtonText %></a>
        </li>
    </ul>
</div>

<script type="text/javascript">
    function dnnSearchResultScopeItemChanged(value) {
        if (value === '') {
            var self = this;
            setTimeout(function() {
                if (self.$activeOption) {
                    self.addItem(self.$activeOption.data('value'));
                    self.refreshOptions(true);
                }
            }, 0);
        }
    }

    function dnnSearchResultPageSizeChanged(value) {
        var pageSize = value;
        if (typeof dnn != 'undefined' && dnn.searchResult && pageSize) {
            dnn.searchResult.queryOptions.pageSize = pageSize;
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();
        }
    }
    $(function () {
        if(typeof dnn != 'undefined' && dnn.searchResult){
            dnn.searchResult.moduleId = <%= ModuleId %>;
            dnn.searchResult.queryOptions = {
                searchTerm: '<%= Localization.GetSafeJSString(SearchTerm) %>',
                sortOption: <%= SortOption %>,
                pageIndex: <%= PageIndex %>,
                pageSize: <%= PageSize %>
            };

            dnn.searchResult.init({
                defaultText: '<%= DefaultText %>',
                comboAdvancedDates: '<%= AdvnacedDatesList.ClientID %>',
                comboAdvancedScope: '<%= SearchScopeList.ClientID %>',
                noresultsText: '<%= NoResultsText %>',
                advancedText: '<%= AdvancedText %>',
                sourceText: '<%= SourceText %>',
                authorText: '<%= AuthorText %>',
                likesText: '<%= LikesText %>',
                viewsText: '<%= ViewsText %>',
                commentsText: '<%= CommentsText %>',
                tagsText: '<%= TagsText %>',
                addTagText: '<%= AddTagText %>',
                lastModifiedText: '<%= LastModifiedText %>',
                resultsCountText: '<%= ResultsCountText %>',
                currentPageIndexText: '<%= CurrentPageIndexText %>',
                linkTarget: '<%= LinkTarget %>',
                showDescription: <%= ShowDescription %>,
                maxDescriptionLength: <%= MaxDescriptionLength %>,
                showSnippet: <%= ShowSnippet %>,
                showSource: <%= ShowSource %>,
                showLastUpdated: <%= ShowLastUpdated %>,
                showTags: <%= ShowTags %>,
                cultureCode: '<%= CultureCode %>'

            });
        }
    });
</script>

