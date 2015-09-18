if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.templatesViewModel = function(rootViewModel, config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;
    var ko = config.ko;

    self.rootViewModel = rootViewModel;

    self.contentTypes = ko.observableArray([]);

    self.mode = config.mode;
    self.isSystemUser = settings.isSystemUser;
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = ko.observable(settings.pageSize);
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.templates_PagerFormat;
    self.pager_NoPagerFormat = resx.templates_NoPagerFormat;
    // ReSharper disable once InconsistentNaming
    self.selectedTemplate = new dcc.templateViewModel(self, config);

    var findTemplates =  function() {
        self.pageIndex(0);
        self.getTemplates();
    };

    var getContentTypes = function () {
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

    self.addTemplate = function(){
        self.mode("editTemplate");
        self.selectedTemplate.init();
        self.selectedTemplate.bindCodeEditor();
    };

    self.editTemplate = function(data) {
        self.selectedTemplate.init();
        util.asyncParallel([
            function(cb1){
                self.getTemplate(data.templateId(), cb1);
            }
        ], function() {
            self.mode("editTemplate");
        });
    };

    self.getTemplate = function (templateId, cb) {
        var params = {
            templateId: templateId
        };
        util.templateService().getEntity("GetTemplate",
            params,
            self.selectedTemplate,
            function(){
                self.selectedTemplate.bindCodeEditor();
            }
        );

        if(typeof cb === 'function') cb();
    };

    self.getTemplates = function () {

        getContentTypes();

        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize()
        };

        util.templateService().getEntities("GetTemplates",
            params,
            self.results,
            function() {
                // ReSharper disable once InconsistentNaming
                return new dcc.templateViewModel(self, config);
            },
            self.totalResults
        );
    };

    self.init = function() {
        dnn.koPager().init(self, config);
        self.searchText.subscribe(function () {
            findTemplates();
        });
        self.pageSize.subscribe(function () {
            findTemplates();
        });
        $rootElement.find("#templates-editView").css("display", "none");
    };

    self.refresh = function(){
        self.getTemplates();
    };
};

