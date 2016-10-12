(function() {
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
            'owl-carousel/owl.carousel': ['jquery'],
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
        }
    });
    requirejs.onError = function (err) {
        // If requireJs throws a timeout reload the page
        if (err.requireType === 'timeout') {
            console.log(err);
            location.reload();
        }
        else {
            throw err;
        }
    };
})();

require(['jquery', 'knockout', 'moment', '../util', '../sf', '../config', '../extension', '../persistent',
        'domReady!', 'owl-carousel/owl.carousel'], function ($, ko, moment, ut, sf, cf, extension, persistent) {
    var iframe = window.parent.document.getElementById("personaBar-mobi-iframe");
    if (!iframe) {
        return;
    }

    var $iframeContainer = $(iframe.parentNode);
    var $parentWindow = $(window.parent);
    var parentHtml = window.parent.document.documentElement;
    var parentBody = window.parent.document.body;
	var config = cf.init();
    var utility = ut.init(config);
    var inAnimation = false;
    var navMenuVisible = false;
    var panelVisible = false;
    var LOGOUT_BUTTON = 'logout';
    var VIEWSITE_BUTTON = 'viewSite';
    var $toogle = $('#toggle');
    var $navMenu = $('#nav-menu');
    var $personaBarPanels = $('#personabar-panels');
    var $breadcrumb = $('#breadcrumb');
    var $tasksPanel = $('#tasks-panel');
    var $header = $("#header-mobi");
    var $totalTaskCounter = $('#nav-menu span.total-tasks');
    var $logo = $('#header-mobi .logo');

    var ANALYTICS_MENU_IDENTIFIER = '#analytics';

    window.requirejs.config({
        paths: {
            'rootPath': utility.getApplicationRootPath()
        }
    });

    var menuViewModel = utility.buildMenuViewModel(config.menuStructure, true);

    (function setPersonaBarLogo($logo, sku) {
        $logo.css('background-image', 'url(\'./Images/' + config.sku + 'Logo.png\')');
    }($logo, config.sku));

    var collapseIframe = function () {
        $iframeContainer.css("bottom", "auto");
        $iframeContainer.height("auto");
        iframe.style.height = $header.outerHeight() + "px";
        $(parentHtml).css("overflow-y", "auto");
        $(parentBody).css({
            "overflow-y": "auto",
            "margin-top": $header.outerHeight()
        });
    };

    var expandIframe = function () {
        $iframeContainer.css("bottom", 0);
        $iframeContainer.height($parentWindow.height());
        iframe.style.height = "100%";
        $(parentHtml).css("overflow-y", "hidden");
        $(parentBody).css({
            "overflow-y": "hidden",
            "margin-top": 0
        });
    };

    var viewSite = function handleViewSite() {
        $('.socialpanel').slideUp(200, function () {
            $breadcrumb.html('');
            $navMenu.slideUp(200, function () {
                inAnimation = false;
                navMenuVisible = false;
                panelVisible = false;
                $toogle.parent().removeClass('expanded');
                collapseIframe();
            });
        });
    };

    var selectMenu = function ($li) {
        $('#nav-menu > ul li.selected').removeClass('selected');
        $li.addClass('selected');
    };
            
    var util = {
        sf: sf.init(config.siteRoot, config.tabId, config.antiForgeryToken),
        persistent: persistent.init(config, sf),
		moment: moment,
        closePersonaBar: function(callback) {
            viewSite();
            if (typeof callback === 'function') {
                callback();
            }
        },
        loadPanel: function (path, params, name) {
            $toogle.parent().removeClass('expanded');
            var self = this;
            $navMenu.slideUp(200, function () {
                $('.socialpanel').hide();

                var panelId = utility.getPanelIdFromPath(path);
                var $panel = $('#' + panelId);
                if ($panel.length === 0) {
                    $panel = $("<div class='socialpanel' id='" + panelId + "'></div>");
                    $personaBarPanels.append($panel);
                }
                var template = path;

                $panel.slideDown(200, function () {
                    self.loadMobileTemplate(template, $panel, params, function () {
                        self.panelLoaded(params);
                    });
                    $breadcrumb.html(name);
                    inAnimation = false;
                    navMenuVisible = false;
                    panelVisible = true;
                });
            });
        },
        loadModuleDashboard: function (moduleName) {
            var path = utility.getPathByModuleName(config.menuStructure, moduleName);
            this.loadPanel(path, { moduleName: moduleName }, moduleName);
        },
        panelLoaded: function (params) {
            extension.load(util, params, true);
        }
    };
    
    util = $.extend(util, utility);
            
    $toogle.click(function handleMenuToogle(e) {
        e.preventDefault();
        if (inAnimation) {
            return;
        }
        inAnimation = true;

        if (!navMenuVisible) {
            expandIframe();
            $('div.body')[0].scrollTop = 0;
            $toogle.parent().addClass('expanded');
            $navMenu.slideDown(200, function () {
                inAnimation = false;
                navMenuVisible = true;
            });
        } else {
            $navMenu.slideUp(200, function () {
                if (!panelVisible) collapseIframe();
                inAnimation = false;
                navMenuVisible = false;
                $toogle.parent().removeClass('expanded');
            });
        }
    });

    $parentWindow.resize(function handleParentWindowResize() {
    	$iframeContainer.height($parentWindow[0].innerHeight);
    });

    util.loadResx(function() {
        ko.applyBindings({
            resx: util.resx.PersonaBar,
            menu: menuViewModel.menu
        }, $navMenu[0]);

        if (window.top.dnn.getVar('evoq_DisableAnalytics', '-1') === "True") {
            $(ANALYTICS_MENU_IDENTIFIER).hide();
        }

        if (config.visible) {

            $('#nav-menu > ul > li > div.arrow').click(function handleMenuItemArrowClick(e) {
                e.stopPropagation();
                $(this).parent().toggleClass("expanded");
            });

            $('#nav-menu > ul > li').click(function handleMenuItemClick(e) {
                e.preventDefault();
                var $this = $(this);

                var href = $this.attr('href');
                if (href) {
                    util.persistent.save({
                        expandPersonaBar: false
                    }, function () {
                        window.top.location.href = href;
                    });

                    return;
                }

                if ($this.attr('id') === LOGOUT_BUTTON) {
                    function onLogOffSuccess() {
                        viewSite();
                        if (typeof window.top.dnn != "undefined" && typeof window.top.dnn.PersonaBar != "undefined") {
                            window.top.dnn.PersonaBar.userLoggedOut = true;
                        }
                        window.top.document.location.href = window.top.document.location.href;
                    };
                    util.sf.rawCall("GET", config.logOff, null, onLogOffSuccess);
                    return;
                }

                if (inAnimation) {
                    return;
                }
                inAnimation = true;
                if ($this.attr('id') === VIEWSITE_BUTTON) {
                    viewSite();
                    return;
                }

                var path = $this.data('path');
                if (!path) {
                    return;
                } else {
                    selectMenu($this);
                }

                var name = $this.data('name');
                util.loadPanel(path, {identifier: $this.attr('id'), path: path}, name);
            });

            $('#nav-menu > ul > li > ul.submenu > li').click(function handleSubMenuItemClick(e) {
                e.stopPropagation();
                var $this = $(this);
                selectMenu($this.parent().parent());
                $this.addClass('selected');
                var path = $this.data('path');
                var moduleName = $this.data('module-name');
                var params = moduleName ? {
                    moduleName: moduleName,
                    identifier: $this.attr('id'),
                    path: path
                } : {};
                var name = moduleName ? moduleName : $this.data('name');
                util.loadPanel(path, params, name);
            });

            $(window).on("total-tasks-changed", function handleTotalTasksChange(event, totalTasks) {
                if (totalTasks > 0) {
                    $totalTaskCounter.show().text(totalTasks);
                } else {
                    $totalTaskCounter.hide();
                }
            });

            util.loadTemplate("tasks", $tasksPanel);
        } else {
            $('#nav-menu > ul > li > ul.submenu > li, #nav-menu > ul > li').addClass("disabled");
        }

        $iframeContainer.css({
            position: 'fixed',
            top: 0,
            bottom: 0,
            left: 0,
            right: 0,
            'z-index': 99999
        });

        collapseIframe();
    });
});