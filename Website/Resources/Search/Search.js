(function ($, dnn) {
    var isDropDownVisible = false;
    var searchSkinObject = null;
    var toggleDropDown = function(eventElement) {
        var choices = $get('SearchChoices');
        if (isDropDownVisible) {
            choices.style.display = 'none';
            isDropDownVisible = false;
        } else {
            choices.style.display = 'block';
            isDropDownVisible = true;

            var clickEvent = function(e) {
                if (isDropDownVisible && e.target.id != "SearchIcon" && e.target.id.indexOf("downArrow") == -1) {
                    toggleDropDown();
                    $(document).unbind("click", clickEvent);
                }
            };

            $(document).bind("click", clickEvent);
        }
    };

    var selectSearch = function(eventElement) {
        toggleDropDown(eventElement);
        $get('SearchIcon').style.backgroundImage = dnn.getVar(eventElement.target.id + 'Url');

        /* We use 'W' and 'S' to keep our code consistent with the old search skin object */
        if (eventElement.target.id.indexOf("Web") > 0) {
            dnn.setVar('SearchIconSelected', 'W');
            if(searchSkinObject)
                searchSkinObject.settings.searchType = 'W';
        } else {
            dnn.setVar('SearchIconSelected', 'S');
            if (searchSkinObject)
                searchSkinObject.settings.searchType = 'S';
        }
    };

    var searchHilite = function(eventElement) {
        eventElement.target.className = 'searchHilite';
    };

    var searchDefault = function(eventElement) {
        eventElement.target.className = 'searchDefault';
    };

    var initSearch = function() {
        var searchIcon = $get('SearchIcon');
        if (dnn.getVar('SearchIconSelected') == 'S') {
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        } else {
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        }
        $addHandler(searchIcon, 'click', toggleDropDown);

        var siteIcon = $get('SearchIconSite');
        siteIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        $addHandler(siteIcon, 'click', selectSearch);
        $addHandler(siteIcon, 'mouseover', searchHilite);
        $addHandler(siteIcon, 'mouseout', searchDefault);

        var webIcon = $get('SearchIconWeb');
        webIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        $addHandler(webIcon, 'click', selectSearch);
        $addHandler(webIcon, 'mouseover', searchHilite);
        $addHandler(webIcon, 'mouseout', searchDefault);

        /* Set the default display style to resolve DOM bug */
        $get('SearchChoices').style.display = 'none';
    };

    dnn.initDropdownSearch = function (skinObject) {
        searchSkinObject = skinObject;
        initSearch();
    };

})(jQuery, window.dnn);
