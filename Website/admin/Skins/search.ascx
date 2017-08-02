<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Search" ViewStateMode="Disabled" Codebehind="Search.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>

<span id="ClassicSearch" runat="server" visible="true">
    <asp:RadioButton ID="WebRadioButton" runat="server" CssClass="SkinObject" GroupName="Search" />
    <asp:RadioButton ID="SiteRadioButton" runat="server" CssClass="SkinObject" GroupName="Search" />
    <span class="searchInputContainer" data-moreresults="<%= SeeMoreText %>" data-noresult="<%= NoResultText %>">
        <asp:TextBox ID="txtSearch" runat="server" EnableViewState="False" CssClass="NormalTextBox" Columns="20" MaxLength="255" aria-label="Search"></asp:TextBox>
        <a class="dnnSearchBoxClearText" title="<%= ClearQueryText %>"></a>
    </span>
    <asp:LinkButton ID="cmdSearch" runat="server" CausesValidation="False" CssClass="SkinObject"></asp:LinkButton>
</span>

<div id="DropDownSearch" runat="server" class="SearchContainer" visible="false">
    <div class="SearchBorder">
        <div id="SearchIcon" class="SearchIcon">
            <dnn:DnnImage ID="downArrow" runat="server" IconKey="Action" />
        </div>
        <span class="searchInputContainer" data-moreresults="<%= SeeMoreText %>" data-noresult="<%= NoResultText %>">
            <asp:TextBox ID="txtSearchNew" runat="server" CssClass="SearchTextBox" MaxLength="255" EnableViewState="False" aria-label="Search"></asp:TextBox>
            <a class="dnnSearchBoxClearText" title="<%= ClearQueryText %>"></a>
        </span>

        <ul id="SearchChoices">
            <li id="SearchIconSite"><%=SiteText%></li>
            <li id="SearchIconWeb"><%=WebText%></li>
        </ul>
    </div>
    <asp:LinkButton ID="cmdSearchNew" runat="server" CausesValidation="False" CssClass="SkinObject SearchButton"></asp:LinkButton>
</div>
<script type="text/javascript">
    $(function() {
        if (typeof dnn != "undefined" && typeof dnn.searchSkinObject != "undefined") {
            var searchSkinObject = new dnn.searchSkinObject({
                delayTriggerAutoSearch : <%= AutoSearchDelayInMilliSecond.ToString() %>,
                minCharRequiredTriggerAutoSearch : <%= MinCharRequired.ToString() %>,
                searchType: '<%= SearchType %>',
                enableWildSearch: <%= EnableWildSearch.ToString().ToLowerInvariant() %>,
                cultureCode: '<%= CultureCode %>',
                portalId: <%= PortalId %>
                }
            );
            searchSkinObject.init();
            
            <% if (!UseDropDownList)
               { %>
            // attach classic search
            var siteBtn = $('#<%= SiteRadioButton.ClientID %>');
            var webBtn = $('#<%= WebRadioButton.ClientID %>');
            var clickHandler = function() {
                if (siteBtn.is(':checked')) searchSkinObject.settings.searchType = 'S';
                else searchSkinObject.settings.searchType = 'W';
            };
            siteBtn.on('change', clickHandler);
            webBtn.on('change', clickHandler);
            
            <% }
               else
               { %>

               // attach dropdown search
            if (typeof dnn.initDropdownSearch != 'undefined') {
                dnn.initDropdownSearch(searchSkinObject);
            }
            
            <% } %>
        }
    });
</script>
