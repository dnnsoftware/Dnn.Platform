// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var toggle = function (element, valueAccessor) {
        var options = ko.utils.unwrapObservable(valueAccessor());
        var visible = ko.unwrap(options.value);
        var duration = options.duration;
        var complete = options.complete;
        if (visible) {
            $(element).show(duration, complete);
        } else {
            $(element).hide(duration, complete);
        }
    }

    ko.bindingHandlers.toggle = {
        init: toggle,
        update: toggle
    };
});