define(['jquery', 'main/extension', 'main/config', './exportables/Sites/SitesListView'], function ($, ext, cf, slv) {
    'use strict';
    var isMobile;
    var identifier;
    var config = cf.init();
    function loadScript() {
        var url = "modules/dnn.sites/scripts/bundles/sites-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;
        window.dnn.initSites = function initializeSites() {
            return params;
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
