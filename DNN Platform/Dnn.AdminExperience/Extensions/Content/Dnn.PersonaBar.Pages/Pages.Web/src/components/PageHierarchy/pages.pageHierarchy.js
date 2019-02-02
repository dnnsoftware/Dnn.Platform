/* eslint-disable no-var, id-match, quotes, no-mixed-spaces-and-tabs, comma-dangle */ // errors
/* eslint-disable spellcheck/spell-checker, no-unused-vars, space-before-function-paren, indent, eqeqeq */ // warnings
/* global $, jQuery, ko */

window.dnn.pages = window.dnn.pages || {};

    var OVER_TIME_TO_OPEN_PAGE_CHILDS;
    var pageHierarchyManager, pageHierarchyDefaultOptions;
    var draggingJqObj, pageDropped, dropOnDroppable, uiOnDragStart;

    OVER_TIME_TO_OPEN_PAGE_CHILDS = 700;

    // Controls if we have a dragging object when open sublist of a page
    // is triggered
    draggingJqObj = null;

    // Store page dropped to other page (not dropped to list), to add when
    // page lists are load
    pageDropped = null;

    // Controls that the drop action occurs over a drop object (page) no over sort list
    // preventing duplicates pages
    dropOnDroppable = false;

    pageHierarchyManager = function(options) {
        this.options = options;
    };

    pageHierarchyManager.prototype = {
        constructor: pageHierarchyManager,

        _initialized: false,

        init: function(panel, initCallback) {
            var handler, viewModel;

            handler = this;
            this.options = $.extend({}, pageHierarchyDefaultOptions, this.options);
            this.panel = $(panel);
            this.container = $('.pagehierarchy-container', this.panel);
            this.dragContainer = this.container.find('.pages-drag-container');
            this.mobile = window.parent.document.getElementById('personaBar-mobi-iframe') !== null;

            this._selectPageInHierarchyHandler = $.proxy(this._selectPageInHierarchy, this);

            this._lastWindowHeight = $(window).height();
            $(window).resize(function onResizeWindow() {
                var newHeight = $(window).height();
                if (handler._lastWindowHeight === newHeight) {
                    return;
                }
                
                var RESIZE_CONTAINER_DELAY = 400;
                setTimeout(function onResizeContainer() {
                    handler._resizeContentContainer(true);
                    handler._lastWindowHeight = newHeight;
                }, RESIZE_CONTAINER_DELAY);
            });

            $('#showsite, .btn_panel').click($.proxy(this._leavePanelClickHandler, this));

            viewModel = this._getViewModel();
            ko.applyBindings(viewModel, this.panel[0]);

            this._initCallback = initCallback;
            this._currentTabId = null;
            this._siteRoot = null;
            this._loadRootPageList();       
            this._initialized = true;     
        },

        getElement: function() {
            return this.container;
        },

        addPage: function(newPage) {
            var viewModel, pageItem;

            viewModel = this._getViewModel();
            viewModel.pagesList()[0].pages.push(newPage);
            this._initScrollView();

            if (!this.mobile) {
                // Floating the new pag item
                pageItem = $('li[data-page-id="' + newPage.id + '"] > div');
                pageItem.hide();
                viewModel.inDrag(true);
                viewModel.isNew(true);
                viewModel.dragPage(newPage);
            }

            if (typeof this.options.onRefresh === "function") {			
                this.options.onRefresh(this.panel);
            }
        },
        
        editPage: function(newPage) {
            this._updatePageData(newPage.id, newPage);
        },

        selectPage: function(pageId, notFindMore) {
            var handler, pageData, find;

            handler = this;
            find = this._findDataPosition(pageId);

            if (find) {
                pageData = this._getViewModel().pagesList()[find.level].pages()[find.index];
                this._needScrollToSelectedPage = true;
                this._supressUI = false;
                this._getViewModel().selectedPage(pageData);

            } else if (typeof notFindMore === 'undefined' || !notFindMore) {
                this._supressUI = true;
                this.getElement().on('childpagesloaded', this._selectPageInHierarchyHandler);

                this._getService().get('GetPageHierarchy', { pageId: pageId }, function(data) {
                    handler._selectPageInHierarchyPath = data;
                    handler._selectPageInHierarchy();
                });

                return;
            }

            this.getElement().off('childpagesloaded', this._selectPageInHierarchyHandler);
        },

        hasPendingChanges: function() {
            return this._getViewModel().dragPage().id > 0;
        },

        handlePendingChanges: function (e) {
            this.utility.notify(this.resx.Pending);

            if (e) {
                e.stopImmediatePropagation();
            }
            return false;
        },

        setItemTemplate: function (template) {
            if (template != "pages-list-item-template") {
                var viewModel = this._getViewModel();
                viewModel.itemTemplate(template);
                viewModel.inDrag(false);
            }
        },

        setDragItemTemplate: function (template) {
            if (template != "pages-drag-item-template") {
                this._getViewModel().dragItemTemplate(template);
            }
        },

        setSearchKeyword: function (searchKeyword) {
            if (searchKeyword === this._getViewModel().searchKeyword()) {
                return;
            }
            this._getViewModel().searchKeyword(searchKeyword);
            this._searchKeywordsChangedHandler();
        },

        setCurrentTabIdAndSiteRoot: function (currentTabId, siteRoot) {
            this._currentTabId = currentTabId;
            this._siteRoot = siteRoot;
        },

        _loadRootPageList: function() {
            var handler, viewModel, params;

            handler = this;
            viewModel = handler._getViewModel();
            viewModel.pagesList.removeAll();

            params = {
                searchKey: viewModel.searchKeyword()
            };

            this._getOverridableService().get('GetPageList', params, function (data) {
                if (viewModel.searchKeyword().length > 0) {
                    viewModel.pagesList.removeAll();
                }
                viewModel.pagesList.push({
                    parentId: -1,
                    level: 0,
                    pages: ko.observableArray(data)                    
                });

                handler._resizeContentContainer(true);
                handler._initDrag();
                if (typeof handler.options.onRefresh === "function") {				
                    handler.options.onRefresh(handler.panel);
                }

                if (typeof handler._initCallback === 'function') {
                    handler._initCallback.call(handler);
                    handler._initCallback = null;
                }
            }, function (error) {
                handler.utility.notifyError(error.statusText);
            });
        },

        _addPagesList: function (parentPage, pagesData) {
            var handler, viewModel, $listItem, $pagesList, level, nextLevel;

            handler = this;
            viewModel = this._getViewModel();
            $listItem = $('li[data-page-id="' + parentPage.id + '"]');
            $pagesList = $listItem.parents('.pages-list');
            level = $pagesList.data('page-level');

            // Hide other list sublevels if we are dragging for not block the ui
            $('.pages-list').each(function hideNextLevels() {
                var self = $(this);
                if (self.data('page-level') > level) {
                    self.hide();
                    self.addClass('removeMe');
                }
            });
            
            nextLevel = viewModel.pagesList().length;
            viewModel.pagesList.push({
                parentId: parentPage.id,
                level: nextLevel,
                pages: ko.observableArray(pagesData || [])
            });

            setTimeout(function() {
                $pagesList.next().addClass('expand');

                handler._initScrollView(true, function () {
                    var listScroller, containerScroller, $nextList, eventTriggeredByHover;

                    eventTriggeredByHover = false;
                    listScroller = $pagesList.data('jsp');
                    containerScroller = handler.container.find('.pages-list-scroller').data('jsp');

                    if (handler._needScrollToSelectedPage) {
                        handler._needScrollToSelectedPage = false; //this should only work for once.
                        if (listScroller) {
                            listScroller.scrollToElement($listItem[0], true, true);
                        }

                        if (containerScroller) {
                            containerScroller.scrollToElement($pagesList[0], true, true);
                        }
                    } else {
                        $nextList = $('div.pages-list[data-page-level="' + nextLevel + '"]');
                        if (containerScroller && $nextList.length > 0) {
                            containerScroller.scrollToElement($nextList[0], true, true);
                        }
                    }

                    handler._initDrag();
                    handler.getElement().trigger('childpagesloaded');
                    if (typeof handler.options.onRefresh === "function") {			
                        handler.options.onRefresh(handler.panel);
                    }
                });
            }, 0);
        },

        _leavePanelClickHandler: function (e) {
            if ($(e.target).hasClass('btn_pages')) {
                return;
            }
            
            if (typeof this.options.onUnload === "function") {
                this.options.onUnload(this.panel);            
            }
        },

        _pageItemClickHandler: function (pageData, e) {
            this._getViewModel().selectedPage(pageData);
        },

        _viewPageClickHandler: function (pageData, e) {
            this.utility.closePersonaBar(function() {
                window.top.location.href = pageData.url;
            });
        },

        _editPageClickHandler: function (pageData, e) {
            this._enterEditMode(pageData);
        },

        _enterEditMode: function (pageData) {
            var handler = this;
            setTimeout(function () {            
                if (typeof handler.options.onUnload === "function") {
                    handler.options.onUnload(handler.panel);            
                }
                handler._getService().post('EditModeForPage?id=' + pageData.id, {}, function() {
                    handler.utility.closePersonaBar(function() {
                        window.top.location.href = pageData.url;
                    });
                });
            }, 100);
            
        },

        _settingsPageClickHandler: function (pageData, e) {
            if (this.callPageSettings) {
                this.callPageSettings('edit', [pageData.id, 0]);
            }
        },

        _deletePageClickHandler: function (pageData, e) {
            var handler, viewModel, confirmText, deleteText, cancelText;

            handler = this;
            viewModel = this._getViewModel();
            confirmText = this.resx.DeletePageConfirm;
            confirmText = confirmText.replace('[NAME]', pageData.name);
            deleteText = this.resx.Delete;
            cancelText = this.resx.Cancel;

            this.utility.confirm(confirmText, deleteText, cancelText, function () {
                handler._getService().post('DeletePage', { id: pageData.id }, function () {
                    if (pageData.id === handler._currentTabId) {
                         window.top.location.href = handler._siteRoot;
                         return;
                    }

                    handler.utility.notify(handler.resx['PageDeletedMessage']);

                    var position, level, parentId;

                    position = handler._findDataPosition(pageData.id);
                    level = position.level;
                    viewModel.pagesList.remove(function(data) {
                        return data.level > level;
                    });

                    handler._updatePageData(pageData.id, null); //doesn't transfer second parameter means delete it
                    viewModel.deletedPagesCount(viewModel.deletedPagesCount() + 1);

                    parentId = viewModel.pagesList()[level].parentId;
                    viewModel.selectedPage(parentId > -1 ? handler._findPageData(parentId) : handler._getEmptyPageData());

                    //reduce parent child count
                    if (parentId > -1) {
                        var parentPosition = handler._findDataPosition(parentId);
                        var cloneData = handler._clonePageData(handler._findPageData(parentId));
                        cloneData.childCount -= 1;
                        viewModel.pagesList()[parentPosition.level].pages.splice(parentPosition.index, 1, cloneData);
                    }
                    // if current page was deleted redirect to Home
                    if (window.top.location.pathname.substr(window.top.location.pathname.lastIndexOf("/") + 1) == pageData.url.substr(pageData.url.lastIndexOf("/") + 1)) {
                        $("#showsite").data('need-homeredirect', true);
                    } else {
                        $("#showsite").data('need-refresh', true);
                    }

                    setTimeout(function() {
                        handler._initScrollView(true);
                    }, 0);
                });
            });
        },

        _removeHiddenLists: function() {
            //Remove hidden levels
            $('.pages-list.removeMe').each(function removeHiddenLevels() {
                ko.cleanNode(this);
                $(this).remove();
            });
            //Remove empty lists
            $('.pages-list ul:not(:has(*))').parents('.pages-list').each(function removeEmptyLists() {
                ko.cleanNode(this);
                $(this).remove();
            });
        },

        _searchKeywordsChangedHandler: function (e) {
            var handler = this;

            if (this._doSearchTimeoutHandler) {
                clearTimeout(this._doSearchTimeoutHandler);
            }

            this._doSearchTimeoutHandler = setTimeout(function() {
                handler._searchPage();
            }, this.options.delayTime);

            return true;
        },

        _searchPage: function () {
            var viewModel = this._getViewModel();
            if (this.hasPendingChanges()) {
                viewModel.searchKeyword('');
                return this.handlePendingChanges();
            }

            viewModel.selectedPage(this._getEmptyPageData());

            this._loadRootPageList();
        },

        _inDragChanged: function (inDrag) {
            var handler = this;
			this._removeHiddenLists();
            this._removeListScrollView();

            this.container.find('.pages-list-container').width(20000);

            setTimeout(function () {
                handler._initScrollView(true);
            }, 300);
        },

        selectPageFromBreadCrumbs: function (pageData) {
            if (this._getViewModel().selectedPage().id != pageData.id) {
                this._needScrollToSelectedPage = true;
                this._getViewModel().selectedPage(pageData);
            }
        },

        _selectedPageChanged: function (newPage) {
            var handler, $listItem, data;

            handler = this;

            $listItem = $('li[data-page-id="' + newPage.id + '"]');
            data = ko.dataFor($listItem[0]);

            if (newPage.id > 0) {
                if (newPage.childCount > 0) {
                    this._getOverridableService().get('GetPageList', { parentId: newPage.id }, function(pageData) {
                        handler._addPagesList(newPage, pageData);
                    });
                } else {
                    handler._addPagesList(newPage, null);
                }
            }

            this._computeSelectedPagePath();
        },

        _selectedPagePathChanged(path) {
            this.selectedPagePathChanged(path);
        },

        // Select page which under hierarchy and need expand level by level.
        _selectPageInHierarchy: function() {
            var pageId, find, pageData;

            if (this._selectPageInHierarchyPath.length == 1) {
                this.selectPage(this._selectPageInHierarchyPath[0], true);
            } else {
                pageId = this._selectPageInHierarchyPath[0];
                this._selectPageInHierarchyPath.splice(0, 1);
                find = this._findDataPosition(pageId);
                if (find) {
                    pageData = this._getViewModel().pagesList()[find.level].pages()[find.index];
                    this._getViewModel().selectedPage(this._clonePageData(pageData));
                }
            }
        },

        // Doesn't transfer second parameter means delete it
        _updatePageData: function(id, newData) {
            var find, pages;

            find = this._findDataPosition(id);
            if (find !== null) {
                pages = this._getViewModel().pagesList()[find.level].pages;
                if (newData) {
                    pages.splice(find.index, 1, newData);
                } else {
                    pages.splice(find.index, 1);
                }
            }
        },

        _findDataPosition: function(pageId) {
            var viewModel, i, j, pagesList, listData;

            viewModel = this._getViewModel();

            for (i = 0; i < viewModel.pagesList().length; i++) {
                pagesList = viewModel.pagesList()[i].pages;
                listData = pagesList();

                for (j = 0; j < listData.length; j++) {
                    if (listData[j].id == pageId) {
                        return { level: i, index: j, parentId: listData[j].parentId, childCount: listData[j].childCount };
                    }
                }
            }
            return null;
        },

        _findPageData: function(pageId) {
            var viewModel, position;

            viewModel = this._getViewModel();
            position = this._findDataPosition(pageId);

            if (position !== null) {
                return viewModel.pagesList()[position.level].pages()[position.index];
            }

            return null;
        },

        _getEmptyPageData: function() {
            return {
                id:   0,
                parentId: 0,
                name: '',
                childCount: 0,
                isspecial: false
            };
        },

        _clonePageData: function(data) {
            var clone = {};

            for (var prop in data) {
                clone[prop] = data[prop];
            }  
            clone.timestamp = (new Date()).getTime();

            return clone;
        },

        _scrollToSelectedPage: function() {
            var selectedPage, $listItem, $pagesList, containerScroller;

            selectedPage = this._getViewModel().selectedPage();

            if (selectedPage.id > 0) {
                // Prevent scroll if click has been triggered by item-can-be-parent-list
                if ($(this.container.selector).find('.item-can-be-parent-list').length > 0) {return;}

                $listItem = this.container.find('li[data-page-id="' + selectedPage.id + '"]');
                $pagesList = $listItem.parents('.pages-list');
                containerScroller = this.container.find('.pages-list-scroller');

                if ($pagesList.hasClass('jspScrollable') && $pagesList.data('jsp')) {
                    $pagesList.data('jsp').scrollToElement($listItem[0], true, true);
                }

                if (containerScroller.hasClass('jspScrollable') && containerScroller.data('jsp')) {
                    containerScroller.data('jsp').scrollToElement($pagesList[0], true, true);
                }
            }
        },

        _pageExist: function(parentId) {
            var pagesList, i;

            pagesList = this._getViewModel().pagesList();

            for (i = 0; i < pagesList.length; i++) {
                if (pagesList[i].parentId == parentId) {
                    return true;
                }
            }

            return false;
        },

        _resizeContentContainer: function(resetContainer) {
            var $body = this.panel.closest(".dnn-persona-bar-page-body");             
            var pageHierarchyContainer = $body.find(".pagehierarchy-container");
            var restHeight = pageHierarchyContainer.position().top;

            this.container.css('height', $(window).height() - restHeight - 20);

            this._initScrollView(resetContainer);
        },

        _initScrollView: function (resetContainer, callback) {
            var handler, inDrag, $pagesList, bottomSpace, scrollContainer, totalWidth;

            handler = this;
            inDrag = this._getViewModel().inDrag();

            if (this._supressUI) {
                if (typeof callback == "function") {
                    callback.call(this);
                }

                return;
            }

            $pagesList = this.container.find('.pages-list-container div.pages-list');
            bottomSpace = 30;
            $pagesList.css('height', this.container.height() - bottomSpace).each(function() {
                var scrollContent = $(this);

                scrollContent.removeClass('animate');

                // Update ul opened height (space to drop)
                var padding = parseInt(scrollContent.css('padding-top')) + parseInt(scrollContent.css('padding-bottom'));
                if (padding == 0) { //jspPane may take the padding value
                    padding = parseInt(scrollContent.find('.jspPane').css('padding-top')) + parseInt(scrollContent.find('.jspPane').css('padding-bottom'));
                }

                scrollContent.find('ul:first').css({
                    'min-height': (scrollContent.innerHeight() - padding * 2) + 'px'
                });

                if (scrollContent.data('jsp')) {
                    scrollContent.data('jsp').reinitialise();
                } else {
                    scrollContent.jScrollPane({contentWidth: scrollContent.width()});
                }
            });

            scrollContainer = this.container.find('.pages-list-scroller');

            if (resetContainer) {
                // Reset the list container width
                totalWidth = 0;
                $pagesList.each(function() {
                    var self = $(this);
                    if (self.css('display') === 'none') {return true;} // Continue next elment if this is hidden
                    totalWidth += self.outerWidth() + parseInt(self.css('margin-left'));
                });
                this.container.find('.pages-list-container').width(totalWidth);

                if (scrollContainer.data('jsp')) {
                    scrollContainer.data('jsp').destroy();
                    scrollContainer = this.container.find('.pages-list-scroller');
                }

                scrollContainer.css('height', handler.container.height());
                scrollContainer.find('> .jspContainer .shadow').height($pagesList.outerHeight(true));
                scrollContainer.jScrollPane().on('jsp-scroll-x', function (e, offset, isLeft) {
                    var $this = $(this);
                    if (!isLeft) {
                        if ($this.find('> .jspContainer .shadow').length == 0) {
                            var $shadow = $('<div class="shadow" />');
                            $this.find('> .jspContainer').append($shadow);
                            $shadow.height($pagesList.outerHeight(true));
                        }
                    } else {
                        $this.find('> .jspContainer .shadow').remove();
                    }
                });
                handler.container.find('.pages-list-scroller > .jspContainer').css('height', handler.container.height());

                handler._scrollToSelectedPage();

                if (typeof callback === 'function') {
                    callback.call(this);
                }
            }

            setTimeout(function() {
                $pagesList.addClass('animate');
            }, 100);
        },

        _removeListScrollView: function() {
            this.container.find('.pages-list-container div.pages-list').each(function () {
                var scrollContent = $(this);
                if (scrollContent.data('jsp')) {
                    scrollContent.data('jsp').destroy();
                }
            });

            this.container.find('.pages-list-container div.pages-list').addClass('animate');
        },

        _initDrag: function () {
            var handler, viewModel, lists, droppables;

            handler = this;
            viewModel = this._getViewModel();

            lists = this.container.find('.pages-list ul');

            droppables = $('.pagehierarchy-container .pages-list ul li');

            droppables.droppable({
                hoverClass: 'item-can-be-parent-list',

                // Open childs on over (wating some time)
                over: function () {
                    var self = $(this);
                    setTimeout(function() {
                        if (self.hasClass('item-can-be-parent-list')) {
                            self.trigger('click'); // Triggers _addPagesList
                        }
                    }, OVER_TIME_TO_OPEN_PAGE_CHILDS);
                },

                // Drop directly on item
                drop: function (event, ui) {
                    var self, item;

                    // Prevent add to list (sortable) on drop on droppable element
                    if ($(event.target).hasClass('page-drag-target')) {
                        dropOnDroppable = false;
                        return;
                    }

                    dropOnDroppable = true;

                    if (!uiOnDragStart) {return;}

                    self = $(this);
                    if (self.hasClass('page-drag-target')) {return;}

                    item = jQuery.extend({}, uiOnDragStart.item);
                    uiOnDragStart = null;

                    function updateHirerarchy() {
                        var sourcePageId, sourceUl, sourceIndex, sourceFind, source, sourceData, targetId, targetIndex, targetFind, target;
                        var params, movePage200Callback, movePageErrorCallback;

                        pageDropped = {
                            item: $(ui.draggable[0]),
                            helper: $(ui.helper[0])
                        };

                        sourcePageId = item.data('page-id');
                        sourceUl     = item.parent();
                        sourceFind   = handler._findDataPosition(sourcePageId);
                        source       = viewModel.pagesList()[sourceFind.level].pages()[sourceFind.index];
                        sourceData   = handler._clonePageData(source);

                        targetId     = self.data('page-id');

                        if (targetId === sourceFind.parentId) {return;}

                        targetFind  = handler._findDataPosition(targetId);
                        target      = viewModel.pagesList()[targetFind.level].pages()[targetFind.index];

                        setTimeout(function removeItemAndCallToUpdate() {
                            var newPagesArray, allowDrop;

                            allowDrop = handler._allowDrop(sourcePageId, targetFind.level);
                            if (!allowDrop) {
                                handler.utility.notifyError(handler.resx.DragInvalid);
                                sourceUl.sortable('cancel');

                                // After call cancel method, refresh page data to let ko populate DOM tree.
                                viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 1, sourceData);
                                ko.cleanNode(item[0]);
                                item.remove();

                                return;
                            }

                            // Remove page from UI
                            sourceUl.sortable('cancel');
                            viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 1);
                            ko.cleanNode(item[0]);
                            item.remove();

                            // Add source page as first child of target page
                            params = {
                                PageId: sourcePageId,
                                RelatedPageId: -1, // Not necesary for parent
                                ParentId: targetId,
                                Action:   'parent'
                            };

                            movePage200Callback = function(data) {
                                var targetDataPosition, targetData, index, sourceParentId, sourceParentFind, sourceParentData;

                                if (data.Status > 0) {
                                    handler.utility.notifyError(handler.resx['Error_' + data.Message]);

                                    viewModel.pagesList()[sourceFind.level].pages.splice(sourceFind.index, 0, sourceData);

                                    draggingJqObj = null;
                                    uiOnDragStart = null;

                                    dropOnDroppable = false;

                                    return;
                                }

                                // No errors, add elemnt to UI
                                sourceData.url = data.Page.url;
                                sourceData.parentId = data.Page.parentId;

                                // Update target child count
                                // If element moved to element below in same list
                                // index will be target - 1
                                if (sourceFind.level === targetFind.level && sourceFind.index < targetFind.index) {
                                    index = targetFind.index - 1;
                                } else {
                                    index = targetFind.index;
                                }

                                targetData = handler._clonePageData(viewModel.pagesList()[targetFind.level].pages()[index]);
                                targetData.childCount += 1;
                                viewModel.pagesList()[targetFind.level].pages.splice(index, 1, targetData);


                                if (sourceFind.parentId != -1) {
                                    sourceParentFind = handler._findDataPosition(sourceFind.parentId);
                                    sourceParentData = handler._clonePageData(viewModel.pagesList()[sourceParentFind.level].pages()[sourceParentFind.index]);
                                    sourceParentData.childCount -= 1;
                                    viewModel.pagesList()[sourceParentFind.level].pages.splice(sourceParentFind.index, 1, sourceParentData);
                                }

                                // If target page is open add page because otherwise it will be painted on wrong level
                                if (self.hasClass('selected') && viewModel.pagesList()[data.Page.level] !== undefined) {
                                    viewModel.pagesList()[data.Page.level].pages.push(sourceData);
                                }

                                // ??
                                $("#showsite").data('need-refresh', true);

                                draggingJqObj = null;
                                uiOnDragStart = null;

                                // Reactivate droppables/sortables
                                setTimeout(function reinitDragAndDrop() {
                                    handler._initDrag();
                                }, 0);
                                viewModel.dragPage(handler._getEmptyPageData());
                                if (viewModel.isNew()) {
                                    viewModel.isNew(false);
                                    handler._enterEditMode(data.Page);
                                }

                                dropOnDroppable = false;
                            };

                            movePageErrorCallback = function (data) {
                                handler.utility.notifyError('Unknown error');

                                draggingJqObj = null;
                                uiOnDragStart = null;


                                viewModel.pagesList()[sourceFind.level].pages.splice(sourceIndex, 0, sourceData);

                                dropOnDroppable = false;

                                return;
                            };

                            // Tray to move by api call
                            handler._getService().post('MovePage', params, movePage200Callback, movePageErrorCallback);
                        }, 0);
                    }

                    updateHirerarchy();
                }
            });

            lists.each(function () {
                var $self = $(this), triggerClickPage;

                $self.sortable({
                    connectWith: '.pagehierarchy-container .pages-list ul',
                    dropOnEmpty: true,
                    cursor: '',
                    cursorAt: { left: 110, top: 10 },
                    handle: 'span.drag-area',
                    placeholder: 'page-drag-target',
                    tolerance: 'intersect',
                    revert: false,
                    helper: function (event, ui) {
                        var $dragItem, data;

                        $dragItem = $('<div class="page-drag-helper"></div>')
                                            .append('<span class="icon" />')
                                            .append('<span class="drag-page-name">'+ ui.find('span.field-name').html() + '</span>')
                                            .appendTo(handler.container);

                        data = ko.dataFor(ui[0]);

                        if (data && data.childCount > 0) {
                            $dragItem.addClass('page-drag-multiple');
                        }

                        return $dragItem;
                    },

                    start: function (event, ui) {
                        uiOnDragStart = {
                            item: $(ui.item)
                        };
                        draggingJqObj = $(ui.item[0]);
                        $self.data('ui-sa-ph', $self.data('ui-sortable').options.placeholder);
                        handler.container.find('div.pages-drag-container').hide();
                    },

                    beforeStop: function(event, ui) {
                        if (dropOnDroppable) {
                            uiOnDragStart = null;
                            dropOnDroppable = false;
                        }
                    },

                    receive: function (event, ui) {
                    },

                    over: function (event, ui) {
                    },

                    sort: function(event, ui) {
                        ui.placeholder.html(ui.helper.html());
                    },

                    stop: function (event, ui) {
                        var item, sourceParentFind,
                        isDragItem, $pageList, level, parentId, pageId, index, find,
                        movePageData, allowDrop, pageItem, moveAction, relatedPageId, params;

                        if (!uiOnDragStart) {return;}

                        item = jQuery.extend({}, uiOnDragStart.item);
                        uiOnDragStart = null;
                        draggingJqObj = null;

                        // Remove hidden lists created when dragging for prevent
                        // view change (list/deail list) revelating this lists
                        handler._removeHiddenLists();

                        // Means drag from drag container, which always need post data.
                        isDragItem = ui.item.find('> div').hasClass('drag-item');

                        $pageList = ui.item.parents('.pages-list:eq(0)');
                        level = $pageList.data('page-level');
                        parentId = $pageList.data('parent-id');
                        pageId = ui.item.data('page-id');
                        index = ui.item.parent().find('> li').index(ui.item);
                        find = handler._findDataPosition(pageId);

                        if (!isDragItem && find && find.level === level && find.index === index) {
                            return;
                        }

                        movePageData = viewModel.pagesList()[find.level].pages()[find.index];

                        allowDrop = handler._allowDrop(pageId, level);
                        if (!allowDrop) {
                            handler.utility.notifyError(handler.resx.DragInvalid);
                            $self.sortable('cancel');

                            // After call cancel method, refresh page data to let ko populate DOM tree.
                            viewModel.pagesList()[find.level].pages.splice(find.index, 1, handler._clonePageData(movePageData));
                            ko.cleanNode(ui.item[0]);
                            ui.item.remove();

                            return;
                        }

                        moveAction = "after";
                        relatedPageId = -1;

                        if (ui.item.prev().length === 0 && ui.item.next().length === 0) {
                            if (isDragItem && parentId === undefined) {
                                // If release drag item not in pane, then append it as last item in root.
                                ui.item.remove();
                                viewModel.inDrag(false);
                                pageItem = $('li[data-page-id="' + pageId + '"] > div');
                                pageItem.show();

                                handler._needScrollToSelectedPage = true;
                                viewModel.selectedPage(movePageData);
                                viewModel.dragPage(handler._getEmptyPageData());

                                return;
                            } else {
                                moveAction = "parent";
                            }

                        } else if (ui.item.prev().length === 0) {
                            moveAction = "before";
                            relatedPageId = ui.item.next().length > 0 ? ui.item.next().data('page-id') : relatedPageId;
                        } else  {
                            relatedPageId = ui.item.prev().data('page-id');
                        }

                        params = {
                            PageId: pageId,
                            RelatedPageId: relatedPageId,
                            ParentId: parentId,
                            Action: moveAction
                        };

                        handler._getService().post('MovePage', params, function(data) {
                            var pageData, sourceParentFindB, sourceParentData, targetId, targetFind, targetFindB;
                        
                            if (data.Status > 0) {
                                handler.utility.notifyError(handler.resx['Error_' + data.Message]);
                                $self.sortable('cancel');

                                // After call cancel method, refresh page data to let ko populate DOM tree.
                                viewModel.pagesList()[find.level].pages.splice(find.index, 1, handler._clonePageData(movePageData));
                                ko.cleanNode(ui.item[0]);
                                ui.item.remove();

                                return;
                            }

                            targetId = data.Page.parentId;
                            pageData = ko.dataFor(ui.item[0]);
                            pageData.url = data.Page.url;
                            pageData.parentId = data.Page.parentId;

                            sourceParentFind = handler._findDataPosition(find.parentId);
                            targetFind = handler._findDataPosition(targetId);
                            handler._updatePagePosition(pageData, level, index, find.parentId);
                            targetFindB = handler._findDataPosition(targetId);
                            sourceParentFindB = handler._findDataPosition(find.parentId);

                            // Update source parent child counter if not yet
                            if (find.parentId != -1 && sourceParentFind.childCount === sourceParentFindB.childCount) {
                                sourceParentData = handler._clonePageData(viewModel.pagesList()[sourceParentFind.level].pages()[sourceParentFind.index]);
                                sourceParentData.childCount -= 1;
                                viewModel.pagesList()[sourceParentFind.level].pages.splice(sourceParentFind.index, 1, sourceParentData);
                            }

                            // Update target parent child counter if not yet
                            if (targetFind && targetFind.childCount === targetFindB.childCount) {
                                sourceParentData = handler._clonePageData(viewModel.pagesList()[targetFind.level].pages()[targetFind.index]);
                                sourceParentData.childCount += 1;
                                viewModel.pagesList()[targetFind.level].pages.splice(targetFind.index, 1, sourceParentData);
                            }

                            if (isDragItem) {
                                handler._needScrollToSelectedPage = true;
                                viewModel.selectedPage(pageData);
                                viewModel.dragPage(handler._getEmptyPageData());

                                if (pageData.isCopy) {
                                    handler._enterEditMode(pageData);
                                }
                            }

                            $("#showsite").data('need-refresh', true);

                            ko.cleanNode(ui.item[0]);
                            ui.item.remove();

                            // Reactivate droppables/sortables
                            setTimeout(function reinitDragAndDrop() {
                                // $('.pages-list-container .pages-list li[data-page-id="' + pageId + '"]').removeClass('selected');
                                handler._initDrag();
                            }, 0);

                            // enter into Edit mode
                            if (viewModel.isNew() && data.Page.pageType === "normal") {
                                viewModel.isNew(false);
                                handler._enterEditMode(pageData);
                            }
                        });
                    }
                });

                // If we are in middle of dragging action, need to refresh
                // sortable elements to update lists conections
                if (draggingJqObj) {
                    setTimeout(function () {
                        $('.pages-list ul').each(function () {
                            $(this).sortable('refresh');
                        });

                        $self.data('ui-sortable').options.placeholder = $self.data('ui-sa-ph');
                    }, 0);
                }

                return true;
            });
        },

        _allowDrop: function(pageId, level) {
            if (level === 0) {
                return true;
            }

            var $pagesList = this.container.find('.pages-list-container .pages-list');
            var $currentList = this.container.find('.pages-list-container .pages-list[data-parent-id="' + pageId + '"]');

            return $currentList.length === 0 || $pagesList.index($currentList) > level;
        },

        _updatePagePosition: function(page, level, index, pId) {
            var viewModel, pagesList, originalPosition, originalLevel, parentId,
                addOffset, find, pageData;

            viewModel = this._getViewModel();
            pagesList = viewModel.pagesList();
            originalPosition = this._findDataPosition(page.id);

            // Page was moved in other function
            if (!originalPosition) {return;}

            originalLevel = originalPosition.level;

            pagesList[originalLevel].pages.splice(originalPosition.index, 1);
            pagesList[level].pages.splice(index, 0, page);

            // Update child count of parent page
            parentId = -1;
            addOffset = 0;

            if (level > originalLevel) { //move as child so need add parent's child count.
                parentId = pagesList[level].parentId;
                if (originalPosition.parentId !== pId) {
                    addOffset = 1;
                } else {
                    addOffset = 0;
                }
            } else if (level < originalLevel) { //move from child so need reduce parent's child count.
                parentId = pagesList[originalLevel].parentId;
                addOffset = -1;
            }

            if (parentId > 0) {
                find = this._findDataPosition(parentId);

                if (find) {
                    pageData = this._clonePageData(pagesList[find.level].pages()[find.index]);
                    pageData.childCount += addOffset;
                    pagesList[find.level].pages.splice(find.index, 1, pageData);

                    if (viewModel.selectedPage().id == pageData.id) {
                        viewModel.selectedPage(pageData);
                    }
                }
            }

            if (originalLevel != level && viewModel.selectedPage().id == page.id) {

                viewModel.pagesList.remove(function(data) {
                    return data.level >= originalLevel;
                });
                //viewModel.selectedPage(this._getEmptyPageData()); // cannot read property 'nodeType' of undefined
                viewModel.selectedPage(page);
            }

            this._initScrollView();
        },

        _computeSelectedPagePath: function() {
            var selectedPage, selectedPagePath, find, i, parentId, $pageItem, pageData;

            selectedPage = this._getViewModel().selectedPage();
            selectedPagePath = this._getViewModel().selectedPagePath;

            var selectedPagePathLenght = selectedPagePath().length;
            if (!selectedPagePathLenght) {
                selectedPagePath.push(selectedPage);
                return;    
            }

            while (selectedPagePathLenght > 0 && 
                   selectedPagePath()[selectedPagePathLenght - 1].id !== selectedPage.parentId) {
                selectedPagePath.pop();
                -- selectedPagePathLenght;           
            }
            
            selectedPagePath.push(selectedPage);
        },

        _getViewModel: function() {
            var handler = this;

            if (typeof this._viewModel.pagesList == "undefined") {
                this._viewModel.itemTemplate = ko.observable("pages-list-item-template");
                this._viewModel.dragItemTemplate = ko.observable("pages-drag-item-template");
                this._viewModel.pagesList = ko.observableArray([]);
                this._viewModel.resx = handler.resx;
                this._viewModel.selectedPage = ko.observable(handler._getEmptyPageData());
                this._viewModel.dragPage = ko.observable({id: 0, name: '', status: '', childCount: 0});
                this._viewModel.inDrag = ko.observable(true);
                this._viewModel.isNew = ko.observable(false);
                this._viewModel.selectedPagePath = ko.observableArray([]);
                this._viewModel.selectedPagePath.subscribe($.proxy(this._selectedPagePathChanged, this));
                this._viewModel.searchKeyword = ko.observable('');
                this._viewModel.searchFocus = ko.observable(true);
                this._viewModel.deletedPagesCount = ko.observable(0);

                this._viewModel.selectedPage.subscribe($.proxy(this._selectedPageChanged, this));
                this._viewModel.inDrag.subscribe($.proxy(this._inDragChanged, this));
                
                this._viewModel.pageItemClick = $.proxy(this._pageItemClickHandler, this);
                this._viewModel.viewPageClick = $.proxy(this._viewPageClickHandler, this);
                this._viewModel.editPageClick = $.proxy(this._editPageClickHandler, this);
                this._viewModel.settingsPageClick = $.proxy(this._settingsPageClickHandler, this);
                this._viewModel.deletePageClick = $.proxy(this._deletePageClickHandler, this);

                this._viewModel.doSearch = $.proxy(this._searchPage, this);

                if (window.dnn.pages.viewModelExtension) {
                    for (var prop in window.dnn.pages.viewModelExtension) {
                        if (typeof window.dnn.pages.viewModelExtension[prop] === 'function') {
                            this._viewModel[prop] = $.proxy(window.dnn.pages.viewModelExtension[prop], this);
                        } else {
                            this._viewModel[prop] = window.dnn.pages.viewModelExtension[prop];
                        }
                    }                
                }
            }
            return this._viewModel;
        },       
        _getOverridableService() {
            this.utility.sf.moduleRoot = "PersonaBar";
            this.utility.sf.controller =  window.dnn.pages.apiController;

            return this.utility.sf;
        },
        _getService() {
            this.utility.sf.moduleRoot = "PersonaBar";
            this.utility.sf.controller = "Pages";

            return this.utility.sf;
        }
    };    

    pageHierarchyDefaultOptions = {
        delayTime: 500,
        requestDelayTime: 2000,
        requestTimeout: 4000
    };

window.dnn.pages.pageHierarchyManager = new pageHierarchyManager(window.dnn.pages.pageHierarchyManagerOptions);
module.exports = {
    pageHierarchyManager: window.dnn.pages.pageHierarchyManager
};
