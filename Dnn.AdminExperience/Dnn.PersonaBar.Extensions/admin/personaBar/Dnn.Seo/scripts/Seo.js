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

                window.dnn.initSeo = function initializeSeo() {
                    return {
                        utility: utility,
                        settings: params.settings,
                        moduleName: 'Seo'
                    };
                };
                utility.loadBundleScript('modules/dnn.seo/scripts/bundles/seo-bundle.js');

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


