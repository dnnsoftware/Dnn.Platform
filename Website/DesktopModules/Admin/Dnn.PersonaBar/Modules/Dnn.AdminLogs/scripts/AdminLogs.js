define(['jquery', 'main/extension',
    'main/config'], function ($, ext, cf) {
        'use strict';
        var identifier;

        var utility;
        var config = cf.init();

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
            utility.loadBundleScript('modules/dnn.adminlogs/scripts/bundles/adminLogs-bundle.js');

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
