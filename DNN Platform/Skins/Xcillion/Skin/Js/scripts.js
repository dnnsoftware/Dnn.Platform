function buttonUp() {
    var e = $(".search .searchInputContainer input").val();
    e = $.trim(e).length, 0 !== e ?
        (
            $(".search").css("overflow", "visible")
        )
        :
        (
            $(".search .searchInputContainer input").val(""),
            $(".search-toggle-icon").css("display", "block"),
            $(".search").css("overflow", "hidden")
        ),
        $(".search .searchInputContainer a.dnnSearchBoxClearText").click(function () {
            $(".search .searchInputContainer a.dnnSearchBoxClearText").hasClass("dnnShow")
                ?
                $(this).css("overflow", "visible")
                :
                $(".search").css("overflow", "hidden")
        });
}
$(document).ready(function () {
    $(".navbar-nav.sm-collapsible .caret").click(function (e) {
        e.preventDefault()
    }), 
    $('[data-toggle="tooltip"]').length && 
    $('[data-toggle="tooltip"]').tooltip(), 
    $('<span class="search-toggle-icon"></span>').insertAfter(".search a.SearchButton");
    var searchBox = $(".search"),
        searchToggleIcon = $(".search-toggle-icon"),
        clearSearchBox = $(".search .searchInputContainer a.dnnSearchBoxClearText"),
        searchInput = $(".search .searchInputContainer input"),
        searchActive = false;
    searchToggleIcon.click(function (event) {
        event.stopPropagation();
        !searchActive
            ?
            (
                searchBox.addClass("search-open"),
                searchInput.focus(),
                searchActive = true
            )
            :
            (
                searchBox.removeClass("search-open"),
                searchInput.focusout(),
                searchInput.val(""),
                clearSearchBox.removeClass("dnnShow"),
                $(".search").css({ "overflow": "hidden" }),
                searchActive = false
            )
    }),
        searchBox.mouseup(function () {
            return false
        }),
        searchToggleIcon.mouseup(function () {
            return false
        }), $(document).click(function (event) {
            if (!($(event.target).parents(".search").length)) {
                searchActive == true && (searchToggleIcon.click())
            }
        }), searchInput.keyup(buttonUp), $("a#search-action").click(function () {
            $("#search-top").toggleClass("active")
        })
});
