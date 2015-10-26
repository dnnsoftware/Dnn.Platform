ko.bindingHandlers.truncatedText = {
    update: function (element, valueAccessor, allBindingsAccessor) {
        var originalText = ko.utils.unwrapObservable(valueAccessor()),
            // 10 is a default maximum length
            length = ko.utils.unwrapObservable(allBindingsAccessor().maxTextLength) || 30,
            truncatedText = originalText.length ? originalText.substring(0, length) + "..." : originalText;
        // updating text binding handler to show truncatedText
        ko.bindingHandlers.text.update(element, function () {
            return truncatedText;
        });
    }
};


function Manager($, ko, codeEditor, settings, resx){
    var $rootElement;
    var activePanel;

    var viewModel = {};

    var menuClick = function (target, panel) {
        $rootElement.find(".dccMenu li").removeClass("selected");
        $rootElement.find(".dccPanel").hide();

        var listItem = $(target);

        if(listItem.is("li") === false){
            listItem = listItem.closest('li');
        }

        listItem.addClass("selected");

        $(panel).show();

        if (activePanel === panel) {
            return;
        }

        activePanel = panel;
    };

    var selectContentTypes = function(data, e) {
        menuClick(e.target, settings.contentTypesPanel);
        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();
        viewModel.mode("listTypes");
    };

    var selectDataTypes = function (data, e) {
        menuClick(e.target, settings.dataTypesPanel);
        viewModel.dataTypes.pageIndex(0);
        viewModel.dataTypes.searchText('');
        viewModel.dataTypes.getDataTypes();
        viewModel.mode("dataTypes");
    };

    var selectTemplates = function (data, e) {
        menuClick(e.target, settings.contentTemplatesPanel);
        viewModel.templates.pageIndex(0);
        viewModel.templates.searchText('');
        viewModel.templates.getTemplates();
        viewModel.mode("listTemplates");
    };

    var selectSettings = function (data, e) {
        menuClick(e.target, settings.settingsPanel);
    };

    var init = function(element) {
        $rootElement = $(element);

        activePanel = settings.initialPanel;

        var util = dnn.utility(settings, resx);

        util.languageService = function(){
            util.sf.serviceController = "Language";
            return util.sf;
        };

        util.contentTypeService = function(){
            util.sf.serviceController = "ContentType";
            return util.sf;
        };

        util.dataTypeService = function(){
            util.sf.serviceController = "DataType";
            return util.sf;
        };

        util.templateService = function(){
            util.sf.serviceController = "Template";
            return util.sf;
        };

        util.handleServiceError = function (xhr, status, err) {
            if (xhr && xhr.responseText) {
                var json = JSON.parse(xhr.responseText);
                if (json && json.Message) {
                    util.alert(json.Message, resx.ok);
                    return;
                }
            }
            util.alert(status + ":" + err, resx.ok);
        };

        var config = {
            settings: settings,
            resx: resx,
            util: util,
            $rootElement: $rootElement,
            mode: ko.observable("listTypes"),
            codeEditor: codeEditor,
            ko: ko
        };

        //Build the ViewModel
        viewModel.resx = resx;

        viewModel.languages = ko.observableArray([]);
        viewModel.selectedLanguage = ko.observable('');
        viewModel.isLocalized = ko.observable(false);

        util.languageService().get("GetEnabledLanguages", {},
            function (data) {
                if (typeof data !== "undefined" && data != null) {
                    //Success
                    for (var i = 0; i < data.results.length; i++) {
                        var result = data.results[i];
                        var language = { code: result.code, language: result.language };
                        viewModel.languages.push(language);
                    }
                    viewModel.isLocalized(viewModel.languages().length > 1);
                    viewModel.selectedLanguage(data.defaultLanguage);
                    viewModel.defaultLanguage = data.defaultLanguage;
                }
            },
            function () {
                //Failure
            }
        );

        viewModel.mode = config.mode;

        //Wire up contentTypes subModel
        // ReSharper disable once InconsistentNaming
        viewModel.contentTypes = new dcc.contentTypesViewModel(viewModel, config);
        viewModel.contentTypes.init();

        //Wire up dataTypes subModel
        // ReSharper disable once InconsistentNaming
        viewModel.dataTypes = new dcc.dataTypesViewModel(viewModel, config);
        viewModel.dataTypes.init();

        // ReSharper disable once InconsistentNaming
        viewModel.templates = new dcc.templatesViewModel(viewModel, config);
        viewModel.templates.init();

        viewModel.settings = dcc.settings(ko, resx, settings);

        viewModel.selectContentTypes = selectContentTypes;
        viewModel.selectDataTypes = selectDataTypes;
        viewModel.selectTemplates = selectTemplates;
        viewModel.selectSettings = selectSettings;

        viewModel.pageSizeOptions = ko.observableArray([
                                { text: 10, value: 10 },
                                { text: 25, value: 25 },
                                { text: 50, value: 50 },
                                { text: 100, value: 100 }
        ]);

        viewModel.showCloseIcon = ko.computed(function() {
            var showIcon = false;
            switch(viewModel.mode()){
                case "editField":
                case "editType":
                case "editTemplate":
                    showIcon = true;
                    break;
            }
            return showIcon;
        });

        viewModel.heading = ko.computed(function() {
            var heading = resx.dataTypes;
            switch(viewModel.mode()){
                case "listTypes":
                    heading = resx.contentTypes;
                    break;
                case "editField":
                case "editType":
                    heading = resx.contentType + " - " + viewModel.contentTypes.selectedContentType.name();
                    break;
                case "dataTypes":
                    heading = resx.dataTypes;
                    break;
                case "listTemplates":
                    heading = resx.templates;
                    break;
                case "editTemplate":
                    heading = resx.template + " - " + viewModel.templates.selectedTemplate.name();
                    break;
            }
            return heading;
        });

        viewModel.closeEdit = function() {
            switch(viewModel.mode()){
                case "editField":
                case "editType":
                    viewModel.contentTypes.closeEdit();
                    break;
                case "editTemplate":
                    viewModel.templates.closeEdit();
                    break;
            }
        };

        viewModel.fieldMessage = function(localizedValues) {
            return util.getLocalizationStatus(viewModel.defaultLanguage, localizedValues, viewModel.resx.defaultValueMissing, viewModel.resx.defaultLocalizedValueMissing, viewModel.resx.translationMissing);
        };

        viewModel.fieldStatus = function(localizedValues) {
            return util.getLocalizationStatus(viewModel.defaultLanguage, localizedValues, "dccError", "dccError", "dccWarning");
        };

        ko.applyBindings(viewModel, $rootElement[0]);

        if (activePanel === "#content-templates-panel") {
            selectTemplates(null, { target: $rootElement.find("#contentTemplates-menu")[0] });
        } else {
            selectContentTypes(null, { target: $rootElement.find("#contentTypes-menu")[0] });
        }
        $rootElement.find('input[type="checkbox"]').dnnCheckbox();
    }

    return {
        init: init
    }
}
