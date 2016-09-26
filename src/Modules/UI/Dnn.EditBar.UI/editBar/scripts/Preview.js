define(['jquery', './PreviewModeManager'], function ($, previewModeManager) {
    'use strict';
    var menuItem, util, customLayout, $menu;

    var init = function (menu, utility, params, callback) {
        menuItem = menu;
        util = utility;
        customLayout = params.html;
        $menu = $('#menu-' + menu.name);
        $menu.append('<div class="submenuEditBar">' + customLayout + '</div>');

        var previewManager = previewModeManager.getInstance(menuItem, util, params);
        previewManager.initViewMode();

        $menu.mouseenter(function() {
            util.switchMode('middle');
        }).mouseleave(function() {
            util.switchMode('small');
        });

        if (typeof callback === 'function') {
            callback();
        }
    };

    var onClick = function () {

    }

    return {
        init: init,
        onClick: onClick
    };
});
