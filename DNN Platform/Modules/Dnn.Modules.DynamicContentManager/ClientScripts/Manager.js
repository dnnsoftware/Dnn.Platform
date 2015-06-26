function Manager($, ko, codeEditor, settings, resx){
    var $rootElement;
    var activePanel;

    var viewModel = {};

    var menuClick = function (target, panel) {
        $rootElement.find(".dccMenu li").removeClass("selected");

        var listItem = $(target);

        if(listItem.is("li") == false){
            listItem = listItem.closest('li');
        }

        listItem.addClass("selected");

        if (activePanel === panel) {
            return;
        }

        //slide panels in
        var zIndex = $(panel).css("z-index");
        $(panel).css("z-index", zIndex + 10);
        $(activePanel).animate({ opacity: 0 }, 400, function () {
            $(this).offset({ left: -850 });
            $(this).css("opacity", 1);
            $(panel).animate({ left: 0 }, 1500);
            $(panel).css("z-index", zIndex);
        });

        activePanel = panel;
    };

    var selectContentTypes = function(data, e) {
        menuClick(e.target, settings.contentTypesPanel);
        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();
        viewModel.templates.mode("listTypes");
    };

    var selectDataTypes = function (data, e) {
        menuClick(e.target, settings.dataTypesPanel);
        viewModel.dataTypes.pageIndex(0);
        viewModel.dataTypes.searchText('');
        viewModel.dataTypes.getDataTypes();
        viewModel.templates.mode("dataTypes");
    };

    var selectTemplates = function (data, e) {
        menuClick(e.target, settings.contentTemplatesPanel);
        viewModel.templates.pageIndex(0);
        viewModel.templates.searchText('');
        viewModel.templates.getTemplates();
        viewModel.templates.mode("listTemplates");
    };

    var selectSettings = function (data, e) {
        menuClick(e.target, settings.settingsPanel);
    };

    var init = function(element) {
        $rootElement = $(element);

        activePanel = settings.initialPanel;

        var util = dcc.utility(settings, resx);

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

        var config = {
            settings: settings,
            resx: resx,
            util: util,
            $rootElement: $rootElement,
            mode: ko.observable("listTypes"),
            codeEditor: codeEditor
        };

        //Build the ViewModel
        viewModel.resx = resx;

        viewModel.languages = ko.observableArray([]);
        viewModel.selectedLanguage = ko.observable('');
        viewModel.isLocalized = ko.observable(false);

        viewModel.mode = config.mode;

        //Wire up contentTypes subModel
        viewModel.contentTypes = new dcc.contentTypesViewModel(config, viewModel);
        viewModel.contentTypes.init();

        //Wire up dataTypes subModel
        viewModel.dataTypes = new dcc.dataTypesViewModel(config, viewModel);
        viewModel.dataTypes.init();

        viewModel.templates = new dcc.templatesViewModel(config, viewModel);
        viewModel.templates.init();

        viewModel.settings = dcc.settings(ko, resx, settings);

        viewModel.selectContentTypes = selectContentTypes;
        viewModel.selectDataTypes = selectDataTypes;
        viewModel.selectTemplates = selectTemplates;
        viewModel.selectSettings = selectSettings;

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
                    heading = resx.contentType + " - " + viewModel.contentTypes.selectedContentType.name()
                    break;
                case "dataTypes":
                    heading = resx.dataTypes;
                    break;
                case "listTemplates":
                    heading = resx.templates;
                    break;
                case "editTemplate":
                    heading = resx.template + " - " + viewModel.templates.selectedTemplate.name()
                    break;
            }
            return heading;
        });

        viewModel.closeEdit = function() {
            switch(viewModel.mode()){
                case "editField":
                case "editType":
                    viewModel.mode("listTypes");
                    viewModel.contentTypes.refresh();
                    break;
                case "editTemplate":
                    viewModel.mode("listTemplates");
                    viewModel.templates.refresh();
                    break;
            }
        };

        ko.applyBindings(viewModel, $rootElement[0]);

        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();
        $rootElement.find("#contentTypes-menu").addClass("selected");

        $rootElement.find('input[type="checkbox"]').dnnCheckbox();

        util.languageService().get("GetEnabledLanguages", {},
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    for (var i = 0; i < data.data.results.length; i++) {
                        var result = data.data.results[i];
                        var language = { code: result.code, language: result.language };
                        viewModel.languages.push(language);
                    }
                    viewModel.isLocalized(viewModel.languages().length > 0);
                    viewModel.selectedLanguage(data.data.defaultLanguage);
                }
                else {
                    //Error
                }
            },
            function(){
                //Failure
            }
        );
    }

    return {
        init: init
    }
}
