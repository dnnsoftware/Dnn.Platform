define(['jquery', 'main/extension', 'main/config'], function ($, ext, cf) {
    'use strict';
    var identifier, utility;
    var config = cf.init();

    function loadScript() {
        var url = "modules/dnn.roles/scripts/bundles/roles-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }

    var init = function (wrapper, util, params, callback) { 
        identifier = params.identifier;
        utility = util;

        window.dnn.initRoles = function initializeRoles() {
            return {
                utility: utility,
                moduleName: 'Roles'
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
