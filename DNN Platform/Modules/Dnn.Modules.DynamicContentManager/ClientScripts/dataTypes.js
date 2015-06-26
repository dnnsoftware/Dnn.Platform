if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.dataTypesViewModel = function(config, rootViewModel) {
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;

    self.rootViewModel = rootViewModel;

    self.isSystemUser = settings.isSystemUser;
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = settings.pageSize;
    self.pager_PageDesc = resx.pager_PageDesc;
    self.pager_PagerFormat = resx.dataTypes_PagerFormat;
    self.pager_NoPagerFormat = resx.dataTypes_NoPagerFormat;
    self.baseDataTypeOptions = ko.observableArray([
        { name: resx.string, value:0},
        { name: resx.bool, value:1},
        { name: resx.integer, value:2},
        { name: resx.float, value:3},
        { name: resx.bytes, value:4},
        { name: resx.guid, value:5},
        { name: resx.uri, value:6},
        { name: resx.dateTime, value:7},
        { name: resx.timeSpan, value:8}
    ]);
    self.selectedDataType = new dcc.dataTypeViewModel(self, config);

    var findDataTypes =  function() {
        self.pageIndex(0);
        self.getDataTypes();
    };

    self.addDataType = function(){
        var tbody = $rootElement.find("#dataTypes-addbody");

        util.asyncParallel([
            function(cb1){
                self.selectedDataType.init();
                cb1();
            }            ,
            function(cb2){
                $rootElement.find('#dataTypes-editrow > td > div').slideUp(200, 'linear', function(){
                    cb2();
                });
            }
        ], function() {
            $rootElement.find('#dataTypes-editrow').appendTo(tbody);
            $rootElement.find('#dataTypes-editrow > td > div').slideDown(400, 'linear');
        });
    };

    self.editDataType = function(data, e){
        var row = $rootElement.find(e.target);

        if(row.is("tr") == false){
            row = row.closest('tr');
        }

        if(row.hasClass('in-edit-row')){
            row.removeClass('in-edit-row');
            $rootElement.find('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
                $rootElement.find('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            });
            return;
        }

        var tbody = row.parent();
        $rootElement.find('tr', tbody).removeClass('in-edit-row');
        row.addClass('in-edit-row');

        util.asyncParallel([
            function(cb1){
                self.getDataType(data.dataTypeId(), cb1);
            },
            function(cb2){
                $rootElement.find('#dataTypes-editrow > td > div').slideUp(200, 'linear', function(){
                    cb2();
                });
            }
        ], function() {
            $rootElement.find('#dataTypes-editrow').insertAfter(row);
            $rootElement.find('#dataTypes-editrow > td > div').slideDown(400, 'linear');
        });
    };

    self.getDataType = function (dataTypeId, cb) {
        var params = {
            dataTypeId: dataTypeId
        };
        util.dataTypeService().getEntity(params, "GetDataType", self.selectedDataType);

        if(typeof cb === 'function') cb();
    };

    self.getDataTypes = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize
        };
        util.dataTypeService().getEntities(params,
            "GetDataTypes",
            self.results,
            function() {
                return new dcc.dataTypeViewModel(self, config);
            },
            self.totalResults
        );
    };

    self.init = function() {
        $rootElement.find('#dataTypes-editrow > td > div').hide();
        dcc.pager().init(self);

        self.searchText.subscribe(function () {
            findDataTypes();
        });
    };

    self.refresh = function() {
        self.getDataTypes();
    };
}

dcc.dataTypeViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    var $rootElement = config.$rootElement;

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.dataTypeId = ko.observable(-1);
    self.baseType = ko.observable(0);
    self.isSystem = ko.observable(false);
    self.localizedNames = ko.observableArray([]);

    self.isAddMode = ko.computed(function() {
        return self.dataTypeId() == -1;
    });

    self.name = ko.computed({
        read: function () {
            return util.getLocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames());
        },
        write: function(value) {
            util.setlocalizedValue(self.rootViewModel.selectedLanguage(), self.localizedNames(), value);
        }
    });

    var collapseDetailRow = function(cb) {
        $rootElement.find("tr.in-edit-row").removeClass('in-edit-row')
        $rootElement.find('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
            $rootElement.find('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            if(typeof cb === 'function') cb();
        });
    };

    self.cancel = function(data, e) {
        collapseDetailRow();
    },

    self.deleteDataType = function (data, e) {
        util.confirm(resx.deleteDataTypeConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                dataTypeId: data.dataTypeId(),
                baseType: data.baseType(),
                name: data.name(),
                isSystem: data.isSystem()
            };

            util.dataTypeService().post("DeleteDataType", params,
                function(data){
                    //Success
                    collapseDetailRow(parentViewModel.refresh);
                },

                function(data){
                    //Failure
                }
            );
        });
    };

    self.init = function() {
        self.dataTypeId(-1);
        self.canEdit(false);
        self.baseType(0);
        self.isSystem(false);

        util.initializeLocalizedValues(self.localizedNames, self.rootViewModel.languages());
    };

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.created(data.created);
        self.dataTypeId(data.dataTypeId);
        self.baseType(data.baseType);
        self.isSystem(data.isSystem);

        util.loadLocalizedValues(self.localizedNames, data.localizedNames)
    };

    self.saveDataType = function(data, e) {
        var jsObject = ko.toJS(data);
        var params = {
            dataTypeId: jsObject.dataTypeId,
            baseType: jsObject.baseType,
            localizedNames: jsObject.localizedNames,
            isSystem: jsObject.isSystem
        };

        util.dataTypeService().post("SaveDataType", params,
            function(data){
                //Success
                collapseDetailRow(parentViewModel.refresh);
            },

            function(data){
                //Failure
            }
        )

    };
}

