'use strict';
define(['jquery', 'main/config'], function ($, cf) {
    var config = cf.init();
    var loadImage = function(wrapper, path) {
        wrapper.append($('<img />').attr('src', path));
    }

    var loadSvg = function (wrapper, path) {
        $.get(path, {}, function (data) {
            wrapper.html(data);
        }, 'text');
    }

    var loadIcons = function() {
        $('#personabar .personabarnav span.icon-loader').each(function() {
            var $this = $(this);
            var path = $this.data('path');
            if (!path) {
                return;
            }

            if (path.indexOf('cdv=') === -1) {
                path += (path.indexOf('?') > -1 ? '&' : '?') + 'cdv=' + config.buildNumber;
            }

            var ext = path.split('?')[0].split('.').pop().toLowerCase();
            if (ext === "svg") {
                loadSvg($this, path);
            } else {
                loadImage($this, path);
            }

            $this.data('path', null);
        });
    }

    return {
        load: function () {
            loadIcons();
        } 
    };
});
