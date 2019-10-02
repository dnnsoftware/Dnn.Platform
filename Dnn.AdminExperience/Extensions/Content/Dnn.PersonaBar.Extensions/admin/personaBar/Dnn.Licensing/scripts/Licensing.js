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

                window.dnn.initLicensing = function initializeLicensing() {
                    return {
                        utility: utility,
                        moduleName: 'Licensing'
                    };
                };
                utility.loadBundleScript('modules/dnn.licensing/scripts/bundles/licensing-bundle.js');

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


