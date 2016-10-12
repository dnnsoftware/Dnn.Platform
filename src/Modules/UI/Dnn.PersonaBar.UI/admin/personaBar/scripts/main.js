(function () {
    'use strict';

    var debugMode = window.parent['personaBarSettings']['debugMode'] === true;
    var cdv = window.parent['personaBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css/'
        },
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : ''),
        shim: {
            'jquery.hoverintent.min': ['jquery'],
            'jquery.qatooltip': ['jquery.hoverintent.min']
            
        },
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
        },
        packages: [{
            name: "codemirror",
            location: "./../../../../Resources/Shared/components/CodeEditor",
            main: "lib/codemirror"
        }]
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

require(['jquery', 'knockout', 'moment', '../util', '../sf', '../config', './../extension',
        '../persistent', '../eventEmitter', '../gateway', 'domReady!', '../exports/dist/export-bundle'],
    function ($, ko, moment, ut, sf, cf, extension, persistent, eventEmitter, Gateway) {
        var iframe = window.parent.document.getElementById("personaBar-iframe");
        if (!iframe) return;
        
        var onTouch = "ontouchstart" in document.documentElement;
        // Checking touch screen for second level menu - the above onTouch won't work on windows tablet - IE I didnt test but read about it.
        var isTouch = ('ontouchstart' in window) || (navigator.msMaxTouchPoints > 0);

        var activePath = null;
        var activemodule = '';
        var parentBody = window.parent.document.body;
        var body = window.document.body;
        var config = cf.init();
        var utility = ut.init(config);
        var inAnimation = false;
        var personaBarMenuWidth = 85;
        var $iframe = $(iframe);
        var $body = $(body);
        var $personaBarPanels = $("#personabar-panels");
        var $personaBarPlaceholder = $('.socialpanel-placeholder');
        var $mask = $(".socialmask");
        var $avatarImage;
        var $personaBar = $('#personabar');
        var $showSiteButton = $('#showsite');
        var customModules = [];
        
        window.requirejs.config({
            paths: {
                'rootPath': utility.getApplicationRootPath()
            }
        });

        if (config.skin) {
            $personaBar.addClass(config.skin);
        }
        
        var menuViewModel = utility.buildMenuViewModel(config.menuStructure);
        
        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
            onTouch: onTouch,
            moment: moment,
            persistent: persistent.init(config, sf),
            inAnimation: inAnimation,
            closePersonaBar: function handleClosePersonarBar(callback) {
                var self = this;

                if ($personaBarPlaceholder.is(":hidden")) {
                    if (typeof callback === 'function') {
                        callback();
                    }

                    return;
                }

                $('.btn_panel, .hovermenu > ul > li').removeClass('selected');

                parentBody.style.overflow = "auto";
                body.style.overflow = "hidden";

                function persistentSaveCallback() {
                    inAnimation = true;
                    $personaBarPlaceholder.hide();
                    self.leaveCustomModules();
                    var $activePanel = $('#' + utility.getPanelIdFromPath(activePath));
                    $activePanel.animate({ left: -860 }, 189, 'linear', function () {
                        $('.socialpanel').css({ left: -860 }).hide();
                        $mask.animate({
                            opacity: 0.0
                        }, 200, function () {
                            $iframe.width(personaBarMenuWidth);

                            // for mobile pad device...
                            if (onTouch) {
                                iframe.style["min-width"] = "0";
                                iframe.style.position = "fixed";
                            }

                            $mask.css("display", "none");
                            $showSiteButton.hide();
                            activePath = null;
                            inAnimation = false;
                            $(document).unbind('keyup');

                            eventEmitter.emitClosePanelEvent();

                            if (typeof callback === 'function') {
                                callback();
                            }
                        });
                    });
                };

                self.persistent.save({
                    expandPersonaBar: false
                }, persistentSaveCallback);
            },
            loadPanel: function handleLoadPanel(path, params) {
                var savePersistentCallback;

                if (inAnimation) return;
                var self = this;
                var moduleName = self.getModuleNameByParams(params);
                var identifier = self.getIdentifierByParams(params);

                iframe.style.width = "100%";
                parentBody.style.overflow = "hidden";
                body.style.overflow = 'auto';

                // for mobile pad device...
                if (onTouch) {
                    iframe.style["min-width"] = "1245px";
                    iframe.style.position = "fixed";
                }

                if (activePath === path && activemodule === moduleName) {
                    return;
                }

                var $menuItems = $(".btn_panel");
                var $hoverMenuItems = $(".hovermenu > ul > li");
                
                $menuItems.removeClass('selected');
                $hoverMenuItems.removeClass('selected');

                var $btn = $(".btn_panel[data-path='" + path + "']");
                $btn.addClass('selected');

                var hoverMenuItemSelector = ".hovermenu > ul > li[data-path='" + path + "']";
                if (moduleName) {
                    hoverMenuItemSelector += "[data-module-name='" + moduleName + "']";
                } else {
                    hoverMenuItemSelector += ":not([data-module-name])";
                }
                $(hoverMenuItemSelector)
                    .addClass('selected')
                    .closest('.btn_panel').addClass('selected');

                var panelId = utility.getPanelIdFromPath(path);
                var $panel = $('#' + panelId);
                if ($panel.length === 0) {
                    $panel = $("<div class='socialpanel' id='" + panelId + "'></div>");
                    $personaBarPanels.append($panel);
                }
                var template = path;

                if ($mask.css("display") === 'none') {
                    savePersistentCallback = function () {
                        activePath = path;
                        activemodule = moduleName;
                        $showSiteButton.show();
                        eventEmitter.emitOpenPanelEvent();

                        $mask.css("display", "block");
                        inAnimation = true;
                        $mask.animate({
                            opacity: 0.85
                        }, 200, function () {
                            $panel.show().delay(100).animate({ left: personaBarMenuWidth }, 189, 'linear', function () {
                                inAnimation = false;
                                $personaBarPlaceholder.show();
                                self.loadTemplate(template, $panel, params, function () {
                                    self.panelLoaded(params);
                                });
                                $(document).keyup(function (e) {
                                    if (e.keyCode === 27) {
                                        e.preventDefault();
                                        if (!window.dnn.stopEscapeFromClosingPB) {
                                            util.closePersonaBar();
                                        }
                                    }
                                });
                            });
                        });
                    };

                    self.persistent.save({
                        expandPersonaBar: true,
                        activePath: path,
                        activeIdentifier: identifier
                    }, savePersistentCallback);

                } else {
                    if (activePath !== path) {
                        inAnimation = true;
                        var $activePanel = $('#' + util.getPanelIdFromPath(activePath));
                        $activePanel.fadeOut("fast", function handleHideCurrentPanel() {
                            $panel.css({ left: personaBarMenuWidth }).fadeIn("fast", function handleShowSelectedPanel() {

                                savePersistentCallback = function () {
                                    activePath = path;
                                    activemodule = moduleName;
                                    inAnimation = false;
                                    self.loadTemplate(template, $panel, params, function() {
                                        self.panelLoaded(params);
                                    });
                                };
                                self.persistent.save({
                                    expandPersonaBar: true,
                                    activePath: path,
                                    activeIdentifier: identifier
                                }, savePersistentCallback);
                            });
                        });
                    } else if (activemodule !== moduleName) {
                        activemodule = moduleName;
                        self.loadTemplate(template, $panel, params, function () {
                            self.panelLoaded(params);
                        });
                    }
                }
            },
            loadModuleDashboard: function handleLoadModuleDashboard(moduleName) {
                var path = utility.getPathByModuleName(config.menuStructure, moduleName);
                this.loadPanel(path, { moduleName: moduleName });
            },
            loadPageAnalytics: function handleLoadPageAnalytics() {
                this.loadPanel('page-traffic');
            },
            panelLoaded: function (params) {
                extension.load(util, params);

                this.loadCustomModules();
            },
            loadCustomModules: function () {
                if (config.customModules && config.customModules.length > 0) {
                    var self = this;
                    for (var i = 0; i < config.customModules.length; i++) {
                        var path = '../' + config.customModules[i];
                        
                        require([path], function (module) {
                            customModules.push(module);
                            if (typeof module.load === "function") {
                                module.load.call(self);
                            }
                        });
                    }
                }
            },
            leaveCustomModules: function () {
                for (var i = 0; i < customModules.length; i++) {
                    if (typeof customModules[i].leave === "function") {
                        customModules[i].leave.call(this);
                    }
                }
            },
            findMenuSettings: function(identifier, menuItems) {
                menuItems = menuItems || menuViewModel.menu.menuItems;
                var settings = null;
                for (var i = 0; i < menuItems.length; i++) {
                    var menuItem = menuItems[i];
                    if (menuItem.id === identifier) {
                        if (menuItem.settings) {
                            settings = eval("(" + menuItem.settings + ")");
                        } else {
                            settings = {};
                        }
                    } else if (typeof menuItem.menuItems !== "undefined" && menuItem.menuItems.length > 0) {
                        settings = this.findMenuSettings(identifier, menuItem.menuItems);
                    }

                    if (settings) {
                        break;
                    }
                }

                return settings;
            }
        };
        util = $.extend(util, utility);
        // end define util
        
        function onShownPersonaBar() {
            (function handleResizeWindow() {
                var evt = document.createEvent('HTMLEvents');
                evt.initEvent('resize', true, false);
                window.top.dispatchEvent(evt);
            })();

            (function ($) {
                $(parent.document).trigger('personabar:show');
            }(parent.$));
        }

        util.asyncParallel([
                function (callback) {
                    util.loadResx(function onResxLoaded() {
                        ko.applyBindings({
                            resx: util.resx.PersonaBar,
                            menu: menuViewModel.menu,
                            logOff: function() {
                                function onLogOffSuccess() {
                                    if (typeof window.top.dnn != "undefined" && typeof window.top.dnn.PersonaBar != "undefined") {
                                        window.top.dnn.PersonaBar.userLoggedOut = true;
                                    }
                                    window.top.document.location.href = window.top.document.location.href;
                                };
                                util.sf.rawCall("GET", config.logOff, null, onLogOffSuccess);
                                return;
                            }
                        }, document.getElementById('personabar'));

                        setTimeout(function () {

                            // Setting for 1024 resolution
                            var width = parent.document.body.clientWidth;
                            if (width <= 1024 && width > 768) {
                                $personaBarPlaceholder.css({ 'width': '700px' });
                                $personaBarPanels.addClass("view-ipad landscape");
                            } else if (width <= 768) {
                                $personaBarPlaceholder.css({ 'width': '500px' });
                                $personaBarPanels.addClass("view-ipad portrait");
                            }
                            else {
                                $personaBarPanels.removeClass("view-ipad landscape portrait");
                            }

                            if (isTouch) {
                                $('#topLevelMenu .personabarnav > li').click(function () {
                                    var hoverMenuId = $(this).attr('data-hovermenu-id');
                                    $('#topLevelMenu .hovermenu').hide();
                                    if (hoverMenuId) {
                                        var $hoverMenuId = $("#" + hoverMenuId);
                                        $hoverMenuId.toggle();
                                        $iframe.width("100%");
                                    }
                                });
                                $(document).on('touchstart', function (event) {
                                    if (!$(event.target).closest('#topLevelMenu .hovermenu').length) {
                                        $('#topLevelMenu .hovermenu').hide();
                                    }
                                });
                                $body.addClass('touch');
                            } else {
                                $body.addClass('non-touch');
                            }
                            var isMac = navigator.appVersion.indexOf('Mac') > -1;
                            if (isMac) {
                                $body.addClass('mac');
                            }

                            var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
                            if (isSafari) {
                                $body.addClass('safari');
                            }

                            var isIe = (function isInternetExplorer() {
                                if (navigator.appName === 'Microsoft Internet Explorer') return true;
                                if (navigator.appName === 'Netscape') {
                                    var ua = navigator.userAgent;
                                    if (ua.indexOf('Trident') > -1) return true;
                                }
                                return false;
                            })();
                            if (isIe) {
                                $body.addClass('ie');
                                iframe.style.backgroundColor = "rgba(0,0,0,0.01)"; // IE10 flashing bug
                            }
                            
                            if (config.visible) {
                                var mouseOnHovermenu = false;

                                (function setupMenu() {

                                    $(".btn_panel, .hovermenu > ul > li").click(function handleClickOnHoverMenuItem(evt) {
                                        evt.preventDefault();
                                        evt.stopPropagation();

                                        var $this = $(this);

                                        var href = $this.attr('href');
                                        if (href) {
                                            if (href.indexOf('://') > -1) {
                                                window.open(href);
                                            } else {
                                                href = config.siteRoot + href;
                                                util.closePersonaBar(function() {
                                                    window.top.location.href = href;
                                                });
                                            }
                                            return;
                                        }

                                        if ($this.hasClass('selected')) {
                                            var path = $this.data('path');
                                            var panelId = utility.getPanelIdFromPath(path);
                                            var panelAlreadyOpened = $('#' + panelId + ':not(visible)');
                                            if (panelAlreadyOpened) {
                                                panelAlreadyOpened.fadeIn('fast');
                                            }
                                            return;
                                        }

                                        var path = $this.data('path');
                                        var params = null;

                                        var identifier = $this.attr('id');
                                        var moduleName = $this.data('module-name');
                                        var query = $this.data('query');
                                        var settings = util.findMenuSettings(identifier);
                                        if (path === '') {
                                            var menuItems = menuViewModel.menu.menuItems;
                                            for (var i = 0; i < menuItems.length; i++) {
                                                if (menuItems[i].id === identifier) {
                                                    if (menuItems[i].menuItems.length > 0) {
                                                        var subMenu = menuItems[i].menuItems[0];
                                                        identifier = subMenu.id;
                                                        moduleName = subMenu.moduleName;
                                                        path = subMenu.path;
                                                        query = subMenu.query;
                                                        settings = util.findMenuSettings(identifier);
                                                    }
                                                }
                                            }
                                        }

                                        if (!path) return;
                                        
                                        if (moduleName !== undefined) {
                                            params = {
                                                moduleName: moduleName,
                                                identifier: identifier,
                                                path: path,
                                                query: query,
                                                settings: settings
                                            };
                                        };

                                        util.loadPanel(path, params);
                                    });

                                    var $avatarMenu = $('li.useravatar');
                                    if ($avatarMenu.length) {
                                        $avatarMenu.before($showSiteButton);
                                    }

                                    $showSiteButton.click(function handleShowSite(e) {
                                        e.preventDefault();
                                        var needRefresh = $(this).data('need-refresh');
                                        var needHomeRedirect = $(this).data('need-homeredirect');
                                        util.closePersonaBar(function() {
                                            if (needHomeRedirect) {
                                                window.top.location.href = config.siteRoot;
                                            } else {
                                                if (needRefresh) {
                                                    window.top.location.reload();
                                                }
                                            }
                                        });
                                    });
                                }());

                                (function setupHoverMenu() {
                                    if (onTouch) {
                                        return;
                                    }

                                    var showMenuHandlers = [];
                                    var leaveSubMenuHandlers = [];
                                    $('.btn_panel').each(function () {
                                        var mouseOnButton = false;
                                        mouseOnHovermenu = false;

                                        var $this = $(this);
                                        var hoverMenuId = $this.data('hovermenu-id');
                                        if (hoverMenuId === undefined) return;

                                        var $hoverMenu = $('#' + hoverMenuId);
                                        $this.hover(function () {
                                            mouseOnButton = true;
                                            if ($hoverMenu.css('display') === 'none') {
                                                
                                                if (showMenuHandlers.length > 0) {
                                                    $.each(showMenuHandlers, function (index, item) {
                                                        clearTimeout(item);
                                                    });
                                                    showMenuHandlers = [];
                                                }

                                                if (leaveSubMenuHandlers.length > 0) {
                                                    $.each(leaveSubMenuHandlers, function (index, item) {
                                                        clearTimeout(item);
                                                    });
                                                    leaveSubMenuHandlers = [];
                                                }

                                                showMenuHandlers.push(setTimeout(function () {
                                                    if ($hoverMenu.css('display') === 'none' && mouseOnButton) {
                                                        if (!activePath) iframe.style.width = "100%";

                                                        $hoverMenu.css({
                                                            position: 'absolute',
                                                            left: '-1000px'
                                                        });

                                                        $('.btn_panel').each(function () {
                                                            var hoverMenuId = $(this).data('hovermenu-id');
                                                            if (hoverMenuId === undefined) return;

                                                            $('#' + hoverMenuId).hide();
                                                        });

                                                        
                                                        $hoverMenu.show();
                                                        // Fix ie personabar hover menús
                                                        showMenuHandlers.push(setTimeout(function () {
                                                            $('.hovermenu > ul').css('list-style-type', 'square');
                                                            showMenuHandlers.push(setTimeout(function () {
                                                                $('.hovermenu > ul').css('list-style-type', 'none');
                                                                showMenuHandlers.push(setTimeout(function () {
                                                                    $hoverMenu.hide();
                                                                    $hoverMenu.removeAttr('style');
                                                                    showMenuHandlers.push(setTimeout(function () {
                                                                        $hoverMenu.fadeIn('fast');
                                                                    }));

                                                                }, 100));
                                                            }));
                                                        }));

                                                    }
                                                }, 50));
                                            }
                                        }, function () {
                                            mouseOnButton = false;
                                            if ($hoverMenu.css('display') == 'block' && !mouseOnHovermenu) {
                                                setTimeout(function () {
                                                    if ($hoverMenu.css('display') == 'block' && !mouseOnButton && !mouseOnHovermenu) {
                                                        if (!activePath) {
                                                            $iframe.width(personaBarMenuWidth);
                                                        }
                                                        $hoverMenu.hide();
                                                    }
                                                }, 50);
                                            }
                                        });
                                    });

                                    $(".hovermenu").each(function () {
                                        var $this = $(this);
                                        var mouseOnButton = false;
                                        mouseOnHovermenu = false;

                                        $this.hover(function () {
                                            mouseOnHovermenu = true;
                                        }, function () {
                                            mouseOnHovermenu = false;
                                            if ($this.css('display') === 'block' && !mouseOnButton) {
                                                leaveSubMenuHandlers.push(setTimeout(function () {
                                                    if ($this.css('display') === 'block' && !mouseOnButton && !mouseOnHovermenu) {
                                                        if (!activePath) {
                                                            $iframe.width(personaBarMenuWidth);
                                                        }
                                                        $this.hide();
                                                    }
                                                }, 800));
                                            }
                                        });
                                    });
                                })();
                            } else {
                                $(".personabarnav > li.btn_panel, .hovermenu > ul > li").addClass("disabled");
                            }

                            (function setupEditButton() {
                                var $btnEdit = $("#Edit.btn_panel");
                                if (width < 1024) {
                                    $btnEdit.hide();
                                    return;
                                }
                                if (!config.visible) {
                                    return;
                                }

                                function saveBtnEditSettings(callback) {
                                    util.persistent.save({
                                        expandPersonaBar: false,
                                        activePath: null,
                                        activeIdentifier: null
                                    }, callback);
                                };
                                eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
                                    $btnEdit.show();
                                });
                                eventEmitter.addPanelOpenEventListener(function handleOpeningPersonaBar() {
                                    $btnEdit.hide();
                                });

                                if (config.userMode !== 'Edit') {
                                    $btnEdit.on('click', function handleEdit() {
                                        function toogleUserMode(mode) {
                                            util.sf.moduleRoot = 'internalservices';
                                            util.sf.controller = "controlBar";
                                            util.sf.post('ToggleUserMode', { UserMode: mode }, function handleToggleUserMode() {
                                                window.parent.location.reload();
                                            });
                                        };
                                        util.closePersonaBar(saveBtnEditSettings(function() {
                                            toogleUserMode('EDIT');
                                        }));
                                    });
                                } else {
                                    $btnEdit.on('click', function handleEdit() {
                                        util.closePersonaBar(saveBtnEditSettings());
                                    });

                                    $btnEdit.addClass('selected');
                                    eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
                                        $btnEdit.addClass('selected');
                                        saveBtnEditSettings();
                                    });
                                }
                            })();

                            $avatarImage = $('.useravatar span');
                            $avatarImage.css('background-image', 'url(\'' + config.avatarUrl + '\')');

                            var $logout = $('li#Logout');
                            if (!$logout.parents('.hovermenu').length) {
                                $logout.before($showSiteButton);
                            }
                            $logout.off('click').click(function (evt) {
                                evt.preventDefault();
                                evt.stopPropagation();

                                function onLogOffSuccess() {
                                    if (typeof window.top.dnn != "undefined" && typeof window.top.dnn.PersonaBar != "undefined") {
                                        window.top.dnn.PersonaBar.userLoggedOut = true;
                                    }
                                    window.top.document.location.href = window.top.document.location.href;
                                };

                                util.sf.rawCall("GET", config.logOff, null, onLogOffSuccess, null, null, null, null, true);
                                return;
                            });
                            $iframe.width(personaBarMenuWidth);
                        }, 0);
                        callback();
                    });
                },
                function showPersonaBar(callback) {
                    var $personaBar = $(".personabar");
                    var $parentBody = $(parentBody);
                    if ($parentBody.hasClass('dnnEditState')) {
                        $personaBar.css({ left: 0, 'display': 'block' });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 1, 'linear', onShownPersonaBar);
                    } else {
                        $personaBar.show();
                        $personaBar.css({ left: -100 });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 200, 'linear', onShownPersonaBar);
                        $personaBar.animate({ left: 0 }, 200, 'linear', callback);
                    }
                }
        ],
        function loadPanelFromPersistedSetting() {
            var settings = util.persistent.load();
            if (settings.expandPersonaBar && settings.activePath) {
                var path = settings.activePath;
                var identifier = settings.activeIdentifier;
                var $menuItem = $('ul.personabarnav').find('#' + identifier);
                var params = {
                    identifier: identifier,
                    path: path,
                    query: $menuItem.data('query'),
                    settings: util.findMenuSettings(identifier)
            }
                util.loadPanel(path, params);
            }
        });
        
        if (typeof window.parent.dnn === "undefined" || window.parent.dnn === null) {
             window.parent.dnn = {};
        }
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.PersonaBar = new Gateway(util);
    });
