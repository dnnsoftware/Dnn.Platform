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

    }

    return {
        init: init,
        onClick: onClick
    };
});
