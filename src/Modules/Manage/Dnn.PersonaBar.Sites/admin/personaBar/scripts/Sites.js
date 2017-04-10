define(['jquery', 'main/extension', 'main/config', './exportables/Sites/SitesListView'], function ($, ext, cf, slv) {
    'use strict';
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
            params.cultureCode = config.culture;
            return params;
        };
        loadScript();

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
