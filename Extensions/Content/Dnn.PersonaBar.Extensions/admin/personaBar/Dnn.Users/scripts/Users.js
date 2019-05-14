define(['jquery', 'main/extension', 'main/config', './exportables/Users/UsersCommon'], function ($, ext, cf) {
    'use strict';
    var identifier;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;
        window.dnn.initUsers = function initializeUsers() {
            return {
                utility: util,
                settings: params.settings,
                moduleName: 'Users'
            };
        };
        util.loadBundleScript('modules/dnn.users/scripts/bundles/users-bundle.js');

        if (typeof callback === 'function') {
            callback();
        }
    };

    var load = function (params, callback) {
        if (typeof callback === 'function') {
            callback();
        }
    };

    return {
        init: init,
        load: load
    };
});
