var GettingStartedNS = {};
GettingStartedNS.markAsShown = function (action, url) {
    var path = pageCurrentDomainUrl + '/Default.aspx/SetGettingStartedPageAsShown';
    $.ajax({
       type: 'POST',
       url: path,
       data: '{ portailId: ' + pageCurrentPortalId + '}',
       async: false,
       contentType: "application/json; charset=utf-8",
       dataType: "json",
       success: function (result) {
           if (result.d) {
               if (action == 'goto' && window.location.href.indexOf('popUp') > -1) {
                   window.location.href = url;
               } else if (action == 'show' && window.location.href.indexOf('popUp') == -1) {
                   dnnModal.show(url + '?popUp=true', /*showReturn*/false, 550, 950, true, '');
               } else {
                   window.location.href = url;
               }
           }
       },
       error: function () {
           
       }
    });
    return false;
};

GettingStartedNS.deleteBeforeCloseEvent = function () {
    var dialog = parent.$('.ui-dialog:visible'); //this object remains shown when the confirm dialog appears

    if (dialog != null) {
        dialog.unbind('dialogbeforeclose');
    }
};

GettingStartedNS.showModal = function (url) {
    GettingStartedNS.markAsShown('show', url);
    GettingStartedNS.deleteBeforeCloseEvent();
};
GettingStartedNS.goTo = function (url) {
    GettingStartedNS.markAsShown('goto', url);
    GettingStartedNS.deleteBeforeCloseEvent();
};

GettingStartedNS.addBeforeCloseEvent = function (url) {
    var dialog = parent.$('.ui-dialog:visible'); //this object remains shown when the confirm dialog appears

    if (dialog != null) {
        dialog.bind('dialogbeforeclose', function (event, ui) {
            GettingStartedNS.goTo(url);
        });
    }
};

GettingStartedNS.setDialogTitle = function(pageTitle) {
    parent.$('#iPopUp').dialog({ title: pageTitle});
};

$(document).ready(function () {    
        GettingStartedNS.addBeforeCloseEvent(pageCurrentPortalAliasUrl);
        GettingStartedNS.setDialogTitle(gettingStartedPageTitle);
});
