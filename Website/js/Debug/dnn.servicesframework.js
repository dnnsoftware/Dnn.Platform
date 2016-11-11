﻿(function ($) {
    $.dnnSF = function (moduleId) {
        // To avoid scope issues, use 'base' instead of 'this'
        // to reference this class from internal events and functions.
        var base = this;

        base.getServiceRoot = function (moduleName) {
            var serviceRoot = dnn.getVar("sf_siteRoot", "/");
            serviceRoot += "API/" + moduleName + "/";
            return serviceRoot;
        };

        base.getTabId = function () {
            return dnn.getVar("sf_tabId", -1);
        };

        base.getModuleId = function () {
            return moduleId;
        };

        base.setModuleHeaders = function (xhr) {
            var tabId = base.getTabId();
            if (tabId > -1) {
                xhr.setRequestHeader("ModuleId", base.getModuleId());
                xhr.setRequestHeader("TabId", tabId);
            }

            var afValue = base.getAntiForgeryValue();
            if (afValue) {
                xhr.setRequestHeader("RequestVerificationToken", afValue);
            }
        };

        base.getAntiForgeryKey = function () {
            return "__RequestVerificationToken";
        };

        base.getAntiForgeryValue = function () {
            return $('[name="__RequestVerificationToken"]').val();
        };

        return base;
    };

    $.ServicesFramework = function (moduleId) {
        return new $.dnnSF(moduleId);
    };

})(jQuery);
//----------------------------
