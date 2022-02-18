'use strict';
define(['jquery'],
    function ($) {
        function loadScript() {
            var module = document.createElement("script");
            module.type = "module"
            module.src = "/DesktopModules/ResourceManager/Scripts/dnn-resource-manager/dnn-resource-manager.esm.js";
            document.head.appendChild(module);

            var noModule = document.createElement("script");
            noModule.setAttribute("nomodule", "");
            noModule.src = "/DesktopModules/ResourceManager/Scripts/dnn-resource-manager/dnn-resource-manager.js";
            document.head.appendChild(noModule);
            // var url = "/DesktopModules/ResourceManager/Scripts/dnn-resource-manager/dnn-resource-manager.js";
            // //var url = "http://localhost:8099/ListingManager-bundle.js"; //Use this for dev
            // $.ajax({
            //     dataType: "script",
            //     cache: true,
            //     url: url
            // });
        }
        return {
            init: function (wrapper, util, params, callback) {
                loadScript();
            },

            initMobile: function (wrapper, util, params, callback) {
                this.init(wrapper, util, params, callback);
            },

            load: function (params, callback) {
            },

            loadMobile: function (params, callback) {
            }
        };
    });