define(['jquery'], function ($) {
    'use strict';
    var menuItem, util;

    var init = function (menu, utility, params, callback) {
        menuItem = menu;
        util = utility;

        if (typeof callback === 'function') {
            callback();
        }
    };

    var onClick = function () {
        var mode = "View";

        util.sf.moduleRoot = 'internalservices';
        util.sf.controller = "controlBar";
        util.sf.post('ToggleUserMode', { UserMode: mode }, function handleToggleUserMode() {
            window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
        });
    }

    return {
        init: init,
        onClick: onClick
    };
});
