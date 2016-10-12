define(['jquery', './extension', './config', './exportables/Sites/SitesListView'], function ($, ext, cf, slv) {
    'use strict';
    var isMobile;
    var identifier;
    var config = cf.init();
    function loadScript() {
        // var url = "http://localhost:8080/dist/sites-bundle.js";
        var url = "scripts/bundles/sites-bundle.js"
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;
        window.dnn.initSites = function initializeSites() {
            return {
                utility: util,
                moduleName: "Sites"
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
