'use strict';
define(['jquery'],
    function ($) {
        function loadScript() {
            var url = "Modules/ResourceManager/Scripts/dnn-resourcemanager/dnn-resource-manager.esm.js";
            //var url = "http://localhost:8099/ListingManager-bundle.js"; //Use this for dev
            $.ajax({
                dataType: "script",
                cache: true,
                url: url
            });
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