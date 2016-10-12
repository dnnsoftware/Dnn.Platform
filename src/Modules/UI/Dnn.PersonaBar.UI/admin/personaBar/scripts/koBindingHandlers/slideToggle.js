// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

define(['jquery', 'knockout'], function($, ko) {
    'use strict';
    
    var slideToogle = function (element, valueAccessor) {
        var value = valueAccessor();
        $(element).slideToggle(ko.unwrap(value));
    }

    ko.bindingHandlers.slideToggle = {
        init: slideToogle,
        update: slideToogle
    };
});