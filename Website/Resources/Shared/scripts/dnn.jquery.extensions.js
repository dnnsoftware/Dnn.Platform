"use strict";

(function (jQuery) {
    jQuery.extend({
        htmlEncode: function (value) {
            return value ? $('<div />').text(value).html() : '';
        },

        htmlDecode: function (value) {
            return value ? $('<div />').html(value).text() : '';
        },

        inContainer: function ($container, element) {
            return $container && $container.length !== 0 && ($.contains($container[0], element) || $container[0] === element);
        },

        bindIframeLoadEvent: function (iframe, callback) {
            $(iframe).on('load', function () {
                // when we remove iframe from dom the request stops, but in IE load event fires
                if (!iframe.parentNode) {
                    return;
                }
                // fixing Opera 10.53
                try {
                    var contentDocument = iframe.contentDocument;
                    if (contentDocument && contentDocument.body && contentDocument.body.innerHTML == "false") {
                        // In Opera event is fired second time when body.innerHTML changed from false
                        // to server response approx. after 1 sec when we upload file with iframe
                        return;
                    }
                }
                catch (ex) {
                    // supress the exception;
                }
                callback();
            });
        }

    });
})(jQuery);


(function (jQuery) {
    jQuery.fn.extend({
        center: function (host) {

            var $host = host ? jQuery(host) : jQuery(window);

            return this.each(function () {
                var $this = jQuery(this);
                var top = ($host.height() - $this.outerHeight(true)) / 2 + $host.scrollTop();
                top = top > 0 ? top : 0;
                var left = ($host.width() - $this.outerWidth(true)) / 2 + $host.scrollLeft();
                left = left > 0 ? left : 0;
                $this.css({ position: "absolute", margin: 0, top: top + "px", left: left + "px" });
                return $this;
            });
        },

        // Parse style and remove properties with style rebuilding style.
        // Example: jQueryElement.removeCssProperties("height", "width", "left");
        removeCssProperties: function () {
            var propertyNames = arguments;
            return this.each(function () {
                var $this = jQuery(this);
                jQuery.grep(propertyNames, function (propertyName) {
                    propertyName = propertyName.trim();
                    var style = $this.attr('style');
                    if (style) {
                        return $this.attr('style',
                                jQuery.grep($this.attr('style').split(";"),
                                function (cssPropertyName) {
                                    if (cssPropertyName.toLowerCase().indexOf(propertyName.toLowerCase() + ':') < 0) {
                                        return cssPropertyName;
                                    }
                                    return "";
                                }
                            ).join(";"));
                    }
                    else {
                        return $this;
                    }
                });
            });
        },

        inlineStyle: function (prop) {
            return this.prop("style")[prop];
        },

        quickHighlight: function(highlightColor) {
            var highlightBg = highlightColor || "#ffff66";
            this.stop(true, true);
            var originalBg = this.css("backgroundColor");
            var removeCssPropertyProxy = this.inlineStyle("backgroundColor") ? undefined : $.proxy(function() { this.removeCssProperties("background-color"); }, this);
            var proxy = $.proxy(function () { this.animate({ backgroundColor: originalBg }, { duration: 1000, easing: "easeInOutBounce", complete: removeCssPropertyProxy }); }, this);
            return this.animate({ backgroundColor: highlightBg }, { duration: 600, easing: "easeInOutBounce", complete: proxy });
        }

    });
})(jQuery);


if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var pluginName = 'prefixInput';

    // Prefixes user input with specified text
    // Example: jQueryElement.inputPrefix("Http://");
    // Depends on dnn.extensions.js
    // The actual plugin constructor
    var PrefixInput = this.PrefixInput = function (element, options) {
        this.element = element;
        this.options = options;

        this.$element = $(this.element);

        if (this.$element.is("[type=text]")) {
            this.init();
        }

    };

    PrefixInput.prototype = {

        constructor: PrefixInput,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, PrefixInput.defaults(), this.options);

            this.$this = $(this);

            this.prefix = this.options.prefix;

            var onKeyUpHandler = $.proxy(this._onKeyUp, this);
            var onBlurHandler = $.proxy(this._onBlur, this);
            var onFocusInHandler = $.proxy(this._onFocus, this);
            var onPasteHandler = function () { setTimeout($.proxy(onKeyUpHandler, this), 0); };

            this.$element.on("keyup." + pluginName, onKeyUpHandler)
                .on("blur." + pluginName, onBlurHandler)
                .on("focusin." + pluginName, onFocusInHandler)
                .on("paste." + pluginName, onPasteHandler);

        },

        _onKeyUp: function (e) {
            this._prependPrefix();
        },

        _onFocus: function (e) {
            this._prependPrefix();
        },

        _onBlur: function (e) {
            if (this.element.value.trim() === this.prefix) {
                this.element.value = "";
            }
        },

        _prependPrefix: function() {
            var val = this.element.value;
            if ((!val.ltrim().startsWith(this.prefix) && val !== "" && !this.prefix.startsWith(val)) || val === "") {
                this.element.value = this.prefix + this.element.value;
            }
        }

    };

    PrefixInput._defaults = { ignoreLeadingSpaces: true };

    PrefixInput.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(PrefixInput._defaults, settings);
        }
        return PrefixInput._defaults;
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
                $this.data(pluginName, (instance = new PrefixInput(this, options)));
            }
            if (typeof option === "string") {
                instance[option].apply(instance, params);
            }
        });

    };

}).apply(dnn, [jQuery, window, document]);


// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    var pluginName = 'resizer';

    // Prefixes user input with specified text
    // Example: jQueryElement.inputPrefix("Http://");
    // Depends on dnn.extensions.js
    // The actual plugin constructor
    var Resizer = this.Resizer = function (element, options) {
        this.element = element;
        this.options = options;
        this.init();
    };

    Resizer.prototype = {

        constructor: Resizer,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, Resizer.defaults(), this.options);

            this.$element = $(this.element);
            this.$this = $(this);

            this.$container = $(this.options.container);

            this.$element.on("mousedown.resizer", $.proxy(this._onMouseDown, this));
            this._onMouseUpHandler = $.proxy(this._onMouseUp, this);

            this._minSize = this._getSize();

            this._onMouseMoveHandler = $.proxy(this._onMouseMove, this);
        },

        _onMouseMove: function (eventObject) {
            if (this._resizeThrottle) {
                clearTimeout(this._resizeThrottle);
            }
            this._resizeThrottle = setTimeout($.proxy(this._resize, this, eventObject), 0);
        },

        _resize: function (eventObject) {
            var currentPosition = this._getMousePosition(eventObject);
            var offsetX = currentPosition.x - this._initialPosition.x;
            //offsetX = this.options.rl ? -offsetX : offsetX;
            var offsetY = currentPosition.y - this._initialPosition.y;
            var newSize = { width: this._initialSize.width + offsetX, height: this._initialSize.height + offsetY };
            this._setSize(newSize);
        },

        _getSize: function () {
            return { width: this.$container.width(), height: this.$container.height() };
        },

        _setSize: function (size) {
            if (size.width < this._minSize.width) {
                size.width = this._minSize.width;
            }
            if (size.height < this._minSize.height) {
                size.height = this._minSize.height;
            }
            this.$container.css(size);
            this.$this.trigger("resized", [size]);
        },

        _getMousePosition: function (eventObject) {
            return { x: eventObject.clientX, y: eventObject.clientY };
        },

        _onMouseUp: function (eventObject) {
            eventObject.preventDefault();
            eventObject.stopPropagation();
            if (this._resizeThrottle) {
                clearTimeout(this._resizeThrottle);
                delete (this._resizeThrottle);
            }
            var $allElements = $('*');
            if ($allElements.hasClass(this.options.unselectableClass)) {
                $allElements.removeClass(this.options.unselectableClass);
            }
            $(document).off('mousemove.resizer', this._onMouseMoveHandler);
            $(document).off("mouseup.resizer", this._onMouseUpHandler);
            //var ctx = typeof ($(document).data('events').mouseup) == "object";
            this.$this.trigger("completed", this);
        },

        _onMouseDown: function (eventObject) {
            eventObject.preventDefault();
            eventObject.stopPropagation();
            this._initialPosition = this._getMousePosition(eventObject);
            this._initialSize = this._getSize();
            $('*').addClass(this.options.unselectableClass);
            //var ctx = typeof ($(document).data('events').mouseup) == "object";
            $(document).on("mouseup.resizer", this._onMouseUpHandler);
            $(document).on("mousemove.resizer", this._onMouseMoveHandler);
        }

    };

    Resizer._defaults = { unselectableClass: "dnn-unselectable" };

    Resizer.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(Resizer._defaults, settings);
        }
        return Resizer._defaults;
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
                $this.data(pluginName, (instance = new Resizer(this, options)));
            }
            if (typeof option === "string") {
                instance[option].apply(instance, params);
            }
        });

    };

}).apply(dnn, [jQuery, window, document]);

