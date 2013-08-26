// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.subscriptions === 'undefined') dnn.subscriptions = {};

    dnn.subscriptions.SearchResult = function SearchResult($, ko, settings, root, social, model) {
        var that = this;

        $.extend(this, dnn.social.komodel(model));
        
        var localizer = social.getLocalizationController();
        this.service = social.getService('Subscriptions');

       
    };
})(window.dnn);