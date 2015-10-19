if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.contentTypesViewModel = function(rootViewModel, config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;
    var ko = config.ko;

    self.rootViewModel = rootViewModel;

    self.dataTypes = ko.observableArray([]);
    self.contentTypes = ko.observableArray([]);

    self.mode = config.mode;
    self.isSystemUser = settings.isSystemUser;
    self.searchText = ko.observable("").extend({ throttle: 500 });
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = ko.observable(settings.pageSize);
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentTypes_PagerFormat;
    self.pager_NoPagerFormat = resx.contentTypes_NoPagerFormat;
    // ReSharper disable once InconsistentNaming
    self.selectedContentType = new dcc.contentTypeViewModel(self, config);

    var findContentTypes =  function() {
        self.pageIndex(0);
        self.getContentTypes();
    };

    var getAllContentTypes = function () {
        var params = {
            searchTerm: '',
            pageIndex: 0,
            pageSize: 1000
        };

        util.contentTypeService().getEntities("GetContentTypes",
            params,
            self.contentTypes,
            function () {
                // ReSharper disable once InconsistentNaming
                return new dcc.contentTypeViewModel(self, config);
            }
        );
    };

    var getDataTypes = function () {
        var params = {
            searchTerm: '',
            pageIndex: 0,
            pageSize: 1000
        };

        util.dataTypeService().getEntities("GetDataTypes",
            params,
            self.dataTypes,
            function () {
                // ReSharper disable once InconsistentNaming
                return new dcc.dataTypeViewModel(self, config);
            }
        );
    };

    self.addContentType = function(){
        self.mode("editType");
        self.selectedContentType.init();
    };

    self.closeEdit = function () {
        self.mode("listTypes");
        self.refresh();
    }

    self.editContentType = function(data) {
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

        util.contentTypeService().getEntity("GetContentType",
            params,
            self.selectedContentType);

        if(typeof cb === 'function') cb();
    };

    self.getContentTypes = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize()
        };

        util.contentTypeService().getEntities("GetContentTypes",
            params,
            self.results,
            function() {
                // ReSharper disable once InconsistentNaming
                return new dcc.contentTypeViewModel(self, config);
            },
            self.totalResults
        );
    };

    self.init = function() {
        // ReSharper disable once UseOfImplicitGlobalInFunctionScope
        dnn.koPager().init(self, config);
        self.searchText.subscribe(function () {
            findContentTypes();
        });
        self.pageSize.subscribe(function () {
            findContentTypes();
        });

        $rootElement.find("#contentTypes-editView").css("display", "none");

        getDataTypes();
        getAllContentTypes();
    };

    self.refresh = function() {
        self.getContentTypes();
        getAllContentTypes();
    }
};

