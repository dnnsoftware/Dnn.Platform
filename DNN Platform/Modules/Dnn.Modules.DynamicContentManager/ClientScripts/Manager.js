function Manager($, ko, settings, resx){
    var $rootElement;
    var activePanel;

    var viewModel = {};

    var menuClick = function (panel) {
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

    var selectContentTypes = function() {
        menuClick(settings.contentTypesPanel);
        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();
    };

    var selectDataTypes = function () {
        menuClick(settings.dataTypesPanel);
        viewModel.dataTypes.pageIndex(0);
        viewModel.dataTypes.searchText('');
        viewModel.dataTypes.getDataTypes();
    };

    var selectTemplates = function () {
        menuClick(settings.contentTemplatesPanel);
    };

    var selectSettings = function () {
        menuClick(settings.settingsPanel);
    };

    var init = function(element) {
        $rootElement = $(element);

        activePanel = settings.initialPanel;

        var config = {
            settings: settings,
            resx: resx,
            util: dcc.utility(settings)
        };

        //Build the ViewModel
        viewModel.resx = resx;

        //Wire up contentTypes subModel
        dcc.contentTypes(ko, viewModel, resx, settings);
        viewModel.contentTypes.init();

        //Wire up dataTypes subModel
        dcc.dataTypes(ko, viewModel, config);
        viewModel.dataTypes.init();

        viewModel.templates = dcc.templates(ko, resx, settings);

        viewModel.settings = dcc.settings(ko, resx, settings);

        viewModel.selectContentTypes = selectContentTypes;
        viewModel.selectDataTypes = selectDataTypes;
        viewModel.selectTemplates = selectTemplates;
        viewModel.selectSettings = selectSettings;

        ko.applyBindings(viewModel, $rootElement[0]);

        viewModel.contentTypes.pageIndex(0);
        viewModel.contentTypes.searchText('');
        viewModel.contentTypes.getContentTypes();

        $rootElement.find('input[type="checkbox"]').dnnCheckbox();
    }

    return {
        init: init
    }
}