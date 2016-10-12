// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

// Binding handler used to bind the mouseover/out value to a boolean attribute in the model.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var setWhenHover = function (element, valueAccessor) {
        var attribute = valueAccessor();

        function setAttribute(hover) {
            attribute(hover);
        }

        var overElement = setAttribute.bind(null, true);
        var outlement = setAttribute.bind(null, false);

        ko.utils.registerEventHandler($(element), "mouseover", overElement);
        ko.utils.registerEventHandler($(element), "mouseout", outlement);
    }

    ko.bindingHandlers.setWhenHover = {
        init: setWhenHover,
    };
});