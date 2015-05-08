if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.dataTypes = (function(ko, parentViewModel, resx, settings){
    var serviceFramework = settings.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager') + 'DataType/';

    var viewModel = {
        heading: ko.observable(resx.dataTypes),
        searchText: ko.observable(""),
        results: ko.observableArray([]),
        totalResults: ko.observable(0),
        pageSize: settings.pageSize,
        pager_PageDesc: resx.pager_PageDesc,
        pager_PagerFormat: resx.dataTypes_PagerFormat,
        pager_NoPagerFormat: resx.dataTypes_NoPagerFormat,

        getDataTypes: function () {
            var self = this;
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetDataTypes",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    searchTerm: self.searchText(),
                    pageIndex: self.pageIndex(),
                    pageSize: self.pageSize
                }
            }).done(function (data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.load(data.data);
                } else {
                    //Error
                }
            }).fail(function (xhr, status) {
                //Fail
            });
        },

        init: function() {
            dcc.pager().init(viewModel);

            viewModel.searchText.subscribe(function () {
                findDataTypes();
            });
        },

        load: function(data) {
            var self = this;
            self.results(data.results);
            self.totalResults(data.totalResults)
        },

        refresh: function() {
            var self = this;
            self.getDataTypes();
        }
    };

    var findDataTypes =  function() {
        viewModel.pageIndex(0);
        viewModel.getDataTypes();
    };

    parentViewModel.dataTypes = viewModel;

    return;
});