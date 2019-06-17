'use strict';
define(['jquery'], function ($) {
    var initializedModules = {};
    return {
        init: function (config) {
            var loadTempl;
            var injectBeacon;
            var setDialogClass;

            loadTempl = function (folder, template, wrapper, params, self, cb) {
                var callbackInit, moduleFolder, scriptFolder, templateSuffix, cssSuffix, initMethod, moduleJs, loadMethod;

                if (!initializedModules[template]) {
                    templateSuffix = '.html';
                    cssSuffix = '.css';
                    initMethod = 'init';
                    moduleFolder = folder ? 'modules/' + folder + '/' : '';
                    scriptFolder = moduleFolder ? moduleFolder + 'scripts/' : 'scripts/';
                    var requiredArray = ['../../' + scriptFolder + template, 'text!../../' + moduleFolder + template + templateSuffix];
                    requiredArray.push('css!../../' + moduleFolder + 'css/' + template + cssSuffix);

                    window.require(requiredArray, function (module, html) {
                        if (module === undefined) return;

                        wrapper.html(html);

                        // Create objects or Initicialize objects and store
                        if (module.type === 'Class') {
                            initializedModules[template] = new module(wrapper, self, params, cb);
                        } else {
                            module[initMethod].call(module, wrapper, self, params, cb);
                            initializedModules[template] = module;
                        }
                    });
                } else {
                    moduleJs = initializedModules[template];
                    if (typeof moduleJs.load !== 'function') return;

                    loadMethod = 'load';

                    if (moduleJs.type === 'Class') {
                        moduleJs.load(moduleJs, params, cb);
                    } else {
                        moduleJs[loadMethod].call(moduleJs, params, cb);
                    }
                }
                injectBeacon(template);
            };
            // Beacon injection
            injectBeacon = function (template) {
                var beaconUrl = config.beaconUrl !== undefined ? config.beaconUrl : undefined;
                if (beaconUrl != undefined && beaconUrl !== "" && template !== "tasks") {
                    (new Image()).src = beaconUrl + "&f=" + encodeURI(template);
                }
            };

            setDialogClass = function (dialog) {
                if (dialog.parent().find('.socialpanel:visible .dnn-persona-bar-page').hasClass('full-width')) {
                    dialog.addClass('full-width-mode');
                }
                else {
                    dialog.removeClass();
                }
            };

            return {
                loaded: function(template) {
                    return !!initializedModules[template];
                },

                loadTemplate: function (folder, template, wrapper, params, cb) {
                    var self = this;
                    loadTempl(folder, template, wrapper, params, self, cb, false);
                },

                loadResx: function (cb) {
                    var self = this;

                    self.sf.moduleRoot = 'personaBar';
                    self.sf.controller = 'localization';
                    self.sf.getsilence('gettable', { culture: config.culture }, function (d) {
                        self.resx = d;
                        if (typeof cb === 'function') cb();
                    });
                },

                getResx: function (moduleName, key) {
                    if (this.resx[moduleName] && this.resx[moduleName].hasOwnProperty(key)) {
                        return this.resx[moduleName][key];
                    }

                    if (this.resx['SharedResources'] && this.resx['SharedResources'].hasOwnProperty(key)) {
                        return this.resx['SharedResources'][key];
                    }

                    return key;
                },

                getModuleNameByParams: function (params) {
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
                    return params ? (params.identifier || '') : '';
                },
                getFolderByParams: function (params) {
                    return params ? (params.folderName || '') : '';
                },

                asyncParallel: function (deferreds, callback) {
                    var i = deferreds.length;
                    if (i === 0) callback();
                    var call = function () {
                        i--;
                        if (i === 0) {
                            callback();
                        }
                    };

                    $.each(deferreds, function (ii, d) {
                        d(call);
                    });
                },

                asyncWaterfall: function (deferreds, callback) {
                    var call = function () {
                        var deferred = deferreds.shift();
                        if (!deferred) {
                            callback();
                            return;
                        }
                        deferred(call);
                    };
                    call();
                },

                confirm: function (text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
                    setDialogClass($('#confirmation-dialog'));
                    $('#confirmation-dialog > p').html(text);
                    $('#confirmation-dialog a#confirmbtn').html(confirmBtn).unbind('click').bind('click', function () {
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function () { $('#mask').hide(); });
                    });
					
					if (cancelBtn != '') {
						$('#confirmation-dialog a#cancelbtn').show();
						$('#confirmation-dialog a#cancelbtn').html(cancelBtn).unbind('click').bind('click', function () {
							if (typeof cancelHandler === 'function') cancelHandler.apply();
							$('#confirmation-dialog').fadeOut(200, 'linear', function () { $('#mask').hide(); });
						});
					} else {
						$('#confirmation-dialog a#cancelbtn').hide();
					}
                    
                    $('#mask').show();
                    $('#confirmation-dialog').fadeIn(200, 'linear');

                    $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function (evt) {

                        if (evt.keyCode === 27) {
                            $(window).off('keydown.confirmDialog');
                            $('#confirmation-dialog a#cancelbtn').trigger('click');
                        }
                    });
                },

                notify: function (text, options) {
                    var self = this;
                    options = options || {};
                    var notificationDialog = $('#notification-dialog');
                    var notificationMessageContainer = $('#notification-message-container');
                    var notificationMessage = $('#notification-message');
                    var closeNotification = $('#close-notification');
                    if (notificationMessageContainer.data('jsp')) {
                        notificationMessageContainer.data('jsp').destroy();
                    }
                    var timeout = typeof options === 'number' ? options : (options.timeout || 2000);
                    var size = options.size || '';
                    var clickToClose = options.clickToClose || false;
                    var closeButtonText = options.closeButtonText || (self.resx && self.resx.PersonaBar.btn_CloseDialog) || "OK";
                    var type = options.type || 'notify';

                    clearTimeout(self.fadeTimeout);

                    setDialogClass(notificationDialog);
                    notificationMessage.removeClass().html(text);
                    if (size) {
                        notificationDialog.addClass(size);
                    }
                    if (clickToClose !== true) {
                        notificationDialog.addClass('close-hidden');
                    }
					else {
						notificationDialog.removeClass('close-hidden');
					}
                    if (type === 'error') {
                        notificationDialog.addClass('errorMessage');
                    }
                    else {
                        notificationDialog.removeClass('errorMessage');
                    }
                    closeNotification.html(closeButtonText)
                    closeNotification.on('click', function () {
                        notificationDialog.fadeOut(200, 'linear');
                        if (notificationMessageContainer.data('jsp')) {
                            //waits for fade out before destroying.
                            setTimeout(function () {
                                notificationMessageContainer.data('jsp').destroy();
                            }, 200);
                        }
                    });
                    //add delay for proper execution
                    setTimeout(function () {
                        notificationMessageContainer.jScrollPane && notificationMessageContainer.jScrollPane();
                    }, 0);
                    notificationDialog.fadeIn(200, 'linear', function () {
                        if (clickToClose !== true) {
                            self.fadeTimeout = setTimeout(function () {
                                if (self.fadeTimeout) {
                                notificationDialog.fadeOut(200, 'linear');
                                }
                        }, timeout);
                        }
                    });
                },

                notifyError: function (text, options) {
                    var _options = typeof options === 'number' ? {timeout: options} : options || {};
                    _options.type = "error";
                    this.notify(text, _options);
                },

                localizeErrMessages: function (validator) {
                    var self = this;
                    validator.errorMessages = {
                        'required': self.resx.PersonaBar.err_Required,
                        'minLength': self.resx.PersonaBar.err_Minimum,
                        'number': self.resx.PersonaBar.err_Number,
                        'nonNegativeNumber': self.resx.PersonaBar.err_NonNegativeNumber,
                        'positiveNumber': self.resx.PersonaBar.err_PositiveNumber,
                        'nonDecimalNumber': self.resx.PersonaBar.err_NonDecimalNumber,
                        'email': self.resx.PersonaBar.err_Email
                    };
                },

                trimContentToFit: function (content, width) {
                    if (!content || !width) return '';
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);

                    var arr = content.split(' ');
                    var trimmed = '', count = 0;
                    $.each(arr, function (i, v) {
                        count += v.length;
                        if (count < max) {
                            if (trimmed) trimmed += ' ';
                            trimmed += v;
                            count++;
                        } else {
                            trimmed += '...';
                            return false;
                        }
                    });
                    return trimmed;
                },

                deserializeCustomDate: function (str) {
                    if (this.moment) {
                        return this.moment(str, 'YYYY-MM-DD').toDate();
                    }
                },

                serializeCustomDate: function (dateObj) {
                    if (this.moment) {
                        return this.moment(dateObj).format('YYYY-MM-DD');
                    }
                },

                getObjectCopy: function (object) {
                    if (typeof object === "object") {
                        return JSON.parse(JSON.stringify(object));
                    } else {
                        throw new Error("The object " + object + " passed in is not an object.");
                    }
                },

                throttleExecution: function (callback) {
                    if (typeof callback === "function") {
                        setTimeout(callback, 0);
                    }
                },

                ONE_THOUSAND: 1000,
                ONE_MILLION: 1000000,

                formatAbbreviateBigNumbers: function (number) {
                    var size = number;
                    var suffix;

                    if (size >= this.ONE_MILLION) {
                        size = size / this.ONE_MILLION;
                        suffix = this.resx.PersonaBar.label_OneMillionSufix;
                    } else if (size >= this.ONE_THOUSAND) {
                        size = size / this.ONE_THOUSAND;
                        suffix = this.resx.PersonaBar.label_OneThousandSufix;
                    } else {
                        return this.formatCommaSeparate(size);
                    }

                    return this.formatCommaSeparate(size.toFixed(1)) + ' ' + suffix;
                },
				getCulture: function () {
                    return config.culture;
				},
                getSKU: function() {
                    return config.sku;
                },
                getNumbersSeparatorByLocale: function () {
                    var numberWithSeparator = (1000).toLocaleString(config.culture);
                    return numberWithSeparator.indexOf(",") > 0 ? "," : ".";
                },
                formatCommaSeparate: function (number) {
                    var numbersSeparatorByLocale = this.getNumbersSeparatorByLocale();
                    while (/(\d+)(\d{3})/.test(number.toString())) {
                        number = number.toString().replace(/(\d+)(\d{3})/, '$1' + numbersSeparatorByLocale + '$2');
                    }
                    return number;
                },
                secondsFormatter: function (seconds) {
                    var oneHour = 3600;
                    var format = seconds >= oneHour ? "H:mm:ss" : "mm:ss";
                    return moment().startOf('day').add(seconds, 'seconds').format(format);
                },
                getApplicationRootPath: function getApplicationRootPath() {
                    var rootPath = location.protocol + '//' + location.hostname + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
                    return rootPath;
                },
                getPanelIdFromPath: function getPanelIdFromPath(path) {
                    return path + '-panel';
                },
                parseQueryParameter: function (item) {
                    item.Query = '';
                    var pathInfo;
                    if (typeof item.Path !== "undefined" && item.Path.indexOf("?") > -1) {
                        pathInfo = item.Path.split('?');
                        item.Path = pathInfo[0];
                        item.Query = pathInfo[1];
                    } else if (typeof item.path !== "undefined" && item.path.indexOf("?") > -1) {
                        pathInfo = item.path.split('?');
                        item.path = pathInfo[0];
                        item.query = pathInfo[1];
                    }
                },

                /**
                * Builds the view model that will be used to
                * create the DOM structure for the Persona Bar menu
                *
                * @method buildMenuViewModel
                * @param {Object} menuStructure the menu structured stored in config object
                * @return {Object} view model that will be used to build the HTML DOM of the menu 
                */
                buildMenuViewModel: function buildMenuViewModel(menuStructure) {

                    var menu = {
                        menuItems: []
                    };

                    var util = this;
                    menuStructure.MenuItems.forEach(function (menuItem) {
                        util.parseQueryParameter(menuItem);
                        var topMenuItem = {
                            id: menuItem.Identifier,
                            resourceKey: menuItem.ResourceKey,
                            moduleName: menuItem.ModuleName,
                            folderName: menuItem.FolderName,
                            path: menuItem.Path,
                            query: menuItem.Query,
                            link: menuItem.Link,
                            css: menuItem.CssClass,
                            icon: menuItem.IconFile,
                            displayName: menuItem.DisplayName,
                            settings: menuItem.Settings,
                            menuItems: []
                        }
                        if (menuItem.Children) {
                            menuItem.Children.forEach(function (menuItem) {
                                util.parseQueryParameter(menuItem);
                                var subMenuItem = {
                                    id: menuItem.Identifier,
                                    resourceKey: menuItem.ResourceKey,
                                    moduleName: menuItem.ModuleName,
                                    folderName: menuItem.FolderName,
                                    path: menuItem.Path,
                                    query: menuItem.Query,
                                    link: menuItem.Link,
                                    css: menuItem.CssClass,
                                    icon: menuItem.IconFile,
                                    displayName: menuItem.DisplayName,
                                    settings: menuItem.Settings
                                }
                                topMenuItem.menuItems.push(subMenuItem);
                            });
                        }

                        //parse menu items into columns
                        var firstColumn, secondColumn;
                        if (topMenuItem.menuItems.length === 0) {
                            topMenuItem.menuItems = [];
                        } else if (topMenuItem.menuItems.length < 9) {
                            topMenuItem.menuItems = [topMenuItem.menuItems];
                        } else if (topMenuItem.menuItems.length <= 18) {
                            var count = topMenuItem.menuItems.length / 2;
                            if (topMenuItem.menuItems.length % 2 !== 0) {
                                count++;
                            }
                            firstColumn = topMenuItem.menuItems.splice(0, count);
                            topMenuItem.menuItems = [firstColumn, topMenuItem.menuItems];
                            topMenuItem.css += " two-columns-menu";
                        } else {
                            firstColumn = topMenuItem.menuItems.splice(0, 7);
                            secondColumn = topMenuItem.menuItems.splice(0, 7);
                            topMenuItem.menuItems = [firstColumn, secondColumn, topMenuItem.menuItems];
                            topMenuItem.css += " three-columns-menu";
                        }

                        menu.menuItems.push(topMenuItem);
                    });

                    return {
                        menu: menu
                    };
                },

                /**
                * Gets the path defined by the first menu item with a 
                * given module name
                *
                * @method getPathByModuleName
                * @param {Object} menuStructure the menu structured stored in config object
                * @param {String} moduleName moduleName
                * @return {String} path 
                */
                getPathByModuleName: function getPathByModuleName(menuStructure, moduleName) {
                    var path = null;
                    menuStructure.MenuItems.forEach(function (menuItem) {
                        if (menuItem.ModuleName === moduleName) {
                            path = menuItem.Path;
                            return;
                        }

                        if (menuItem.Children) {
                            menuItem.Children.forEach(function (menuItem) {
                                if (menuItem.ModuleName === moduleName) {
                                    path = menuItem.Path;
                                    return;
                                }
                            });
                        }
                    });
                    return path;
                }
            };
        }
    };
});

define('css', {
    load: function (name, require, load, config) {
        function inject(filename) {
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
            link.href = filename;
            link.rel = 'stylesheet';
            link.type = 'text/css';
            head.appendChild(link);
        }

        var path = name;
        for (var i in config.paths) {
            if (path.indexOf(i) === 0) {
                path = path.replace(i, config.paths[i]);
                break;
            }
        }

        if (path.indexOf('://') === -1) {
            path = config.baseUrl + path;
        }

        if (typeof config.urlArgs === 'string') {
            path = path + '?' + config.urlArgs;
        }
        inject(path);

        load(true);
    },
    pluginBuilder: './css-build'
});;
