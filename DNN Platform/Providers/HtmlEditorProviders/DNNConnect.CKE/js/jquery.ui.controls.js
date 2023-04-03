$.widget("ui.form", {
    _init: function () {
        var object = this;
        var form = this.element;
        var inputs = form.find("#lblCurrentDir, input:text, input:checkbox, input:radio , textarea").not("#CKEditor_Options_rBlSetMode input:radio,#BrowserMode input:radio,.groupEdit");
        form.find("fieldset").addClass("ui-widget-content");
        form.find("legend").addClass("ui-widget-header ui-corner-all");
        form.addClass("ui-widget");
        $.each(inputs, function () {
            $(this).addClass('ui-state-default ui-corner-all');
            $(this).wrap("<label />");
            if ($(this).is(":reset ,:submit")) object.buttons(this);
            else if ($(this).is(":checkbox")) object.checkboxes(this);
            else if ($(this).is("input[type='text']") || $(this).is("textarea") || $(this).is("input[type='password']")) object.textelements(this);
            else if ($(this).is(":radio")) object.radio(this);
            else if ($(this).is("select")) object.selector(this);
            if ($(this).hasClass("date")) {
                $(this).datepicker();
            }
        });
        /*var div = jQuery("<div />", {
            css: {
                width: 20,
                height: 16,
                margin: 10,
                textAlign: "center"
            }
        }).addClass("ui-state-default drag");
        var no = Math.ceil(Math.random() * 4);
        var holder = jQuery("<div />", {
            id: 'droppable',
            text: "Drop the box with " + no + " here",
            css: {
                width: 100,
                height: 100,
                float: 'right',
                fontWeight: 'bold'
            }
        }).addClass('ui-state-default');
        $(form).find("fieldset").append(holder);
        for (var i = 1; i < 5; i++) {
            $(form).find("fieldset").append(div.clone().html(i).attr("id", i));
        }
        $(".drag").draggable({
            containment: 'parent'
        });
        $("#droppable").droppable({
            accept: '#' + no,
            drop: function (event, ui) {
                $(this).addClass('ui-state-highlight').html("Right One");
                form.find(":submit").removeClass('ui-state-disabled').unbind('click');
            }
        });*/
        $(".hover").hover(function () {
            $(this).addClass("ui-state-hover");
        }, function () {
            $(this).removeClass("ui-state-hover");
        });
    },
    textelements: function (element) {
        $(element).bind({
            focusin: function () {
                $(this).toggleClass('ui-state-focus');
            },
            focusout: function () {
                $(this).toggleClass('ui-state-focus');
            }
        });
    },
    buttons: function (element) {
        if ($(element).is(":submit")) {
            $(element).addClass("ui-priority-primary ui-corner-all ui-state-disabled hover");
            $(element).bind("click", function (event) {
                event.preventDefault();
            });
        } else if ($(element).is(":reset")) $(element).addClass("ui-priority-secondary ui-corner-all hover");
        $(element).bind('mousedown mouseup', function () {
            $(this).toggleClass('ui-state-active');
        });
    },
    checkboxes: function (element) {
        $(element).parent("label").after("<span />");
        var parent = $(element).parent("label").next();
		
		if ($(element).attr('checked')) {
				parent.toggleClass("ui-icon ui-icon-check");
		}
			
		$(parent).children('span').first().html($(element).children("option:selected").html());
		
        $(element).addClass("ui-helper-hidden");
        parent.css({
            width: 16,
            height: 16,
            display: "block"
        });
        parent.wrap("<span class='ui-state-default ui-corner-all' style='display:inline-block;width:16px;height:16px;margin-right:5px;'/>");
        parent.parent().addClass('hover');
        parent.parent("span").click(function (event) {
            $(this).toggleClass("ui-state-active");
            parent.toggleClass("ui-icon ui-icon-check");
            $(element).click();
        });
    },
    radio: function (element) {
        $(element).parent("label").after("<span />");
        var parent = $(element).parent("label").next();
        $(element).addClass("ui-helper-hidden");
        parent.addClass("ui-icon ui-icon-radio-off");
		
		if ($(element).prop)
		{
			if ($(element).prop('disabled')) {
				parent.wrap("<span class='ui-state-disabled ui-corner-all' style='display:inline-block;width:16px;height:16px;margin-right:5px;'/>");
			} else {
				parent.wrap("<span class='ui-state-default ui-corner-all' style='display:inline-block;width:16px;height:16px;margin-right:5px;'/>");
			}
		} else {
			if ($(element).attr('disabled')) {
				parent.wrap("<span class='ui-state-disabled ui-corner-all' style='display:inline-block;width:16px;height:16px;margin-right:5px;'/>");
			} else {
				parent.wrap("<span class='ui-state-default ui-corner-all' style='display:inline-block;width:16px;height:16px;margin-right:5px;'/>");
			}
		}
		
		
       
        parent.parent().addClass('hover');
		
		if ($(element).prop)
		{
			if ($(element).prop('checked')) {
				parent.toggleClass("ui-icon-radio-off ui-icon-bullet");
				}
		} else {
			if ($(element).attr('checked')) {
				parent.toggleClass("ui-icon-radio-off ui-icon-bullet");
				}
		}
		
		parent.parent("span").click(function (event) {
            $(this).toggleClass("ui-state-active");
            parent.toggleClass("ui-icon-radio-off ui-icon-bullet");
            $(element).click();
        });
    },
    selector: function (element) {
        var parent = $(element).parent().first();
        parent.css({
            "display": "block",
            width: 140,
            height: 21
        }).addClass("ui-state-default ui-corner-all");
        $(element).addClass("ui-helper-hidden");
        parent.append("<span style='float:left;'></span><span style='float:right;display:inline-block' class='ui-icon ui-icon-triangle-1-s' ></span>");
        parent.after("<ul class=' ui-helper-reset ui-widget-content ui-helper-hidden' style='position:absolute;z-index:50;width:140px;' ></ul>");
        $(parent).children('span').first().html($(element).children("option:selected").html());
        $.each($(element).find("option"), function () {
            $(parent).next("ul").append("<li class='hover' style='cursor: pointer'>" + $(this).html() + "</li>");
        });
        $(parent).next("ul").find("li").click(function () {
            $(parent).children('span').first().html($(this).html());
            $(element).val($(this).html());
            $(parent).next().slideToggle('fast');
        });
        $(parent).click(function (event) {
            $(this).next().slideToggle('fast');
            event.preventDefault();
        });
    }
});

