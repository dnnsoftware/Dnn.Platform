// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

﻿if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
﻿if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    ///dnnModuleService

    var dnnModuleService = dnn.dnnModuleService = function (options) {
        this.options = options;
        this.init();
    };

    dnnModuleService.prototype = {
        init: function () {
            this.options = $.extend({}, dnnModuleService.defaultOptions, this.options);

            this._service = $.dnnSF(this.options.moduleId);
        },

        isLoading: function () {
            return this._loading;
        },

        request: function (method, type, params, successCallback, errorCallback) {
            var handler = this;
            $.ajax({
                url: this._getServiceUrl() + method,
                type: type,
                data: params,
                async: this.options.async,
                beforeSend: $.proxy(this._beforeSend, this),
                complete: $.proxy(this._completeRequest, this),
                success: function (data) {
                    if (typeof successCallback == "function") {
                        successCallback(data);
                    }
                },
                error: function (xhr) {
                    if (xhr && xhr.status == '401') {
                        var loginUrl = dnn.getVar('cem_loginurl');
                        if (typeof window.dnnModal != "undefined") {
                            window.dnnModal.show(loginUrl + (loginUrl.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
                        } else {
                            location.href = loginUrl;
                        }

                        return;
                    }

                    if (typeof errorCallback == "function") {
                        errorCallback(xhr);
                        return;
                    }

                    $.dnnAlert({
                        title: 'Error',
                        text: 'Error occurred when request service \'' + method + '\'.'
                    });
                }
            });
        },

        getTabId: function() {
            return this._service.getTabId();
        },

        _beforeSend: function (xhr) {
            this._service.setModuleHeaders(xhr);
            this._loading = true;
        },

        _completeRequest: function (xhr, status) {
            this._loading = false;
        },

        _getServiceUrl: function () {
            return this._service.getServiceRoot(this.options.service) + this.options.controller + '/';
        },
    };

    dnnModuleService.defaultOptions = {
        async: true
    };

	///dnnModuleService END
}(jQuery));
