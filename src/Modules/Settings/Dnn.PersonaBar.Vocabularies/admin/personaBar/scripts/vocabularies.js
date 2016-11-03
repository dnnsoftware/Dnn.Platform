'use strict';
define(['jquery',
    '../scripts/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        function loadScript() {
            var url = "scripts/bundles/vocabulary-bundle.js";
            $.ajax({
                dataType: "script",
                cache: true,
                url: url
            });
        }

        return {
            init: function (wrapper, util, params, callback) {
                utility = util;


                window.dnn.initVocabularies = function initializeVocabularies() {
                    return {
                        utility: utility,
                        moduleName: 'Vocabularies'
                    };
                };
                loadScript();

                if (config.debugMode === true) {
                    window.__REACT_DEVTOOLS_GLOBAL_HOOK__ = window.parent.__REACT_DEVTOOLS_GLOBAL_HOOK__;
                }

                if (typeof callback === 'function') {
                    callback();
                }
            },

            initMobile: function (wrapper, util, params, callback) {
                this.init(wrapper, util, params, callback);
            },

            load: function (params, callback) {
                var fb = window.dnn.formBuilder;
                if (fb && fb.load) {
                    fb.load();
                }
                var optin = window.dnn.optIn;
                if (optin && optin.load) {
                    var mode = getOptInMode();
                    optin.load(mode);
                }

                if (typeof callback === 'function') {
                    callback();
                }
            },

            loadMobile: function (params, callback) {
                this.load(params, callback);
            }
        };
    });


