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

                window.dnn.initSecurity = function initializeSecurity() {
                    return {
                        utility: utility,
                        settings: params.settings,
                        moduleName: 'Security'
                    };
                };
                utility.loadBundleScript('modules/dnn.security/scripts/bundles/security-settings-bundle.js');

                if (typeof callback === 'function') {
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


