// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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