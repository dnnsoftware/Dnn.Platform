var dnnCountryRegionService;

function CountryRegionService($) {
	var baseServicepath = $.dnnSF(-1).getServiceRoot('InternalServices') + 'CountryRegion/';

	this.listCountries = function (searchString, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Countries',
			data: { searchString: searchString }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(eval("(" + xhr.responseText + ")").ExceptionMessage);
		});
	};

	this.listRegions = function (country, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Country/' + country + '/Regions',
			beforeSend: $.dnnSF(moduleId).setModuleHeaders,
			data: {}
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(eval("(" + xhr.responseText + ")").ExceptionMessage);
		});
	};

	this.listSiblingRegions = function (region, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Region/' + region + '/Siblings',
			beforeSend: $.dnnSF(moduleId).setModuleHeaders,
			data: { searchString: searchString }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(eval("(" + xhr.responseText + ")").ExceptionMessage);
		});
	};

}

function setRegionList() {
	if (typeof dnnRegionBoxId !== 'undefined') {
		$('#' + dnnRegionBoxId + '_dropdown').hide();
		$('#' + dnnRegionBoxId + '_text').show();
		var initVal = $('#' + dnnRegionBoxId + '_value').attr('value');
		if (typeof dnnCountryBoxId !== 'undefined') {
			dnnCountryRegionService.listRegions($('#' + dnnCountryBoxId + '_code').val(), function (data) {
				setRegionDropdown(data);
			});
		} else {
			if (initVal != '') {
				dnnCountryRegionService.listSiblingRegions(initVal, function (data) {
					setRegionDropdown(data);
				});
			}
		}
		$('#' + dnnRegionBoxId + '_dropdown').change(function () {
			$('#' + dnnRegionBoxId + '_value').val($('#' + dnnRegionBoxId + '_dropdown option:selected').val());
		});
		$('#' + dnnRegionBoxId + '_text').change(function () {
			$('#' + dnnRegionBoxId + '_value').val($('#' + dnnRegionBoxId + '_text').val());
		});
	}
}

function setRegionDropdown(data) {
	if (typeof dnnRegionBoxId == 'undefined') { return };
	var dd = $('#' + dnnRegionBoxId + '_dropdown');
	$(dd).empty();
	$.each(data, function (index, value) {
		$(dd).append($('<option>').text(value.Text).attr('value', value.Value));
	});
	if ($(dd).children().length == 0) {
		$(dd).hide();
		$('#' + dnnRegionBoxId + '_text').show();
		$('#' + dnnRegionBoxId + '_text').val($('#' + dnnRegionBoxId + '_value').val());
	} else {
		$(dd).show();
		$('#' + dnnRegionBoxId + '_text').hide();
		$(dd).children('option[value="' + $('#' + dnnRegionBoxId + '_value').val() + '"]').attr('selected', '1');
	}
}

function setupCountryAutoComplete() {
	$('input[data-editor="DnnCountryAutocompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			dnnCountryRegionService.listCountries(request.term, function (data) {
				response($.map(data, function (item) {
					return {
						label: item.FullName,
						id: item.Code,
						value: item.Code,
						name: item.Name
					};
				}))
			})
		},
		select: function (event, ui) {
			$('#' + dnnCountryBoxId + '_code').val(ui.item.id);
			$('#' + dnnCountryBoxId + '_name').attr('data-text', ui.item.name);
		},
		close: function () {
			$('#' + dnnCountryBoxId + '_name').val($('#' + dnnCountryBoxId + '_name').attr('data-text'));
			setRegionList();
		}
	})
}

function resetCountryRegionControls() {
	alert('here');
	setupCountryAutoComplete();
	setRegionList();
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(resetCountryRegionControls);


(function ($) {
	$(document).ready(function () {
		// alert($('input[data-editor="DnnCountryAutocompleteControl"]').length);
		// alert($.dnnSF(-1).getServiceRoot('InternalServices') + 'CountryRegion/')
		setupCountryAutoComplete();
	});
})(jQuery);