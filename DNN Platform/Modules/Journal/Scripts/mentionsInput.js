(function($) {
	$.fn.mentionsInput = function(options) {
		if (this.length == 0)
			return this;
		options = $.extend({}, $.fn.mentionsInput.defaultOptions, options);
		var serviceFramework = options.servicesFramework;
	    var getBaseServicePath = function() {
            return serviceFramework.getServiceRoot('Journal') + 'Services/';
	    };

	    return this.each(function() {
	        var $this = $(this);
	        $this.data("mentions", []);
	        var getPosition = function () {
			    var element = $this.get(0);
			    if ('selectionStart' in element) {
				    // Standard-compliant browsers
				    return element.selectionStart;
			    } else if (document.selection) {
				    // IE
				    element.focus();
				    var sel = document.selection.createRange();
				    var selLen = document.selection.createRange().text.length;
				    sel.moveStart('character', -element.value.length);
				    return sel.text.length - selLen;
			    } else {
				    return -1;
			    }
		    };

		    var getContentByCursor = function() {
			    var position = getPosition();
			    if (position > 0) {
				    if ($this.val().length > position) {
					    for (var i = position; i < $this.val().length; i++) {
						    if (/\s/.test($this.val()[i])) {
							    position = i;
							    break;
						    } else if (i == $this.val().length - 1) {
							    position = i + 1;
						    }
					    }
				    }
				    var content = $this.val().substr(0, position);
				    var result = new RegExp(options.identityChar + "([\\S]+?)$").exec(content);
				    if (result) {
					    return result[1];
				    }
			    }

			    return null;
		    };

	        var addMention = function(item) {
	            var mentions = $this.data("mentions");
                if (!itemExists(item)) {
                    mentions.push(item);
                }
	        };

	        var itemExists = function(item) {
	            var mentions = $this.data("mentions");
	            var exists = false;
	            for (var i = 0; i < mentions.length; i++) {
	                if (mentions[i].displayName == item.displayName) {
	                    exists = true;
	                    break;
	                }
	            }

	            return exists;
	        };

	        var rebuildMentions = function() {
	            var mentions = $this.data("mentions");
	            var content = $this.val();
	            for (var i = mentions.length - 1; i >= 0; i--) {
	                if (content.toLowerCase().indexOf(options.identityChar + mentions[i].displayName.toLowerCase()) == -1) {
	                    mentions.splice(i, 1);
	                }
	            }
	        };

	        var filter = function(data) {
	            for (var i = data.length - 1; i >= 0; i--) {
	                if (itemExists(data[i])) {
	                    data.splice(i, 1);
	                }
	            }

	            return data;
	        };

	        $this.on("click", function () {
	            $this.autocomplete("search");
	        }).on("keydown", function (event) {
	            var keyCode = $.ui.keyCode;
	            if (event.keyCode == keyCode.ENTER) {
	                var ac = $this.data('ui-autocomplete');
	                if ($this.attr("menuOpen") && !ac.menu.active) {
	                    return false;
	                }
	            }
	        }).on("keyup", function (event) {
	            var keyCode = $.ui.keyCode;
	            switch (event.keyCode) {
	            case keyCode.PAGE_UP:
	            case keyCode.PAGE_DOWN:
	            case keyCode.UP:
	            case keyCode.DOWN:
	            case keyCode.TAB:
                case keyCode.ENTER:
	                return;
	            default:
	                $this.autocomplete("search");
	            }
	        }).on("input change", function (e) {
	            rebuildMentions();
	        }).autocomplete({
	            autoFocus: true,
	            source: function(request, response) {
	                var content = getContentByCursor();
	                if (content && content.length >= options.minLength) {
	                    $.ajax({
	                        type: "GET",
	                        cache: false,
	                        url: getBaseServicePath() + "GetSuggestions",
	                        beforeSend: serviceFramework.setModuleHeaders,
	                        data: {
	                            keyword: content
	                        }
	                    }).done(function(data) {
	                        response(filter(data));
	                    }).fail(function() {
	                        //displayMessage(settings.searchErrorText, "dnnFormWarning");
	                        response({});
	                    });
	                }
	            },
	            minLength: options.minLength,
	            select: function(event, ui) {
	                var key = options.identityChar + ui.item.key;
	                $this.val($this.val().replace(new RegExp(key + " " + "|" + key + "$", "g"), options.identityChar + ui.item.displayName + " "));
	                addMention(ui.item);
	                return false;
	            },
	            search: function(event, ui) {
	                var content = getContentByCursor();
	                if (!content || content.length < options.minLength) {
	                    $this.autocomplete('close');
	                    return false;
	                }
	            },
	            focus: function(event, ui) {
	                event.preventDefault();
	            },
	            close: function(event, ui) {
	                $this.attr("menuOpen", false);
	            }
	        });

	        $this.data('ui-autocomplete')._renderItem = function(ul, item) {
	            return $('<li></li>')
	                .data('ui-autocomplete-item', item)
	                .append('<a><img src="' + item.avatar + '" /><span class="dn">' + item.displayName + '<span></a>')
	                .appendTo(ul);
	        };

	        $this.data('ui-autocomplete')._renderMenu = function(ul, items) {
	            var that = this;
	            $.each(items, function(index, item) {
	                that._renderItemData(ul, item);
	            });
	            $this.attr("menuOpen", true);
	        };
	    });
	};

	$.fn.mentionsInput.defaultOptions = {
		identityChar: '@',
		minLength: 1
	};
})(window.jQuery);