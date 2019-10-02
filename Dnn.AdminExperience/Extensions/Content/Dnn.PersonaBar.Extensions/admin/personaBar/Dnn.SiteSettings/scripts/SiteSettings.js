'use strict';
define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        return {
            init: function (wrapper, util, params, callback) {
                utility = util;

                window.dnn.initSiteSettings = function initializeSiteSettings() {
                    return {
                        utility: utility,
                        siteRoot: config.siteRoot,
                        settings: params.settings,
                        moduleName: 'SiteSettings',
                        identifier: params.identifier
                    };
                };
                utility.loadBundleScript('modules/dnn.sitesettings/scripts/bundles/site-settings-bundle.js');
                
                if (typeof callback === "function") {
                    callback();
                }
            },

            load: function (params, callback) {
                if (typeof callback === 'function') {
                    callback();
                }
            }
        };
    });


