'use strict';
define(['jquery'], function ($) {
	return {		
		selector: 'input, textarea, select',
		errorMessages: {
			'required': 'Text is required',
			'minLength': 'Text must be at least {0} chars',
			'number':  'Only numbers are allowed',
			'nonNegativeNumber': 'Negative numbers are not allowed',
			'positiveNumber': 'Only positive numbers are allowed',
			'nonDecimalNumber': 'Decimal numbers are not allowed',
			'email': 'Only valid email is allowed'
		},
		validate: function(container, customValidators){
			var valid = true;
			var self = this;
            $(self.selector, container).each(function (index, input) {
                var $input = $(input);
                var errorSpan = $input.parent().find('span.dnnFormError').hide();
                var ctrlValid = true;

                var required = $input.hasClass('required');
                var minlength = $input.attr('minlength') ? $input.attr('minlength') : 0;
                var isNumber = $input.hasClass('number');
				var isNonNegativeNumber = $input.hasClass('nonnegativenumber');
				var isPositiveNumber = $input.hasClass('positivenumber');
                var isEmail = $input.hasClass('email');
				var isCustomValidate = $input.hasClass('customValidate');				

                var appendError = function(errorMsg) {
                    if (errorSpan.length) {
                        errorSpan.html(errorMsg).show();
                    } else {                       
						$('<span></span>').addClass('dnnFormError').html(errorMsg).insertAfter($input).click(function () {
							$(this).hide();
							return false;
						});                        
                        $input.bind('focus', function() {
                            $input.parent().find('span.dnnFormError').hide();
                        });
                    }
                };

                var val = $input.val();
                if (val && typeof val === 'string') val = val.trim();
                if (required) {
                    if (!val) {
                        appendError(self.errorMessages['required']);
                        ctrlValid = false;
                    }
                }

                if (ctrlValid && minlength) {
                    var intMinLength = parseInt(minlength);
                    if (val.length < intMinLength) {
                        var minLengthMsg = self.errorMessages['minLength'];
                        appendError(minLengthMsg.replace('{0}', intMinLength.toString));
                        ctrlValid = false;
                    }
                }

                if (ctrlValid && isNumber) {
                    if (!/^-?\d+$/.test(val)) {                        
                        ctrlValid = false;
                    }
					
					if(!ctrlValid){
						if(/^-?\d+\.?\d+$/.test(val))
							appendError(self.errorMessages['nonDecimalNumber']);					
						else
							appendError(self.errorMessages['number']);	
					}						
                }
				
				if (ctrlValid && isNonNegativeNumber) {
                    if (!/^\d+$/.test(val)) {
                        appendError(self.errorMessages['nonNegativeNumber']);
                        ctrlValid = false;
                    }
                }
				
				if (ctrlValid && isPositiveNumber) {
                    if (/^\d+$/.test(val)) {
						var number = parseInt(val);
						if(number <= 0){
							appendError(self.errorMessages['positiveNumber']);
							ctrlValid = false;
						}
						else{
							ctrlValid = true;
						}						
                    }
					else
						ctrlValid = false;
                }
				
				if(ctrlValid && isEmail){
					if(!/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(val)){
						appendError(self.errorMessages['email']);
						ctrlValid = false;
					}
				}

                if (ctrlValid && isCustomValidate) {
                    if (customValidators) {
                        for (var i = 0; i < customValidators.length; i++) {
                            var customValidator = customValidators[i];
                            var validatorName = customValidator.name;
                            if ($input.hasClass(validatorName)) {
                                if (!customValidator.validate(val, input)) {
                                    appendError(customValidator.errorMsg);
                                    ctrlValid = false;
                                }
                            }
                        }
                    }
                }

                valid = valid && ctrlValid;
            });
            return valid;
		}
	};	
});