dcc.contentTypeViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var ko = config.ko;

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.contentTypeId = ko.observable(-1);
    self.isSystem = ko.observable(false);
    self.selected = ko.observable(false);

    self.localizedNames = ko.observableArray([]);
    self.localizedDescriptions = ko.observableArray([]);

    // ReSharper disable once InconsistentNaming
    self.fields = ko.observable(new dcc.contentFieldsViewModel(self, config));
    self.fields().init();

    self.description = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedDescriptions());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedDescriptions(), value);
        }
    });

    self.isAddMode = ko.computed(function() {
        return self.contentTypeId() === -1;
    });

    self.name = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames(), value);
        }
    });

    var validate = function () {
        return util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedNames()) &&
            util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedDescriptions());
    };

    self.cancel = function(){
        self.rootViewModel.closeEdit();
    };

    self.deleteContentType = function (data) {
        util.confirm(resx.deleteContentTypeConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                contentTypeId: data.contentTypeId(),
                name: data.name(),
                isSystem: data.isSystem()
            };

            util.contentTypeService().post("DeleteContentType", params,
                function(){
                    //Success
                    parentViewModel.refresh();
                },

                function (xhr, status, err) {
                    util.handleServiceError(xhr, status, err);
                }
            );
        });
    };

    self.init = function(){
        self.canEdit(true);
        self.contentTypeId(-1);
        self.isSystem(self.parentViewModel.isSystemUser);

        util.initializeLocalizedValues(self.localizedNames, self.rootViewModel.languages());
        util.initializeLocalizedValues(self.localizedDescriptions, self.rootViewModel.languages());
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.created(data.created);
        self.contentTypeId(data.contentTypeId);
        self.isSystem(data.isSystem);

        util.loadLocalizedValues(self.localizedNames, data.localizedNames);
        util.loadLocalizedValues(self.localizedDescriptions, data.localizedDescriptions);

        if (data.contentFields != null) {
            self.fields().load(data.contentFields);
        }
    };

    self.saveContentType = function(data) {
        if(!validate()) {
            util.alert(resx.invalidContentTypeMessage, resx.ok);
        }
        else {
            var jsObject = ko.toJS(data);
            var params = {
                contentTypeId: jsObject.contentTypeId,
                localizedDescriptions: jsObject.localizedDescriptions,
                localizedNames: jsObject.localizedNames,
                isSystem: jsObject.isSystem
            };

            util.contentTypeService().post("SaveContentType", params,
                function(data) {
                    if (self.isAddMode()) {
                        util.alert(resx.saveContentTypeMessage.replace("{0}", util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames())), resx.ok, function () {
                            self.contentTypeId(data.id);
                            self.fields().clear();
                        });
                    } else {
                        self.cancel();
                    }
                },
                function (xhr, status, err) {
                    util.handleServiceError(xhr, status, err);
                }
            );
        }
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
    var ko = config.ko;

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.mode = config.mode;
    self.contentFieldsHeading = resx.contentFields;
    self.contentFields = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = ko.observable(999);
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.contentFields_PagerFormat;
    self.pager_NoPagerFormat = resx.contentFields_NoPagerFormat;
    // ReSharper disable once InconsistentNaming
    self.selectedContentField = new dcc.contentFieldViewModel(self, config);

    self.addContentField = function() {
        self.mode("editField");
        self.selectedContentField.init();
    }

    self.editContentField = function(data) {
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
        self.pageSize(settings.pageSize);
    };

    self.getContentField = function (contentTypeId, contentFieldId, cb) {
        var params = {
            contentTypeId: contentTypeId,
            contentFieldId: contentFieldId
        };
        util.contentTypeService().getEntity("GetContentField",
            params,
            self.selectedContentField);

        if(typeof cb === 'function') cb();
    };

    self.moveContentField = function(arg) {
        var params = {
            contentTypeId: parentViewModel.contentTypeId(),
            sourceIndex: arg.sourceIndex,
            targetIndex: arg.targetIndex
        };

        util.contentTypeService().post("MoveContentField", params,
            function () {
                //Success
                self.refresh();
            },

            function (xhr, status, err) {
                util.handleServiceError(xhr, status, err);
            }
        );

    };

    self.fixHelper = function (e, tr) {
        var $originals = tr.children();
        var $helper = tr.clone();
        $helper.children().each(function (index) {
            // Set helper cell sizes to match the original sizes
            $(this).width($originals.eq(index).width());
        });
        return $helper;
    },

    self.init = function() {
        // ReSharper disable once UseOfImplicitGlobalInFunctionScope
        dnn.koPager().init(self, config);
    };

    self.load = function(data) {
        self.contentFields.removeAll();

        for(var i=0; i < data.fields.length; i++){
            var result = data.fields[i];
            // ReSharper disable once InconsistentNaming
            var contentField = new dcc.contentFieldViewModel(self, config);
            contentField.load(result);
            self.contentFields.push(contentField);
        }
        self.totalResults(data.totalResults);
    };

    self.refresh = function() {
        var params = {
            contentTypeId: parentViewModel.contentTypeId,
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize()
        };

        util.contentTypeService().getEntities("GetContentFields",
            params,
            self.contentFields,
            function() {
                // ReSharper disable once InconsistentNaming
                return new dcc.contentFieldViewModel(self, config);
            },
            self.totalResults
        );
    };
}

