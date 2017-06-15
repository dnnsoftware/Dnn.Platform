define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

        function loadScript() {
            var url = "modules/dnn.siteimportexport/scripts/bundles/siteimportexport-bundle.js";
            $.ajax({
                dataType: "script",
                cache: true,
                url: url
            });
        }

        var init = function (wrapper, util, params, callback) {
            identifier = params.identifier;
            utility = util;

            window.dnn.initSiteImportExport = function initializeSiteImportExport() {
                if (typeof callback === 'function') {
                    callback();
                }

                return {
                    utility: utility,
                    settings: params.settings,
                    moduleName: 'SiteImportExport'
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
