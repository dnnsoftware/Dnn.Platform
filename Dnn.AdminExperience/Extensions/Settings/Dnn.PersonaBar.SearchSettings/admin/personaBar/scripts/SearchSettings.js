define(['jQuery', './extension'], function ($, ext) {
    'use strict';
    var isMobile;
    var identifier;

    var init = function (wrapper, util, params, callback) {
        identifier = params.identifier;

        wrapper.find('.primarybtn').click(function() {
            ext.callAction(identifier, 'save');
        });

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
