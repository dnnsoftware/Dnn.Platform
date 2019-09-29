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
        var dialog = window.top.dnn.ContentEditorManager.getExsitingModuleDialog();
        var contentPane = dialog.getDefaultPane();
        var moduleManager = contentPane.data('dnnModuleManager');
        if (moduleManager) {
            moduleManager.getExistingModuleHandler().click();
        }
    }

    return {
        init: init,
        onClick: onClick
    };
});
