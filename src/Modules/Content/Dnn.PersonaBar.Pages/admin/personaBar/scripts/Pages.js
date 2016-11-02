define(['jquery', 'knockout', './extension', './config', 'jquery-ui.min', 'dnn.jquery'], function ($, ko, ext, cf) {
    'use strict';
    window.ko = ko;

    var isMobile;
    var identifier;
    var config = cf.init();
    function loadScript() {
        var url = "http://localhost:8080/dist/pages-bundle.js";
        //var url = "scripts/bundles/pages-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;
        window.dnn.initPages = function initializePages() {
            if (typeof callback === 'function') {
                callback();
            }

            return {
                utility: util,
                moduleName: "Pages"
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
