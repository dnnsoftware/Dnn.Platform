// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    var htmlModuleName = 'HTML';
    var htmlModuleId = null;
    var recommendedList = [];
    var moduleAlias = [];
    var listMode = 'category';

    ///dnnModuleDialog Plugin

    var dnnModuleDialog = dnn.dnnModuleDialog = function (options) {
        this.options = options;
        this.init();
    };

    dnnModuleDialog.prototype = {
        constructor: dnnModuleDialog,
        init: function () {
            this.options = $.extend({}, dnnModuleDialog.defaultOptions, this.options);

            var layout = this._generateLayout();
            $(document.body).append(layout);
            layout.hide();

            this._addCloseButton();
            this._addSearchBox();

            this._loadMore = true;
            this._startIndex = 0;
            this._pageSize = 10;
            this._bookmarkedModuleList = [];
            this._minInputLength = 2;
            this._inputDelay = 400;
            this._lastVal = '';
            this._supportCategory = true;
            this._callFromExistingModule = false;

            this._syncCompleteHandler = $.proxy(this._syncComplete, this);
            this._refreshCompleteHandler = $.proxy(this._refreshComplete, this);

            this._attachEvents();
        },

        apply: function (moduleManager, callFromExistingModule) {
            this._moduleManager = moduleManager;
            this._callFromExistingModule = callFromExistingModule;
            this.options = $.extend({}, this.options, {
                paneName: moduleManager.options.pane
            });
            return this;
        },

        open: function () {
            this._showDialog();
            this._isOpen = true;
            this._noFloat = false;
            return this;
        },

        close: function (callback) {
            this._isOpen = false;
            this._hideDialog(callback);
            return this;
        },

        getElement: function () {
            return this._dialogLayout;
        },

        getModuleManager: function () {
            if (!this._moduleManager) {
                this._moduleManager = this.getDefaultPane().data('dnnModuleManager');
            }
            return this._moduleManager;
        },

        getDefaultPane: function() {
            return this.getPaneById();
        },

        getPaneById: function (paneName) {
            var defaultPane = null;
            if (!paneName) {
                paneName = 'contentpane';
            } else {
                paneName = paneName.toLowerCase();
            }
            $('.dnnSortable[id]').each(function () {
                var paneId = $(this).attr('id').toLowerCase();
                if (paneId.length >= paneName.length
                        && paneId.indexOf(paneName) == (paneId.length - paneName.length)) {
                    defaultPane = $(this);
                    return false;
                }

                return true;
            });

            return defaultPane;
        },

        isOpen: function () {
            return this._isOpen;
        },

        addPage: function (id, content) {
			if ($('#' + id).length) {
				return;
			}

            var pageContainer = this._dialogLayout.find('.dnnPageContainer');
            var newPage = $('<div class="dnnPage ' + id + '" id="' + id + '"></div>');
            newPage.append(content);
            pageContainer.append(newPage);
        },

        showPage: function (id, immediate) {
            var pageContainer = this._dialogLayout.find('.dnnPageContainer');
            var page;
            if (typeof id == "number") {
                page = pageContainer.find('.dnnPage').eq(id);
            } else {
                page = pageContainer.find('.dnnPage.' + id);
            }

            var left = page.position().left;
            if (!immediate) {
                var handler = this;
                pageContainer.animate({ marginLeft: 0 - left }, 'fast', function () {
                    handler._dialogLayout.trigger('pageChanged', [page]);
                });
            } else {
                pageContainer.css({ marginLeft: 0 - left });
            }
        },

        refreshPane: function (paneName, args, callback, callOnReload) {
            var paneId;
            if (!paneName) {
                paneId = this.getModuleManager().getPane().attr('id');
            } else {
                paneId = this.getPaneById(paneName).attr('id');
            }

            var pane = $('#' + paneId);
            var parentPane = pane.data('parentpane');
            if (parentPane) {
                this.refreshPane(parentPane, args, callback);
                return;
            }
            //set module manager to current refresh pane.
            this._moduleManager = pane.data('dnnModuleManager');
            var ajaxPanel = $('#' + paneId + "_SyncPanel");
            if (ajaxPanel.length) {
                //remove action menus from DOM bbefore fresh pane.
                var handler = this;
                pane.find('div.DnnModule').each(function () {
                    var moduleId = handler._moduleManager._findModuleId($(this));
                    $('#moduleActions-' + moduleId).remove();
                });

                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._refreshCompleteHandler);
                this._refreshPaneId = paneId;
                this._refreshCallback = callback;
                window.setTimeout(function () { __doPostBack(ajaxPanel.attr('id'), args); }, 100);
            } else {
                //save the args into cookie, after page reload then catch the cookie
                //and float the module for drag
                if (args && !this._noFloat) {
                    this._setCookie('CEM_CallbackData', args);
                }

                if (callOnReload && typeof callback == "function") {
                    callback.call($('#' + paneId), [true]);
                }

                location.reload();
            }
        },

        noFloat: function() {
            this._noFloat = true;
        },

        addModule: function (moduleId, callback, callbackBeforeRefresh) {
            if (this._working) {
                return false;
            }

            this._working = true;

            var params = {
                Visibility: 0,
                Position: -1,
                Module: moduleId,
                Pane: this.options.paneName,
                AddExistingModule: false,
                CopyModule: false,
                Sort: -1
            };

            this._addingDesktopModuleId = moduleId;
            var handler = this;

            this.showProgressBar();

            this._getService().request('AddModule', 'POST', params, function (data) {
				if (callbackBeforeRefresh && typeof callback === "function") {
					callback.call(handler.getModuleManager().getPane(), [data.TabModuleID]);
					callback = null;
				}

                handler._addModuleComplete(data, callback);
            });

            return false;
        },

        setModuleId: function (moduleId) {
            if (moduleId <= 0) {
                this._removeCookie('CEM_NewModuleId');
            } else {
                this._setCookie('CEM_NewModuleId', moduleId);
            }
        },

        getModuleId: function () {
            return this._getCookie('CEM_NewModuleId');
        },

        getSiteRoot: function () {
            return dnn.getVar("sf_siteRoot", "/").replace(/\/$/, "");
        },

		showProgressBar: function() {
			var $progressBar = this._dialogLayout.find('.dnnProgressBar');
			var $bar = $progressBar.find('> span');
			$bar.width(0);
			$progressBar.show();
			$bar.animate({
				width: $progressBar.width() * 0.8
			}, 5000);
		},

		hideProgressBar: function() {
			var $progressBar = this._dialogLayout.find('.dnnProgressBar');
			var $bar = $progressBar.find('> span');
			$bar.stop().width($progressBar.width());
			$progressBar.hide();
		},

		_refreshComplete: function (sender, args) {
			this.hideProgressBar();
            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(this._refreshCompleteHandler);

			if (args.get_error() != undefined){
				args.set_errorHandled(true);
				location.reload();
			}

            var handler = this;
            var moduleManager = this.getModuleManager();
            //run inline script execute
            setTimeout(function () {
                if (typeof window.dnnLoadScriptsInAjaxMode === "undefined" || window.dnnLoadScriptsInAjaxMode.length == 0) {
                    handler._executeModuleScripts();
                } else {
                    $(window).one('dnnScriptLoadComplete', function() {
                        handler._executeModuleScripts();
                    });
                }
            }, 50);

            var callback = this._refreshCallback;
            var paneId = this._refreshPaneId;
            var $pane = $('#' + paneId);
            this._refreshPaneId = this._refreshCallback = null;

            if (typeof callback == "function") {
                callback.call($pane);
            }
        },

        _executeModuleScripts: function () {
            $(window).off('load');
            var handler = this;
            var moduleManager = this.getModuleManager();
            moduleManager.getPane().find('div.DnnModule').not('[class~="floating"]').find('script').each(function() {
                var script = $(this).html();
                if (script) {
                    handler._executeScript(script);
                }
            });

            //trigger window load event
            $(window).trigger('load').trigger('resize');
        },

        _executeScript: function (script) {
            try {
                if (window.eval) {
                    return (function () {
                        return window.eval.call(window, script);
                    })();
                }
		        else if (window.execScript) {
			        return window.execScript(script);
		        } else {
                    return null;
                }
	        } catch (ex) {
		        return null;
	        }
        },

        _calcPosition: function (handler) {
            var left, top;
            var dialogWidth = this._dialogLayout.outerWidth();
            var dialogHeight = this._dialogLayout.outerHeight();
            var personaBarWidth = $('#personaBar-iframe').outerWidth();
            left = $(document).scrollLeft() + personaBarWidth / 2 + ($(window).width() - dialogWidth) / 2;
            top = $(document).scrollTop() + ($(window).height() - dialogHeight) / 2;
            this._dialogLayout.css({
                left: left,
                top: top
            });
        },

        _generateLayout: function () {
            var layout = this._dialogLayout = $('' +
                '<div class="dnnModuleDialog">' +
                    '<div class="dnnPageContainer">' +
                        '<div class="dnnPage">' +
                            '<div class="dnnDialogTitle">' +
                                '<span class="title">' + dnn.ContentEditorManagerResources.title + '</span>' +
                            '</div>' +
                            '<div class="dnnDialogBody dnnModuleList">' +
                                '<h2>' + dnn.ContentEditorManagerResources.categoryRecommended + '</h2>' +
                                '<div class="listContainer listRecommended"><ul></ul></div>' +
                                '<h2>' + dnn.ContentEditorManagerResources.categoryAll + '</h2>' +
                                '<div class="listContainer listAll"><ul></ul></div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
					'<div class="dnnProgressBar"><span /></div>' +
                '</div>');

            return layout;
        },

        _addCloseButton: function() {
            var closeButton = this._closeButton = $('<span class="btn-close" />');
            $(".dnnModuleDialog .dnnDialogTitle .title").after(closeButton);

            return closeButton;
        },

        _addSearchBox: function () {
            var searchBox = $(
                    '<div class="search-container"><div class="search-input-container">' +
                    '<input type="text" id="AddModule_SearchModulesInput" placeholder="' + dnn.ContentEditorManagerResources.searchPlaceHolder + '" class="search-input" aria-label="Search"/></div>' +
                    '<a href="javascript:void(0);" title="Search" class="search-button" aria-label="Search"></a>' +
                    '<a href="javascript:void(0);" title="Clear" class="clear-button" aria-label="Clear"></a></div>' +
                    '</div>');

            $(".dnnModuleDialog .dnnDialogTitle .title").after(searchBox);

            return searchBox;
        },

        _showDialog: function () {
            this._createMask();
            this.showPage(0, true);
            this._dialogLayout.show('fast', $.proxy(this._dialogOpened, this));
            $(document).on('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
        },

        _hideDialog: function (callback) {
            this._destroyMask();
            this._dialogLayout.hide('fast', function() {
                if (typeof callback == "function") {
                    callback.call(this);
                }
            });

            $(document).off('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
        },

        _createMask: function () {
            var mask = $('<div class="dnnDialogMask"></div>');
            mask.css({
                left: 0,
                top: 0,
                width: '100%',
                height: $(document).outerHeight()
            });

            $(document.body).append(mask).css('overflow', 'hidden');

            this._calcPosition(this._moduleManager.getHandler());
        },

        _destroyMask: function () {
            $(document.body).css('overflow', '').find('.dnnDialogMask').remove();
        },

        _dialogOpened: function () {
            this._calcPosition(this._moduleManager.getHandler());

            //when dialog first open, raise dialog initial event
            if (!this._initialized) {
                this._dialogLayout.trigger('dialoginit');
                this._initialized = true;

                this._loadRecommendedList(function () {
                    this._loadModuleList('', false);
                });
            }

            this._dialogLayout.trigger('dialogopen');
        },

        _loadModuleList: function (val, isSearch) {
            if ((this._loadMore || isSearch) && !this._getService().isLoading()) {
                if (isSearch && this._startIndex == 0) {
                    $('.listContainer.listAll ul li.dnnModuleItem').remove();
                    $('.listContainer.listAll div.dnnModuleDialog_ModuleListMessage').remove();
                }

                if (val === '' && this._supportCategory) {
                    this._switchToCategoryMode();
                } else {
                    this._switchToSimpleMode();
                }

                if (this._loadMore) {
                    var isAdmin = dnn.getVar('editbar_isAdmin') === "True";
                    this._getService().request('GetPortalDesktopModules', 'GET', {
                        category: 'All',
                        loadingStartIndex: this._startIndex,
                        loadingPageSize: this._pageSize,
                        searchTerm: val,
                        excludeCategories: isAdmin ? '' : 'Admin,Professional',
                        sortBookmarks: true,
                        topModule: htmlModuleName
                    }, $.proxy(this._renderModuleList, this));
                }
            }
        },

        _loadRecommendedList: function (callback) {
            var handler = this;
            this._getEditorService().request('GetRecommendedModules', 'GET', {
            }, function (data) {
                handler._supportCategory = data.length > 0;
                if (!handler._supportCategory) {
                    handler._switchToSimpleMode();
                }

                handler._renderModuleList(data, 'listRecommended');
                recommendedList = data;
                if (callback && typeof callback == "function") {
                    callback.call(handler);
                }
            });
        },

        _renderModuleList: function (data, list) {
            if (!list) {
                list = 'listAll';
            }

            for (var i = 0; i < data.length; i++) {
                var moduleName = data[i].ModuleName;
                if (moduleAlias[moduleName]) {
                    var recommendExist = false;
                    for (var j = 0; j < recommendedList.length; j++) {
                        if (recommendedList[j].ModuleName === moduleName) {
                            recommendExist = true;
                            break;
                        }
                    }

                    if (!recommendExist) {
                        recommendedList.push(this._cloneItem(data[i]));
                    }
                }
            }

            var container = this._dialogLayout.find(".dnnModuleList .listContainer." + list + " ul");

            var searchWord = $("#AddModule_SearchModulesInput").val().toLowerCase();
            if (listMode === 'search' && searchWord) {
                for (var name in moduleAlias) {
                    if (moduleAlias.hasOwnProperty(name) && moduleAlias[name].toLowerCase().indexOf(searchWord) > -1) {
                        for (var i = 0; i < recommendedList.length; i++) {
                            if (recommendedList[i].ModuleName === name) {
                                var cloneItem = this._cloneItem(recommendedList[i]);
                                data.push(cloneItem);
                            }
                        }
                    }
                }
            }
            if (data.length > 0) {
                //render rearranged list
				for (var i = 0; i < data.length; i++) {
				    var itemData = data[i];
				    if (list === 'listAll' && listMode === 'category' && this._isRecommendModule(itemData)) {
                        continue;
                    }

					var $item = $(this._renderItem(itemData));
					container.append($item);
					this._injectModuleScript($item, itemData);
				}

				if (list === "listAll") {
				    if (data.length < this._pageSize) {
				        this._loadMore = false;
				    } else {
				        this._startIndex += data.length;
				    }

				    if (!htmlModuleId) {
				        for (var i = 0; i < data.length; i++) {
				            if (data[i].ModuleName === htmlModuleName) {
				                htmlModuleId = data[i].ModuleID;
				            }
				        }
				    }
				    if (htmlModuleId > -1) {
				        var $htmlItem = this._dialogLayout.find('.dnnModuleItem[data-moduleid="' + htmlModuleId + '"]');
				        $htmlItem.parent().prepend($htmlItem);
				    }

                    this._dialogLayout.trigger('moduleloaded');

                    this._initScrollView();
                }
            }
            else {
                if (list == "listAll") {
                if (container.has("li").length == 0 && this._startIndex == 0) {
                    $(".jspVerticalBar").hide();
                    //scroll top to render no modules message
                    $(".jspDrag").css("top", "0px");
                    $(".jspPane").css("top", "0px");
                    this._loadMore = false;
                    container.after(this._getNoResultTemplate());
                }
}
            }
            $("#AddModule_SearchModulesInput").focus();
        },

        _isRecommendModule: function(item) {
            for (var i = 0; i < recommendedList.length; i++) {
                if (recommendedList[i].ModuleName == item.ModuleName) {
                    return true;
                }
            }

            return false;
        },

        _switchToSimpleMode: function () {
            this._dialogLayout.find(".dnnModuleList .listContainer.listRecommended").hide();
            this._dialogLayout.find("h2").hide();

            this._dialogLayout.find(".search-container .search-button").hide();
            this._dialogLayout.find(".search-container .clear-button").show();

            listMode = 'search';
        },

        _switchToCategoryMode: function () {
            this._dialogLayout.find(".dnnModuleList .listContainer.listRecommended").show();
            this._dialogLayout.find("h2").show();

            this._dialogLayout.find(".search-container .search-button").show();
            this._dialogLayout.find(".search-container .clear-button").hide();

            listMode = 'category';
        },

        _getItemTemplate: function () {
            return '<li class="dnnModuleItem" data-moduleid="[$ModuleID$]">' +
                '<span class="icon [$ModuleName|css]"><img src="[$ModuleImage$]" alt="[$ModuleName$]" /></span>' +
                '<span class="title {0}">[$ModuleName$]</span>' +
                '<span class="actions">' +
                    '<a href="#" class="button bookmarkModule" data-moduleid="[$ModuleID$]" aria-label="Bookmark"></a>' +
                '</span>' +
                '</li>';
        },

        _getNoResultTemplate: function () {
            return '<div class="dnnModuleDialog_ModuleListMessage"><span>' + dnn.ContentEditorManagerResources.nomodules + '</span></div>';
        },

        _renderItem: function (item) {
            var extraclass;
            var template = this._getItemTemplate();
            var renderItem = this._cloneItem(item);
            if (moduleAlias[renderItem.ModuleName]) {
                renderItem.ModuleName = moduleAlias[renderItem.ModuleName];
            }

            for (var a in renderItem) {
                if (renderItem.hasOwnProperty(a)) {
                    var shortMatchRegex = new RegExp('\\[\\$' + a + '\\|(\\d+)\\$\\]', 'g');
                    var shortMatch = shortMatchRegex.exec(template);
                    while (shortMatch) {
                        var val = renderItem[a].toString();
                        var length = parseInt(shortMatch[1], 10);
                        if (val.length > length) {
                            val = val.substr(0, length) + "...";
                        }

                        template = template.replace(shortMatch[0], val);
                        shortMatch = shortMatchRegex.exec(template);

                    };

                    template = this._replaceAll(template, '[$' + a + '|css]', this._replaceAll(renderItem[a].toString().toLowerCase(), ' ', '-'));
                    template = this._replaceAll(template, '[$' + a + '$]', renderItem[a]);

                    if (a === 'ModuleName') {
                        extraclass = renderItem[a].length > 20 ? 'longTitle' : '';
                        template = template.replace('{0}', extraclass);
                    }

                    if (renderItem.ModuleName === htmlModuleName) {
                        template = this._replaceAll(template, 'bookmarkModule', 'topModule');
                    } else if (renderItem.Bookmarked) {
                        template = this._replaceAll(template, 'bookmarkModule', 'bookmarkedModule');
                    }
                }
            }

            return template;
        },

        _cloneItem: function(item) {
            var renderItem = {};
            for (var i in item) {
                if (item.hasOwnProperty(i)) {
                    renderItem[i] = item[i];
                }
            }
            return renderItem;
        },

        _injectModuleScript: function (item, data) {
            if (data.ModuleID == -1) return;
	        var handler = this;
			this._getEditorService().request('LoadModuleScript', 'GET', {
				desktopModuleId: data.ModuleID
			}, function (returnData) {
				if (returnData.Script) {
					var model = handler._executeScript(returnData.Script);
					if (model && model.addModuleHandler && typeof model.addModuleHandler === "function") {
						model.addModuleHandler.call(window, data.ModuleID, data.ModuleName);
					}
				}

				if (returnData.StyleFile) {
					$(document.body).append('<link href="' + returnData.StyleFile + '" type="text/css" rel="stylesheet" />');
				}

				item.data('modulejs', true);
			}, function() {
				item.data('modulejs', true);
			});
		},

        _initScrollView: function () {
            var container = this._dialogLayout.find(".dnnModuleList");
            if (container.data('jsp')) {
                container.data('jsp').reinitialise();
            } else {
                container.jScrollPane();
                var $this = this;
                container.bind('jsp-scroll-y', function (e, y, atTop, atBottom) {
                    if (atBottom) {
                        if ($("#AddModule_SearchModulesInput").val()) {
                            $this._loadModuleList($("#AddModule_SearchModulesInput").val(), true);
                        } else {
                            $this._loadModuleList('', false);
                        }
                    }
                });
            }

            $(".jspVerticalBar").show();
        },

        // SEARCH
        _getSearchModules: function () {
            var $searchInput = $("#AddModule_SearchModulesInput");
            this._startIndex = 0;
            this._loadMore = true;
            this._loadModuleList($searchInput.val(), true);
        },

        _resetModuleSearch: function () {
            var $searchInput = $("#AddModule_SearchModulesInput");
            $searchInput.val('').focus();
            $('.listContainer.listAll ul li.dnnModuleItem:not([class~="dnnLayoutItem"])').remove();
            $('.listContainer.listAll div.dnnModuleDialog_ModuleListMessage').remove();
            this._loadMore = true;
            this._getService()._loading = false;
            this._startIndex = 0;
            this._loadModuleList('', true);
        },

        _searchKeyPress: function (e) {
            if (e.keyCode == 13) {
                var val = $("#AddModule_SearchModulesInput").val();
                clearTimeout(this._searchTimeout);
                this._startIndex = 0;
                this._loadMore = true;
                this._loadModuleList(val, true);
                return false;
            }
        },

        _searchKeyUp: function () {
            var val = $("#AddModule_SearchModulesInput").val();
            if (this._lastVal.length != val.length &&
                (val.length == 0 || val.length >= this._minInputLength)) {
                this._startIndex = 0;
                this._loadMore = true;
                this._loadModuleList(val, true);
            }
            this._lastVal = val;
        },
        // END - SEARCH

        _attachEvents: function () {
            this._dialogLayout.on('click', '.dnnModuleItem', $.proxy(this._doAddModule, this));
            $(window).on('resize', '', $.proxy(this._layoutResized, this));
            this._dialogLayout.on('click', 'span.actions .button.bookmarkModule, span.actions .button.bookmarkedModule', $.proxy(this._toggleBookmark, this));
            this._dialogLayout.on('click', 'span.btn-close', $.proxy(this._closeButtonHandler, this));
            var $searchButton = $(".dnnModuleDialog .search-container .search-button");
            $searchButton.on('click', $.proxy(this._getSearchModules, this));

            var $clearButton = $(".dnnModuleDialog .search-container .clear-button");
            $clearButton.on('click', $.proxy(this._resetModuleSearch, this));

            var $searchInput = $(".dnnModuleDialog .search-container .search-input");
            $searchInput.on('mouseup', function () { return false; });
            $searchInput.on('keypress', $.proxy(this._searchKeyPress, this));
            $searchInput.on('keyup', $.proxy(this._searchKeyUp, this));

            $(window).on('beforeunload', $.proxy(this._windowBeforeUnload, this));
        },

        _closeButtonHandler: function() {
            this.getModuleManager().getHandler().click();
        },

        _handleKeyEvent: function (e) {
            if (e.keyCode == 27) {
                this._moduleManager.getHandler().click();
            }
        },

        _doAddModule: function (e) {
        	var $item = $(e.target);
	        $item = $item.hasClass('dnnModuleItem') ? $item : $item.parents('.dnnModuleItem');
	        var handler = this;
			if (!$item.data('modulejs')) {
				setTimeout(function() {
					handler._doAddModule(e);
				}, 500);

				return false;
			}

            var moduleId = $item.data("moduleid");
            if (!moduleId || moduleId == -1) {
                return false;
            }
            this.addModule(moduleId);

            return false;
        },

        _addModuleComplete: function (data, callback) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._syncCompleteHandler);

            if (this._noFloat) {
                this._setCookie('noFloat', 'true');
                dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
            }

            this.setModuleId(data.TabModuleID);
            this.refreshPane('', 'module-' + data.TabModuleID, callback, true);
        },

        // BOOKMARK
        _bookmarkUpdated: function (data) {
        },

        _saveBookmark: function (item, removeBookmark) {
            var params = {
                Title: 'module',
                Bookmark: this._bookmarkedModuleList.join(',')
            };
            this._getService().request('SaveBookmark', 'POST', params, $.proxy(this._bookmarkUpdated, this));
            if (removeBookmark) {
                $((item).parent()).parent().find("span.bookmarkholder a.button.bookmarkedModule").removeClass("bookmarkedModule").addClass("bookmarkModule").unbind("click");
                $((item).parent()).parent().find("span.actions a.button.bookmarkedModule").removeClass("bookmarkedModule").addClass("bookmarkModule").bind("click");
            }
            else {
                $((item).parent()).parent().find("span.bookmarkholder a.button.bookmarkModule").removeClass("bookmarkModule").addClass("bookmarkedModule").bind("click");
                $((item).parent()).parent().find("span.actions a.button.bookmarkModule").removeClass("bookmarkModule").addClass("bookmarkedModule").unbind("click");
            }

            return false;
        },

        _toggleBookmark: function(e) {
            if ($(e.target).hasClass('bookmarkedModule')) {
                this._removeBookmarkModule(e);
            } else {
                this._addBookmarkModule(e);
            }

            e.stopImmediatePropagation();
            return false;
        },

        _addBookmarkModule: function (e) {
            var moduleId = $(e.target).data("moduleid");
            if (this._bookmarkedModuleList.indexOf(moduleId) < 0) {
                this._bookmarkedModuleList.push(moduleId);
            }
            this._saveBookmark($(e.target), false);
        },

        _removeBookmarkModule: function (e) {
            var moduleId = $(e.target).data("moduleid");
            if (moduleId == 84) {
                return;
            }
            var index = this._bookmarkedModuleList.indexOf(moduleId);
            if (index >= 0) {
                this._bookmarkedModuleList.splice(index, 1);
            }
            this._saveBookmark($(e.target), true);
        },
        // END - BOOKMARK

        _cancelAddModule: function(e) {
            var target = $(e.target);
            var moduleId = target.data('moduleid');

            var handler = this;
            var opts = {
                callbackTrue: function () {
                    //remove all action menus in current layout
                    handler._getEditorService().request('DeleteModule?moduleId=' + moduleId, 'POST', {
                    }, function (data) {
                        if (data.Status != 0) {
                            $.dnnAlert({ text: data.Message });
                        }

                        handler._resetPending();
                        handler.refreshPane('');
                    });
                },
                text: dnn.ContentEditorManagerResources.cancelConfirm,
                yesText: dnn.ContentEditorManagerResources.confirmYes,
                noText: dnn.ContentEditorManagerResources.confirmNo,
                title: dnn.ContentEditorManagerResources.confirmTitle
            };

            $.dnnConfirm(opts);
            return false;
        },

        _processModuleForDrag: function (module) {
            var handler = this;
            var moduleId = this.getModuleManager()._findModuleId(module);

            //hide related modules
            var relatedModules = [];
            if (!this._callFromExistingModule) {
                $('div.DnnModule').each(function() {
                    var id = handler.getModuleManager()._findModuleId($(this));
                    if (id > moduleId) {
                        $(this).hide();
                        $('#moduleActions-' + id).hide();
                        relatedModules.push(id);
                    }
                });
            }
            module.data('relatedModules', relatedModules);

            module.addClass('floating');
            //move module to current screen
            var left, top;
            var moduleWidth = module.outerWidth();
            var moduleHeight = module.outerHeight();
            left = $('#personaBar-iframe').outerWidth() / 2
                + ($(window).width() - moduleWidth) / 2;
            top = $(window).height() / 2 - moduleHeight;
            module.css({
                left: left,
                top: top
            });

            var initContentTimeout;
            var initFloatingContent = function () {
                var $dragHint = module.find('> div.dnnDragHint');

                if (initContentTimeout) {
                    clearTimeout(initContentTimeout);
                    initContentTimeout = null;
                }
                if ($dragHint.length === 0) {
                    initContentTimeout = setTimeout(initFloatingContent, 50);
                    return;
                }

                $('div.dnnDragHint').off('mouseenter').addClass('dnnDragDisabled');
                $dragHint.removeClass('dnnDragDisabled');
                var $dragContent = $('<div />');
                $dragHint.append($dragContent);

                var moduleItem = handler._callFromExistingModule ?
                                    $('.dnnExistingModuleDialog li[data-moduleid="' + handler._addingDesktopModuleId + '"]')
                                    : handler.getElement().find('li[data-moduleid="' + handler._addingDesktopModuleId + '"]');

                if (moduleItem.length > 0) {
                    var $icon = moduleItem.find('span.icon');
                    var $cloneIcon = $icon.clone();
                    if ($icon.css('background-image') && $icon.css('background-image') != 'none') {
                        var backImg = $icon.css('background-image').match(/^(url\()?(['"]?)(.+?)\2\)?$/)[3];
                        $cloneIcon.find('img').attr('src', backImg);
                    }
                    $dragContent.append($cloneIcon);
                    $dragContent.append(moduleItem.find('span.title').clone());
                } else {
                    $dragHint.append($dragContent);

                    var title = module.data('module-title');
                    $('<span class="title" />').appendTo($dragContent).html(title);
                }

                $('<a name="' + moduleId + '" href="#" class="cancel-module" data-moduleid="' + moduleId + '" />').appendTo($dragContent).click($.proxy(handler._cancelAddModule, handler));
            };
            initFloatingContent();

            //show the drag tip if user add module first time.
            if (module.hasClass('dragtip')) {
                var dragTip = $('<div class="module-drag-tip"></div>');
                module.before(dragTip.css({opacity: 0}));
                dragTip.html(dnn.ContentEditorManagerResources.dragtip).css({
                    top: module.offset().top - $(document).scrollTop() - dragTip.outerHeight() - 28,
                    left: module.offset().left - $(document).scrollLeft() - 35 //the value calculate as: (dragtip.width - module.width) / 2
                });

                dragTip.animate({
                    top: '+=10',
                    opacity: 1
                }, 200);
            }

            module.addClass('drift').mouseover(function() {
                module.removeClass('drift');
            }).mouseout(function() {
                module.addClass('drift');
            });

            this._setPending('module-' + moduleId);
        },

        _syncComplete: function (sender, args) {
            if (args.get_error() != null) {
                args.set_errorHandled(true);
                location.reload();
                return;
            }

            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(this._syncCompleteHandler);
            var handler = this;
            setTimeout(function() {
                handler._syncCompleteFunc();
            }, 25);
        },

        _syncCompleteFunc: function() {
            var moduleId = this.getModuleId();
            var handler = this;
            var newModule = $('div.DnnModule-' + moduleId);

            if (!this._noFloat) {
	            setTimeout(function() {
					handler._processModuleForDrag(newModule);
	            }, 150);
                
            } else {
                //animate the new module
                newModule.addClass('highlight');
                setTimeout(function() {
                    newModule.addClass('animate');
                }, 500);
                setTimeout(function() {
                    newModule.removeClass('highlight animate');
                }, 1000);

                // scroll to new added module
                var moduleTop = (newModule.offset().top - 80);
                if (moduleTop > 0) {
                    $('html, body').animate({scrollTop: moduleTop}, 500);
                }

                setTimeout(function () { //execute after inner script executed.
                    if (typeof window.dnnLoadScriptsInAjaxMode === "undefined" || window.dnnLoadScriptsInAjaxMode.length == 0) {
                        newModule.trigger('editmodule');
                    } else {
                        $(window).one('dnnScriptLoadComplete', function() {
                            newModule.trigger('editmodule');
                        });
                    }
                }, 300);

                this.setModuleId(-1);
            }

            this.getElement().trigger('addmodulecomplete', [newModule]);
            this._motionToNewModule(moduleId);

            setTimeout(function() {
                if (!handler._noFloat) {
                    $('#moduleActions-' + moduleId).addClass('floating');
                }

                handler._noFloat = false;
                handler._removeCookie('noFloat');

				if (dnn
	                && dnn.ContentEditorManager
	                && typeof dnn.ContentEditorManager.catchSortEvents === "function") {
	                dnn.ContentEditorManager.catchSortEvents();
                }
            }, 250);

            this._working = false;
        },

        _motionToNewModule: function(moduleId) {
            var $listItem = this._callFromExistingModule ?
                                $('.dnnExistingModuleDialog li[data-moduleid="' + this._addingDesktopModuleId + '"]')
                                : this.getElement().find('li[data-moduleid="' + this._addingDesktopModuleId + '"]');
            if ($listItem.length === 0) {
                $listItem = this.getModuleManager().getHandler();
            }

            var $newModule = $('div.DnnModule-' + moduleId);
            var $motion = $('<div class="module-motion" />').hide().css({
                width: $listItem.width(),
                height: $listItem.height(),
                left: $listItem.offset().left,
                top: $listItem.offset().top
            }).appendTo($(document.body));

            this.close(function() {
                $motion.show().animate({
                    width: $newModule.width(),
                    height: $newModule.height(),
                    left: $newModule.offset().left,
                    top: $newModule.offset().top
                }, 'fast', function() {
                    $motion.remove();
                });
            });
        },

        _layoutResized: function (e) {
            if (this.isOpen()) {
                this._calcPosition(this._moduleManager.getHandler());
            }
        },

        _replaceAll: function (input, find, replace) {
            var str = input;
            str += "";
            var indexOfMatch = str.indexOf(find);

            while (indexOfMatch != -1) {
                str = str.replace(find, replace);
                indexOfMatch = str.indexOf(find);
            }

            return (str);
        },

        // SERVICES HELPERS
        _getService: function () {
            if (!this._serviceController) {
                this._serviceController = new dnn.dnnModuleService({
                    service: 'internalservices',
                    controller: 'controlbar'
                });
            }

            return this._serviceController;
        },

        _getEditorService: function () {
            if (!this._editorServiceController) {
                this._editorServiceController = new dnn.dnnModuleService({
                    service: 'editBar/common',
                    controller: 'ContentEditor'
                });
            }

            return this._editorServiceController;
        },
        // END - SERVICES HELPERS

        // MANAGE PENDING STATE
        _setPending: function(data) {
            this._pending = true;
            $('.actionMenu').addClass('floating');
            this._setCookie('cem_pending', data);
        },

        _resetPending: function() {
            this._pending = false;
            $('.actionMenu').removeClass('floating');
            this._removeCookie('cem_pending');
        },

        _windowBeforeUnload: function() {
            if (this._pending) {
                return dnn.ContentEditorManagerResources.pendingsave;
            }

            return;
        },
        // END - MANAGE PENDING STATE

        // COOKIE HELPERS
        _getCookie: function(name) {
            if (dnn.dom) {
                return dnn.dom.getCookie(name);
            } else {
                return '';
            }
        },

        _setCookie: function(name, value) {
            if (dnn.dom) {
                dnn.dom.setCookie(name, value, 0, this.getSiteRoot());
            }
        },

        _removeCookie: function(name) {
            dnn.dom.setCookie(name, '', -1, this.getSiteRoot());
        }
        // END - COOKIE HELPERS
    };

    dnnModuleDialog.defaultOptions = {};

    ///dnnModuleDialog Plugin END
}(jQuery));