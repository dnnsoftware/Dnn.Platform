jQuery.browser = {}; (function () {
    jQuery.browser.msie = false;
    jQuery.browser.version = 0;
    if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
        jQuery.browser.msie = true; jQuery.browser.version = RegExp.$1;
    }
})();

function SwitchView(n) {
    jQuery("#ListViewState").val(n);
    var t = jQuery("#FilesBox ul");
    t.attr("class", "Files" + n),
    jQuery(".SwitchDetailView").css("font-weight", "normal"),
    jQuery(".SwitchListView").css("font-weight", "normal"),
    jQuery(".SwitchIconsView").css("font-weight", "normal"),
    jQuery(".Switch" + n).css("font-weight", "bold");
}

jQuery(document).ready(function () {
    jQuery("#BrowserMode input:checked").parent("td").addClass("SelectedPager"),
    jQuery("#dnntreeTabs li .rtIn, #dnntreeTabs li .rtImg").click(function () {
        jQuery("#panelLoading").show();
    }), SwitchView(jQuery("#ListViewState").val()),
    jQuery('input[type="submit"],.LinkNormal,.LinkDisabled').button(), jQuery(".Toolbar").buttonset(),
    jQuery("#txtWidth,#txtHeight").spinner(),
    jQuery("#BrowserMode td").addClass("ui-state-default ui-corner-top"),
    jQuery("#BrowserMode label").addClass("ui-tabs-anchor"),
    jQuery(".SelectedPager").addClass("ui-tabs-active ui-state-active ui-state-focus"),

    jQuery("#BrowserMode td").hover(function () {
        jQuery(this).addClass("ui-state-hover");
    }, function () {
        jQuery(this).removeClass("ui-state-hover");
    });

    jQuery("#panUploadDiv .MessageBox").draggable({ cursor: "move", handle: "div.modalHeader" });
});
