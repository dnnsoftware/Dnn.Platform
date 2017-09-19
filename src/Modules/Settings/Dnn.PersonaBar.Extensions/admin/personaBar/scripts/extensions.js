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

                window.dnn.initExtensions = function initializeExtensions() {
                    return {
                        utility: utility,
                        siteRoot: config.siteRoot,
                        settings: params.settings,
                        moduleName: 'Extensions'
                    };
                };
                utility.loadBundleScript('modules/dnn.extensions/scripts/bundles/extensions-bundle.js');

                if (typeof callback === "function") {
                    callback();
                }
            },

            load: function (params, callback) {
                if (typeof callback === "function") {
                    callback();
                }
            }
        };
    });


