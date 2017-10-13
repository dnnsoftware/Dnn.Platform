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

                window.dnn.initVocabularies = function initializeVocabularies() {
                    return {
                        utility: utility,
                        settings: params.settings,
                        moduleName: 'Vocabularies'
                    };
                };
                utility.loadBundleScript('modules/dnn.vocabularies/scripts/bundles/vocabulary-bundle.js');

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


