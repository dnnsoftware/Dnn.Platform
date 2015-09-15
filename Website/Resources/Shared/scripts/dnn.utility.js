if (typeof dnn === 'undefined' || dnn === null) {
    dnn = {};
};

dnn.utility = function (settings, resx) {

    var alertConfirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
        $('#confirmation-dialog > div.dnnDialog').html(text);
        $('#confirmation-dialog a#confirmbtn').html(confirmBtnText).unbind('click').bind('click', function() {
            if (typeof confirmHandler === 'function') confirmHandler.apply();
            $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
        });

        var $cancelBtn = $('#confirmation-dialog a#cancelbtn');
        if(cancelBtnText !== ''){
            $cancelBtn.html(cancelBtnText).unbind('click').bind('click', function() {
                if (typeof cancelHandler === 'function') cancelHandler.apply();
                $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
            });
            $cancelBtn.show();
        }
        else {
            $cancelBtn.hide();
        }

        $('#mask').show();
        $('#confirmation-dialog').fadeIn(200, 'linear');

        $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {

            if (evt.keyCode === 27) {
                $(window).off('keydown.confirmDialog');
                $('#confirmation-dialog a#cancelbtn').trigger('click');
            }
        });
    };

    var alert = function(text, closeBtnText, closeBtnHandler) {
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.alert);
        alertConfirm(text, closeBtnText, "", closeBtnHandler, null);
    };

    var asyncParallel = function(deferreds, callback) {
        var i = deferreds.length;
        if (i === 0) callback();
        var call = function() {
            i--;
            if (i === 0) {
                callback();
            }
        };

        $.each(deferreds, function(ii, d) {
            d(call);
        });
    };

    var confirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.confirm);
        alertConfirm(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler);
    };

    var initializeLocalizedValues = function(localizedValues, languages){
        localizedValues.removeAll();
        for(var i = 0; i < languages.length; i++) {
            var language = languages[i];
            var localizedValue = { code: ko.observable(language.code), value: ko.observable('')};
            localizedValues.push(localizedValue);
        }
    }
    
    var getEntity = function (values, predicate) {
        var value = null;
        for (var i = 0; i < values.length; i++) {
            if (predicate(values[i])) {
                value = values[i];
                break;
            }
        }
        return value;
    }
    
    var hasDefaultValue = function (defaultLanguage, localizedValues) {
        var value = true;
        for (var i = 0; i < localizedValues.length; i++) {
            var localizedValue = localizedValues[i];
            if (defaultLanguage === localizedValue.code() && localizedValue.value() === "") {
                value = false;
                break;
            }
        }
        return value;
    }

    var isTranslated = function (defaultLanguage, localizedValues) {
        var value = true;
        for (var i = 0; i < localizedValues.length; i++) {
            var localizedValue = localizedValues[i];
            if (defaultLanguage !== localizedValue.code() && localizedValue.value() === "") {
                value = false;
                break;
            }
        }
        return value;
    }

    var getLocalizationStatus = function (selectedLanguage, localizedValues, defaultMissingText, defaultLocalizedMissingText, translationMissingText) {
        var status = "";
        if(!hasDefaultValue(selectedLanguage, localizedValues)) {
            status = (localizedValues.length = 1) ? defaultMissingText : defaultLocalizedMissingText;
        }
        else if (!isTranslated(selectedLanguage, localizedValues)) {
            status = translationMissingText;
        }
        return status;
    };

    var getLocalizedValue = function(selectedLanguage, localizedValues) {
        var value = "";
        for (var i = 0; i < localizedValues.length; i++) {
            if (selectedLanguage === localizedValues[i].code()) {
                value = localizedValues[i].value();
                break;
            }
        }
        return value;
    }

    var loadLocalizedValues = function(localizedValues, data) {
        localizedValues.removeAll();
        for(var i = 0; i < data.length; i++) {
            var result = data[i];
            var localizedValue = { code: ko.observable(result.code), value: ko.observable(result.value)};
            localizedValues.push(localizedValue);
        }
    }

    var setlocalizedValue = function(selectedLanguage, localizedValues, value){
        for (var i = 0; i < localizedValues.length; i++) {
            if (selectedLanguage === localizedValues[i].code()) {
                localizedValues[i].value(value);
            }
        }
    }

    var sf = dnn.sf();
    sf.init(settings);

    return {
        alert: alert,
        asyncParallel: asyncParallel,
        confirm: confirm,
        getEntity: getEntity,
        getLocalizationStatus: getLocalizationStatus,
        getLocalizedValue: getLocalizedValue,
        hasDefaultValue: hasDefaultValue,
        isTranslated: isTranslated,
        initializeLocalizedValues: initializeLocalizedValues,
        loadLocalizedValues: loadLocalizedValues,
        setlocalizedValue: setlocalizedValue,
        sf: sf
    }
};

