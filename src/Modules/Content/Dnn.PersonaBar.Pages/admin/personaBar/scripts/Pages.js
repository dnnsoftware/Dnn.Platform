define(['jquery', 'knockout', './extension', './config', 'jquery-ui.min', 'dnn.jquery'], function ($, ko, ext, cf) {
    'use strict';
    window.ko = ko;

    var isMobile;
    var config = cf.init();
    function loadScript() {
        //var url = "http://localhost:8080/dist/pages-bundle.js"
        var url = "scripts/bundles/pages-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        window.dnn.initPages = function initializePages() {
            return {
                utilities: util,
                moduleName: "Pages",
                config: config,
                viewName: (params && params.viewName) ? params.viewName : null,
                viewParams: (params && params.viewParams) ? params.viewParams : null,
                settings: params.settings
            };
        };

        loadScript();

        if (typeof callback === 'function') {
            callback();
        }
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
