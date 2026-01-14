if (typeof contactList === 'undefined' || contactList === null) {
    contactList = {};
};

contactList.utility = function(settings, resx){

    var resx = resx;

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
        alertConfirm(text, closeBtnText, "", closeBtnHandler, null)
    };

    var confirm = function(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler) {
        $('#confirmation-dialog > div.dnnDialogHeader').html(resx.confirm);
        alertConfirm(text, confirmBtnText, cancelBtnText, confirmHandler, cancelHandler)
    };

    var sf = contactList.sf();
    sf.init(settings);

    return {
        alert: alert,
        confirm: confirm,
        sf: sf
    }
};

contactList.sf = function(){
    var serviceController = "";
    var serviceFramework;
    var baseServicepath;

    var call = function(httpMethod, url, params, success, failure){
        var options = {
            url: url,
            beforeSend: serviceFramework.setModuleHeaders,
            type: httpMethod,
            async: true,
            success: function(data){
                if(typeof success === 'function'){
                    success(data || {});
                }
            },
            error: function(xhr, status, err){
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

        return $.ajax(options);
    };

    var get = function (method, params, success, failure) {
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
        return call('GET', url, params, success, failure);
    };

    var init = function(settings){
        serviceFramework = settings.servicesFramework;
        baseServicepath = serviceFramework.getServiceRoot('Dnn/ContactList');
    };

    var post = function (method, params, success, failure, loading) {
        var self = this;
        var url = baseServicepath + self.serviceController + '/' + method;
        return call('POST', url, params, success, failure);
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
