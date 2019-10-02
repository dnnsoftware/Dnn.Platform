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
            personaBar.offPanelClose(pageEditComplete);
        }

        if (window.top.dnn.EditBar) {
            window.top.dnn.EditBar.action('PrivatePage', 'update');
        }
    }

    var onClick = function () {
        var personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel('Dnn.Pages', { viewName: 'edit', viewParams: null });
            personaBar.onPanelClose(pageEditComplete);
        }
    }

    return {
        init: init,
        onClick: onClick
    };
});
