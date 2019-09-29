'use strict';
define(['jquery', 'main/config'], function ($, cf) {
    var config = cf.init();
    var loadImage = function(wrapper, path, alt) {
        wrapper.append($('<img />').attr({
            'src': path,
            'alt': alt
        }));
    }

    var loadSvg = function (wrapper, path, alt, iconId) {
        $.get(path, {}, function (data) {
            wrapper.html(function () {
                /* Add a <title> element to the SVG, and an aria-labelledby attribute to help assistive technology */
                var titleId = iconId + '_title';
                var $titleEl = $('<title id="' + titleId + '">' + alt + '</title>');
                var svgIcon = $.parseHTML($.trim(data), null, false);

                /* Make sure there isn't already a <title> */
                if ($(svgIcon).has('title') === true) {
                    $(svgIcon)
                        .attr('aria-labelledby', titleId)
                        .find('title')
                        .replaceWith($titleEl);
                }
                else {
                    $(svgIcon)
                        .attr('aria-labelledby', titleId)
                        .prepend($titleEl);
                }
                return $(svgIcon);
            });

        }, 'text');
    }

    var loadIcons = function() {
        $('#personabar .personabarnav span.icon-loader').each(function() {
            var $this = $(this);
            var path = $this.data('path');
            var alt = $this.data('name');
            var iconId = $this.data('id');
            if (!path) {
                return;
            }

            if (path.indexOf('cdv=') === -1) {
                path += (path.indexOf('?') > -1 ? '&' : '?') + 'cdv=' + config.buildNumber;
            }

            var ext = path.split('?')[0].split('.').pop().toLowerCase();
            if (ext === "svg") {
                loadSvg($this, path, alt, iconId);
            } else {
                loadImage($this, path, alt);
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
