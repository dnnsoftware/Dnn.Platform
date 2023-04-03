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
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
	};

	this.listRegions = function (country, success) {
		$.ajax({
			type: "GET",
			url: baseServicepath + 'Regions',
			data: { country: country }
		}).done(function (data) {
			if (success != undefined) {
				success(data);
			}
		}).fail(function (xhr, status) {
			alert(JSON.parse(xhr.responseText).ExceptionMessage);
		});
	};

}

function setupRegionLists() {
	$('div[data-list="Region"]').each(function (index, value) {
		var stringValue = $(value).children('input[data-editor="DNNRegionEditControl_Hidden"]').val();
		var $row = $(value);
		var $label = $row.prev().find('label');
		var $textControl = $row.children('input[data-editor="DNNRegionEditControl_Text"]');
		var $selectControl = $row.children('select');
		if ($selectControl.children('option').length < 2) {
			$selectControl.hide();
			$textControl.show();
			$textControl.val(stringValue);

			$label.attr('for', $textControl.attr('id'));
		} else {
			$selectControl.show();
			$textControl.hide();
			$selectControl.children('option[value="' + stringValue + '"]').prop('selected', true);

			$label.attr('for', $selectControl.attr('id'));
		};
	});
	$('select[data-editor="DNNRegionEditControl_DropDown"]').change(function () {
		$(this).parent().children('input[data-editor="DNNRegionEditControl_Hidden"]').val($(this).val());
	});
}

function loadRegionList(category, country) {
	var selector = category ? 'div[data-list="Region"][data-category="' + category + '"]' : 'div[data-list="Region"]';
	$(selector).each(function (index, value) {
		var dd = $(value).children('select');
		$(dd).children().not(':first').remove();
		if (country != '') {
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			dnnCountryRegionService.listRegions(country, function (data) {
				$.each(data, function (index, value) {
					$(dd).append($('<option>').text(value.Text).attr('value', value.Value));
				});
				setupRegionLists();
			});
		}
	});
}

function clearCountryValue(countryControl) {
	$('#' + $(countryControl).attr('id').replace('_name', '') + '_id').val('');
	$(countryControl).attr('data-value', '');
	$(countryControl).attr('value', '');
	loadRegionList($(countryControl).attr('data-category'), '');
	setupRegionLists();
}

function setupCountryAutoComplete() {
	$('input[data-editor="DnnCountryAutocompleteControl"]').autocomplete({
		minLength: 2,
		source: function (request, response) {
			var control = this.element;
			if (dnnCountryRegionService == undefined) { dnnCountryRegionService = new CountryRegionService($) };
			dnnCountryRegionService.listCountries(request.term, function (data) {
				if (data.length == 0) {
					clearCountryValue(control);
				};
				response($.map(data, function (item) {
					return {
						label: item.FullName,
						id: item.Id,
						value: item.Id,
						name: item.Name
					};
				}))
			})
		},
		select: function (event, ui) {
			$('#' + $(this).attr('id').replace('_name', '') + '_id').val(ui.item.id);
			$(this).attr('data-value', ui.item.id);
			$(this).attr('data-text', ui.item.name);
		},
		close: function () {
			if ($(this).attr('data-text') != undefined) {
				$(this).val($(this).attr('data-text'));
				loadRegionList($(this).attr('data-category'), $(this).attr('data-value'));
			} else {
				clearCountryValue(this);
			}
		},
		focus: function (event, ui) {
			event.preventDefault();
			this.value = ui.item.name;
			$(this).attr('data-value', ui.item.id);
			$(this).attr('data-text', ui.item.name);
		}
	});
	$('input[data-editor="DnnCountryAutocompleteControl"]').change(function () {
		if ($(this).val().length < 2) {
			clearCountryValue(this);
		}
	});
}

function initCountryRegionControls() {
	setupCountryAutoComplete();
	setupRegionLists();
}

Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initCountryRegionControls);


(function ($) {
	$(document).ready(function () {
		initCountryRegionControls();
	});
})(jQuery);
