define(['jquery', './extension', '../scripts/config'], function ($, ext, cf) {
    'use strict';
    var isMobile;
    var identifier, utility;
    var config = cf.init();

    function loadScript() {
        var url = "scripts/bundles/roles-bundle.js";
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

    var initMobile = function (wrapper, util, params, callback) {
        isMobile = true;
        this.init(wrapper, util, params, callback);
    };

    var load = function (params, callback) {
        if (typeof callback === 'function') {
            callback();
        }
    };

    var loadMobile = function (params, callback) {
        isMobile = true;
        this.load(params, callback);
    };

    return {
        init: init,
        load: load,
        initMobile: initMobile,
        loadMobile: loadMobile
    };
});
