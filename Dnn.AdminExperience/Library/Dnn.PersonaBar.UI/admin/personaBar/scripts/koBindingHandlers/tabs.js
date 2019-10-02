// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2018, DNN Corp.
// All rights reserved.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var init = function (element, valueAccessor) {
        var options = valueAccessor();

        // initialize
        $(element).tabs(options);
    }


    ko.bindingHandlers.tabs = {
        init: init,
    };
});