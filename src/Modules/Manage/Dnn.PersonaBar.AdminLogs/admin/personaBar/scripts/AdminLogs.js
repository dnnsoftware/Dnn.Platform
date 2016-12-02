define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

        function loadScript() {
            var url = "modules/dnn.adminlogs/scripts/bundles/adminLogs-bundle.js";
            $.ajax({
                dataType: "script",
                cache: true,
                url: url
            });
        }

        var init = function (wrapper, util, params, callback) {
            identifier = params.identifier;
            utility = util;

            window.dnn.initAdminLogs = function initializeAdminLogs() {
                return {
                    utility: utility,
                    settings: params.settings,
                    moduleName: 'AdminLogs'
                };
            };
            loadScript();

            if (typeof callback === 'function') {
                callback();
            }
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
