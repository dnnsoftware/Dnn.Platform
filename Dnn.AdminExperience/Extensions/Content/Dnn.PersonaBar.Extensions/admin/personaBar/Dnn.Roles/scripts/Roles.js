define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
    'use strict';
    var identifier, utility;
    var config = cf.init();

    var init = function (wrapper, util, params, callback) { 
        identifier = params.identifier;
        utility = util;

        window.dnn.initRoles = function initializeRoles() {
            return {
                utility: utility,
                settings: params.settings,
                moduleName: 'Roles'
            };
        };
        utility.loadBundleScript('modules/dnn.roles/scripts/bundles/roles-bundle.js');

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
