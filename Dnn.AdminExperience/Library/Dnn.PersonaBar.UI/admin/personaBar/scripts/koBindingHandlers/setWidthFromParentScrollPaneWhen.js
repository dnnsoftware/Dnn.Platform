// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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