(function($, undefined) {
	$.widget('ui.selectgroup', {
		version: '@VERSION',
		options: {
			autoWidth: true,
			classInherit: {
				select: true,
				option: true,
				optionGroup: true
			},
			positioning: {
				of: null,
				my: 'left top',
				at: 'left bottom',
				offset: null,
				collision: 'none'
			},
			style: 'dropdown',
			handleWidth: 26
		},
		isOpen: false,
		isActive: false,
		position: 0,
		search: ['', '', 1, 1, 0],
		timer: null,
		_create: function() {
			var that = this,
				id = this.element.attr('id');
			this.identifiers = ['ui-' + id, 'ui-' + id];
			// Append one selectgroup.group for the widget as outlined in the flyweight pattern.
			if ($.ui.selectgroup.group.initialised === false) {
				$('body').append($.ui.selectgroup.group);
				$.ui.selectgroup.group.hide();
			}
			$.ui.selectgroup.group.initialised = true;
			// Obtain the placeholder text either from the selected option or the first option.
			if ($(this.element).find('option:selected').length) {
				this.copy = this.element.find('option:selected').text();
			}
			else {
				this.copy = this.element.find('option').first().text();
			}
			// Create an element to replace the select element interface.
			this.placeholder = $('<a href="#" id="' + this.identifiers[1] + '" class="' + this.widgetBaseClass + ' ui-widget ui-state-default ui-corner-all"'
				+ 'role="button" aria-haspopup="true" aria-owns="' + this.widgetBaseClass + '-group">'
				+ '<span class="' + this.widgetBaseClass + '-copy">'+ this.copy +'</span>'
				+ '<span class="' + this.widgetBaseClass + '-icon ui-icon ui-icon-triangle-1-s"></span></a>');
			// If the option to inherit classes is true then get the classnames from the select element and apply to the placeholder element.
			if (this.options.classInherit.select) {
				this.placeholder.addClass(this.element.attr('class'));
			}
			// Gets the title attribute from the select element and apply to the placeholder element.
			this.placeholder.attr('title',this.element.attr('title'));
			// If the option to use the popup style is true then we need to add the popup classes.
			if (this.options.style === 'popup') {
				this.placeholder.addClass(this.widgetBaseClass + '-popup');
				this.placeholder.find('.' + this.widgetBaseClass + '-icon').removeClass('ui-icon-triangle-1-s').addClass('ui-icon-triangle-2-n-s');
			}
			// Place the placeholder element after the select element and hide the select element.
			this.element.after(this.placeholder).hide();
			// Binds events to the placeholder element to match the native select element events.
			this._placeholderEvents(true);
			// Bind the focus event on the associated label to the placeholder element.
			$('label[for="' + id + '"]').attr( 'for', this.identifiers[0] )
				.bind('click.selectgroup', function(event) {
					event.preventDefault();
					that.placeholder.focus();
				});
				// Bind click event to the document that will close the selectgroup.group if it is open.
			$(document).bind('click.selectmenu', function(event) {
				if (that.isOpen && !($(event.target).closest('.ui-selectgroup').length || $(event.target).closest('.ui-selectgroup-group').length)) {
					window.setTimeout( function() {
						that.blur();
						that.close();
						$.ui.selectgroup.group.past = null;
					}, (100));
				}
			});
		},
		_placeholderEvents: function(value) {
			var that = this;
			// If true then we want to bind events to the placeholder.
			if (value === true) {
				this.placeholder.removeClass('ui-state-disabled')
					.bind('click.selectgroup', function(event) {
						event.preventDefault();
						that._toggle();
					})
					.bind('keydown.selectgroup', function(event) {
						switch (event.keyCode) {
							case $.ui.keyCode.ENTER:
								event.preventDefault();
								if (that.isOpen) {
									that.close();
								}
								break;
							case $.ui.keyCode.ESCAPE:
								event.preventDefault();
								if (that.isOpen) {
									that.blur();
									that.close();
								}
								break;
							case $.ui.keyCode.UP:
							case $.ui.keyCode.LEFT:
								event.preventDefault();
								if (!that.isActive) {
									that.focus();
								}
								that._traverse(-1);
								break;
							case $.ui.keyCode.DOWN:
							case $.ui.keyCode.RIGHT:
								event.preventDefault();
								if (!that.isActive) {
									that.focus();
								}
								that._traverse(1);
								break;
							case $.ui.keyCode.TAB:
								if (!that.isActive) {
									that.blur();
								}
								if (that.isOpen) {
									that.close();
								}
								break;
							default:
								event.preventDefault();
								if (!that.isActive) {
									that.focus();
								}
								that._typeahead(String.fromCharCode(event.keyCode).toLowerCase());
								break;
						}
					})	
					.bind('mouseover.selectgroup', function(event) {
						$(this).addClass('ui-state-hover');
					})
					.bind('mouseout.selectgroup', function(event) {
						$(this).removeClass('ui-state-hover');
					});
			}
			// Else binding events is false and we want to repath them to do nothing.
			else {
				this.placeholder.addClass('ui-state-disabled').unbind('.selectgroup');
				this.placeholder.bind('click.selectgroup', function(event) {
					event.preventDefault();
				})
				.bind('keydown.selectgroup', function(event) {
					event.preventDefault();
				});
			}
		},
		_index: function() {
			// Get all the options within the select element. Map their properties to an array for quick reference. 
			this.selectors = $.map($('option', this.element), function(value) {
				return {
					element: $(value),
					text: $(value).text(),
					classname: $(value).attr('class'),
					optgroup: $(value).parent('optgroup'),
					optgroupClassname: $(value).parent('optgroup').attr('class'),
					optDisabled: $(value).parent('optgroup').attr('disabled'),
					value: $(value).attr('value'),
					selected: $(value).attr('selected'),
					disabled: $(value).attr('disabled')
				};
			});
		},
		_renderGroup: function() {
			var that = this, 
				hidden = false;
			// Create the selectgroup list container.
			this.group = $('<ul class="' + this.widgetBaseClass + '-list" role="listbox" aria-hidden="true"></ul>');
			if (this.options.autoWidth) {
				if (this.options.style === 'dropdown') {
					$.ui.selectgroup.group.width(this.placeholder.width());
				}
				else {
					$.ui.selectgroup.group.width(this.placeholder.width() - this.options.handleWidth);
				}
			}
			if (this.options.style === 'popup') {
				this.group.addClass(this.widgetBaseClass + '-popup');
			}
			// Render the options available within the select. 
			this._renderOption();
			this.group.attr('aria-labelledby', this.identifiers[0]);
			$($.ui.selectgroup.group).html(this.group);
		},
		_renderOption: function() {
			var that = this;
			// Iterate through the selectors array which is a map of the options within the select element.
			$.each(this.selectors, function(index) {
				var self = this;
				// Each option will be represented as a list item. Bind properties and events to each list item.
				var list = $('<li role="presentation"><a role="option" href="#">'+ this.text +'</a></li>')
					.bind('click.selectgroup', function(event) {
						event.preventDefault();
						if (!(self.disabled === 'disabled' || self.optDisabled === 'disabled')) {
							that.copy = that.selectors[index].text;
							that.placeholder.find('.ui-selectgroup-copy').text(that.copy);
							that.element.find('option:selected').removeAttr("selected");
							$(that.selectors[index].element).attr('selected', 'selected');
							that.position = index;
							that.element.trigger('change');
							that._toggle();
						}
					})
					.bind('mouseover.selectgroup', function() {
						if (!(self.disabled === 'disabled' || self.optDisabled === 'disabled')) {
							$(this).addClass('ui-state-hover');
						}
					})
					.bind('mouseout.selectmenu', function() {
						$(this).removeClass('ui-state-hover');
					});
				if (that.options.classInherit.option) {
					list.addClass(this.classname);
				}
				if (typeof this.selected !== "undefined" && this.selected === 'selected') {
					list.addClass('ui-state-active');
					that.position = index;
				}
				if (typeof this.disabled !== "undefined" && this.disabled === 'disabled') {
					list.addClass('ui-state-disabled');
				}
				// The option may be part of an option group.
				if (this.optgroup.length) {
					var name = that.widgetBaseClass + '-optgroup-' + that.element.find('optgroup').index(this.optgroup);
					// If the option group has already been rendered append the option list item to the option group list.
					if (that.group.find('li.' + name).length ) {
						that.group.find('li.' + name + ' ul').append(list);
					}
					// Else we need to create a representation of an option group and then append the option list item.
					else {
						var opt = $('<li class="' + name + ' ' + that.widgetBaseClass + '-optgroup"><span>'+ this.optgroup.attr('label') +'</span><ul></ul></li>');
						if (that.options.classInherit.optionGroup) {
							opt.addClass(this.optgroupClassname);
						}
						if (typeof this.optDisabled !== "undefined" && this.optDisabled === 'disabled') {
							opt.addClass('ui-state-disabled').appendTo(that.group).find('ul').append(list);
						}
						else {
							opt.appendTo(that.group).find('ul').append(list);
						}
					}
				}
				// Else it is not part of an option group and we can append directly to the selectgroup list container.
				else {
					list.appendTo(that.group);
				}
			});	
		},
		_position: function() {
			var options = this.options,
				local = this.group.find('li').not('.ui-selectgroup-optgroup'),
				instance = local.get(this.position);
			// Reset the selectgroup.group position.
			$($.ui.selectgroup.group).css({'top': 0, 'left': 0});
			$($.ui.selectgroup.group).show();
			// Adjust position if it is a popup.
			if (this.options.style === 'popup' && !this.options.positioning.offset) {
				var adjust = '0 -' + ($(instance).outerHeight() + $(instance).offset().top);
			}
			// Position the selectgroup.group using the jQuery UI position widget and any applied options.
			$($.ui.selectgroup.group).position({
				of: options.positioning.of || this.placeholder,
				my: options.positioning.my,
				at: options.positioning.at,
				offset: options.positioning.offset || adjust,
				collision: options.positioning.collision
			});
		},
		_toggle: function() {
			// Toggle the selectgroup to open or close.
			if ($.ui.selectgroup.group.past !== null) {
				if ($.ui.selectgroup.group.past.element !== this.element) {
					this.focus();
					this.close();
				}
			}
			$.ui.selectgroup.group.past = this;
			if (!this.isActive) {
				this.focus();
				if (!this.isOpen) {
					this.open();
				}
				return;
			}
			if (!this.isOpen) {
				this.open();
				return;
			}
			if (this.isActive) {
				this.blur();
				if (this.isOpen) {
					this.close();
				}
				return;
			}
			if (this.isOpen) {
				this.close();
				return;
			}
		},
		_traverse: function(value, record) {
			var local = this.group.find('li').not('.ui-selectgroup-optgroup'),
				maximum = local.length - 1,
				position = this.position,
				instance = null;	
			  position = this.position + value;
			// Traverse the selectgroup based on a numerical iteration.
			if (position < 0) {
				position = 0;
			}
			if (position > maximum) {
				position = maximum;
			}
			// If we have traversed through the positions and not found a new one due to a disabled state we can return to the last available position.
			if (position === record) { 
				return;
			}
			if (this.selectors[position].disabled === 'disabled' || this.selectors[position].optDisabled === 'disabled') {
				if (value > 0) {
					++value;
				}
				else {
					--value;
				}
				// The next position may be disabled. Iterate through until a position is found, zero or maxmimum position reached.
				this._traverse(value, position);
			}
			else {
				this.position = position;
				instance = local.get(this.position);
				this.copy = $(instance).find('a').text();
				local.removeClass('ui-state-hover');
				$(instance).addClass('ui-state-hover');						
				this.placeholder.find('.ui-selectgroup-copy').text(this.copy);
				this.element.find('option:selected').removeAttr('selected');
				$(this.selectors[this.position].element).attr('selected', 'selected');
			}
			$.ui.selectgroup.group.position = value;
		},
		_typeahead: function(character) {
			var that = this,
				options = this.options,
				local = this.group.find('li').not('.ui-selectgroup-optgroup'),
				instance = null,
				found = false;
			// Type ahead based on an alpha numeric charater input.
			character = character.toLowerCase();
			this.search[1] += character;
			window.clearTimeout(this.timer);
			function focusOption(index) {
				that.position = index;
				instance = local.get(that.position);
				local.removeClass('ui-state-hover');
				$(instance).addClass('ui-state-hover');
				that.placeholder.find('.ui-selectgroup-copy').text(that.selectors[index].text);
				that.element.find('option:selected').removeAttr('selected');
				$(that.selectors[index].element).attr('selected', 'selected');
				found = true;
				that.search[3] = index;
			};
			// Typing ahead we need to remember an array of possible word states based on time and character position.
			// Is the new character a repetition of the first character.
			if (this.search[0] === this.search[1][0]) {
				// Are we attempting to iterate through the selectors using the same character?
				if (this.search[1].length < 2) {
					$.each(this.selectors, function(index) {
						if (!found) {
							if ($.trim(that.selectors[index].text).toLowerCase().indexOf(that.search[1][0]) === 0) {
								if (that.search[0] == that.search[1][0]) {
									if (that.search[3] < index) {
										focusOption(index);
									}
								}
							}
						}
					});				
					this.search[0] = this.search[1][0];
				}
				// Or are we navigating to a character combination based on the first character?
				else {
					$.each(this.selectors, function(index) {
						if (!found) {
							if ($.trim(that.selectors[index].text).toLowerCase().indexOf(that.search[1]) === 0) {
								if (that.search[0][0] == that.search[1][0]) {
									if (that.search[3] < index) {
										focusOption(index);
									}
								}
							}
						}
					});
					this.search[0] = this.search[1][0];
				}
			}
			else {
				// Iterate through the selectors with the new unrepeated character.
				$.each(this.selectors, function(index) {
					if (!found) {
						if (that.search[1] === $.trim(that.selectors[index].text).substring(0, that.search[1].length).toLowerCase()) {
							that.search[2] = index;
							focusOption(index);
						}
					}
				});
				this.search[0] = this.search[1][0];
			}
			// If the end of available matches is meet we can reposition to the start of available matches.
			if (that.search[4] === that.search[3]) {
				that.search[3] = that.search[2];
				focusOption(that.search[3]);
			}
			this.search[4] = this.search[3];
			// Clear the initial search after 1 second.
			this.timer = window.setTimeout(function() {that.search[1] = '';}, (1000));
		},
		destroy: function() {
			// Remove the selectgroup completely.
			// Has known issues.
			var id = this.identifiers[0].split('ui-')
			if (this.isOpen) {
				this.close();
			}
			this.placeholder.remove();
			$(document).unbind('.selectgroup');
			$('label[for="' + this.identifiers[0] + '"]')
				.attr( 'for', id[1] )
				.unbind( '.selectmenu');
			this.element.show();
		},
		enable: function(index, type) {
			// Enable the placeholder, an option group or option in the original select.
			// Trigger a 'change' event to redraw the selectgroup.
			if (this.isOpen) {
				this.close();
			}
			if (typeof (index) == 'undefined') {
				this._placeholderEvents(true);
			}
			else {
				if ( type == 'optgroup' ) {
					this.element.find('optgroup').eq(index).removeAttr('disabled');
				} 
				else {
					this.element.find('option').eq(index).removeAttr('disabled');
				}
			}
		},
		disable: function(index, type) {
			// Disable the placeholder, an option group or option in the original select.
			// Trigger a 'change' event to redraw the selectgroup.
			if (this.isOpen) {
				this.close();
			}
			if (typeof (index) == 'undefined') {
				this._placeholderEvents(false);
			}
			else {
				if ( type == 'optgroup' ) {
					this.element.find('optgroup').eq(index).attr('disabled', 'disabled');
				} 
				else {
					this.element.find('option').eq(index).attr('disabled', 'disabled');
				}
			}
		},
		focus: function() {
			// Focus on the placeholder to activate without opening the selectgroup.group.
			this._index();
			this._renderGroup();
			this.isActive = true;
		},
		blur: function() {
			// After losing focus on the placeholder. 
			this.isActive = false;
		},
		change: function() {
			// Redraw the selectgroup.group.
			this._index();
			this._renderGroup();
		},
		refresh: function() {
			// Refresh the placeholder.
			if ($(this.element).find('option:selected').length) {
				this.copy = this.element.find('option:selected').text();
			}
			else {
				this.copy = this.element.find('option').first().text();
			}
			this.placeholder.find('.ui-selectgroup-copy').text(this.copy);
		},
		open: function() {
			// Open the selectgroup.group.
			if (this.options.style === 'dropdown') {
				$.ui.selectgroup.group.removeClass('ui-corner-all').addClass('ui-corner-bottom');
				this.placeholder.removeClass('ui-corner-all').addClass('ui-state-active ui-corner-top');
			}
			else {
				$.ui.selectgroup.group.removeClass('ui-corner-bottom').addClass('ui-corner-all');
				this.placeholder.addClass('ui-state-active');
			}
			this._position();
			this.group.attr('aria-hidden', 'false');
			this.isOpen = true;
		},
		close: function() {
			// Close the selectgroup.group.
			if ($.ui.selectgroup.group.past !== null) {
				$.ui.selectgroup.group.past.placeholder.removeClass('ui-state-active');
			}
			if (this.options.style === 'dropdown') {
				this.placeholder.addClass('ui-corner-all').removeClass('ui-state-active');
			}
			else {
				this.placeholder.removeClass('ui-state-active');
			}
			$.ui.selectgroup.group.hide();
			this.placeholder.focus();
			this.group.attr('aria-hidden', 'true');
			this.isOpen = false;
		}
	});
	// Publically available objects used once by the selectgroup to enable the flyweight pattern.
	$.ui.selectgroup.group = $('<div class="ui-selectgroup-group ui-widget ui-widget-content ui-corner-all"></div>');
	$.ui.selectgroup.group.initialised = false;
	$.ui.selectgroup.group.past = null;
})(jQuery);