if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.templatesViewModel = function(config){
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;

    self.mode = config.mode;
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.templates_PagerFormat;
    self.pager_NoPagerFormat = resx.templates_NoPagerFormat;
    self.selectedTemplate = new dcc.templateViewModel(self, config);

    self.heading = ko.computed(function() {
        var heading = resx.templates;
        if(self.mode() != "listTemplates"){
            heading = resx.template + " - " + self.selectedTemplate.name()
        }
        return heading;
    });

    var findTemplates =  function() {
        self.pageIndex(0);
        self.getTemplates();
    };

    self.addTemplate = function(){
        self.mode("editTemplate");
        self.selectedTemplate.init();
        self.selectedTemplate.bindCodeEditor();
    };

    self.closeEdit = function() {
        self.mode("listTemplates");
        self.refresh();
    }

    self.editTemplate = function(data, e) {
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

        util.templateService().get("GetTemplate", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedTemplate.load(data.data.template);
                    self.selectedTemplate.bindCodeEditor();
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

    self.getTemplates = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize
        };

        util.templateService().get("GetTemplates", params,
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
            findTemplates();
        });

        $rootElement.find("#templates-editView").css("display", "none")
    };

    self.load =function(data) {
        self.results.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var template = new dcc.templateViewModel(self, config);
            template.init();
            template.load(result);
            self.results.push(template);
        }
        self.totalResults(data.totalResults)
    };

    self.refresh = function(){
        self.getTemplates();
    };
};

dcc.templateViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var settings = config.settings;
    var $rootElement = config.$rootElement;
    var codeEditor = config.codeEditor;

    self.parentViewModel = parentViewModel;
    self.canEdit = ko.observable(false);
    self.templateId = ko.observable(-1);
    self.name = ko.observable('');
    self.contentType = ko.observable('');
    self.contentTypeId = ko.observable(-1);
    self.filePath = ko.observable('');
    self.isSystem = ko.observable(false);
    self.content = ko.observable('');
    self.selected = ko.observable(false);
    self.contentTypes = ko.observableArray([]);

    self.name.subscribe(function(newValue) {
        if(self.filePath() === ""){
            self.filePath("Content Templates/" + newValue.replace(" ", "") + ".cshtml");
        }
    });

    var getContentTypes = function() {
        var params = {
            searchTerm: '',
            pageIndex: 0,
            pageSize: 1000
        };

        util.contentTypeService().get("GetContentTypes", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.contentTypes.removeAll();
                    for(var i = 0; i < data.data.results.length; i++){
                        var result = data.data.results[i];
                        self.contentTypes.push({
                            contentTypeId: result.contentTypeId,
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

    self.bindCodeEditor = function() {
        codeEditor.setValue(self.content());
    };

    self.cancel = function(){
        parentViewModel.closeEdit();
    };

    self.deleteTemplate = function (data, e) {
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
        self.templateId(-1);
        self.name("");
        self.contentType("");
        self.contentTypeId(-1);
        self.filePath('');
        self.isSystem(self.parentViewModel.isSystemUser);
        self.content('');

        getContentTypes();
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.templateId(data.templateId);
        self.name(data.name);
        self.contentType(data.contentType);
        self.contentTypeId(data.contentTypeId);
        self.isSystem(data.isSystem);
        self.filePath(data.filePath);
        self.content(data.content);
   };

    self.saveTemplate = function(data, e) {
        var params = {
            templateId: data.templateId(),
            name: data.name(),
            contentTypeId: data.contentTypeId(),
            isSystem: data.isSystem(),
            filePath: data.filePath(),
            content: codeEditor.getValue()
        };

        util.templateService().post("SaveTemplate", params,
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