if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.contentTypes = (function(ko, parentViewModel, resx, settings){
    var serviceFramework = settings.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager') + 'ContentType/';

    var viewModel = {
        heading: ko.observable(resx.contentTypes),
        searchText: ko.observable(""),
        results: ko.observableArray([]),
        totalResults: ko.observable(0),
        pageSize: settings.pageSize,
        pager_PageDesc: resx.pager_PageDesc,
        pager_PagerFormat: resx.contentTypes_PagerFormat,
        pager_NoPagerFormat: resx.contentTypes_NoPagerFormat,

        getContentTypes: function () {
            var self = this;
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetContentTypes",
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
                findContentTypes();
            });
        },

        load: function(data) {
            var self = this;
            self.results(data.results);
            self.totalResults(data.totalResults)
        },

        refresh: function() {
            var self = this;
            self.getContentTypes();
        }
    };

    var findContentTypes =  function() {
        viewModel.pageIndex(0);
        viewModel.getContentTypes();
    };

    parentViewModel.contentTypes = viewModel;

    return;
});