'use strict';
define(['jquery'], function () {
    var loadedExtensions = {};
    var utility;

    function load(util, params) {
        if (!utility) {
            utility = util;
        }

        var menuIdentifier = params.identifier;
        var path = params.path;

        if (!menuIdentifier || !path) {
            return;
        }

        var $panel = $('#' + util.getPanelIdFromPath(path));

        if (loadedExtensions[menuIdentifier]) {
            reloadExtension($panel, menuIdentifier, params);
            var extensions = getExtensionsByMenu(menuIdentifier);
            $panel.trigger('load.extension', [extensions]);
            return;
        }

        getExtensionsList(menuIdentifier, function (extensions) {
            var initCount = 0;
            var initCompleteCallback = function() {
                initCount++;

                if (initCount === extensions.length) {
                    $panel.trigger('init.extension', [getExtensionsByMenu(menuIdentifier)]);
                }
            }
            if (extensions.length === 0) {
                $panel.trigger('init.extension', []);
            } else {
                for (var i = 0; i < extensions.length; i++) {
                    var extension = extensions[i];
                    utility.parseQueryParameter(extension);

                    initExtension($panel, menuIdentifier, extension, params, initCompleteCallback);
                }
            }
        });
    };

    function getParams(extension, params) {
        var extensionParams = $.extend({}, params, { identifier: extension.identifier, path: extension.path, query: extension.query, settings: extension.settings });

        return extensionParams;
    }

    function getExtensionsList(menuIdentifier, callback) {
        var service = utility.sf;
        service.moduleRoot = 'personaBar';
        service.controller = 'MenuExtensions';
        service.getsilence('GetExtensions', { menu: menuIdentifier }, function (data) {
            if (data && typeof data.length === "number" && typeof callback === "function") {
                callback.call(self, data);
            }
        });
    }

    function initExtension($panel, menuIdentifier, extension, params, callback) {
        var extensionIdentifier = extension.identifier;
        var extensionFolder = extension.folderName || extensionIdentifier;
        var container = extension.container;
        var path = extension.path;
        var extensionParams = getParams(extension, params);
        var $container = $panel.find('#' + container);
        if (!$container.length) {
            return;
        }

        var $wrapper = $('<div />').attr('id', extensionIdentifier + '-extension');
        $wrapper.appendTo($container);

        var templateSuffix = '.html';
        var cssSuffix = '.css';
        var initMethod = 'init';
        
        if (extensionFolder.indexOf('_') > -1) {
            extensionFolder = extensionFolder.split('_')[0];
        }
        var requiredArray = ['../../Modules/' + extensionFolder + '/scripts/' + path, 'text!../../Modules/' + extensionFolder + "/" + path + templateSuffix];
        requiredArray.push('css!../../Modules/' + extensionFolder + '/css/' + path + cssSuffix);

        window.require(requiredArray, function (loader, html) {
            if (loader === undefined) {
                if (typeof callback === "function") {
                    callback();
                }
                return;
            }

            $wrapper.css('visibility', 'hidden').html(html);

            var callbackInit = function ($wrapper) {
                $wrapper.css('visibility', 'visible');
            };

            loader[initMethod].call(loader, $wrapper, utility, extensionParams, callbackInit);

            if (!loadedExtensions[menuIdentifier]) {
                loadedExtensions[menuIdentifier] = {};
            }

            loadedExtensions[menuIdentifier][extensionIdentifier] = { extension: extension, loader: loader, params: extensionParams };

            if (typeof callback === "function") {
                callback();
            }
        });
    }

    function reloadExtension($panel, menuIdentifier, params) {
        var loadMethod = 'load';
        for (var identifier in loadedExtensions[menuIdentifier]) {
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                var loadedExtension = loadedExtensions[menuIdentifier][identifier];
                loadedExtension.loader[loadMethod].call(loadedExtension.loader, loadedExtension.params);
            }
        }
    }

    function getExtensionsByMenu(menuIdentifier) {
        var menuExtensions = [];
        for (var identifier in loadedExtensions[menuIdentifier]) {
            if (loadedExtensions[menuIdentifier].hasOwnProperty(identifier)) {
                menuExtensions.push(loadedExtensions[menuIdentifier][identifier]);
            }
        }

        return menuExtensions;
    }

    function callAction(menuIdentifier, actionName) {
        var extensions = getExtensionsByMenu(menuIdentifier);
        for (var i = 0; i < extensions.length; i++) {
            var loader = extensions[i].loader;
            if (typeof (loader[actionName]) === "function") {
                loader[actionName].call(loader);
            }
        }
    }

    return {
        load: load,
        getExtensions: getExtensionsByMenu,
        callAction: callAction
    }
});