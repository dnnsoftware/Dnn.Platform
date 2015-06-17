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
    };

    self.closeEdit = function() {
        self.mode("listTemplates");
        self.refresh();
    }

    self.editTemplate = function(data, e) {
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
    var $rootElement = config.$rootElement;

    self.parentViewModel = parentViewModel;
    self.canEdit = ko.observable(false);
    self.templateId = ko.observable(-1);
    self.name = ko.observable('');
    self.contentType = ko.observable('');
    self.contentTypeId = ko.observable(-1);
    self.isSystem = ko.observable(false);
    self.selected = ko.observable(false);

    self.cancel = function(){
        parentViewModel.closeEdit();
    };

    self.init = function(){
        self.canEdit(true);
        self.templateId(-1);
        self.name("");
        self.contentType("");
        self.contentTypeId(-1);
        self.isSystem(self.parentViewModel.isSystemUser);
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.templateId(data.templateId);
        self.name(data.name);
        self.contentType(data.contentType);
        self.contentTypeId(data.contentTypeId);
        self.isSystem(data.isSystem);
    };

    self.toggleSelected = function() {
        self.selected(!self.selected());
    };
}