// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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