define(['jquery', './extension', './config', './exportables/Users/UsersCommon'], function ($, ext, cf) {
    'use strict';
    var isMobile;
    var identifier;

    var config = cf.init();

    function loadScript() {
        var url = "scripts/bundles/users-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;
        window.dnn.initUsers = function initializeUsers() {
            return {
                utility: util,
                moduleName: 'Users'
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
