(function () {
    'use strict';

    var debugMode = window.parent['personaBarSettings']['debugMode'] === true;
    var cdv = window.parent['personaBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css/',
            'main': '../../scripts'
        },
        urlArgs: (cdv ? 'cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random() : ''),
        shim: {
            'jquery.hoverintent.min': ['jquery'],
            'jquery.qatooltip': ['jquery.hoverintent.min']
            
        },
        map: {
			'*': {
			    'dnn.jquery': ['../../../../../Resources/Shared/Scripts/dnn.jquery'],
			    'dnn.jquery.extensions': ['../../../../../Resources/Shared/Scripts/dnn.jquery.extensions'],
			    'dnn.extensions': ['../../../../../Resources/Shared/scripts/dnn.extensions'],
			    'jquery.tokeninput': ['../../../../../Resources/Shared/components/Tokeninput/jquery.tokeninput'],
			    'dnn.jScrollBar': ['../../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar'],
			    'dnn.servicesframework': ['../../../../../js/dnn.servicesframework'],
			    'dnn.DataStructures': ['../../../../../Resources/Shared/scripts/dnn.DataStructures'],
			    'jquery.mousewheel': ['../../../../../Resources/Shared/scripts/jquery/jquery.mousewheel'],
			    'dnn.TreeView': ['../../../../../Resources/Shared/scripts/TreeView/dnn.TreeView'],
			    'dnn.DynamicTreeView': ['../../../../../Resources/Shared/scripts/TreeView/dnn.DynamicTreeView'],
			    'dnn.DropDownList': ['../../../../../Resources/Shared/Components/DropDownList/dnn.DropDownList'],
			    'css.DropDownList': ['css!../../../../../Resources/Shared/components/DropDownList/dnn.DropDownList.css'],
			    'css.jScrollBar': ['css!../../../../../Resources/Shared/scripts/jquery/dnn.jScrollBar.css']
	        }
        },
        packages: [{
            name: "codemirror",
            location: "./../../../../../Resources/Shared/components/CodeEditor",
            main: "lib/codemirror"
        }],
        waitSeconds: 30
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

// Enable React Dev Tools inside the iframe, this should be loaded before React has been loaded
if (window.parent['personaBarSettings'].debugMode === true) {
    window.__REACT_DEVTOOLS_GLOBAL_HOOK__ = window.parent.__REACT_DEVTOOLS_GLOBAL_HOOK__;
}

require(['jquery', 'knockout', 'moment', '../util', '../sf', '../config', './../extension',
        '../persistent', '../eventEmitter', '../menuIconLoader', '../gateway', 'domReady!', '../exports/export-bundle'],
    function ($, ko, moment, ut, sf, cf, extension, persistent, eventEmitter, iconLoader, Gateway) {
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
        var personaBarMenuWidth = parseInt($("#personabar").width());
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
            closePersonaBar: function handleClosePersonarBar(callback, keepSelection) {
                var self = this;

                if ($personaBarPlaceholder.is(":hidden")) {
                    if (typeof callback === 'function') {
                        callback();
                    }

                    return;
                }

                if (keepSelection) {
                    $('.btn_panel.selected, .hovermenu > ul > li.selected').removeClass('selected').addClass('pending');
                    $('.btn_panel, .hovermenu > ul > li').removeClass('selected');
                } else {
                    $('.btn_panel, .hovermenu > ul > li').removeClass('selected pending');
                }

                parentBody.style.overflow = "auto";
                body.style.overflow = "hidden";

                function closeCallback() {
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

                if (keepSelection) {
                    closeCallback();
                } else {
                    self.persistent.save({
                        expandPersonaBar: false
                    }, closeCallback);
                }
            },
            loadPanel: function handleLoadPanel(identifier, params) {
                var savePersistentCallback;

                if (inAnimation) return;

                var $menuItem = $('ul.personabarnav').find('[id="' + identifier + '"]');
                if ($menuItem.length === 0) {
                    return;
                }

                if (!params.identifier) {
                    params.identifier = identifier;
                }

                if (!params.settings) {
                    params.settings = util.findMenuSettings(params.identifier);
                }

                if (!params.moduleName) {
                    params.moduleName = $menuItem.data('module-name');
                }

                if (!params.folderName) {
                    params.folderName = $menuItem.data('folder-name');
                }

                if (!params.query) {
                    params.query = $menuItem.data('query');
                }

                if (!params.path) {
                    params.path = $menuItem.data('path');
                }

                var self = this;
                var path = params.path;
                var moduleName = params.moduleName;
                var folderName = params.folderName || identifier;

                if (activePath === path && activemodule === moduleName) {
                    return;
                }

                var $menuItems = $(".btn_panel");
                var $hoverMenuItems = $(".hovermenu > ul > li");
                
                $menuItems.removeClass('selected pending');
                $hoverMenuItems.removeClass('selected pending');

                var $btn = $(".btn_panel[id='" + identifier + "']");
                $btn.addClass('selected');

                var hoverMenuItemSelector = ".hovermenu > ul > li[id='" + identifier + "']";
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

                        iframe.style.width = "100%";
                        parentBody.style.overflow = "hidden";
                        body.style.overflow = 'auto';

                        // for mobile pad device...
                        if (onTouch) {
                            iframe.style["min-width"] = "1245px";
                            iframe.style.position = "fixed";
                        }

                        $mask.css("display", "block");
                        inAnimation = true;
                        $mask.animate({
                            opacity: 0.85
                        }, 200, function () {
                            $panel.show().delay(100).animate({ left: personaBarMenuWidth }, 189, 'linear', function () {
                                inAnimation = false;
                                $personaBarPlaceholder.show();
                                self.loadTemplate(folderName, template, $panel, params, function () {
                                    self.panelLoaded(params);
                                });
                                $(document).keyup(function (e) {
                                    if (e.keyCode === 27) {
                                        e.preventDefault();
                                        if (!window.dnn.stopEscapeFromClosingPB) {
                                            util.closePersonaBar(null, true);
                                        }
                                    }
                                });
                            });
                        });
                    };

                    self.persistent.save({
                        expandPersonaBar: true,
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
                                    self.loadTemplate(folderName, template, $panel, params, function () {
                                        self.panelLoaded(params);
                                    });
                                };
                                self.persistent.save({
                                    expandPersonaBar: true,
                                    activeIdentifier: identifier
                                }, savePersistentCallback);
                            });
                        });
                    } else if (activemodule !== moduleName) {
                        activemodule = moduleName;
                        self.loadTemplate(folderName, template, $panel, params, function () {
                            self.panelLoaded(params);
                        });
                    }
                }
            },
            panelLoaded: function (params) {
                extension.load(util, params);

                this.loadCustomModules();
            },
            initCustomModules: function (callback) {
                if (config.customModules && config.customModules.length > 0) {
                    var self = this;
                    for (var i = 0; i < config.customModules.length; i++) {
                        (function (index) {
                            var path = '../' + config.customModules[index];
                            require([path], function (module) {
                                customModules.push(module);
                                if (typeof module.init === "function") {
                                    module.init.call(self, util);
                                }

                                if (index === config.customModules.length - 1 && typeof callback === "function") {
                                    callback();
                                }
                            });
                        })(i);

                    }
                } else {
                    callback();
                }
            },
            loadCustomModules: function () {
                for (var i = 0; i < customModules.length; i++) {
                    if (typeof customModules[i].load === "function") {
                        customModules[i].load.call(this);
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
                    if (typeof menuItem.length === "number" && menuItem.length > 0) {
                        settings = this.findMenuSettings(identifier, menuItem);
                        if (settings) {
                            break;
                        }
                    } else {
                        if (menuItem.id === identifier) {
                            if (menuItem.settings) {
                                var defaultSettings = { isAdmin: config.isAdmin, isHost: config.isHost };
                                settings = $.extend({}, defaultSettings, eval("(" + menuItem.settings + ")"));
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

        function checkMenuLink($menu) {
            var href = $menu.attr('href');
            if (href) {
                if (href.indexOf('://') > -1) {
                    window.open(href);
                } else {
                    href = config.siteRoot + href;
                    util.closePersonaBar(function () {
                        window.top.location.href = href;
                    });
                }
                return true;
            }

            return false;
        }

        function calculateHoverMenuPosition($menu) {
            var bottom = $menu.parent().offset().top + $menu.outerHeight();
            var availableArea = $(window).height() + $(window).scrollTop() - bottom;
            $menu.css('top', availableArea < 0 ? availableArea + 'px' : '');
        }

        util.asyncParallel([
                function (callback) {
                    util.loadResx(function onResxLoaded() {
                        var viewModel = {
                            resx: util.resx.PersonaBar,
                            menu: menuViewModel.menu,
                            updateLink: ko.observable(''),
                            updateType: ko.observable(0),
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
                        };

                        viewModel.updateText = ko.computed(function() {
                            return viewModel.updateType() === 2 ? util.resx.PersonaBar.CriticalUpdate : util.resx.PersonaBar.NormalUpdate;
                        });

                        ko.applyBindings(viewModel, document.getElementById('personabar'));

                        iconLoader.load();

                        util.sf.moduleRoot = 'personabar';
                        util.sf.controller = "serversummary";
                        util.sf.getsilence('GetUpdateLink', {}, function (data) {
                            viewModel.updateLink(data.Url);
                            viewModel.updateType(data.Type);
                        });

                        document.addEventListener("click", function(e) {
                            $('#topLevelMenu .hovermenu').hide();
                        });

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
                                    $(".btn_panel .hovermenu").click(function(e) {
                                        e.stopPropagation();
                                    });

                                    $(".btn_panel, .hovermenu > ul > li").click(function handleClickOnHoverMenuItem(evt) {
                                        evt.preventDefault();
                                        evt.stopPropagation();

                                        var $this = $(this);

                                        if ($this.hasClass('selected')) {
                                            var panelId = utility.getPanelIdFromPath($this.data('path'));
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
                                        var folderName = $this.data('folder-name');
                                        var query = $this.data('query');
                                        var settings = util.findMenuSettings(identifier);
                                        if (path === '') {
                                            var menuItems = menuViewModel.menu.menuItems;
                                            for (var i = 0; i < menuItems.length; i++) {
                                                if (menuItems[i].id === identifier) {
                                                    if (menuItems[i].menuItems.length > 0) {
                                                        var subMenu = menuItems[i].menuItems[0][0];
                                                        identifier = subMenu.id;
                                                        moduleName = subMenu.moduleName;
                                                        folderName = subMenu.folderName;
                                                        path = subMenu.path;
                                                        query = subMenu.query;
                                                        settings = util.findMenuSettings(identifier);
                                                    }
                                                }
                                            }
                                        }

                                        if (checkMenuLink($('li[id="' + identifier + '"]'))) {
                                            return;
                                        }

                                        if (!path) return;
                                        
                                        if (moduleName !== undefined) {
                                            params = {
                                                moduleName: moduleName,
                                                folderName: folderName,
                                                identifier: identifier,
                                                path: path,
                                                query: query,
                                                settings: settings
                                            };
                                        };

                                        util.loadPanel(identifier, params);

                                        $('.btn_panel > .hovermenu').fadeOut('fast');
                                    });

                                    var $avatarMenu = $('li.useravatar');
                                    if ($avatarMenu.length) {
                                        $avatarMenu.before($showSiteButton);
                                    }

                                    $showSiteButton.click(function handleShowSite(e, keepSelection) {
                                        e.preventDefault();
                                        var needRefresh = $(this).data('need-refresh');
                                        var needHomeRedirect = $(this).data('need-homeredirect');
                                        $showSiteButton.hide();
                                        util.closePersonaBar(function() {
                                            if (needHomeRedirect) {
                                                window.top.location.href = config.siteRoot;
                                            } else {
                                                if (needRefresh) {
                                                    window.top.location.reload();
                                                }
                                            }
                                        }, keepSelection);
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
                                                                    calculateHoverMenuPosition($hoverMenu);
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

                            var retryTimes = 0;
                            var handleLogoutFunc = function() {
                                var $logout = $('li#Logout');
                                if (!$logout.length && retryTimes < 3) {
                                    setTimeout(handleLogoutFunc, 500);
                                    retryTimes++;
                                }

                                $logout.off('click').click(function(evt) {
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
                            };

                            handleLogoutFunc();
                            if (!$iframe.attr('style') || $iframe.attr('style').indexOf("width") === -1) {
                                $iframe.width(personaBarMenuWidth);
                            }
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
                        callback();
                    } else {
                        $personaBar.show();
                        $personaBar.css({ left: -100 });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 200, 'linear', onShownPersonaBar);
                        $personaBar.animate({ left: 0 }, 200, 'linear', callback);
                    }

                    $mask.click(function(e) {
                        $showSiteButton.trigger('click', [true]);
                    });
                },
                function initCustomModules(callback) {
                    util.initCustomModules(callback);
                }
        ],
        function loadPanelFromPersistedSetting() {            
            var pageUrl = window.top.location.href.toLowerCase();
            if (pageUrl.indexOf("skinsrc=") > -1 || pageUrl.indexOf("containersrc=") > -1 || pageUrl.indexOf("dnnprintmode=") > -1) {
                return;
            }

            var settings = util.persistent.load();
            if (settings.expandPersonaBar && settings.activeIdentifier) {
                var identifier = settings.activeIdentifier;
                util.loadPanel(identifier, {});
            }
        });
        
        if (typeof window.parent.dnn === "undefined" || window.parent.dnn === null) {
             window.parent.dnn = {};
        }
        // Register a PersonaBar object in the parent window global scope
        // to allow easy integration between the site and the persona bar
        window.parent.dnn.PersonaBar = new Gateway(util);
        window.dnn.utility = util;
    });
