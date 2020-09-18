; if (typeof dnn === "undefined") { dnn = {}; }; //var dnn = dnn || {};

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($, window, document, undefined) {
    "use strict";

    var pluginName = "jScrollbar";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var Scrollbar = this.Scrollbar = function (element, options) {
        this.element = element;
        this.options = options;
        this.init();
    };

    Scrollbar.prototype = {
        constructor: Scrollbar,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, Scrollbar.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element).addClass('ps-container');

            this._onMouseMoveXHandler = $.proxy(this._onMouseMoveX, this);
            this._onMouseMoveYHandler = $.proxy(this._onMouseMoveY, this);
            this._onMouseUpXHandler = $.proxy(this._onMouseUpX, this);
            this._onMouseUpYHandler = $.proxy(this._onMouseUpY, this);

            this._$content = this.$element.children();
            this._$scrollbarX = $("<div class='ps-scrollbar-x'></div>").appendTo(this.$element);
            this._$scrollbarX.on('mousedown.perfect-scroll', $.proxy(this._onMouseDownX, this));

            this._$scrollbarY = $("<div class='ps-scrollbar-y'></div>").appendTo(this.$element);
            this._$scrollbarY.on('mousedown.perfect-scroll', $.proxy(this._onMouseDownY, this));

            this._scrollbarXBottom = parseInt(this._$scrollbarX.css('bottom'), 10);
            this._scrollbarYRight = parseInt(this._$scrollbarY.css('right'), 10);

            if (this.$element.mousewheel) {
                this.$element.mousewheel($.proxy(this._onMouseWheel, this));
            }
            this.update();
        },

        destroy: function () {
            this._$scrollbarX.remove();
            this._$scrollbarY.remove();
            this.$element.off("mousewheel");
        },

        _scrollY: function () {
            var scrollTop = parseInt(this._scrollbarYTop * this._contentHeight / this._containerHeight, 10);
            this.$element.scrollTop(scrollTop);
            this._$scrollbarX.css({ bottom: this._scrollbarXBottom - scrollTop });
        },

        _scrollX: function () {
            var scrollLeft = parseInt(this._scrollbarXLeft * this._contentWidth / this._containerWidth, 10);
            this.$element.scrollLeft(scrollLeft);
            this._$scrollbarY.css({ right: this._scrollbarYRight - scrollLeft });
        },

        update: function () {
            this._containerWidth = this.$element.width();
            this._containerHeight = this.$element.height();
            this._contentWidth = this._$content.outerWidth(false);
            this._contentHeight = this._$content.outerHeight(false);
            if (this._containerWidth < this._contentWidth) {
                this._scrollbarXWidth = parseInt(this._containerWidth * this._containerWidth / this._contentWidth, 10);
                this._scrollbarXLeft = parseInt(this.$element.scrollLeft() * this._containerWidth / this._contentWidth, 10);
            }
            else {
                this._scrollbarXWidth = 0;
                this._scrollbarXLeft = 0;
                this.$element.scrollLeft(0);
            }
            if (this._containerHeight < this._contentHeight) {
                this._scrollbarYHeight = parseInt(this._containerHeight * this._containerHeight / this._contentHeight, 10);
                this._scrollbarYTop = parseInt(this.$element.scrollTop() * this._containerHeight / this._contentHeight, 10);
            }
            else {
                this._scrollbarYHeight = 0;
                this._scrollbarYTop = 0;
                this.$element.scrollTop(0);
            }

            if (this._scrollbarYTop >= this._containerHeight - this._scrollbarYHeight) {
                this._scrollbarYTop = this._containerHeight - this._scrollbarYHeight;
            }
            if (this._scrollbarXLeft >= this._containerWidth - this._scrollbarXWidth) {
                this._scrollbarXLeft = this._containerWidth - this._scrollbarXWidth;
            }

            this._$scrollbarX.css({ left: this._scrollbarXLeft + this.$element.scrollLeft(), bottom: this._scrollbarXBottom - this.$element.scrollTop(), width: this._scrollbarXWidth });
            this._$scrollbarY.css({ top: this._scrollbarYTop + this.$element.scrollTop(), right: this._scrollbarYRight - this.$element.scrollLeft(), height: this._scrollbarYHeight });
        },

        _moveBarX: function (currentLeft, deltaX) {
            var newLeft = currentLeft + deltaX;
            var maxLeft = this._containerWidth - this._scrollbarXWidth;

            if (newLeft < 0) {
                this._scrollbarXLeft = 0;
            }
            else if (newLeft > maxLeft) {
                this._scrollbarXLeft = maxLeft;
            }
            else {
                this._scrollbarXLeft = newLeft;
            }
            this._$scrollbarX.css({ left: this._scrollbarXLeft + this.$element.scrollLeft() });
        },

        _moveBarY: function (currentTop, deltaY) {
            var newTop = currentTop + deltaY;
            var maxTop = this._containerHeight - this._scrollbarYHeight;

            if (newTop < 0) {
                this._scrollbarYTop = 0;
            }
            else if (newTop > maxTop) {
                this._scrollbarYTop = maxTop;
            }
            else {
                this._scrollbarYTop = newTop;
            }
            this._$scrollbarY.css({ top: this._scrollbarYTop + this.$element.scrollTop() });
        },

        _getDocument: function() {
            return this.options.document || document;
        },

        _onMouseDownX: function (e) {
            this._currentPageX = e.pageX;
            this._currentLeft = this._$scrollbarX.position().left;
            this._$scrollbarX.addClass('in-scrolling');
            $(this._getDocument()).on('mousemove.perfect-scroll', this._onMouseMoveXHandler);
            $(this._getDocument()).on('mouseup.perfect-scroll', this._onMouseUpXHandler);
            e.stopPropagation();
            e.preventDefault();
        },

        _onMouseDownY: function (e) {
            this._currentPageY = e.pageY;
            this._currentTop = this._$scrollbarY.position().top;
            this._$scrollbarY.addClass('in-scrolling');
            $(this._getDocument()).on('mousemove.perfect-scroll', this._onMouseMoveYHandler);
            $(this._getDocument()).on('mouseup.perfect-scroll', this._onMouseUpYHandler);
            e.stopPropagation();
            e.preventDefault();
        },

        _onMouseMoveX: function (e) {
            if (this._$scrollbarX.hasClass('in-scrolling')) {
                this._moveBarX(this._currentLeft, e.pageX - this._currentPageX);
                this._scrollX();
                e.stopPropagation();
                e.preventDefault();
            }
        },

        _onMouseMoveY: function (e) {
            if (this._$scrollbarY.hasClass('in-scrolling')) {
                this._moveBarY(this._currentTop, e.pageY - this._currentPageY);
                this._scrollY();
                e.stopPropagation();
                e.preventDefault();
            }
        },

        _onMouseUpX: function (e) {
            if (this._$scrollbarX.hasClass('in-scrolling')) {
                this._$scrollbarX.removeClass('in-scrolling');
            }
            $(this._getDocument()).off('mousemove.perfect-scroll', this._onMouseMoveXHandler);
            $(this._getDocument()).off('mouseup.perfect-scroll', this._onMouseUpXHandler);
        },

        _onMouseUpY: function (e) {
            if (this._$scrollbarY.hasClass('in-scrolling')) {
                this._$scrollbarY.removeClass('in-scrolling');
            }
            $(this._getDocument()).off('mousemove.perfect-scroll', this._onMouseMoveYHandler);
            $(this._getDocument()).off('mouseup.perfect-scroll', this._onMouseUpYHandler);
        },

        _onMouseWheel: function (e, delta, deltaX, deltaY) {
            this.$element.scrollTop(this.$element.scrollTop() - (deltaY * this.options.wheelSpeed));
            this.$element.scrollLeft(this.$element.scrollLeft() + (deltaX * this.options.wheelSpeed));

            // update bar position
            this.update();

            if (this._shouldPreventDefault(deltaX, deltaY)) {
                e.preventDefault();
            }
        },

        _shouldPreventDefault: function (deltaX, deltaY) {
            var scrollTop = this.$element.scrollTop();
            if (scrollTop === 0 && deltaY > 0 && deltaX === 0) {
                return !this.options.wheelPropagation;
            }
            else if (scrollTop >= this._contentHeight - this._containerHeight && deltaY < 0 && deltaX === 0) {
                return !this.options.wheelPropagation;
            }

            var scrollLeft = this.$element.scrollLeft();
            if (scrollLeft === 0 && deltaX < 0 && deltaY === 0) {
                return !this.options.wheelPropagation;
            }
            else if (scrollLeft >= this._contentWidth - this._containerWidth && deltaX > 0 && deltaY === 0) {
                return !this.options.wheelPropagation;
            }
            return true;
        }

    };

    Scrollbar._defaults = { wheelSpeed: 10, wheelPropagation: false };

    Scrollbar.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(Scrollbar._defaults, settings);
        }
        return Scrollbar._defaults;
    };

    $.fn[pluginName] = function (option) {

        // get the arguments
        var args = $.makeArray(arguments);
        var params = args.slice(1);

        return this.each(function () {
            var $this = $(this);
            var instance = $this.data(pluginName);
            if (!instance) {
                var options = $.extend({}, typeof option === "object" && option);
                $this.data(pluginName, (instance = new Scrollbar(this, options)));
            }
            if (typeof option === "string") {
                instance[option].apply(instance, params);
            }
        });

    };

}).apply(dnn, [jQuery, window, document]);

