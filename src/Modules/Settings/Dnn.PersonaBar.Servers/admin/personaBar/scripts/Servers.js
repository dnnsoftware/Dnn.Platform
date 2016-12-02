define(['main/config'], function (cf) {
    'use strict';
    var config = cf.init();
    function loadScript() {
        var url = "modules/dnn.servers/scripts/bundles/servers-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        window.dnn.initServers = function initServers() {
            if (typeof callback === 'function') {
                callback();
            }
            return {
                utilities: util,
                moduleName: "Servers",
                config: config,
                settings: params.settings
            };
        };
        loadScript();       
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
