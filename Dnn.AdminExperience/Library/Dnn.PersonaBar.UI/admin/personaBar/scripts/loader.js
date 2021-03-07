define(['jquery'], function ($) {
    var loadingbarId = '#personaBar-loadingbar';
    var loaded = false;
    var error = true;

    /* START persian-dnnsoftware */
    var defaultOptions;
    if (window.parent['personaBarSettings']['culture'] == 'fa-IR') {
        defaultOptions = {
            slowOperationMessage: "ما در حال پردازش داده‌های شما هستیم، لطفا صبر کنید ..."
        };
    }
    else {
        defaultOptions = {
            slowOperationMessage: "We are processing your content, please wait..."
        };
    }
    /* END persian-dnnsoftware */
    function loading(options) {
        if (window.dnn.loading) return;

        options = $.extend(defaultOptions, options);
        var loadingbar = $(loadingbarId);
        var progressbar = $(loadingbarId + ' > div');
        var width = loadingbar.width();
        var closeLoadingBar = $("#close-load-error");
        var loadingBarMessage = loadingbar.children('span');
        var slowConnectionTimeout = 4000;

        loadingbar.show();
        loadingBarMessage.html("");
        window.dnn.loading = true;
        closeLoadingBar.hide();

        function hideLoadingBar() {
            loadingbar.hide();
        }

        progressbar.removeClass().css({ width: 0, height: 5 }).stop(true, false).animate({ width: 0.75 * width }, 300, 'linear', function () {
            var pingDuration = 0;
            function checkloaded() {
                if (loaded) {
                    loaded = false;
                    window.dnn.loading = false;

                    clearTimeout(checkloaded);

                    checkloaded = null;
                    pingDuration = 0;

                    if (!error) {
                        progressbar.stop(true, false).animate({ width: "100%" }, 100, 'linear', hideLoadingBar);
                    } else {
                          /* START persian-dnnsoftware */
                        if (window.parent['personaBarSettings']['culture'] == 'fa-IR') {
                            loadingBarMessage.show().html("هنگام بازیابی داده‌ها خطایی روی داد. لطفا اتصال اینترنت خود را بررسی یا بخش لاگ‌ها چک کنید.");
                        }
                        else{
                            loadingBarMessage.show().html("There was an error retrieving your content. Please check your internet connection.");
                        }
                         /* END persian-dnnsoftware */
                        progressbar.stop(true, false).addClass("load-error").animate({ width: "100%" }, 100, 'linear').animate({ height: 25 }, 100, 'linear', function () {
                            closeLoadingBar.show().click(hideLoadingBar);
                        });
                    }
                }
                else {
                    pingDuration += 20;
                    if (pingDuration >= slowConnectionTimeout) {
                        var message = options.slowOperationMessage;
                        loadingBarMessage.show().html(message);
                        progressbar.animate({
                            height: 25
                        }, 'linear');
                    }
                    setTimeout(checkloaded, 20);
                }
            };
            checkloaded();
        });
    };

    return {
        startLoading: function (options) {
            loading(options);
        },
        stopLoading: function stopLoading(_error) {
            if (window.dnn.loading) {
                loaded = true;
                error = _error;
            }
        }
    }
});