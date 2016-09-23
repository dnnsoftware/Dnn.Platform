(function ($) {
    $(document.body).ready(function () {
        var loadEditBar = function () {
            var w = window,
                d = document,
                e = d.documentElement,
                g = d.getElementsByTagName('body')[0],
                x = w.innerWidth || e.clientWidth || g.clientWidth,
                settings = window.top.editBarSettings || {},
                appPath = settings.applicationPath || "/";

            if (typeof settings.items == "undefined" || settings.items.length === 0) {
                return;
            }

            var debugMode = settings.debugMode === true;
            var src = appPath + 'admin/Dnn.EditBar/index.html';
            src += '?cdv=' + settings.buildNumber + (debugMode ? '&t=' + Math.random() : '');
            var $iframe = $('<iframe id="editBar-iframe" allowTransparency="true" frameBorder="0" scrolling="false"></iframe>');

            $iframe.appendTo(document.body).attr('src', src)
                .mouseenter(function() {
                    $iframe.addClass("summary");
                }).mouseleave(function () {
                    $iframe.removeClass("summary detail");
                });
        }

        loadEditBar();
    });
})(jQuery);