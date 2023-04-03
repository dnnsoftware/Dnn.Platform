'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
            var inIframe = window.parent && typeof window.parent.dnn !== "undefined";

            var tabId = inIframe ? window.parent.dnn.getVar('sf_tabId') : '';
            var siteRoot = inIframe ? window.parent.dnn.getVar('sf_siteRoot') : '';
            var antiForgeryToken = inIframe ? window.parent.document.getElementsByName('__RequestVerificationToken')[0].value : '';

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['personaBarSettings'] : {});

            return config;   
       } 
    };
});