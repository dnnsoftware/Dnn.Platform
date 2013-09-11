// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    dnn.social.ServiceCaller = function ServiceCaller ($, settings, controller) {
        var that = this;

        if (typeof ($.dnnSF) === 'undefined') {
            throw new Error('You must call ServicesFramework.Instance.RequestAjaxScriptSupport in your code-behind');
        }

        this.services = $.ServicesFramework(settings.moduleId);

        this.getRoot = function () {
            return that.services.getServiceRoot(settings.moduleRoot) + controller + '/';
        };

        this.getModuleId = function () {
            return that.services.getModuleId();
        };

        this.getTabId = function () {
            return that.services.getTabId();
        };

        this.privateCall =
            function (httpMethod, method, params, success, failure, loading, beforeSend, synchronous) {
                var beforeCallback;
                
                if (typeof beforeSend === 'function') {
                    beforeCallback = function (xhr) {
                        that.services.setModuleHeaders(xhr);

                        return beforeSend(xhr);
                    };
                }
                else {
                    beforeCallback = that.services.setModuleHeaders;
                }

                var options = {
                    url: '{0}{1}'.format(that.getRoot(), method),
                    beforeSend: beforeCallback,
                    type: httpMethod,
                    async: synchronous == false,
                    success: function (d) {
                        if (typeof loading === 'function') {
                            loading(false);
                        }

                        if (typeof success === 'function') {
                            success(d || {});
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        if (typeof loading === 'function') {
                            loading(false);
                        }

                        if (typeof failure === 'function') {
                            var message = undefined;

                            if (xhr != null) {
                                message = that.parseException(xhr.responseText);

                                failure(xhr, message || errorThrown);
                            } else {
                                failure(null, 'Unknown error');
                            }
                        }
                    }
                };

                if (httpMethod == 'GET') {
                    options.data = params;
                } else {
                    options.contentType = 'application/json; charset=UTF-8';
                    options.data = ko.toJSON(params);
                    options.dataType = 'json';
                }

                if (typeof loading === 'function') {
                    loading(true);
                }

                return $.ajax(options);
            };

        this.parseException = function (json) {
            try {
                var parsed = $.parseJSON(json) || { ExceptionMessage: 'Unknown error' };

                switch (typeof parsed) {
                case 'undefined':
                    break;
                case 'string':
                    return parsed;
                case 'object':
                    if (parsed.hasOwnProperty('ExceptionMessage')) {
                        return parsed.ExceptionMessage;
                    } else if (parsed.hasOwnProperty('Message')) {
                        return parsed.Message;
                    } else if (parsed.hasOwnProperty('Exception')) {
                        return parsed.Exception;
                    }
                    break;
                }
            }
            catch (e) { }
            
            return 'Unknown error';
        };

        this.post = function (method, params, success, failure, loading, beforeSend) {
            return that.privateCall('POST', method, params, success, failure, loading, beforeSend, false);
        };

        this.postsync = function (method, params, success, failure, loading, beforeSend) {
            return that.privateCall('POST', method, params, success, failure, loading, beforeSend, true);
        };

        this.get = function (method, params, success, failure, loading, beforeSend) {
            return that.privateCall('GET', method, params, success, failure, loading, beforeSend, false);
        };

        this.getsync = function (method, params, success, failure, loading, beforeSend) {
            return that.privateCall('GET', method, params, success, failure, loading, beforeSend, true);
        };
    };
})(window.dnn);