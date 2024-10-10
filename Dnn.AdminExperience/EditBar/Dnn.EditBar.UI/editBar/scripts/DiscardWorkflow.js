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
        util.sf.moduleRoot = 'internalservices';
        util.sf.controller = "contentWorkflowService";
        util.sf.post('DiscardWorkflow', {}, function done() {
            window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
        });
    }

    return {
        init: init,
        onClick: onClick
    };
});
