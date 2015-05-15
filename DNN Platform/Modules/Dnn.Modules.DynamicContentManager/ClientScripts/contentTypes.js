if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.contentTypesViewModel = function(config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;

    util.contentTypeService = function(){
        util.sf.serviceController = "ContentType";
        return util.sf;
    };

    self.heading = ko.observable(resx.contentTypes);
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentTypes_PagerFormat;
    self.pager_NoPagerFormat = resx.contentTypes_NoPagerFormat;
    self.selectedContentType = new dcc.contentTypeViewModel(self, config);

    var findContentTypes =  function() {
        self.pageIndex(0);
        self.getContentTypes();
    };

    self.getContentTypes = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize
        };

        util.contentTypeService().get("GetContentTypes", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.load(data.data);
                } else {
                    //Error
                }
            },
            function(){
                //Failure
            }
        );
    };

    self.init = function() {
        dcc.pager().init(self);
        self.searchText.subscribe(function () {
            findContentTypes();
        });
    };

    self.load =function(data) {
        self.results.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var contentType = new dcc.contentTypeViewModel(self, config);
            contentType.load(result);
            self.results.push(contentType);
        }
        self.totalResults(data.totalResults)
    };

    self.refresh = function() {
        self.getContentTypes();
    }
}

dcc.contentTypeViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    self.parentViewModel = parentViewModel;
    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.contentTypeId = ko.observable(-1);
    self.name = ko.observable('');
    self.isSystem = ko.observable(false);

    self.isAddMode = ko.computed(function() {
        return self.contentTypeId() == -1;
    });

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.created(data.created);
        self.contentTypeId(data.contentTypeId);
        self.name(data.name);
        self.isSystem(data.isSystem);
    };
}
