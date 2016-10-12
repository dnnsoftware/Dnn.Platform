// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

// Binding handler used to update the width when an attribute is updated by taking the width of the nearest parent scrollpanel.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var setWidthFromParentScrollPaneWhen = function (element, valueAccessor) {
        var attribute = valueAccessor();

        if (attribute) {
            attribute.subscribe(function () {
                var scroll = null;
                var parent = $(element);
                while (parent) {
                    scroll = parent.data("jsp");
                    if (scroll) {
                        $(element).width(scroll.getContentWidth());
                        break;
                    }
                    parent = parent.parent();
                }
            });
        }
    }

    ko.bindingHandlers.setWidthFromParentScrollPaneWhen = {
        init: setWidthFromParentScrollPaneWhen,
    };
});