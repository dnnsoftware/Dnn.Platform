if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.dataTypes = function(ko, parentViewModel, resx, settings, util){
    var serviceFramework = settings.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager') + 'DataType/';

    var viewModel = {
        heading: ko.observable(resx.dataTypes),
        searchText: ko.observable(""),
        results: ko.observableArray([]),
        totalResults: ko.observable(0),
        pageSize: settings.pageSize,
        pager_PageDesc: resx.pager_PageDesc,
        pager_PagerFormat: resx.dataTypes_PagerFormat,
        pager_NoPagerFormat: resx.dataTypes_NoPagerFormat,
        selectedDataType: {
            dataTypeId: ko.observable(-1),
            baseType: ko.observable(0),
            name: ko.observable(''),
            isSystem: ko.observable(false),

            cancel: function(data, e) {
                collapseDetailRow();
            },

            deleteType: function (data, e) {
                var params = {
                    dataTypeId: data.dataTypeId(),
                    baseType: data.baseType(),
                    name: data.name(),
                    isSystem: data.isSystem()
                };

                util.sf.serviceController = "DataType";
                util.sf.post("DeleteDataType", params,
                    function(data){
                        //Success
                        collapseDetailRow(viewModel.refresh);
                    },

                    function(data){
                        //Failure
                    }
                )
            },

            saveType: function(data, e) {
                var params = {
                    dataTypeId: data.dataTypeId(),
                    baseType: data.baseType(),
                    name: data.name(),
                    isSystem: data.isSystem()
                };

                util.sf.serviceController = "DataType";
                util.sf.post("SaveDataType", params,
                    function(data){
                        //Success
                        collapseDetailRow(viewModel.refresh);
                    },

                    function(data){
                        //Failure
                    }
                )

            }
        },

        expand: function(data, e){
            viewDetail(data, e.target);
            e.preventDefault();
        },

        getDataType: function (dataTypeId, cb) {
            var params = {
                dataTypeId: dataTypeId
            };

            util.sf.serviceController = "DataType";
            util.sf.get("GetDataType", params,
                function(data) {
                    if (typeof data !== "undefined" && data != null && data.success === true) {
                        //Success
                        var dataType = data.data.dataType;
                        viewModel.selectedDataType.dataTypeId(dataType.dataTypeId);
                        viewModel.selectedDataType.baseType(dataType.baseType);
                        viewModel.selectedDataType.name(dataType.name);
                        viewModel.selectedDataType.isSystem(dataType.isSystem);
                    } else {
                        //Error
                    }
                },

                function(){
                    //Failure
                }
            );

            if(typeof cb === 'function') cb();
        },

        getDataTypes: function () {
            var params = {
                searchTerm: viewModel.searchText(),
                pageIndex: viewModel.pageIndex(),
                pageSize: viewModel.pageSize
            };

            util.sf.serviceController = "DataType";
            util.sf.get("GetDataTypes", params,
                function(data) {
                    if (typeof data !== "undefined" && data != null && data.success === true) {
                        //Success
                        viewModel.load(data.data);
                    } else {
                        //Error
                    }
                },

                function(){
                    //Failure
                }
            );
        },

        init: function() {
            $('#dataTypes-editrow > td > div').hide();
            dcc.pager().init(viewModel);

            viewModel.searchText.subscribe(function () {
                findDataTypes();
            });
        },

        load: function(data) {
            viewModel.results(data.results);
            viewModel.totalResults(data.totalResults)
        },

        refresh: function() {
            viewModel.getDataTypes();
        }
    };

    var collapseDetailRow = function(cb) {
        $("tr.in-edit-row").removeClass('in-edit-row')
        $('#dataTypes-editrow > td > div').slideUp(600, 'linear', function(){
            $('#dataTypes-editrow').appendTo('#dataTypes-editbody');
            if(typeof cb === 'function') cb();
        });
    }

    var findDataTypes =  function() {
        viewModel.pageIndex(0);
        viewModel.getDataTypes();
    };

    var viewDetail = function(dataType, target){
        var row = $(target);

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
                viewModel.getDataType(dataType.dataTypeId, cb1);
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

    parentViewModel.dataTypes = viewModel;

    return;
};