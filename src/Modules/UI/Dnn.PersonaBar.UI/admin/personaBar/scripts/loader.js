define(['jquery'], function ($) {
    var loadingbarId = '#personaBar-loadingbar';
    var loaded = false;

    function loading() {
        if (loaded) return;

        var loadingbar = $(loadingbarId);
        var progressbar = $(loadingbarId + ' > div');
        var width = loadingbar.width();
        loadingbar.show();
        progressbar.css({ width: 0 }).animate({ width: 0.75 * width }, 300, 'linear', function () {
            function checkloaded() {
                if (loaded) {
                    loaded = false;
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

    return {
        startLoading: loading,
        stopLoading: function stopLoading() {
            loaded = true;
        }
    }
});