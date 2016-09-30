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
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : '')
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

        var $editBar = $("#edit-bar");

        var menuLoaders = {}, callAction;

        window.requirejs.config({
            paths: {
                'rootPath': utility.getApplicationRootPath()
            }
        });

        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
            switchMode: function(mode) {
                window.top.$('#editBar-iframe').removeClass('small middle').addClass(mode);
            }
        };
        util = $.extend(util, utility);
        // end define util

        var getMenuItem = function(name) {
            for (var i = 0; i < config.items.length; i++) {
                if (config.items[i].name === name) {
                    return config.items[i];
                }
            }

            return null;
        }

        var getMenuLoader = function (item, callback) {
            if (!item.loader && !item.group) {
                return;
            }

            var loaderName = item.loader || item.group;
            if (typeof menuLoaders[loaderName] !== "undefined") {
                if (typeof callback === "function") {
                    callback(menuLoaders[loaderName]);
                }
                return;
            }

            
            var initMethod = 'init';
            var requiredArray = ['../' + loaderName];
            if (item.customLayout) {
                var templateSuffix = '.html';
                requiredArray.push('text!../../' + loaderName + templateSuffix);
            }

            window.require(requiredArray, function (loader, html) {
                if (typeof loader === "undefined") {
                    return;
                }

                if (html) {
                    html = html.replace(/\[resx:(.+?)\]/gi, function (matches, key) {
                        return item.resx[key] || key;
                    });
                }
                var params = { html: html };

                menuLoaders[loaderName] = loader;

                loader[initMethod].call(loader, item, util, params, function() {
                    if (typeof callback === "function") {
                        callback(loader);
                    }
                });
            });
        }

        util.callAction = function (menuName, actionName, params) {
            var menuItem = getMenuItem(menuName);
            if (menuItem) {
                getMenuLoader(menuItem, function(loader) {
                    if (typeof loader[actionName] === "function") {
                        loader[actionName].apply(menuLoaders[menuName], params);
                    }
                });
            }
        }

        var loadMenu = function (item) {
            if (!item.loader && !item.group) {
                return;
            }

            var cssSuffix = '.css';
            var requiredArray = ['css!../../css/' + (item.loader || item.group) + cssSuffix];

            window.require(requiredArray);

            if (item.customLayout) {
                getMenuLoader(item);
            }
        }

        var renderMenu = function (menuItem) {
            if (menuItem.template) {
                return menuItem.template;
            } else {
                var text = menuItem.resx[menuItem.text] || menuItem.text;
                var menu = '<button href="javascript:void(0);">' + text + '</button>';

                var tooltip = menuItem.resx[menuItem.name + ".Tooltip"];
                if (tooltip) {
                    menu += '<div class="submenuEditBar">' + tooltip + '</div>';
                }

                return menu;
            }
        }

        var authorizeCheck = function (callback) {
            util.sf.moduleRoot = 'editBar/common';
            util.sf.controller = 'Common';
            util.sf.getsilence('CheckAuthorized', {}, function (data) {
                if (data.success) {
                    if (typeof callback === "function") {
                        callback();
                    }
                } else {
                    var loginUrl = config.loginUrl;
                    if (typeof window.parent.dnnModal != "undefined") {
                        window.parent.dnnModal.show(loginUrl + (loginUrl.indexOf('?') === -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
                    } else {
                        location.href = loginUrl;
                    }
                }
            });
        };

        var menuItemClick = function (menuItem) {
            for (var name in menuLoaders) {
                if (menuLoaders.hasOwnProperty(name) && name !== menuItem.name) {
                    var loader = menuLoaders[name];
                    if (typeof loader["onBlur"] === "function") {
                        loader["onBlur"].call(loader);
                    }
                }
            }
            authorizeCheck(function() {
                getMenuLoader(menuItem, function(loader) {
                    if (typeof loader["onClick"] === "function") {
                        loader["onClick"].call(loader, menuItem);
                    }
                });
            });
        }

        var buildViewModel = function() {
            var viewModel = {};
            viewModel.leftMenus = ko.observableArray([]);
            viewModel.rightMenus = ko.observableArray([]);

            for (var i = 0; i < config.items.length; i++) {
                var menuItem = config.items[i];
                menuItem.resx = util.resx[menuItem.name] || (menuItem.group ? util.resx[menuItem.group] : null) || util.resx.Common;
                loadMenu(menuItem);

                switch (menuItem.parent.toLowerCase()) {
                    case "leftmenu":
                        viewModel.leftMenus.push(menuItem);
                        break;
                    case "rightmenu":
                        viewModel.rightMenus.push(menuItem);
                        break;
                }
            }

            viewModel.renderMenu = renderMenu;
            viewModel.menuItemClick = menuItemClick;

            return viewModel;
        }

        var loadMenus = function() {
            var viewModel = buildViewModel();
            ko.applyBindings(viewModel, $editBar[0]);
        }

        util.loadResx(function() {
            loadMenus();
        });
        
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.EditBar = new Gateway(util);
    });
