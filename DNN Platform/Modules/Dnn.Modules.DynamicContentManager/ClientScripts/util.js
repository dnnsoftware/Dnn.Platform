if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.utility = function(settings){

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

    var sf = dcc.sf();
    sf.init(settings);

    return {
        asyncParallel: asyncParallel,
        sf: sf
    }
};

dcc.sf = function(){
    var isLoaded = false;
    var loadingBarId;
    var serviceController = "";
    var serviceFramework;
    var baseServicepath;

    var call = function(httpMethod, url, params, success, failure, loading, sync, silence){
        var options = {
            url: url,
            beforeSend: serviceFramework.setModuleHeaders,
            type: httpMethod,
            async: sync == false,
            success: function(data){
                if(loadingBarId && !silence) isLoaded = true;
                if(typeof loading === 'function'){
                    loading(false);
                }

                if(typeof success === 'function'){
                    success(data || {});
                }
            },
            error: function(xhr, status, err){
                if(loadingBarId && !silence) isLoaded = true;
                if(typeof loading === 'function'){
                    loading(false);
                }

                if(typeof failure === 'function'){
                    if(xhr){
                        failure(xhr, err);
                    }
                    else{
                        failure(null, 'Unknown error');
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

        if (typeof loading === 'function') {
            loading(true);
        }

        if(loadingBarId && !silence) loadingBar(loadingBarId);
        return $.ajax(options);
    };

    var get = function (method, params, success, failure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
        return call('GET', url, params, success, failure, loading, false, false);
    };

    var init = function(settings){
        loadingBarId = settings.loadingBarId;
        serviceFramework = settings.servicesFramework;
        baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager');
    };

    var loadingBar = function(loadingBarId){
        if(isLoaded) return;
        var loadingbar = $(loadingBarId);
        var progressbar = $(loadingBarId + ' > div');
        var width = loadingbar.width();
        loadingbar.show();
        progressbar.css({ width: 0 }).animate({ width: 0.75 * width }, 300, 'linear', function(){
            var checkloaded = function(){
                if(isLoaded){
                    isLoaded = false;
                    clearTimeout(checkloaded);
                    checkloaded = null;
                    progressbar.animate({ width: width }, 100, 'linear', function(){
                        loadingbar.hide();
                    });
                }
                else{
                    setTimeout(checkloaded, 20);
                }
            };
            checkloaded();
        });
    };

    var post = function (method, params, success, failure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
        return call('POST', url, params, success, failure, loading, false, false);
    };

    var setHeaders = function(xhr){
        if(tabId){
            xhr.setRequestHeader('TabId', tabId);
        }

        if(antiForgeryToken){
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
