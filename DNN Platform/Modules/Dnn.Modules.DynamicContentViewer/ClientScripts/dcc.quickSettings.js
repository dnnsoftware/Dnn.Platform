if (typeof dcc === "undefined" || dcc === null) {
    dcc = {};
};

dcc.quickSettings = function ($, ko, options, resx) {
    var opts = $.extend({}, dcc.quickSettings.defaultOptions, options);
    var $rootElement;

    // ReSharper disable once UseOfImplicitGlobalInFunctionScope
    var util = dnn.utility(opts, resx);
    util.settingsService = function () {
        util.sf.serviceController = "Settings";
        return util.sf;
    };

    var viewModel = {};

    var init = function(element) {
        $rootElement = $(element);

        viewModel.contentTypes = opts.contentTypes;
        viewModel.editTemplates = ko.observableArray([]);
        viewModel.viewTemplates = ko.observableArray([]);
        viewModel.selectedTypeId = ko.observable(opts.selectedTypeId);
        viewModel.selectedViewTemplateId = ko.observable(opts.selectedViewTemplateId);
        viewModel.selectedEditTemplateId = ko.observable(opts.selectedEditTemplateId);

        var refreshTemplates = function(templates) {
            viewModel.editTemplates.removeAll();
            viewModel.viewTemplates.removeAll();
            viewModel.editTemplates.push({ name: resx.autoTemplate, value: -1 });
            viewModel.viewTemplates.push({ name: resx.autoTemplate, value: -1 });
            for (var i = 0; i < templates.length; i++) {
                var result = templates[i];
                var template = { name: result.name, value: result.value };
                if (result.isEdit) {
                    viewModel.editTemplates.push(template);
                } else {
                    viewModel.viewTemplates.push(template);
                }
            }
        }

        refreshTemplates(opts.templates);

        var getTemplates = function (contentTypeId) {
            var params = {
                contentTypeId: contentTypeId
            };

            util.settingsService().get("GetTemplates", params,
            function (data) {
                if (typeof data !== "undefined" && data !== null) {
                    //Success
                    refreshTemplates(data.results);
                }
            },
            function () {
                //Failure
            });
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
                        $(opts.container).load(opts.url + " " + opts.container + " .dccViewContent");
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