﻿define(['jquery'], function ($) {
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
        var dialog = window.top.dnn.ContentEditorManager.getModuleDialog();
        var contentPane = dialog.getDefaultPane();
        var moduleManager = contentPane.data('dnnModuleManager');
        if (moduleManager) {
            moduleManager.getHandler().click();
        }
    }

    return {
        init: init,
        onClick: onClick
    };
});
