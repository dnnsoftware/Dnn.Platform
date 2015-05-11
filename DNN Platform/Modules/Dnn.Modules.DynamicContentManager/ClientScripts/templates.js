if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.templates = (function(ko, resx, settings){
    var serviceFramework = settings.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager') + 'Template/';
    var pageSize = settings.pageSize;

    var templates = ko.observableArray([]);
    var heading = ko.observable(resx.templates);
    var searchText = ko.observable("");

    var findTemplates =  function() {
        getTemplates(searchText(), 0);
    };

    var getTemplates = function (searchTerm, pageIndex) {
        var self = this;
        $.ajax({
            type: "GET",
            cache: false,
            url: baseServicepath + "GetTemplates",
            beforeSend: serviceFramework.setModuleHeaders,
            data: {
                searchTerm: searchTerm,
                pageIndex: pageIndex,
                pageSize: pageSize
            }
        }).done(function (data) {
            if (typeof data !== "undefined" && data != null && data.success === true) {
                //Success
                load(data.data);
            } else {
                //Error
            }
        }).fail(function (xhr, status) {
            //Fail
        });
    };

    var load = function(data) {
        templates(data.templates);
    };

    return {
        templates: templates,
        heading: heading,
        searchText: searchText,
        findTemplates: findTemplates,
        getTemplates: getTemplates,
        load: load
    }
})
