(function ($) {
	$.fn.termComplete = function (options) {
		options = $.extend({}, $.fn.termComplete.defaultOptions, options);
		this.each(function () {
			var instance = $(this);
			instance.autocomplete({
				source: function (request, response) {
					var vocabularyId = instance.data("vocabularyid") ? instance.data("vocabularyid") : -1;
					var termId = instance.data("termid") ? instance.data("termid") : -1;
					var parentId = options.getParent() ? options.getParent() : -1;
					$.ajax({
						url: options.serviceFramework.getServiceRoot('Taxonomy') + 'Services/Search?vocabularyId=' + vocabularyId + "&termId=" + termId + "&parentId=" + parentId,
						beforeSend: options.serviceFramework.setModuleHeaders,
						data: {
							termName: request.term
						},
						success: function (data) {
							updateButtonStatus(instance);
							response(data);
						}
					});
				},
				close: function() {
					updateButtonStatus(instance);
				}
			});
			
			instance.on("termVal", function (e) {
				updateButtonStatus(instance);
			});
		});

		function updateButtonStatus(instance) {
			var vocabularyId = instance.data("vocabularyid") ? instance.data("vocabularyid") : -1;
			var termId = instance.data("termid") ? instance.data("termid") : -1;
			var parentId = options.getParent() ? options.getParent() : -1;
			$.ajax({
				url: options.serviceFramework.getServiceRoot('Taxonomy') + 'Services/Exist?vocabularyId=' + vocabularyId + "&termId=" + termId + "&parentId=" + parentId + "&termName=" + instance.val(),
				beforeSend: options.serviceFramework.setModuleHeaders,
				success: function (data) {
					if (data) {
						options.saveButton.addClass("dnnActionDisabled");
					} else {
						options.saveButton.removeClass("dnnActionDisabled");
					}
				}
			});
		}

		options.saveButton.click(function() {
			return !$(this).hasClass("dnnActionDisabled");
		});
		
		return this;
	};
	
	$.fn.termComplete.defaultOptions = {
		serviceFramework: null,
		getParent: null,
		saveButton: null
	};
})(jQuery);