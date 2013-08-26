<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchBoxControl.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.SearchBoxControl" %>

<div id="dnnModuleDigitalAssetsSearchBox">            
    <input type="text" class="searchInput" placeholder='<%=LocalizeString("Search.Placeholder")%>' />
    <a href="#" title='<%=LocalizeString("Search.Title")%>' class="searchButton"></a>
</div>

<script type="text/javascript">
    $(function () {
        var searchBox = new dnnModule.DigitalAssetsSearchBox($, $('#<%=Parent.ClientID%>'), $.ServicesFramework(<%=ModuleId %>));
        searchBox.init();

        dnnModule.digitalAssets.setSearchProvider(searchBox);
    });
</script>