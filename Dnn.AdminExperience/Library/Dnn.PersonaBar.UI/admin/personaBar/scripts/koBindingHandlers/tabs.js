// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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