define(['./config'], function (cf) {
    'use strict';
    var isMobile;
    var config = cf.init();
    function loadScript() {
        var url = "scripts/bundles/servers-bundle.js";
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

    var initMobile = function (wrapper, util, params, callback) {
        isMobile = true;
        this.init(wrapper, util, params, callback);
    };

    var load = function (params, callback) {
        if (typeof callback === 'function') {
            callback();
        }
    };

    var loadMobile = function (params, callback) {
        isMobile = true;
        this.load(params, callback);
    };

    return {
        init: init,
        load: load,
        initMobile: initMobile,
        loadMobile: loadMobile
    };
});
