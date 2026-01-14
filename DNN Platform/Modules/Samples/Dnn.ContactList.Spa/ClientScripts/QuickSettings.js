var dnn = dnn || {};
dnn.modules = dnn.modules || {};
dnn.modules.spa = dnn.modules.spa || {};
dnn.modules.spa.dnnContactListSpa = dnn.modules.spa.dnnContactListSpa || {};

dnn.modules.spa.dnnContactListSpa.quickSettings = function (config, root, moduleId, ko) {
    var savedIsFormEnabled = config.isFormEnabled;
    var quickSettingsDispatcher = config.quickSettingsDispatcher;

    var viewModel = {
        isFormEnabled: ko.observable(savedIsFormEnabled)
    };

    var settings = {
        servicesFramework: $.ServicesFramework(moduleId)
    }
    var util = contactList.utility(settings, {});
    util.settingsService = function(){
        util.sf.serviceController = "Settings";
        return util.sf;
    };

    // The function dnnQuickSettings definded in 'ModuleActions.js' requires working with promises.
    var SaveSettings = function () {
        var deferred = $.Deferred();

        if (viewModel.isFormEnabled() == savedIsFormEnabled){
            deferred.resolve();    
        } else {
            var params = {
                isFormEnabled: viewModel.isFormEnabled()
            };

            util.settingsService().post("SaveSettings", params,
                function(data){
                    //Success
                    deferred.resolve();
                    savedIsFormEnabled = params.isFormEnabled;
                    quickSettingsDispatcher.notify(moduleId, quickSettingsDispatcher.eventTypes.SAVE, params);
                },

                function(data){
                    //Failure
                    deferred.reject();
                }
            );
        }
        
        return deferred.promise();
    };

    var CancelSettings = function () {
        var deferred = $.Deferred();
        viewModel.isFormEnabled(savedIsFormEnabled);
        deferred.resolve();

        quickSettingsDispatcher.notify(moduleId, quickSettingsDispatcher.eventTypes.CANCEL);
        return deferred.promise();
    };

    var init = function () {
        var $root = $("#"+ root);
        ko.applyBindings(viewModel, $root[0]);

        // dnnQuickSettings needs three parameters: moduleId, onSave and onCancel.
        // These two functions are associated to the save and cancel buttons and the
        // callbacks mechanism is based on promises.
        $(root).dnnQuickSettings({
            moduleId: moduleId,
            onSave: SaveSettings,
            onCancel: CancelSettings
        });
    }

    return {
        init: init
    }
};