dcc.contentFieldViewModel = function(parentViewModel, config) {
    var self = this;
    var resx = config.resx;
    var util = config.util;
    var ko = config.ko;

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.mode = config.mode;
    self.contentTypeId = ko.observable(-1);
    self.contentFieldId = ko.observable(-1);
    self.fieldTypeId = ko.observable("");
    self.selected = ko.observable(false);
    self.isList = ko.observable(false);

    self.localizedDescriptions = ko.observableArray([]);
    self.localizedLabels = ko.observableArray([]);
    self.localizedNames = ko.observableArray([]);

    self.isAddMode = ko.computed(function() {
        return self.contentFieldId() === -1;
    });

    self.description = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedDescriptions());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedDescriptions(), value);
        }
    });

    self.heading = ko.computed(function() {
        var heading = resx.contentField;
        if (!self.isAddMode()) {
            heading = heading + " - " + self.name();
        }
        return heading;
    });

    self.isContentType = ko.computed(function() {
        return self.fieldTypeId().substring(0, 1) === "C";
    });

    self.label = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedLabels());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedLabels(), value);
        }
    });

    self.name = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames(), value);
        }
    });

    self.fieldTypes = ko.computed(function () {
        var i, contentType, dataType;
        var contentTypes = parentViewModel.parentViewModel.parentViewModel.contentTypes();
        var dataTypes;
        var fieldTypes = [];

        fieldTypes.push({
            enabled: false,
            fieldTypeId: "C0",
            fieldName: resx.contentTypes
        });
        for (i = 0; i < contentTypes.length; i++) {
            contentType = contentTypes[i];
            if (contentType.contentTypeId() !== self.contentTypeId()) {
                fieldTypes.push({
                    enabled: true,
                    fieldTypeId: "C" + contentType.contentTypeId(),
                    fieldName: util.getLocalizedValue(self.rootViewModel.selectedLanguage(), contentType.localizedNames())
                });
            }
        }

        if (parentViewModel.parentViewModel.parentViewModel.dataTypes != null) {
            dataTypes = parentViewModel.parentViewModel.parentViewModel.dataTypes();
            fieldTypes.push({
                enabled: false,
                fieldTypeId: "D0",
                fieldName: resx.dataTypes
            });
            for (i = 0; i < dataTypes.length; i++) {
                dataType = dataTypes[i];
                fieldTypes.push({
                    enabled: true,
                    fieldTypeId: "D" + dataType.dataTypeId(),
                    fieldName: util.getLocalizedValue(self.rootViewModel.selectedLanguage(), dataType.localizedNames())
                });
            }
        }
        return fieldTypes;
    });

    self.fieldType = ko.computed(function () {
        var value = "";
        var entity = util.getEntity(self.fieldTypes(),
            function (fieldType) {
                return (self.fieldTypeId() === fieldType.fieldTypeId);
            });
        if (entity != null) {
            if (self.isList()) {
                value = resx.list + "<" + entity.fieldName + ">";
            } else {
                value = entity.fieldName;
            }
        }
        return value;
    });

    var validate = function () {
        return util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedNames()) &&
            util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedLabels()) &&
            util.hasDefaultValue(self.rootViewModel.defaultLanguage, self.localizedDescriptions());
    };

    self.cancel = function(){
        self.mode("editType");
        parentViewModel.refresh();
    };

    self.deleteContentField = function (data) {
        util.confirm(resx.deleteContentFieldConfirmMessage, resx.yes, resx.no, function () {

            var params = {
                contentFieldId: data.contentFieldId(),
                contentTypeId: data.contentTypeId(),
                name: data.name(),
                label: data.label(),
                description: data.description(),
                fieldTypeId: data.fieldTypeId().substring(1),
                isList: data.isList()
        };

            util.contentTypeService().post("DeleteContentField", params,
                function(){
                    //Success
                    parentViewModel.refresh();
                },

                function (xhr, status, err) {
                    util.handleServiceError(xhr, status, err);
                }
            );
        });
    };

    self.init = function() {
        self.contentFieldId(-1);
        self.contentTypeId(self.parentViewModel.parentViewModel.contentTypeId());
        self.fieldTypeId("");
        self.isList(false);

        util.initializeLocalizedValues(self.localizedNames, self.rootViewModel.languages());
        util.initializeLocalizedValues(self.localizedLabels, self.rootViewModel.languages());
        util.initializeLocalizedValues(self.localizedDescriptions, self.rootViewModel.languages());
    };

    self.load = function (data) {
        var fieldTypeId = (data.isReferenceType) ? "C" + data.fieldTypeId : "D" + data.fieldTypeId;
        self.contentFieldId(data.contentFieldId);
        self.contentTypeId(data.contentTypeId);
        self.fieldTypeId(fieldTypeId);
        self.isList(data.isList);

        util.loadLocalizedValues(self.localizedNames, data.localizedNames);
        util.loadLocalizedValues(self.localizedLabels, data.localizedLabels);
        util.loadLocalizedValues(self.localizedDescriptions, data.localizedDescriptions);
    }

    self.saveContentField = function(data) {
        if(!validate()) {
            util.alert(resx.invalidContentFieldMessage, resx.ok);
        }
        else {
            var jsObject = ko.toJS(data);
            var params = {
                contentFieldId: jsObject.contentFieldId,
                contentTypeId: jsObject.contentTypeId,
                localizedDescriptions: jsObject.localizedDescriptions,
                localizedNames: jsObject.localizedNames,
                localizedLabels: jsObject.localizedLabels,
                fieldTypeId: jsObject.fieldTypeId.substring(1),
                isReferenceType: jsObject.isContentType,
                isList: jsObject.isList
        };

            util.contentTypeService().post("SaveContentField", params,
                function() {
                    self.cancel();
                },
                function (xhr, status, err) {
                    util.handleServiceError(xhr, status, err);
                }
            );
        }
    };

    self.toggleSelected = function() {
        self.selected(!self.selected());
    };
}