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

        viewModel.languages = [];

        util.languageService().get("GetEnabledLanguages", {},
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    for (var i = 0; i < data.data.results.length; i++) {
                        var result = data.data.results[i];
                        var language = { code: result.code, language: result.language };
                        viewModel.languages.push(language);
                    }
                    viewModel.isLocalized = viewModel.languages.length > 0;
                }
                else {
                    //Error
                }
            },
            function(){
                //Failure
            }
        );

        //Wire up contentTypes subModel
        viewModel.contentTypes = new dcc.contentTypesViewModel(config);
        viewModel.contentTypes.init();

        //Wire up dataTypes subModel
        viewModel.dataTypes = new dcc.dataTypesViewModel(config);
        viewModel.dataTypes.init();

        viewModel.templates = new dcc.templatesViewModel(config);
        viewModel.templates.init();

        viewModel.settings = dcc.settings(ko, resx, settings);

        viewModel.selectContentTypes = selectContentTypes;
        viewModel.selectDataTypes = selectDataTypes;
        viewModel.selectTemplates = selectTemplates;
        viewModel.selectSettings = selectSettings;

        ko.applyBindings(viewModel, $rootElement[0]);

        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();
        $rootElement.find("#contentTypes-menu").addClass("selected");

        $rootElement.find('input[type="checkbox"]').dnnCheckbox();
    }

    return {
        init: init
    }
}

dcc.languageViewModel = function(){
    var self = this;

}