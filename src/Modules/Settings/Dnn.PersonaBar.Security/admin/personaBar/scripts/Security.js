'use strict';
define(['jquery',
    'main/config'
],
    function ($, cf) {
        var utility;
        var config = cf.init();

        function loadScript() {
            var url = "modules/dnn.security/scripts/bundles/security-settings-bundle.js";
            $.ajax({
                dataType: "script",
                cache: true,
                url: url
            });
        }

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
                loadScript();

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


