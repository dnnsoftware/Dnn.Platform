if (typeof dcc === "undefined" || dcc === null) {
    dcc = {};
};

dcc.quickSettings = function ($, ko, options, resx) {
    var opts = $.extend({}, dcc.quickSettings.defaultOptions, options);
    var $rootElement;

    var util = dcc.utility(opts, resx);
    util.settingsService = function () {
        util.sf.serviceController = "Settings";
        return util.sf;
    };

    var viewModel = {};

    var init = function(element) {
        $rootElement = $(element);

        viewModel.contentTypes = opts.contentTypes;
        viewModel.templates = ko.observableArray(opts.templates);
        viewModel.selectedTypeId = ko.observable(opts.selectedTypeId);
        viewModel.selectedViewTemplateId = ko.observable(opts.selectedViewTemplateId);
        viewModel.selectedEditTemplateId = ko.observable(opts.selectedEditTemplateId);

        var getTemplates = function (contentTypeId) {
            var params = {
                contentTypeId: contentTypeId
            };

            util.settingsService().get("GetTemplates", params,
            function (data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    viewModel.templates.removeAll();
                    for (var i = 0; i < data.data.results.length; i++) {
                        var result = data.data.results[i];
                        var template = { name: result.name, value: result.value };
                        viewModel.templates.push(template);
                    }
                }
            },
            function () {
                //Failure
            }
        );
        };

        var saveSettings = function () {
            var params = {
                contentTypeId: viewModel.selectedTypeId(),
                viewTemplateId: viewModel.selectedViewTemplateId(),
                editTemplateId: viewModel.selectedEditTemplateId()
            };

            util.settingsService().post("SaveSettings", params,
                function (data) {
                    if (data.success === true) {
                        //Success
                    } else {
                        //Error
                        util.alert(data.message, resx.ok);
                    }
                },
                function () {
                    //Failure
                }
            );

        };

        viewModel.selectedTypeId.subscribe(function (newValue) {
            getTemplates(newValue);
        });

        ko.applyBindings(viewModel, $rootElement[0]);

        $(element).dnnQuickSettings({
            moduleId: opts.moduleId,
            onSave: saveSettings
        });
    }



    return {
        init: init
    }
};

dcc.quickSettings.defaultOptions = {

};