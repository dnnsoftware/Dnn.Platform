// IE8 doesn't like using var dnnModule = dnnModule || {}
if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

dnnModule.DigitalAssetsSearchBox = function ($, $scope, servicesFramework) {

    var onSearchFunction = null;

    function search() {
        var newPattern = $("#dnnModuleDigitalAssetsSearchBox>input.searchInput", $scope).val();
        newPattern = $.trim(newPattern);
        
        if (onSearchFunction) {
            onSearchFunction(newPattern);
        }
    }

    function clearSearch() {
        $("#dnnModuleDigitalAssetsSearchBox>input.searchInput", $scope).val(''); 
    }
    
    function doSearch(currentFolderId, pattern, startIndex, numItems, sortExpression, before, done, fail, always) {
        var contentServiceUrl = servicesFramework.getServiceRoot('DigitalAssets') + 'ContentService/';
        
        before();
        $.ajax({
            url: contentServiceUrl + "SearchFolderContent",
            data: {
                "folderId": currentFolderId,
                "pattern": pattern,
                "startIndex": startIndex,
                "numItems": numItems,
                "sortExpression": sortExpression
            },
            type: "POST",
            beforeSend: servicesFramework.setModuleHeaders
        }).done(done).fail(fail).always(always);
    };

    function init () {
        $("#dnnModuleDigitalAssetsSearchBox>input.searchInput", $scope).keypress(function(e) {
            if (e.which == 13) {
                e.preventDefault();
                search();
                return false;
            }
            return true;
        });

        $("#dnnModuleDigitalAssetsSearchBox>a.searchButton", $scope).click(function(e) {
            e.preventDefault();
            search();
            return false;
        });
    }
    
    function onSearch(onSearchFunc) {
        onSearchFunction = onSearchFunc;
    }
    
    function highlightItemName(pattern, itemName) {
        if (!pattern || pattern == "" || pattern.indexOf("?") != -1) {
            return itemName;
        }

        if (pattern.lastIndexOf("*") == pattern.length - 1) {   // example*            
            pattern = pattern.substring(0, pattern.length - 1);

            if (pattern.lastIndexOf("*") == 0) { // *example* --> Highlight al occurrences
                pattern = pattern.substring(1, pattern.length);

                if (pattern.indexOf("*") == -1) {

                    var matches = itemName.match(new RegExp(pattern, "i"));
                    if (matches) {
                        for (var i = 0; i < matches.length; i++) {
                            itemName = itemName.replace(matches[i], "<font class='dnnModuleDigitalAssetsHighlight'>" + matches[i] + "</font>");
                        }
                    }
                }

                return itemName;
            }
        }

        if (pattern.indexOf("*") == -1) {   // highlight the beginning                    
            itemName = "<font class='dnnModuleDigitalAssetsHighlight'>" + itemName.substring(0, pattern.length) + "</font>" +
                itemName.substring(pattern.length);
        }

        return itemName;
    }

    return {
        init: init,
        doSearch: doSearch,
        onSearch: onSearch,
        clearSearch: clearSearch,
        highlightItemName: highlightItemName,
    };
}