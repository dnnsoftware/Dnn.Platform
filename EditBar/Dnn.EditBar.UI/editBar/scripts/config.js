'use strict';
define(['jquery'], function ($) {
    return {
        init: function () {
            var inIframe = window !== window.top && typeof window.top.dnn !== "undefined";

            var tabId = inIframe ? window.top.dnn.getVar('sf_tabId') : '';
            var siteRoot = inIframe ? window.top.dnn.getVar('sf_siteRoot') : '';
            var antiForgeryToken = inIframe ? window.top.document.getElementsByName('__RequestVerificationToken')[0].value : '';

            var config = $.extend({}, {
                tabId: tabId,
                siteRoot: siteRoot,
                antiForgeryToken: antiForgeryToken
            }, inIframe ? window.parent['editBarSettings'] : {});

            return config;   
       } 
    };
});