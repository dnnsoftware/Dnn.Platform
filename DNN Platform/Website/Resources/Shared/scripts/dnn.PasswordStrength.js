; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

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

    var pluginName = 'passwordStrength';

    // The actual plugin constructor
    var PasswordStrength = this.PasswordStrength = function (element, options) {
        this.element = element;
        this.options = options; 

        this.init();
    };

    PasswordStrength.prototype = {

        constructor: PasswordStrength,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, PasswordStrength.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);
            this._$container = this.$element.parent();

            //$("<span/>").addClass(this.options.labelCss).text(this.options.minLength + this.options.minLengthText).appendTo(this._$container);
            this._$meter = $("<div/>").addClass(this.options.meterCss).appendTo(this._$container).width(this.$element.outerWidth());
	        var handler = this;
	        setInterval(function () {
	            handler._$meter.width(handler.$element.outerWidth());
                handler._$tooltipContent.width(handler.$element.outerWidth() - 20);
	        }, 50);
            this._$meterValue = $('<div><span class=""></span><span class=""></span><span class=""></span><span class="last"></span></div>').appendTo(this._$meter);
            this._$meterText = $("<label/>").prependTo(this._$meterValue);

            this._$tooltipContainer = $('<div class="password-strength-tooltip" />');
            this.$element.after(this._$tooltipContainer);
            this._$tooltipContainer.hide();
            this._initializeTooltip();

            this.$element.on("change keyup paste input propertychange", $.proxy(this._onInput, this));

	        this.$element.on('blur', function() {
	            handler._updateTooltipVisible();
	        });

            var meterHover = {
                over: function () {
                    handler._forceTooltipVisible();
                },
                timeout: 200,
                out: function () {
                    handler._updateTooltipVisible();
                }
            };
            this._$meter.hoverIntent(meterHover);

	        var validateFunction = window['ValidatorValidate'];
			if (typeof validateFunction === 'function') {
				window['ValidatorValidate'] = function() {
					validateFunction.apply(window, arguments);

					var validator = arguments[0];
					var field = $('#' + validator.controltovalidate);
					if (validator.isvalid) {
						field.removeClass('validate-fail').addClass('validate-success');
					} else {
						field.removeClass('validate-success').addClass('validate-fail');
					}
				}
			}

            this._updateState();
        },

        _initializeTooltip: function() {
        	this._$tooltipContent = $("<div/>").addClass("password-strength-tooltip-content").appendTo(this._$tooltipContainer);
			this._$tooltipContent.width(this.$element.outerWidth() - 20);
			$('<h2 />').html(this.options.passwordRulesHeadText).appendTo(this._$tooltipContent);

			var list = $("<ul/>").appendTo(this._$tooltipContent);
            this._$labelAtLeastNChars = $("<label/>").text(this.options.criteriaAtLeastNChars).appendTo($("<li/>").appendTo(list));
            this._$labelAtLeastNSpecialChars = $("<label/>").text(this.options.criteriaAtLeastNSpecialChars).appendTo($("<li/>").appendTo(list));
            this._$labelValidationExpression = $("<label/>").text(this.options.criteriaValidationExpression).appendTo($("<li/>").appendTo(list));

            
            if (this.options.minNumberOfSpecialChars === 0) {
                this._$labelAtLeastNSpecialChars.hide();
            }

            
            if (this.options.validationExpression === '') {
                this._$labelValidationExpression.hide();
            }
            
            if (!this._strength) {
                this._strength = this._getStrength(this.$element.val(), this.options);
            }
            this._updateTooltipState(this._strength);
        },

        _onInput: function (e) {
	        if (e.type === 'propertychange' && e.originalEvent.propertyName.toLowerCase() !== 'value') {
		        return;
	        }

            if (this.$element.val() !== '') {
                this._$meter.addClass('visible');
            }

            this._updateState();
        },

        _updateState: function() {
            var password = this.$element.val();
            this._strength = this._getStrength(password, this.options);
            this._updateMeterState(this._strength);
            this._updateTooltipState(this._strength);
	        this._updateFieldState(this._strength);
        },

        _updateMeterState: function (strength) {
            var rating = Math.min(strength.rating, strength.maxRating);
            var bgColor = '';
	        var ratingIndex = 0;
            if (rating === 0) {
                this._$meterText.text(this.options.emptyText);
            }
            else if (rating < 3) {
                this._$meterText.text(this.options.weakText);
                bgColor = this.options.weakColor;
	            ratingIndex = rating;
            }
            else if (rating > 2 && rating < 5) {
                this._$meterText.text(this.options.fairText);
                bgColor = this.options.fairColor;
	            ratingIndex = 3;
            }
            else if (rating > 4) {
                this._$meterText.text(this.options.strongText);
                bgColor = this.options.strongColor;
	            ratingIndex = 4;
            }

	        this._$meter.find('span').css('background-color', '').each(function(index, item) {
		        if (index < ratingIndex) {
			        $(this).css('background-color', bgColor);
		        }
	        });

            rating === 0 ? this._$meterText.hide() : this._$meterText.show();
        },

        _updateTooltipState: function (strength) {
            if (!this._$tooltipContent) {
                return;
            }

            this._updateCriteriaLabel(this._$labelAtLeastNChars, strength.hasLengthOfNChars);
            this._updateCriteriaLabel(this._$labelAtLeastNSpecialChars, strength.hasMinNumberOfSpecialChars);
            this._updateCriteriaLabel(this._$labelValidationExpression, strength.matchValidationExpression);
        },

        _updateTooltipVisible: function () {
            var password = this.$element.val();
            var strength = this._strength = this._getStrength(password, this.options);

            var matchedPasswordPolicy = this._matchedPasswordPolicy(strength);
            if (matchedPasswordPolicy || this.$element.val() === '') {
                this._$tooltipContainer.fadeOut('fast');
            } else {
                this._$tooltipContainer.fadeIn('fast');
            }
        },

        _forceTooltipVisible: function() {
            this._$tooltipContainer.fadeIn('fast');
        },

        _updateCriteriaLabel: function($label, satisfied) {
            if (satisfied) {
                $label.addClass("satisfied");
            }
            else {
                $label.removeClass("satisfied");
            }
        },

		_updateFieldState: function(strength) {
			if (this.$element.val().length > 0) {
			    var matchedPasswordPolicy = this._matchedPasswordPolicy(strength);
				if (matchedPasswordPolicy) {
					this.$element.removeClass('validate-fail').addClass('validate-success');
				} else {
					this.$element.removeClass('validate-success').addClass('validate-fail');
				}
			} else {
				this.$element.removeClass('validate-success validate-fail');
			}
		},

        _matchedPasswordPolicy: function(strength) {
            return strength.hasLengthOfNChars
                        && (this.options.minNumberOfSpecialChars === 0 || strength.hasMinNumberOfSpecialChars)
                        && (this.options.validationExpression === '' || strength.matchValidationExpression);
        },

        _getStrength: function(password, options) {
            var rating = 0;

            //this next property will be initialised with a server value
            var minLength = options.minLength;

            var hasOneUpperCaseChar = false;
            var hasOneLowerCaseChar = false;
            var hasMinNumberOfSpecialChars = false;
            var hasOneNumericChar = false;
            var hasLengthOfNChars = false;
            var matchValidationExpression = false;

            var minNumberOfSpecialChars = options.minNumberOfSpecialChars || 0;
            var validationExpression = options.validationExpression || '';

            if (password.length > 0) {

                if (password.match(/[a-z]/)) {
                    rating++;
                    hasOneLowerCaseChar = true;
                }
                if (password.match(/[A-Z]/)) {
                    rating++;
                    hasOneUpperCaseChar = true;
                }
                if (password.match(/[0-9]/g)) {
                    rating++;
                    hasOneNumericChar = true;
                }

                var matches = password.match(/[!,@,#,$,%,&,*,(,),\-,_,=,+,\',\",\\,|,\,,<,.,>,;,:,/,?,\[,{,\],}]/g);
                if (matches && matches.length >= minNumberOfSpecialChars) {
                    rating++;
                    hasMinNumberOfSpecialChars = true;
                }

                if (password.length >= minLength) {
                    rating++;
                    hasLengthOfNChars = true;
                }

                if (password.length >= minLength + 3) {
                    rating++;
                }

                if (validationExpression) {
                    matchValidationExpression = new RegExp(validationExpression, 'g').test(password);
                }
            }
            return {
                rating: rating,
                maxRating: 5,
                hasOneUpperCaseChar: hasOneUpperCaseChar,
                hasOneLowerCaseChar: hasOneLowerCaseChar,
                hasMinNumberOfSpecialChars: hasMinNumberOfSpecialChars,
                hasOneNumericChar: hasOneNumericChar,
                hasLengthOfNChars: hasLengthOfNChars,
                matchValidationExpression: matchValidationExpression
            };
        }
    };

    PasswordStrength._defaults = {
        emptyText: 'Empty'
    };

    PasswordStrength.defaults = function (settings) {
        if (typeof settings !== "undefined") {
        	$.extend(PasswordStrength._defaults, settings);
        }
        return PasswordStrength._defaults;
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
                $this.data(pluginName, (instance = new PasswordStrength(this, options)));
            }
            if (typeof option === "string") {
                instance[option].apply(instance, params);
            }
        });

    };

}).apply(dnn, [jQuery, window, document]);


dnn.initializePasswordStrength = function (selector, options) {
    $(document).ready(function() { $(selector).passwordStrength(options); });
};
