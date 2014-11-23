; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };  //var dnn = dnn || {}; IE 8 doesn't respect var

// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var pluginName = 'inputComparer';

    // The actual plugin constructor
    var InputComparer = this.InputComparer = function (element, options) {
        this.element = element;
        this.options = options;

        this.valid = this.init();
    };

    InputComparer.prototype = {

        constructor: InputComparer,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, InputComparer.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);

            this._$firstElement = this.$element.find(this.options.firstElementSelector);
            if (this._$firstElement.length === 0) {
                return false;
            }
            this._$secondElement = this.$element.find(this.options.secondElementSelector);
            if (this._$secondElement.length === 0) {
                return false;
            }
            this._focusOutHandler = $.proxy(this._onFocusOut, this);
            this._$firstElement.on('focusout', this._focusOutHandler);
            this._$secondElement.on('focusout', this._focusOutHandler);

            return true;
        },

        _onFocusOut: function (eventObject) {
            if (eventObject.target === this._$firstElement[0]) {
                // leaving the first element
                this._isSecondElementVisited && this._compare(this._$firstElement, this._$secondElement);
            }
            else {
                // leaving the second element
                this._isSecondElementVisited = true;
                this._compare(this._$firstElement, this._$secondElement);
            }
        },

        _compare: function ($first, $second) {
            var firstText = $first.val();
            var secondText = $second.val();
            var e = $.Event('on-compare');
            this.$this.trigger(e, [firstText, secondText]);
        },

        compare: function () {
            this._compare(this._$firstElement, this._$secondElement);
        }

    };

    InputComparer._defaults = {};

    InputComparer.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(InputComparer._defaults, settings);
        }
        return InputComparer._defaults;
    };

    $.fn[pluginName] = function (option) {

        return this.each(function () {

            // get the arguments
            var args = $.makeArray(arguments);
            var params = args.slice(1);

            return this.each(function () {
                var $this = $(this);
                var instance = $this.data(pluginName);
                if (!instance) {
                    var options = $.extend({}, typeof option === "object" && option);
                    $this.data(pluginName, (instance = new InputComparer(this, options)));
                }
                if (typeof option === "string") {
                    instance[option].apply(instance, params);
                }
            });
        });
    };


    var PasswordComparer = this.PasswordComparer = function (options) {
        this.options = options;
        this.init();
    };

    PasswordComparer.prototype = {

        constructor: PasswordComparer,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options

            this.$this = $(this);

            this._$container = $(this.options.containerSelector);
            this._comparer = new InputComparer(this._$container, this.options);
            
            if (!this._comparer.valid) {
                return;
            }

            var onCompareHandler = $.proxy(this._onCompare, this);
            $(this._comparer).bind("on-compare", onCompareHandler);

            this._$confirmElement = this._$container.find(this.options.secondElementSelector);

            this._$confirmElement.tooltip({
                getTooltipMarkup: $.proxy(this._getTooltipMarkup, this),
                cssClass: "confirm-password-tooltip",
                left: 214,
                top: -32,
                closeOnMouseLeave: true,
                disabled: true
            });

            this._comparer.compare();
        },

        _getTooltipMarkup: function (element) {
            if (!this._$tooltipContent) {
                this._$tooltipContent = $("<label/>").addClass("confirm-password-tooltip-content");
                this._updateTooltipState(this._isMatched);
            }
            return this._$tooltipContent;
        },

        _updateTooltipState: function (isMatched) {
            if (!this._$tooltipContent) {
                return;
            }
            var text = isMatched ? this.options.confirmPasswordMatchedText : this.options.confirmPasswordUnmatchedText;
            this._$tooltipContent.text(text);
        },

        _onCompare: function (eventObject, val1, val2) {
            this._$confirmElement.removeClass(this.options.unmatchedCssClass);
            this._$confirmElement.removeClass(this.options.matchedCssClass);

            this._isMatched = (val1 === val2);
            if (this._isMatched) {
                if (val1) {
                    // bith fields are not empty
                    this._$confirmElement.addClass(this.options.matchedCssClass);
                }
            }
            else {
                this._$confirmElement.addClass(this.options.unmatchedCssClass);
            }
            this._$confirmElement.tooltip("disabled", (!val1 && !val2));

            this._updateTooltipState(this._isMatched);
        }

    };

}).apply(dnn, [jQuery, window, document]);


dnn.initializePasswordComparer = function(options) {
    $(document).ready(function() { var instance = new dnn.PasswordComparer(options); });
};
