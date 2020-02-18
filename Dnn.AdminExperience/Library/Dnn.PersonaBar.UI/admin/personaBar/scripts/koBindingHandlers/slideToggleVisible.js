// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';
    ko.bindingHandlers.slideToggleVisible = {
        init: function (element, valueAccessor) {
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).show() : $(element).hide();
        },
        update: function (element, valueAccessor) {
            var value = valueAccessor();
            ko.unwrap(value) ? $(element).slideDown() : $(element).slideUp();
        }
    };
});