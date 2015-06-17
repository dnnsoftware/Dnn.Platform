if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.contentTypesViewModel = function(config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;

    self.mode = config.mode;
    self.isSystemUser = settings.isSystemUser;
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
        if(self.mode() != "listTypes"){
            heading = resx.contentType + " - " + self.selectedContentType.name()
        }
        return heading;
    });

    var findContentTypes =  function() {
        self.pageIndex(0);
        self.getContentTypes();
    };

    self.addContentType = function(){
        self.mode("editType");
        self.selectedContentType.init();
    };

    self.closeEdit = function() {
        self.mode("listTypes");
        self.refresh();
    }

    self.editContentType = function(data, e) {
        util.asyncParallel([
            function(cb1){
                self.getContentType(data.contentTypeId(), cb1);
            }
        ], function() {
            self.mode("editType");
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
                        self.fields().clear();
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

    self.mode = config.mode;
    self.contentFieldsHeading = resx.contentFields;
    self.contentFields = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentFields_PagerFormat;
    self.pager_NoPagerFormat = resx.contentFields_NoPagerFormat;
    self.selectedContentField = new dcc.contentFieldViewModel(self, config);

    self.addContentField = function() {
        self.mode("editField");
        self.selectedContentField.init();
    }

    self.editContentField = function(data, e) {
        util.asyncParallel([
            function(cb1){
                self.getContentField(self.parentViewModel.contentTypeId, data.contentFieldId(), cb1);
            }
        ], function() {
            self.mode("editField");
        });
    };

    self.clear = function() {
        self.contentFields.removeAll();
        self.pageIndex(0);
        self.pageSize = settings.pageSize;
    };

    self.getContentField = function (contentTypeId, contentFieldId, cb) {
        var params = {
            contentTypeId: contentTypeId,
            contentFieldId: contentFieldId
        };

        util.contentTypeService().get("GetContentField", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedContentField.load(data.data.contentField);
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
    var resx = config.resx;
    var util = config.util;

    self.parentViewModel = parentViewModel;
    self.mode = config.mode;
    self.contentTypeId = ko.observable(-1);
    self.contentFieldId = ko.observable(-1);
    self.name = ko.observable('');
    self.label = ko.observable('');
    self.description = ko.observable('');
    self.dataType = ko.observable('');
    self.dataTypeId = ko.observable(-1);
    self.selected = ko.observable(false);

    self.dataTypes = ko.observableArray([]);

    self.isAddMode = ko.computed(function() {
        return self.contentFieldId() == -1;
    });

    self.heading = ko.computed(function() {
        var heading = resx.contentField;
        if (!self.isAddMode()) {
            heading = heading + " - " + self.name();
        }
        return heading;
    });

    var getDataTypes = function() {
        var params = {
            searchTerm: '',
            pageIndex: 0,
            pageSize: 1000
        };

        util.dataTypeService().get("GetDataTypes", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.dataTypes.removeAll();
                    for(var i = 0; i < data.data.results.length; i++){
                        var result = data.data.results[i];
                        self.dataTypes.push({
                            dataTypeId: result.dataTypeId,
                            name: result.name
                        });
                    }
                } else {
                    //Error
                }
            },

            function(){
                //Failure
            }
        );
    };

    self.cancel = function(){
        self.mode("editType");
        parentViewModel.refresh();
    };

    self.deleteContentField = function (data, e) {
        util.confirm(resx.deleteContentFieldConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                contentFieldId: data.contentFieldId(),
                contentTypeId: data.contentTypeId(),
                name: data.name(),
                label: data.label(),
                description: data.description(),
                dataTypeId: data.dataTypeId()
            };

            util.contentTypeService().post("DeleteContentField", params,
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

    self.init = function() {
        self.contentFieldId(-1);
        self.contentTypeId(self.parentViewModel.parentViewModel.contentTypeId());
        self.name('');
        self.label('');
        self.description('');
        self.dataTypeId(-1);
        getDataTypes();
    };

    self.load = function(data) {
        self.contentFieldId(data.contentFieldId);
        self.contentTypeId(data.contentTypeId);
        self.name(data.name);
        self.label(data.label);
        self.description(data.description);
        self.dataType(data.dataType);
        self.dataTypeId(data.dataTypeId);
    }

    self.saveContentField = function(data, e) {
        var params = {
            contentFieldId: data.contentFieldId(),
            contentTypeId: data.contentTypeId(),
            name: data.name(),
            label: data.label(),
            description: data.description(),
            dataTypeId: data.dataTypeId()
        };

        util.contentTypeService().post("SaveContentField", params,
            function (data) {
                //Success
                self.cancel();
            },

            function (data) {
                //Failure
            }
        )
    };

    self.toggleSelected = function() {
        self.selected(!self.selected());
    };
}