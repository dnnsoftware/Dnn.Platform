if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.dataTypesViewModel = function(rootViewModel, config) {
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;
    var $rootElement = config.$rootElement;
    var ko = config.ko;

    self.rootViewModel = rootViewModel;

    self.isSystemUser = settings.isSystemUser;
    self.searchText = ko.observable("");
    self.results = ko.observableArray([]);
    self.totalResults = ko.observable(0);
    self.pageSize = ko.observable(settings.pageSize);
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
    // ReSharper disable once InconsistentNaming
    self.selectedDataType = new dcc.dataTypeViewModel(self, config);

    var findDataTypes =  function() {
        self.pageIndex(0);
        self.getDataTypes();
    };

    self.addDataType = function(event, ui){
        var tbody = $rootElement.find("#dataTypes-addbody");

        $(ui.target).fadeOut(200);
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
		$rootElement.find('a.dccButton').fadeIn(200);
		
        var row = $rootElement.find(e.target);

        if(row.is("tr") === false){
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
        util.dataTypeService().getEntity("GetDataType",
            params,
            self.selectedDataType);

        if(typeof cb === 'function') cb();
    };

    self.getDataTypes = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize()
        };
        util.dataTypeService().getEntities("GetDataTypes",
            params,
            self.results,
            function() {
                // ReSharper disable once InconsistentNaming
                return new dcc.dataTypeViewModel(self, config);
            },
            self.totalResults
        );
    };

    self.init = function() {
        $rootElement.find('#dataTypes-editrow > td > div').hide();
        // ReSharper disable once UseOfImplicitGlobalInFunctionScope
        dnn.koPager().init(self, config);

        self.searchText.subscribe(function () {
            findDataTypes();
        });
        self.pageSize.subscribe(function () {
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
    var ko = config.ko;

    self.parentViewModel = parentViewModel;
    self.rootViewModel = parentViewModel.rootViewModel;

    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.dataTypeId = ko.observable(-1);
    self.baseType = ko.observable(0);
    self.isSystem = ko.observable(false);
    self.localizedNames = ko.observableArray([]);

    self.isAddMode = ko.computed(function() {
        return self.dataTypeId() === -1;
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
        $rootElement.find("tr.in-edit-row").removeClass('in-edit-row');
        $rootElement.find('a.dccButton').fadeIn(200);
        $rootElement.find('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
            $rootElement.find('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            if(typeof cb === 'function') cb();
        });
    };

    var validate = function(){
        return util.hasDefaultValue(self.rootViewModel.defaultLanguage,self.localizedNames());
    };

    self.cancel = function() {
        collapseDetailRow();
    },

    self.deleteDataType = function (data) {
        util.confirm(resx.deleteDataTypeConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                dataTypeId: data.dataTypeId(),
                baseType: data.baseType(),
                name: data.name(),
                isSystem: data.isSystem()
            };

            util.dataTypeService().post("DeleteDataType", params,
                function(){
                    //Success
                    collapseDetailRow(parentViewModel.refresh);
                },

                function (xhr, status, err) {
                    //Failure
                    util.alert(status + ":" + err, resx.ok);
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

        util.loadLocalizedValues(self.localizedNames, data.localizedNames);
    };

    self.saveDataType = function(data) {
        if(!validate()) {
            util.alert(resx.invalidDataTypeMessage, resx.ok);

        }
        else {
            var jsObject = ko.toJS(data);
            var params = {
                dataTypeId: jsObject.dataTypeId,
                baseType: jsObject.baseType,
                localizedNames: jsObject.localizedNames,
                isSystem: jsObject.isSystem
            };

            util.dataTypeService().post("SaveDataType", params,
                function() {
                    collapseDetailRow(parentViewModel.refresh);
                },
                function (xhr, status, err) {
                    util.alert(status + ":" + err, resx.ok);
                }
            );
        }
    };
}

