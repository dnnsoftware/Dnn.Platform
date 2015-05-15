if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.dataTypesViewModel = function(config) {
    var self = this;
    var resx = config.resx;
    var settings = config.settings;
    var util = config.util;

    util.dataTypeService = function(){
        util.sf.serviceController = "DataType";
        return util.sf;
    };

    self.isSystemUser = settings.isSystemUser;
    self.heading = ko.observable(resx.dataTypes);
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
        var tbody = $("#dataTypes-addbody");

        util.asyncParallel([
            function(cb1){
                self.selectedDataType.load( {
                    canEdit: false,
                    baseType: 0,
                    dataTypeId: -1,
                    isSystem: false,
                    name: ""
                });
                cb1();
            }            ,
            function(cb2){
                $('#dataTypes-editrow > td > div').slideUp(200, 'linear', function(){
                    cb2();
                });
            }
        ], function() {
            $('#dataTypes-editrow').appendTo(tbody);
            $('#dataTypes-editrow > td > div').slideDown(400, 'linear');
        });
    };

    self.editDataType = function(data, e){
        var row = $(e.target);

        if(row.is("tr") == false){
            row = row.closest('tr');
        }

        if(row.hasClass('in-edit-row')){
            row.removeClass('in-edit-row');
            $('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
                $('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            });
            return;
        }

        var tbody = row.parent();
        $('tr', tbody).removeClass('in-edit-row');
        row.addClass('in-edit-row');

        util.asyncParallel([
            function(cb1){
                self.getDataType(data.dataTypeId(), cb1);
            },
            function(cb2){
                $('#dataTypes-editrow > td > div').slideUp(200, 'linear', function(){
                    cb2();
                });
            }
        ], function() {
            $('#dataTypes-editrow').insertAfter(row);
            $('#dataTypes-editrow > td > div').slideDown(400, 'linear');
        });
    };

    self.getDataType = function (dataTypeId, cb) {
        var params = {
            dataTypeId: dataTypeId
        };

        util.dataTypeService().get("GetDataType", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedDataType.load(data.data.dataType);
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

    self.getDataTypes = function () {
        var params = {
            searchTerm: self.searchText(),
            pageIndex: self.pageIndex(),
            pageSize: self.pageSize
        };

        util.dataTypeService().get("GetDataTypes", params,
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
        $('#dataTypes-editrow > td > div').hide();
        dcc.pager().init(self);

        self.searchText.subscribe(function () {
            findDataTypes();
        });
    };

    self.load = function(data) {
        self.results.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var dataType = new dcc.dataTypeViewModel(self, config);
            dataType.load(result);
            self.results.push(dataType);
        }
        self.totalResults(data.totalResults)
    };

    self.refresh = function() {
        self.getDataTypes();
    };
}

dcc.dataTypeViewModel = function(parentViewModel, config){
    var self = this;
    var util = config.util;
    var resx = config.resx;
    self.parentViewModel = parentViewModel;
    self.canEdit = ko.observable(false);
    self.created = ko.observable('');
    self.dataTypeId = ko.observable(-1);
    self.baseType = ko.observable(0);
    self.name = ko.observable('');
    self.isSystem = ko.observable(false);

    self.isAddMode = ko.computed(function() {
        return self.dataTypeId() == -1;
    });

    var collapseDetailRow = function(cb) {
        $("tr.in-edit-row").removeClass('in-edit-row')
        $('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
            $('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            if(typeof cb === 'function') cb();
        });
    };

    self.cancel = function(data, e) {
        collapseDetailRow();
    },

    self.deleteType = function (data, e) {
        util.confirm(resx.deleteDataTypeConfirmMessage, resx.yes, resx.no, function() {
            var params = {
                dataTypeId: data.dataTypeId(),
                baseType: data.baseType(),
                name: data.name(),
                isSystem: data.isSystem()
            };

            util.dataTypeService().sf.post("DeleteDataType", params,
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

    self.load = function(data) {
        self.canEdit(data.canEdit);
        self.created(data.created);
        self.dataTypeId(data.dataTypeId);
        self.baseType(data.baseType);
        self.name(data.name);
        self.isSystem(data.isSystem);
    };

    self.saveType = function(data, e) {
        var params = {
            dataTypeId: data.dataTypeId(),
            baseType: data.baseType(),
            name: data.name(),
            isSystem: data.isSystem()
        };

        util.dataTypeService().sf.post("SaveDataType", params,
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

