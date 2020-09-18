(function($) {
    return {
        addModuleHandler: function(moduleId, moduleName) {
            $('.dnnModuleDialog').on('click', ' .dnnModuleItem[data-moduleid=' + moduleId + ']', function() {
                window.forceLoadScriptsInSingleMode = true;
            });
        }
    };
})(jQuery);