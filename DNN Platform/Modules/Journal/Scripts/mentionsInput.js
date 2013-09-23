(function($) {
	$.fn.mentionsInput = function(options) {
		var $this = this;
		if ($this.length == 0)
			return;
		options = $.extend({}, $.fn.mentionsInput.defaultOptions, options);
		var serviceFramework = options.servicesFramework;
		var baseServicepath = serviceFramework.getServiceRoot('Journal') + 'Services/';

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
				sel.moveStart('character', -input.value.length);
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

		$this.bind("click", function() {
			$this.autocomplete("search");
		}).bind("keyup", function() {
			$this.autocomplete("search");
		}).autocomplete({
			source: function(request, response) {
				var content = getContentByCursor();
				if (content && content.length >= options.minLength) {
					$.ajax({
						type: "GET",
						cache: false,
						url: baseServicepath + "GetSuggestions",
						beforeSend: serviceFramework.setModuleHeaders,
						data: {
							keyword: content
						}
					}).done(function(data) {
						response(data);
					}).fail(function() {
						//displayMessage(settings.searchErrorText, "dnnFormWarning");
						response({});
					});
				}
			},
			minLength: options.minLength,
			select: function(event, ui) {
				var key = options.identityChar + ui.item.key;
				$this.val($this.val().replace(new RegExp(key + "\\s" + "|" + key + "$", "g"), options.identityChar + ui.item.username + " "));
				return false;
			},
			search: function(event, ui) {
				var content = getContentByCursor();
				if (!content || content.length < options.minLength) {
					return false;
				}
			}
		}).data('ui-autocomplete')._renderItem = function(ul, item) {
			return $('<li></li>')
				.data('ui-autocomplete-item', item)
				.append('<a><img src="' + item.avatar + '" /><span class="dn">' + item.displayName + '<span><span class="un">@' + item.username + '</span></a>')
				.appendTo(ul);
		};
	};

	$.fn.mentionsInput.defaultOptions = {
		identityChar: '@',
		minLength: 1
	};
})(window.jQuery);