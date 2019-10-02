define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

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
            utility.loadBundleScript('modules/dnn.siteimportexport/scripts/bundles/siteimportexport-bundle.js');
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
