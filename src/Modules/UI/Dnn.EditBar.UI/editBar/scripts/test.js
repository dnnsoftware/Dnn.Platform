define(['jquery'], function ($) {
    'use strict';

    var init = function (menu, util, params, callback) {

        console.log('initialized');

        if (typeof callback === 'function') {
            callback();
        }
    };

    return {
        init: init
    };
});
