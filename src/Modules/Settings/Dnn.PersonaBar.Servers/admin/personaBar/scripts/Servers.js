define(['main/config'], function (cf) {
    'use strict';
    var config = cf.init();

    var init = function (wrapper, util, params, callback) {
        params.handleTabViewInModule = true;

        window.dnn.initServers = function initServers() {
            if (typeof callback === 'function') {
                callback();
            }
            return {
                utilities: util,
                moduleName: "Servers",
                path: params.path,
                config: config,
                settings: params.settings
            };
        };
        util.loadBundleScript('modules/dnn.servers/scripts/bundles/servers-bundle.js');
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
