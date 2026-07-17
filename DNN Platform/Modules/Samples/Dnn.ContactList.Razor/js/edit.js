jQuery(document).ready(function ($) {
    var $editContact = $('.contactEdit-container');
    var moduleId = $editContact.attr('data-moduleid');
    var returnUrl = $editContact.attr('data-returnurl');
    
    $('#btnSave').click(function () {
        var params = getData();
        sf.post("SaveContact", params,
            function (data) {
                //Success
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    window.location.href = returnUrl;
                }
                else {
                    //Failure
                    alert(data.message);
                }
            },
            function (data) {
                //Failure
            }
        )
        return false;
    });

    var getData = function () {
        var data = {};
        $editContact.find("input, select, textarea").each(function () {
            data[$(this).attr("name")] = $(this).val();
        });
        return data;
    }

    var services = function () {
        var serviceController = "Contact";
        var serviceFramework;
        var baseServicepath;

        var call = function (httpMethod, url, params, success, failure) {
            var options = {
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                type: httpMethod,
                async: true,
                success: function (data) {
                    if (typeof success === 'function') {
                        success(data || {});
                    }
                },
                error: function (xhr, status, err) {
                    if (typeof failure === 'function') {
                        if (xhr) {
                            failure(xhr, err);
                        }
                        else {
                            failure(null, 'Unknown error');
                        }
                    } else {
                        if (xhr) {
                            console.error(xhr, err);
                            alert((xhr.responseJSON && xhr.responseJSON.Message) || err);
                        } else {
                            console.error('Unknown error');
                            alert('Unknown error');
                        }
                    }
                }
            };

            if (httpMethod == 'GET') {
                options.data = params;
            }
            else {
                options.contentType = 'application/json; charset=UTF-8';
                options.data = JSON.stringify(params);
                options.dataType = 'json';
            }

            return $.ajax(options);
        };

        var get = function (method, params, success, failure) {
            var self = this;
            var url = baseServicepath + self.serviceController + '/' + method;
            return call('GET', url, params, success, failure);
        };

        var init = function (settings) {
            serviceFramework = settings.servicesFramework;
            baseServicepath = serviceFramework.getServiceRoot('Dnn/RazorContactList');
        };

        var post = function (method, params, success, failure, loading) {
            var self = this;
            var url = baseServicepath + self.serviceController + '/' + method;
            return call('POST', url, params, success, failure);
        };

        var setHeaders = function (xhr) {
            if (tabId) {
                xhr.setRequestHeader('TabId', tabId);
            }

            if (antiForgeryToken) {
                xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
            }
        };

        return {
            get: get,
            init: init,
            post: post,
            serviceController: serviceController
        }
    };

    var sf = new services();
    sf.init({ servicesFramework: $.ServicesFramework(moduleId) })

});