dnn.sf = function(){
    var isLoaded = false;
    var loadingBarId;
    var serviceController = "";
    var serviceFramework;
    var baseServicepath;

    var loadingBar = function (loadingBarId) {
        if (isLoaded) return;
        var loadingbar = $(loadingBarId);
        var progressbar = $(loadingBarId + ' > div');
        var width = loadingbar.width();
        loadingbar.show();
        progressbar.css({ width: 0 }).animate({ width: 0.75 * width }, 300, 'linear', function () {
            var checkloaded = function () {
                if (isLoaded) {
                    isLoaded = false;
                    clearTimeout(checkloaded);
                    checkloaded = null;
                    progressbar.animate({ width: width }, 100, 'linear', function () {
                        loadingbar.hide();
                    });
                }
                else {
                    setTimeout(checkloaded, 20);
                }
            };
            checkloaded();
        });
    };

    var call = function (httpMethod, url, params, onSuccess, onFailure, loading, sync, silence) {
        var options = {
            url: url,
            beforeSend: serviceFramework.setModuleHeaders,
            type: httpMethod,
            async: sync === false,
            success: function(data){
                if(loadingBarId && !silence) isLoaded = true;
                if(typeof loading === "function"){
                    loading(false);
                }

                if(typeof onSuccess === "function"){
                    onSuccess(data || {});
                }
            },
            error: function(xhr, status, err){
                if(loadingBarId && !silence) isLoaded = true;
                if(typeof loading === "function"){
                    loading(false);
                }

                if(typeof onFailure === "function"){
                    if(xhr){
                        onFailure(xhr, status, err);
                    }
                    else{
                        onFailure(null, "Unknown error", "");
                    }
                }
            }
        };

        if (httpMethod === "GET") {
            options.data = params;
        }
        else {
            options.contentType = "application/json; charset=UTF-8";
            options.data = JSON.stringify(params);
            options.dataType = "json";
        }

        if (typeof loading === "function") {
            loading(true);
        }

        if(loadingBarId && !silence) loadingBar(loadingBarId);
        return $.ajax(options);
    };

    var deleteInternal = function (method, params, onSuccess, onFailure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        return call("DELETE", url, params, onSuccess, onFailure, loading, false, false);
    };

    var get = function (method, params, onSuccess, onFailure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        return call("GET", url, params, onSuccess, onFailure, loading, false, false);
    };

    var getEntities = function (method, params, array, createEntity, updateTotal, onSuccess, onFailure) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        call("GET", url, params,
            function(data) {
                if (typeof data !== "undefined" && data != null) {
                    //Success
                    array.removeAll();
                    var results = data.results;
                    for(var i=0; i < results.length; i++){
                        var result = results[i];
                        var entity = createEntity();
                        entity.load(result);
                        array.push(entity);
                    }

                    if (typeof updateTotal === 'function') {
                        updateTotal(data.total);
                    }

                    if (typeof onSuccess === 'function') {
                        onSuccess();
                    }
                }
            },

            function(){
                //Failure
                if(typeof onFailure === 'function') onFailure();
            }
        );
    };

    var getEntity = function (method, params, entity, onSuccess, onFailure) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        call("GET", url, params,
            function(data) {
                if (typeof data !== "undefined" && data != null) {
                    //Success
                    entity.load(data);
                    if(typeof onSuccess === 'function') onSuccess();
                } 
            },

            function(){
                //Failure
                if(typeof onFailure === 'function') onFailure();
            }
        );
    };

    var init = function(settings){
        loadingBarId = settings.loadingBarId;
        serviceFramework = settings.servicesFramework;
        baseServicepath = serviceFramework.getServiceRoot(settings.servicePath);
    };

    var post = function (method, params, onSuccess, onFailure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        return call("POST", url, params, onSuccess, onFailure, loading, false, false);
    };

    var put = function (method, params, onSuccess, onFailure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + "/" + method;
        return call("PUT", url, params, onSuccess, onFailure, loading, false, false);
    };

    return {
        "delete": deleteInternal,
        get: get,
        getEntities: getEntities,
        getEntity: getEntity,
        init: init,
        post: post,
        put: put,
        serviceController: serviceController
    }
};
