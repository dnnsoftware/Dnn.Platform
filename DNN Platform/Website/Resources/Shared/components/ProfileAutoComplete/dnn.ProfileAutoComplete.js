var dnnProfileAutoCompleteService;

function ProfileAutoCompleteService($) {
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'ProfileService/';

	this.searchProfilePropertyValues = function (portalId, propName, searchString, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'ProfilePropertyValues',
			beforeSend: $.dnnSF().setModuleHeaders,
			data: { portalId: portalId, propName: propName, searchString: searchString }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
	};

}

function initProfileAutoCompleteControls() {
	$('input[data-editor="AutoCompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			if (dnnProfileAutoCompleteService == undefined) {
				dnnProfileAutoCompleteService = new ProfileAutoCompleteService($);}
			dnnProfileAutoCompleteService.searchProfilePropertyValues(this.element.attr('data-pid'), this.element.attr('data-name'), request.term, function (data) {
				response(data);
			});
		}
	});
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initProfileAutoCompleteControls);


(function ($) {
	$(document).ready(function () {
		initProfileAutoCompleteControls();
	});
})(jQuery);