// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

(function (dnn) {
    'use strict';

    if (typeof dnn === 'undefined') dnn = {};
    if (typeof dnn.social === 'undefined') dnn.social = {};

    dnn.social.topLevelExceptionHandler = function (settings, ex) {
        var message = settings.sharedResources['ExceptionMessage'] || 'A fatal JavaScript error has occurred in \'{0}\'.';

        message = message.format(settings.moduleTitle || 'Unknown module');

        $.dnnAlert({
            title: settings.sharedResources['ExceptionTitle'] || 'Fatal error',
            text: message
        });
        
        // Rethrow the exception if we are in debug mode.
        if (typeof settings.debug === 'boolean' && settings.debug) {
            throw ex;
        }
    };

})(window.dnn);