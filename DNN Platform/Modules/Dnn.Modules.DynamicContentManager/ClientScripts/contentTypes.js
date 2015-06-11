if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.contentTypesViewModel = function(config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;

    util.contentTypeService = function(){
        util.sf.serviceController = "ContentType";
        return util.sf;
    };

    self.isEditMode = ko.observable(false);
    self.isSystemUser = settings.isSystemUser;
    self.heading = ko.observable(resx.contentTypes);
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentTypes_PagerFormat;
    self.pager_NoPagerFormat = resx.contentTypes_NoPagerFormat;
    self.selectedContentType = new dcc.contentTypeViewModel(self, config);

    self.heading = ko.computed(function() {
        var heading = resx.contentTypes;
        if(self.isEditMode()){
            heading = resx.contentType + " - " + self.selectedContentType.name()
        }
        return heading;
    });

    var findContentTypes =  function() {
        self.pageIndex(0);
        self.getContentTypes();
    };

    var toggleView = function() {
        if(self.isEditMode()){
            $rootElement.find("#contentTypes-listView").show();
            $rootElement.find("#contentTypes-editView").hide();
            self.isEditMode(false);
        }
        else {
            $rootElement.find("#contentTypes-listView").hide();
            $rootElement.find("#contentTypes-editView").show();
            self.isEditMode(true);
        }
    };

    self.addContentType = function(){
        toggleView();
        self.selectedContentType.init();
    };

    self.closeEdit = function() {
        toggleView();
        self.refresh();
    }

    self.editContentType = function(data, e) {
        util.asyncParallel([
            function(cb1){
                self.getContentType(data.contentTypeId(), cb1);
            }
        ], function() {
            toggleView();
        });
    };

    self.getContentType = function (contentTypeId, cb) {
        var params = {
            contentTypeId: contentTypeId
        };

        util.contentTypeService().get("GetContentType", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedContentType.load(data.data.contentType);
                } else {
                    //Error
                }
            },

            function(){
                //Failure
            }
        );

        if(typeof cb === 'function') cb();
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

        $rootElement.find("#contentTypes-editView").css("display", "none")
    };

    self.load =function(data) {
        self.results.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var contentType = new dcc.contentTypeViewModel(self, config);
            contentType.init();
            contentType.load(result);
            self.results.push(contentType);
        }
        self.totalResults(data.totalResults)
    };

    self.refresh = function() {
        self.getContentTypes();
    }
};

dcc.contentTypeViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var $rootElement = config.$rootElement;

    self.parentViewModel = parentViewModel;
    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.contentTypeId = ko.observable(-1);
    self.description = ko.observable('');
    self.name = ko.observable('');
    self.isSystem = ko.observable(false);
    self.selected = ko.observable(false);

    self.fields = ko.observable(new dcc.contentFieldsViewModel(self, config));
    self.fields().init();

    self.isAddMode = ko.computed(function() {
        return self.contentTypeId() == -1;
    });

    self.cancel = function(){
        parentViewModel.closeEdit();
    };

    self.deleteContentType = function (data, e) {
        util.confirm(resx.deleteContentTypeConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                contentTypeId: data.contentTypeId(),
                name: data.name(),
                isSystem: data.isSystem()
            };

            util.contentTypeService().post("DeleteContentType", params,
                function(data){
                    //Success
                    parentViewModel.refresh();
                },

                function(data){
                    //Failure
                }
            );
        });
    };

    self.init = function(){
        self.canEdit(true);
        self.contentTypeId(-1);
        self.description("");
        self.name("");
        self.isSystem(self.parentViewModel.isSystemUser);
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.created(data.created);
        self.contentTypeId(data.contentTypeId);
        self.description(data.description);
        self.name(data.name);
        self.isSystem(data.isSystem);

        if(data.contentFields != null) {
            self.fields().load(data.contentFields)
        }
    };

    self.saveContentType = function(data, e) {
        var params = {
            contentTypeId: data.contentTypeId(),
            description: data.description(),
            name: data.name(),
            isSystem: data.isSystem()
        };

        util.contentTypeService().post("SaveContentType", params,
            function(data){
                //Success
                if(self.isAddMode()){
                    util.alert(resx.saveContentTypeMessage.replace("{0}", params.name), resx.ok, function() {
                        self.contentTypeId(data.data.contentTypeId)
                    });
                }
                else{
                    self.cancel();
                }
            },

            function(data){
                //Failure
            }
        )


    };

    self.toggleSelected = function() {
        self.selected(!self.selected());
    };
}

dcc.contentFieldsViewModel = function(parentViewModel, config) {
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;

    self.parentViewModel = parentViewModel;

    self.contentFieldsHeading = resx.contentFields;
    self.contentFields = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentFields_PagerFormat;
    self.pager_NoPagerFormat = resx.contentFields_NoPagerFormat;

    self.init = function() {
        dcc.pager().init(self);
    };

    self.load = function(data) {
        self.contentFields.removeAll();

        for(var i=0; i < data.fields.length; i++){
            var result = data.fields[i];
            var contentField = new dcc.contentFieldViewModel(self, config);
            contentField.load(result);
            self.contentFields.push(contentField);
        }
        self.totalResults(data.totalResults)
    };

    self.refresh = function() {
        var params = {
            contentTypeId: parentViewModel.contentTypeId,
            pageIndex: self.pageIndex,
            pageSize: self.pageSize
        };

        util.contentTypeService().get("GetContentFields", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.load(data.data.contentFields);
                } else {
                    //Error
                }
            },

            function(){
                //Failure
            }
        );
    };
}

dcc.contentFieldViewModel = function(parentViewModel, config) {
    var self = this;

    self.parentViewModel = parentViewModel;
    self.name = ko.observable('');
    self.label = ko.observable('');
    self.dataType = ko.observable('');

    self.init = function() {
        dcc.pager().init(self);
    };

    self.load = function(data) {
        self.name(data.name);
        self.label(data.label);
        self.dataType(data.dataType);
    }
}