define(['jquery'], function ($) {
    'use strict';
    var menuItem, util;

    var init = function (menu, utility, params, callback) {
        menuItem = menu;
        util = utility;

        if (typeof callback === 'function') {
            callback();
        }
    };

    var pageEditComplete = function () {
        var personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;

        if (personaBar) {
            personaBar.onPanelClose(pageEditComplete);
        }

        if (window.top.dnn.EditBar) {
            window.top.dnn.EditBar.action('PrivatePage', 'update');
        }
    }

    var onClick = function () {
        var personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel('pages', { viewName: 'edit', viewParams: [null, 1] });
            personaBar.onPanelClose(pageEditComplete);
        }
    }

    var update = function () {
        util.sf.moduleRoot = 'editBar/common';
        util.sf.controller = 'Common';
        util.sf.getsilence('IsPrivatePage', {}, function (data) {
            if (!data.IsPrivate) {
                $('#menu-' + menuItem.name).remove();
            }
        });
    }

    return {
        init: init,
        onClick: onClick,
        update: update
    };
});
