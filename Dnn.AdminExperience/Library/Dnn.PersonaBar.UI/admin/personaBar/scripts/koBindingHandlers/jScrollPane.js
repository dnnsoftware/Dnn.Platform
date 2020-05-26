// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

define(['jquery', 'knockout'], function ($, ko) {
    'use strict';

    var init = function (element, valueAccessor) {
        var o = valueAccessor() || {};
        var options = o.jScrollPaneOptions || {};
        var parentHeightOptions = o.parentHeightOptions;
        var parentScrollSettings = o.parentScrollSettings;
        var additionalStyles = o.additionalStyles;

        // initialize
        // use setTimeout so the DOM finishes updating before reinitialising
        setTimeout(function () {
            $(element).jScrollPane(options);
            if (options.mouseWheelSpeed === 0) {
                $(".jspPane", element).unmousewheel();
                $(".jspPane", element).bind("mousewheel", function mouseWheelHandler(event, delta) {
                    //Propagate the event to the parent scrolls    
                    if (parentScrollSettings) {
                        var scrollOffset = 15;
                        var parentScroll = $(parentScrollSettings.selector).data("jsp");
                        if (delta < 0) {
                            parentScroll.scrollByY(scrollOffset);
                        } else {
                            parentScroll.scrollByY((-1)* scrollOffset);
                        }
                    }
                    
                    return false;
                });
            }            
        }, 0);

        var reinit = function () {
            var parentHeight, height, correction;
            if (parentHeightOptions) {
                parentHeight = $(element).parent().height();
                correction = parentHeightOptions.heightCorrection ? parentHeightOptions.heightCorrection : 0;
                height = parentHeight + correction;
                $(element).height(height <= 0 ? parentHeight : height);
            }

            var scroll = $(element).data("jsp");
            if (scroll) {
                scroll.reinitialise();
            }
            setTimeout(function () {
                if (additionalStyles) {
                    var h = $(element).find('.jspDrag');
                    if (h.length > 0) {
                        $(element).addClass(additionalStyles.whenScrollsClass);
                    }
                }
            }, 0);
        };

        reinit();

        // handle window resize (not really necessary if your box has a set pixel width)
        $(window).resize(reinit);

        // add subscription to observable if passed in
        if (o.observableElement) {
            o.observableElement.subscribe(function (value) {
                // use setTimeout so the DOM finishes updating before reinitialising
                if (value) {             
                    setTimeout(reinit, 0);
                }                
            });
        }
    };

    ko.bindingHandlers.jScrollPane = {
        init: init
    };
});