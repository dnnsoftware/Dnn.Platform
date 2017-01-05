define(['jquery', 'knockout', 'main/extension', 'main/config', 'jquery-ui.min', 'dnn.jquery'], function ($, ko, ext, cf) {
    'use strict';
    window.ko = ko;

    var config = cf.init();
    function loadScript() {
        //var url = "http://localhost:8080/dist/pages-bundle.js"
        var url = "modules/dnn.pages/scripts/bundles/pages-bundle.js";
        $.ajax({
            dataType: "script",
            cache: true,
            url: url
        });
    }
    var init = function (wrapper, util, params, callback) {
        window.dnn.initPages = function initializePages() {
            return {
                utilities: util,
                moduleName: "Pages",
                config: config,
                viewName: params.viewName,
                viewParams: params.viewParams,
                settings: params.settings
            };
        };

        loadScript();

        if (typeof callback === 'function') {
            callback();
        }
    };

    var load = function (params, callback) {
        window.dnn.pages.load({
            viewName: params.viewName,
            viewParams: params.viewParams,
            settings: params.settings
        });
        if (window.dnn.pages.pageHierarchyManager &&
            window.dnn.pages.pageHierarchyManager._initialized) {
            window.dnn.pages.pageHierarchyManager._resizeContentContainer(true);
        }
        if (typeof callback === 'function') {
            callback();
        }
    };

    return {
        init: init,
        load: load
    };
});
