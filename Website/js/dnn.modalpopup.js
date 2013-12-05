(function (window, $) {
    window.dnnModal = {

        load: function () {

            try {
                if (parent.location.href !== undefined) {

                    var windowTop = parent;
                    var parentTop = windowTop.parent;

                    if (typeof (parentTop.$find) != "undefined") {
                        if (location.href.indexOf('popUp') == -1 || windowTop.location.href.indexOf("popUp") > -1) {

                            var popup = windowTop.jQuery("#iPopUp");
                            var refresh = popup.dialog("option", "refresh");
                            var closingUrl = popup.dialog("option", "closingUrl");
                            var width = popup.dialog("option", "minWidth");
                            var height = popup.dialog("option", "minHeight");
                            var showReturn = popup.dialog("option", "showReturn");

                            if (!closingUrl) {
                                closingUrl = location.href;
                            }

                            if (popup.dialog('isOpen') === true) {

                                popup.dialog("option", {
                                    close: function (event, ui) {
                                        dnnModal.refreshPopup({
                                            url: closingUrl,
                                            width: width,
                                            height: height,
                                            showReturn: showReturn,
                                            refresh: refresh
                                        });
                                    }
                                }).dialog('close');
                            }
                        } else {
                            windowTop.jQuery("#iPopUp").dialog({ autoOpen: false, title: document.title });
                        }
                    }
                }
                return true;
            } catch (err) {
                return false;
            }
        },

        show: function (url, showReturn, height, width, refresh, closingUrl) {
            var $modal = $("#iPopUp");
            if ($modal.length == 0) {
                $modal = $("<iframe id=\"iPopUp\" src=\"about:blank\" scrolling=\"auto\" frameborder=\"0\"></iframe>");
                $(document.body).append($modal);
            } else {
                $modal.attr('src', 'about:blank');
            }
            $(document).find('html').css('overflow', 'hidden');

            $modal.dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "dnnFormPopup",
                position: "center",
                minWidth: width,
                minHeight: height,
                maxWidth: 1920,
                maxHeight: 1080,
                resizable: true,
                closeOnEscape: true,
                refresh: refresh,
                showReturn: showReturn,
                closingUrl: closingUrl,
                close: function (event, ui) { dnnModal.closePopUp(refresh, closingUrl); }
            })
                .width(width - 11)
                .height(height - 11);
            var mask = dnn.addIframeMask($(".ui-widget-overlay")[0]);
            if (mask != null) {
                mask.style.zIndex = 1;
            }
            if ($modal.parent().find('.ui-dialog-title').next('a.dnnModalCtrl').length === 0) {
                var $dnnModalCtrl = $('<a class="dnnModalCtrl"></a>');
                $modal.parent().find('.ui-dialog-titlebar-close').wrap($dnnModalCtrl);
                var $dnnToggleMax = $('<a href="#" class="dnnToggleMax"><span>Max</span></a>');
                $modal.parent().find('.ui-dialog-titlebar-close').before($dnnToggleMax);

                $dnnToggleMax.click(function (e) {
                    e.preventDefault();

                    var $window = $(window),
                        newHeight,
                        newWidth;

                    if ($modal.data('isMaximized')) {
                        newHeight = $modal.data('height');
                        newWidth = $modal.data('width');
                        $modal.data('isMaximized', false);
                    } else {
                        $modal.data('height', $modal.dialog("option", "minHeight"))
                            .data('width', $modal.dialog("option", "minWidth"))
                            .data('position', $modal.dialog("option", "position"));

                        newHeight = $window.height() - 46;
                        newWidth = $window.width() - 40;
                        $modal.data('isMaximized', true);
                    }

                    $modal.dialog({ height: newHeight, width: newWidth });
                    $modal.dialog({ position: 'center' });
                });
            }

            var showLoading = function () {
                var loading = $("<div class=\"dnnLoading\"></div>");
                loading.css({
                    width: $modal.width(),
                    height: $modal.height()
                });
                $modal.before(loading);
            };

            var hideLoading = function () {
                $modal.prev(".dnnLoading").remove();
            };

            showLoading();

            $modal[0].src = url;

            $modal.bind("load", function () {
                hideLoading();
            });

            if (showReturn.toString() == "true") {
                return false;
            }
        },

        closePopUp: function (refresh, url) {
            var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
            var popup = windowTop.jQuery("#iPopUp");

            if (typeof refresh === "undefined" || refresh == null) {
                refresh = true;
            }

            if (refresh.toString() == "true") {
                if (typeof url === "undefined" || url == "") {
                    url = windowTop.location.href;
                }

                windowTop.location.href = url;
                if ($(".ui-widget-overlay").length > 0) {
                    dnn.removeIframeMask($(".ui-widget-overlay")[0]);
                }
                popup.hide();
            } else {
                if ($(".ui-widget-overlay").length > 0) {
                    dnn.removeIframeMask($(".ui-widget-overlay")[0]);
                }
                popup.dialog('option', 'close', null).dialog('close');
            }
            $(windowTop.document).find('html').css('overflow', '');
        },

        refreshPopup: function (options) {
            var windowTop = parent;
            var windowTopTop = windowTop.parent;
            if (windowTop.location.href !== windowTopTop.location.href &&
                windowTop.location.href !== options.url) {
                windowTopTop.dnnModal.show(options.url, options.showReturn, options.height, options.width, options.refresh, options.closingUrl);
            } else {
                dnnModal.closePopUp(options.refresh, options.url);
            }
        }
    };

    window.dnnModal.load();
}(window, jQuery));