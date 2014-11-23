<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.SearchResults.SearchResults" CodeFile="SearchResults.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnSearchBoxPanel">
    <a href="javascript:void(0)" class="dnnSearchResultAdvancedTip"><%= LinkAdvancedTipText %></a>
    <input type="text" id="dnnSearchResult_dnnSearchBox" value="<%= SearchDisplayTerm %>" />
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
        <dnn:DnnComboBox ID="ResultsPerPageList" runat="server" OnClientSelectedIndexChanged="dnnSearchResultPageSizeChanged">
            <Items>
                <dnn:DnnComboBoxItem runat="Server" Text="15" Value="15" />
                <dnn:DnnComboBoxItem runat="Server" Text="25" Value="25" />
                <dnn:DnnComboBoxItem runat="Server" Text="50" Value="50" />
                <dnn:DnnComboBoxItem runat="Server" Text="100" Value="100" />
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
        <dnn:Label ID="lblAdvancedTags" runat="server" ResourceKey="lblAdvancedTags" />
        <input type="text" id="advancedTagsCtrl" value="<%=TagsQuery %>" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedDates" runat="server" ResourceKey="lblAdvancedDates" />
        <dnn:DnnComboBox ID="AdvnacedDatesList" runat="server">
            <Items>
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionAll.Text" Value="" Selected="True" />
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionDay.Text" Value="day" />
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionWeek.Text" Value="week" />
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionMonth.Text" Value="month" />
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionQuarter.Text" Value="quarter" />
                <dnn:DnnComboBoxItem runat="Server" ResourceKey="optionYear.Text" Value="year" />
            </Items>
        </dnn:DnnComboBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedScope" runat="server" ResourceKey="lblAdvancedScope" />
        <dnn:DnnComboBox ID="SearchScopeList" runat="server" CheckBoxes="true" Width="235px" OnClientItemChecking="dnnSearchResultScopeItemChecking" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lblAdvancedExactSearch" runat="server" ResourceKey="lblAdvancedExactSearch" />
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
    function dnnSearchResultScopeItemChecking(sender, e) {
        var combo = $find('<%= SearchScopeList.ClientID %>');
        var items = combo.get_items();
        var countOfChecked = 0;
        for (var i = 0; i < items.get_count(); i++) {
            var checked = items.getItem(i).get_checked();
            if (checked) countOfChecked++;
        }
        
        if (countOfChecked == 1) {
            var item = e.get_item();
            if (item.get_checked()) e.set_cancel(true);
        }
    }
    
    function dnnSearchResultPageSizeChanged(sender, e) {
        var combo = $find('<%= ResultsPerPageList.ClientID %>');
        var pageSize = combo.get_value();
        if (typeof dnn != 'undefined' && dnn.searchResult) {
            dnn.searchResult.queryOptions.pageSize = pageSize;
            dnn.searchResult.queryOptions.pageIndex = 1;
            dnn.searchResult.doSearch();
        }
    }
    $(function () {
        if(typeof dnn != 'undefined' && dnn.searchResult){
            dnn.searchResult.moduleId = <%= ModuleId %>;
            dnn.searchResult.queryOptions = {
                searchTerm: '<%= SearchTerm %>',
                sortOption: 0,
                pageIndex: 1,
                pageSize: 15
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
                cultureCode: '<%= CultureCode %>'
                
            });
        }
    });
</script>

