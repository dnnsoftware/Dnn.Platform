﻿$(window).on('load', function () {
    window.cookieconsentoptions = window.cookieconsentoptions || {
        "palette": {
            "popup": {
                "background": "#000"
            },
            "button": {
                "background": "#f1d600"
            }
        }
    }
    window.cookieconsentoptions.content = {
        message: window.dnn.getVar('cc_message'),
        dismiss: window.dnn.getVar('cc_dismiss'),
        link: window.dnn.getVar('cc_link')
    }
    if (window.dnn.getVar('cc_morelink') != '') {
        window.cookieconsentoptions.content.href = window.dnn.getVar('cc_morelink')
    }
    window.cookieconsent.initialise(window.cookieconsentoptions);
});    