dcc.templateViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var codeEditor = config.codeEditor;
    var ko = config.ko;

    var $rootElement = config.$rootElement;
    var $contextMenu = $rootElement.find("#templateEditorContextMenu");

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.canEdit = ko.observable(false);
    self.canSelectGlobal = ko.observable(false);
    self.templateId = ko.observable(-1);
    self.localizedNames = ko.observableArray([]);
    self.contentTypeId = ko.observable(-1);
    self.filePath = ko.observable('');
    self.isSystem = ko.observable(false);
    self.content = ko.observable('');
    self.selected = ko.observable(false);

    self.contentTypes = parentViewModel.contentTypes;
    self.codeSnippets = ko.observableArray([]);
    self.contentFields = ko.observableArray([]);

    self.isAddMode = ko.computed(function() {
        return self.templateId() === -1;
    });

    self.name = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames(), value);
        }
    });

    self.name.subscribe(function(newValue) {
        if (self.filePath() === "" && newValue !== "") {
            self.filePath("Content Templates/" + newValue.replace(/\s/g, "") + ".cshtml");
        }
    });

    self.contentTypeId.subscribe(function() {
        var isSystemType = false;
        var contentTypes = self.contentTypes();
        var contentTypeId = self.contentTypeId();
        for (var i = 0; i < contentTypes.length; i++) {
            var contentType = contentTypes[i];
            if (contentType.contentTypeId() === contentTypeId) {
                isSystemType = contentType.isSystem();
                break;
            }
        }
        self.canSelectGlobal(self.parentViewModel.isSystemUser && self.isAddMode() && isSystemType);
        self.isSystem(false);
    });

    self.contentType = ko.computed(function() {
        var value = "";
        if (self.contentTypes !== undefined) {
            var entity = util.getEntity(
                self.contentTypes(),
                function (contentType) {
                return (self.contentTypeId() === contentType.contentTypeId());
            });
            if (entity != null) {
                value = entity.name;
            }
        }
        return value;
    });

    var getCodeSnippets = function() {
        var params = { };

        util.templateService().get("GetSnippets", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.codeSnippets.removeAll();
                    for(var i = 0; i < data.data.results.length; i++){
                        var result = data.data.results[i];
                        self.codeSnippets.push({
                            name: result.name,
                            snippet: result.snippet
                        });
                    }
                }
            },

            function(){
                //Failure
            }
        );
    };

    var getContentFields = function () {
        if(self.contentTypeId() !== "undefined" && self.contentTypeId() > 0 && self.contentTypeId() !== self.previousContentTypeId){
            var params = {
                contentTypeId: self.contentTypeId()
            };

            util.contentTypeService().get("GetContentFields", params,
                function(data) {
                    if (typeof data !== "undefined" && data != null && data.success === true) {
                        //Success
                        self.contentFields.removeAll();
                        for(var i = 0; i < data.data.results.length; i++){
                            var result = data.data.results[i];
                            var localizedNames = ko.observableArray([]);
                            util.loadLocalizedValues(localizedNames, result.localizedNames);
                            var localizedDescriptions = ko.observableArray([]);
                            util.loadLocalizedValues(localizedDescriptions, result.localizedDescriptions);
                            var localizedLabels = ko.observableArray([]);
                            util.loadLocalizedValues(localizedLabels, result.localizedLabels);
                            self.contentFields.push({
                                contentTypeId: result.contentTypeId,
                                contentFieldId: result.contentFieldId,
                                name: util.getLocalizedValue(self.rootViewModel.selectedLanguage(), localizedNames()),
                                label: util.getLocalizedValue(self.rootViewModel.selectedLanguage(), localizedDescriptions()),
                                description: util.getLocalizedValue(self.rootViewModel.selectedLanguage(), localizedLabels())
                            });
                        }
                    }
                },

                function(){
                    //Failure
                }
            );

            self.previousContentTypeId = self.contentTypeId();
        }
    };

    var validate = function(){
        return util.hasDefaultValue(self.rootViewModel.defaultLanguage,self.localizedNames());
    };

    self.bindCodeEditor = function () {
        codeEditor.setValue(self.content());

        util.onVisible($rootElement.find(".CodeMirror"), 250, function () {
            codeEditor.refresh();
        });
    };

    self.cancel = function(){
        self.rootViewModel.closeEdit();
    };

    self.deleteTemplate = function (data) {
        util.confirm(resx.deleteTemplateConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                templateId: data.templateId(),
                name: data.name(),
                contentTypeId: data.contentTypeId(),
                isSystem: data.isSystem(),
                filePath: data.filePath(),
                content: codeEditor.getValue()
            };

            util.templateService().post("DeleteTemplate", params,
                function(){
                    //Success
                    parentViewModel.refresh();
                },

                function(){
                    //Failure
                }
            );
        });
    };

    self.init = function(){
        self.canEdit(true);
        self.templateId(-1);
        self.contentTypeId(-1);
        self.filePath('');
        self.isSystem(false);
        self.content('');

        util.initializeLocalizedValues(self.localizedNames, self.rootViewModel.languages());

        self.contentTypeId.subscribe(function () {
            getContentFields();
        });

        getCodeSnippets();
    };

    self.insertField = function(data) {
        var doc = codeEditor.doc;
        doc.replaceSelection("@Dnn.DisplayFor(\""+ data.name + "\")");
        $contextMenu.hide();
    };

    self.inserSnippet = function(data) {
        var doc = codeEditor.doc;
        doc.replaceSelection(data.snippet);
        $contextMenu.hide();
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.templateId(data.templateId);
        self.contentTypeId(data.contentTypeId);
        self.isSystem(data.isSystem);
        self.filePath(data.filePath);
        self.content(data.content);

        util.loadLocalizedValues(self.localizedNames, data.localizedNames);
    };

    self.saveTemplate = function(data) {
        if(!validate()) {
            util.alert(resx.invalidTemplateMessage, resx.ok);

        }
        else {
            var jsObject = ko.toJS(data);
            var params = {
                templateId: jsObject.templateId,
                localizedNames: jsObject.localizedNames,
                contentTypeId: jsObject.contentTypeId,
                isSystem: jsObject.isSystem,
                filePath: jsObject.filePath,
                content: codeEditor.getValue()
            };

            util.templateService().post("SaveTemplate", params,
            function (data) {
                if (data.success === true) {
                    //Success
                    self.cancel();
                }
                else {
                    //Error
                    util.alert(data.message, resx.ok);
                }
                },
            function () {
                    //Failure
                }
        );
        }
    };

    self.toggleSelected = function() {
        self.selected(!self.selected());
    };

    var $codeEditor = $rootElement.find(".CodeMirror");
    $codeEditor.bind("contextmenu", function (event) {
        event.preventDefault();

        var cursorLocation = codeEditor.cursorCoords();

        $contextMenu.show();
        $contextMenu.offset({ top: cursorLocation.top, left: cursorLocation.left });

        return false;
    });

    codeEditor.on("mousedown", function() {
        $contextMenu.hide();
    });
}