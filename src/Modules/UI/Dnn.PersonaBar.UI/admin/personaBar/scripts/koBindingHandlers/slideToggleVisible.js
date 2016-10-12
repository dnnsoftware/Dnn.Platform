// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

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