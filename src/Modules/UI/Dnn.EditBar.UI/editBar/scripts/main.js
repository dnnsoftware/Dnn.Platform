(function () {
    'use strict';

    var debugMode = window.parent['editBarSettings']['debugMode'] === true;
    var cdv = window.parent['editBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css/'
        },
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : ''),
        map: {
			'*': {
		        'dnn.jquery': ['../../../../Resources/Shared/Scripts/dnn.jquery'],
		        'dnn.jquery.extensions': ['../../../../Resources/Shared/Scripts/dnn.jquery.extensions'],
		        'dnn.extensions': ['../../../../Resources/Shared/scripts/dnn.extensions'],
		        'jquery.tokeninput': ['../../../../Resources/Shared/components/Tokeninput/jquery.tokeninput'],
		        'dnn.jScrollBar': ['../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar'],
		        'dnn.servicesframework': ['../../../../js/dnn.servicesframework'],
		        'dnn.DataStructures': ['../../../../Resources/Shared/scripts/dnn.DataStructures'],
		        'jquery.mousewheel': ['../../../../Resources/Shared/scripts/jquery/jquery.mousewheel'],
				'dnn.TreeView': ['../../../../Resources/Shared/scripts/TreeView/dnn.TreeView'],
				'dnn.DynamicTreeView': ['../../../../Resources/Shared/scripts/TreeView/dnn.DynamicTreeView'],
				'dnn.DropDownList': ['../../../../Resources/Shared/Components/DropDownList/dnn.DropDownList'],
                'css.DropDownList': ['css!../../../../Resources/Shared/components/DropDownList/dnn.DropDownList.css'],
		        'css.jScrollBar': ['css!../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar.css']
	        }
        }
    });
    requirejs.onError = function (err) {
        // If requireJs throws a timeout reload the page
        if (err.requireType === 'timeout') {
            console.error(err);
            location.reload();
        }
        else {
            console.error(err);
            throw err;
        }
    };
})();

require(['jquery', 'knockout', '../util', '../sf', '../config', '../eventEmitter', '../gateway'],
    function ($, ko, ut, sf, cf, eventEmitter, Gateway) {
        var iframe = window.parent.document.getElementById("editBar-iframe");
        if (!iframe) return;

        var config = cf.init();
        var utility = ut.init(config);

        window.requirejs.config({
            paths: {
                'rootPath': utility.getApplicationRootPath()
            }
        });

        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken)
        };
        util = $.extend(util, utility);
        // end define util

        var loadMenuResources = function (item) {
            if (!item.loader) {
                return;
            }

            var templateSuffix = '.html';
            var cssSuffix = '.css';
            var initMethod = 'init';
            var requiredArray = ['../' + item.loader, 'text!../../' + item.loader + templateSuffix];
            requiredArray.push('css!../../css/' + item.loader + cssSuffix);

            window.require(requiredArray, function (loader, html) {
                if (typeof loader === "undefined") {
                    return;
                }

                var params = { html: html };

                loader[initMethod].call(loader, item, utility, params, null);
            });
        }

        var buildViewModel = function() {
            var viewModel = {};
            viewModel.leftMenus = ko.observableArray([]);
            viewModel.rightMenus = ko.observableArray([]);

            for (var i = 0; i < config.items.length; i++) {
                var menuItem = config.items[i];
                switch (menuItem.parent.toLowerCase()) {
                case "leftmenu":
                    viewModel.leftMenus.push(menuItem);
                    break;
                case "rightmenu":
                        viewModel.rightMenus.push(menuItem);
                        break;
                default:
                }

                loadMenuResources(menuItem);
            }

            return viewModel;
        }

        var loadMenus = function() {
            var viewModel = buildViewModel();
            ko.applyBindings(viewModel, $("#edit-bar")[0]);
        }

        loadMenus();
        
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.EditBar = new Gateway(util);
    });
