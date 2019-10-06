(function ($) {
    if (typeof dnn === 'undefined') dnn = {};
    dnn.logViewer = dnn.logViewer || {};

    //handle window unload event, when window in unload process, then don't remove the cookie.
    var windowUnloaded = false;
    $(window).on('beforeunload', function () {
        windowUnloaded = true;
    });
    
    dnn.logViewer.init = function () {
        if (windowUnloaded) {
            return;
        }
        var handler = this;
        var serviceFramework = $.ServicesFramework();
        var guid = dnn.dom.getCookie('LogGUID');
        if (!guid || guid.length <= 0) {
            return;
        }

        dnn.dom.deleteCookie('LogGUID');
        var servicePath = serviceFramework.getServiceRoot('InternalServices') + 'EventLogService/GetLogDetails';
        $('div[class*=dnnFormMessage][class*=dnnFormValidationSummary]').click(function() {
            $.ajax({
                type: "GET",
                url: servicePath,
                data: { guid: guid },
                beforeSend: serviceFramework.setModuleHeaders,
                success: function (data) {
                    var container = handler.getContainer();
                    container.html(data.Content).dialog({
                        modal: true,
                        autoOpen: true,
                        title: data.Title,
                        dialogClass: "dnnFormPopup",
                        position: "center",
                        width: 800,
                        height: 480,
                        resizable: false,
                        closeOnEscape: true
                    });
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        }).css('cursor', 'pointer');
    };

    dnn.logViewer.getContainer = function() {
        var container = $('div[class=errorWin]');
        if (container.length == 0) {
            $(document.body).append('<div class="errorWin"></div>');
        }

        return $('div[class=errorWin]');
    };

    $(document).ready(function() {
        dnn.logViewer.init.call(dnn.logViewer);
    });
}(jQuery));