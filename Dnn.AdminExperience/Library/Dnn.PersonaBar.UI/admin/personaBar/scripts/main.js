(function () {
    'use strict';

    var debugMode = window.parent['personaBarSettings']['debugMode'] === true;
    var cdv = window.parent['personaBarSettings']['buildNumber'];

    requirejs.config({
        baseUrl: 'scripts/contrib/',
        paths: {
            'templatePath': '../../',
            'cssPath': '../../css',
            'main': '../../scripts',
            'modules': '../../modules'
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
        var cachedPersonaBarPageWidth = 860;

        // define util -- very important
        var util = {
            sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
            onTouch: onTouch,
            moment: moment,
            persistent: persistent.init(config, sf),
            inAnimation: inAnimation,

            setConfirmationDialogPosition: function () {
                var confirmation = document.getElementById('confirmation-dialog');
                var personaBarPage = document.getElementsByClassName('dnn-persona-bar-page')[0];
                var condition = personaBarPage.classList.contains('full-width')
                confirmation.classList.toggle("confirmation-dialog-full-width-center", condition);
            },


            openSocialTasks: function openTaskWindow(){
                 var taskWindow = $('.socialtasks');
                 taskWindow.css({visibility:'visible'});
            },

            closeSocialTasks: function closeTaskWindow() {
                 var taskWindow = $('.socialtasks')
                 taskWindow.css({visibility:'hidden'});
            },

            expandPersonaBarPage: function expandPersonaBar(){
                var personaBarPage = $(".dnn-persona-bar-page")
                personaBarPage.css({width:"1159px"});
            },

            contractPersonaBarPage: function contractPersonaBar(){
                var personaBarPage = $('.dnn-persona-bar-page');
                personaBarPage.css({width: cachedPersonaBarPageWidth+'px'});
            },

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
                    self.panelViewData(null, null);
                }

                parentBody.style.removeProperty('overflow');
                body.style.overflow = "hidden";

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

                if (!keepSelection) {
                    saveUserSetting({
                        expandPersonaBar: false
                    });
                }
            },
            loadPanel: function handleLoadPanel(identifier, params) {
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
                } else {
                    params.settings = $.extend({}, util.findMenuSettings(params.identifier), params.settings);
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
                $showSiteButton.hide();
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
                var loaded = self.loaded(template);

                if ($mask.css("display") === 'none') {
                    activePath = path;
                    activemodule = moduleName;
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
                                self.panelLoaded(params, loaded);
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

                    saveUserSetting({
                        expandPersonaBar: true,
                        activeIdentifier: identifier
                    });

                } else {
                    if (activePath !== path) {
                        inAnimation = true;
                        var $activePanel = $('#' + util.getPanelIdFromPath(activePath));
                        $activePanel.fadeOut("fast", function handleHideCurrentPanel() {
                            $panel.css({ left: personaBarMenuWidth }).fadeIn("fast", function handleShowSelectedPanel() {

                                activePath = path;
                                activemodule = moduleName;
                                inAnimation = false;
                                self.loadTemplate(folderName, template, $panel, params, function () {
                                    self.panelLoaded(params, loaded);
                                });

                                saveUserSetting({
                                    expandPersonaBar: true,
                                    activeIdentifier: identifier
                                });
                            });
                        });
                    } else if (activemodule !== moduleName) {
                        activemodule = moduleName;
                        self.loadTemplate(folderName, template, $panel, params, function () {
                            self.panelLoaded(params, loaded);
                        });
                    }
                }
                setCloseButtonClass(panelId);
            },
            panelLoaded: function (params, loaded) {
                extension.load(util, params);
                this.loadCustomModules();

                if (params.handleTabViewInModule !== true && loaded === false) {
                    var panelId = util.getPanelIdFromPath(params.path);
                    util.updatePanelTabView(panelId);
                }
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
            },
            updateMenuSettings: function(identifier, settings, menuItems) {
                menuItems = menuItems || menuViewModel.menu.menuItems;
                for (var i = 0; i < menuItems.length; i++) {
                    var menuItem = menuItems[i];
                    if (typeof menuItem.length === "number" && menuItem.length > 0) {
                        this.updateMenuSettings(identifier, settings, menuItem);
                    } else {
                        if (menuItem.id === identifier) {
                            menuItem.settings = JSON.stringify(settings);
                        } else if (typeof menuItem.menuItems !== "undefined" && menuItem.menuItems.length > 0) {
                            this.updateMenuSettings(identifier, settings, menuItem.menuItems);
                        }
                    }
                }
            },
            /**
             * in case path is an array, it is expected to be a sorted list of dependent sources.
             * 
             * @param path string|array
             */
            loadBundleScript: function (path) {

                var urls = path;
                if(Array.isArray(urls) === false) {
                    urls = [path];
                }
                function ajax(urls, build) {
                    if(urls.length == 0) {
                        return;
                    }
                    $.ajax({
                        dataType: "script",
                        cache: true,
                        data: {
                            cdv: build
                        },
                        url: urls.pop(),
                        complete: function() {
                            ajax(urls, build);
                        }
                    });
                }
                    
                ajax(urls.reverse(), config.buildNumber);
            },
            panelViewData: function (panelId, viewData) {
                var localStorageAllowed = function () {
                    var mod = 'DNN_localStorageTEST';
                    try {
                        window.localStorage.setItem(mod, mod);
                        window.localStorage.removeItem(mod);
                        return true;
                    } catch (e) {
                        return false;
                    }
                };

                if (!localStorageAllowed()) {
                    return {};
                }

                var cacheKey = "DNN_PB_PANEL_VIEW";
                var savedData = window.localStorage[cacheKey];
                if (!savedData) {
                    savedData = {};
                } else {
                    savedData = JSON.parse(savedData);
                }

                if (typeof viewData !== "undefined") {
                    if (panelId === null && viewData === null) {
                        window.localStorage.removeItem(cacheKey);
                        savedData = {};
                    } else {
                        if (panelId === null) {
                            savedData = viewData;
                        } else {
                            savedData[panelId] = viewData;
                        }
                        window.localStorage.setItem(cacheKey, JSON.stringify(savedData));
                    }
                }

                return panelId ? savedData[panelId] : savedData;
            },
            savePanelTabView: function(panelId) {
                var $panel = $('#' + panelId);
                var $primaryTabs = $panel.find('.dnn-tabs.primary, .ui-tabs').eq(0).find('> ul > li');
                if (!$primaryTabs.length) {
                    $primaryTabs = $panel.find('.dnn-tabs.secondary').eq(0).find('> ul > li');
                }
                var $primarySelected = $primaryTabs.parent().find('>li[aria-selected="true"],>li[class*="selected"]');
                if ($primarySelected.length) {
                    var primaryIndex = $primaryTabs.index($primarySelected);

                    var viewData = [primaryIndex];
                    var $primaryPanel = $('#' + $primarySelected.attr('aria-controls'));
                    if ($primaryPanel.length) {

                        var $secondaryTabs = $primaryPanel.find('.dnn-tabs.secondary').eq(0).find('> ul > li');
                        var $secondarySelected = $secondaryTabs.parent().find('>li[aria-selected="true"],>li[class*="selected"]');
                        if ($secondarySelected.length) {
                            var secondaryIndex = $secondaryTabs.index($secondarySelected);

                            viewData.push(secondaryIndex);
                        }
                    }

                    util.panelViewData(panelId, { tab: viewData });
                }
            },
            updatePanelTabView: function (panelId) {
                var viewData = (util.panelViewData(panelId) || {}).tab;
                if (!viewData || !viewData.length) {
                    return;
                }

                var sleep = function(timeout) {
                    setTimeout(function() {
                        util.updatePanelTabView(panelId);
                    }, timeout);
                }

                var $panel = $('#' + panelId);
                var $primaryTab = $panel.find('.dnn-tabs.primary,.ui-tabs').eq(0).find('> ul > li').eq(viewData[0]);
                if (!$primaryTab.length) {
                    $primaryTab = $panel.find('.dnn-tabs.secondary').eq(0).find('> ul > li').eq(viewData[0]);
                }

                if (!$primaryTab.length) {
                    sleep(50);
                    return;
                }

                if ($primaryTab.attr('aria-selected') !== "true" && $primaryTab.attr('class').indexOf('selected') === -1) {
                    if ($primaryTab.find('a').length) {
                        $primaryTab.find('a').trigger('click', [true]);
                    } else {
                        $primaryTab.trigger('click', [true]);
                    }
                }

                if (viewData.length > 1) {
                    var $primaryPanel = $('#' + $primaryTab.attr('aria-controls'));
                    var $secondaryTab = $primaryPanel.find('.dnn-tabs.secondary').eq(0).find('> ul > li').eq(viewData[1]);
                    if (!$secondaryTab.length) {
                        sleep(50);
                    }

                    $secondaryTab.trigger('click', [true]);
                }
            }
        };
        util = $.extend(util, utility);
        // end define util

        function setCloseButtonClass(id) {
            var panel = document.querySelector('#' + id + '>div');
            if (panel != null && panel.innerHTML !== "") {
                var page = panel.querySelector('.dnn-persona-bar-page');
                if (page != null && page.classList.contains('full-width')) {
                    $showSiteButton.addClass('full-width-mode');
                }
                else {
                    $showSiteButton.removeClass();
                }
                $showSiteButton.show();
                return;
            }
            else {
                setTimeout(function () {
                    setCloseButtonClass(id);
                }, 100);
            }
        }
        
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
            if ($menu.length === 0) {
                return;
            }

            var bottom = $menu.parent().offset().top + $menu.outerHeight();
            var availableArea = $(window).height() + $(window).scrollTop() - bottom;
            $menu.css('top', availableArea < 0 ? availableArea + 'px' : '');
        }

        function getSiteRoot() {
            return config.siteRoot || "/";
        }

        function saveBtnEditSettings(success, error) {
            saveUserSetting({
                expandPersonaBar: false,
                activePath: null,
                activeIdentifier: null
            }, success, error);
        }

        function saveUserSetting(settings, success, error) {
            util.persistent.save(settings, success, error);
        }

        function inLockEditMode() {
            if (typeof window.top.dnn != "undefined") {
                return window.top.dnn.dom.getCookie('StayInEditMode') === "YES";
            }

            return false;
        }

        function setLockEditMode(locked) {
            if (typeof window.top.dnn != "undefined") {
                window.top.dnn.dom.setCookie('StayInEditMode', locked ? "YES" : "NO", '', getSiteRoot());

                updateLockModeTooltip(locked);
            }
        }

        function updateLockModeTooltip(locked) {
            var $btnEdit = $("#Edit.btn_panel");
            var title, message;
            if (locked) {
                title = util.resx.PersonaBar["UnlockEditMode"];
                message = util.resx.PersonaBar["UnlockEditMode.Help"];
            } else {
                title = util.resx.PersonaBar["LockEditMode"];
                message = util.resx.PersonaBar["LockEditMode.Help"];
            }

            $btnEdit.find(".editmode-tooltip > span").fadeOut('fast', '', function() {
                $btnEdit.find('.tooltip-title').html(title);
                $btnEdit.find('.tooltip-message').html(message);

                $btnEdit.find(".editmode-tooltip > span").fadeIn('fast');
            });

            if (locked) {
                $btnEdit.removeClass('unlocked').addClass('locked');
            } else {
                $btnEdit.removeClass('locked').addClass('unlocked');
            }
        }

        function handleLockEditState($btnEdit) {
            var $tooltip = $('<div class="editmode-tooltip"><span class="tooltip-title"></span><span class="tooltip-message"></span></div>');
            $tooltip.click(function(e) {
                e.preventDefault();
                e.stopImmediatePropagation();
            });
            $btnEdit.append($tooltip);
            var lockEdit = inLockEditMode();
            updateLockModeTooltip(lockEdit);


            $btnEdit.on('click', function handleEdit() {
                setLockEditMode(!inLockEditMode());
                util.closePersonaBar(saveBtnEditSettings);
            });

            eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
                saveBtnEditSettings();
            });
        }

        function handleTabSelection (callback) {
            $('#personabar-panels').on('click',
                '> .socialpanel .dnn-tabs.primary > ul > li,' +
                '> .socialpanel .dnn-tabs.secondary > ul > li,' +
                '> .socialpanel .ui-tabs > ul > li > a', function (e, byScript) {
                if (byScript) {
                    return;
                }

                var panelId = $(this).parents('.socialpanel').attr('id');
                setTimeout(function() {
                    util.savePanelTabView(panelId);
                }, 0);
            });

            callback();
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
                                $personaBar.addClass("view-ipad landscape");
                            } else if (width <= 768) {
                                $personaBarPlaceholder.css({ 'width': '500px' });
                                $personaBarPanels.addClass("view-ipad portrait");
                                $personaBar.addClass("view-ipad portrait");
                            }
                            else {
                                $personaBarPanels.removeClass("view-ipad landscape portrait");
                                $personaBar.removeClass("view-ipad landscape portrait");
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


                                    var showMenuHandlers = [];
                                    var leaveSubMenuHandlers = [];
                                    var mouseOnHovermenu = false;
                                    var mouseOnButton = false;

                                    var resetHandlers = function() {
                                        if (showMenuHandlers.length > 0) {
                                            $.each(showMenuHandlers, function(index, item) {
                                                clearTimeout(item);
                                            });
                                            showMenuHandlers = [];
                                        }

                                        if (leaveSubMenuHandlers.length > 0) {
                                            $.each(leaveSubMenuHandlers, function(index, item) {
                                                clearTimeout(item);
                                            });
                                            leaveSubMenuHandlers = [];
                                        }
                                    };

                                    $('.btn_panel').each(function () {
                                        var $this = $(this);
                                        var hoverMenuId = $this.data('hovermenu-id');
                                        if (hoverMenuId === undefined) return;

                                        var $hoverMenu = $('#' + hoverMenuId);
                                        $this.hover(function () {
                                            mouseOnButton = true;
                                            if ($hoverMenu.css('display') === 'none' || $this.find('> div').length > 0) {
                                                resetHandlers();

                                                showMenuHandlers.push(setTimeout(function () {
                                                    if (($hoverMenu.css('display') === 'none' || $this.find('> div').length > 0) && mouseOnButton) {
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

                                                        // Set aria-expanded to true when menu is shown
                                                        $hoverMenu.show();
                                                        $this.attr('aria-expanded', 'true');

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
                                            if (($hoverMenu.css('display') == 'block' || $this.find('> div').length > 0) && !mouseOnHovermenu) {
                                                setTimeout(function () {
                                                    if (($hoverMenu.css('display') == 'block' || $this.find('> div').length > 0) && !mouseOnButton && !mouseOnHovermenu) {
                                                        if (!activePath) {
                                                            $iframe.width(personaBarMenuWidth);
                                                        }
                                                        // Set aria-expanded to false when menu is hidden
                                                        $hoverMenu.hide();
                                                        $this.attr('aria-expanded', 'false');

                                                        resetHandlers();
                                                    }
                                                }, 50);
                                            }
                                        });
                                    });

                                    $(".hovermenu").each(function () {
                                        var $this = $(this);

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

                                                        resetHandlers();
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
                                if (!config.visible) {
                                    return;
                                }

                                eventEmitter.addPanelCloseEventListener(function handleClosingPersonaBar() {
                                    $btnEdit.show();
                                });
                                eventEmitter.addPanelOpenEventListener(function handleOpeningPersonaBar() {
                                    $btnEdit.hide();
                                });

                                if (config.userMode !== 'Edit') {
                                    $btnEdit.on('click', function handleEdit() {
                                        function toogleUserMode(mode, successCallback) {
                                            util.sf.moduleRoot = 'internalservices';
                                            util.sf.controller = "controlBar";
                                            util.sf.post('ToggleUserMode', { UserMode: mode }, successCallback);
                                        };
                                        util.closePersonaBar(function () {
                                            toogleUserMode('EDIT', function() {
                                                function reloadPage() {
                                                    window.top.location = window.top.location.protocol + '//' + window.top.location.host + window.top.location.pathname + window.top.location.search;
                                                }
                                                saveBtnEditSettings(reloadPage, reloadPage);
                                            });
                                        });
                                    });
                                } else {
                                    handleLockEditState($btnEdit);
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
                        $iframe.width(personaBarMenuWidth);
                        $personaBar.show();
          
                        $personaBar.css({ left: 0, 'display': 'block' });
                        $parentBody.animate({ marginLeft: personaBarMenuWidth }, 1, 'linear', onShownPersonaBar);
                        $personaBar.animate({ left: 0 }, 1, 'linear', callback);
                    }

                    $mask.click(function(e) {
                        $showSiteButton.trigger('click', [true]);
                    });
                },
                function initCustomModules(callback) {
                    util.initCustomModules(callback);
                },
                function (callback) {
                    handleTabSelection(callback);
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
