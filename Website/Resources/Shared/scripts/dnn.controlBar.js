if (typeof dnn === 'undefined') dnn = {};
dnn.controlBar = dnn.controlBar || {};
dnn.controlBar.init = function (settings) {
    dnn.controlBar.selectedPage = null,
    dnn.controlBar.selectedModule = null;
    dnn.controlBar.addNewModule = true;
    dnn.controlBar.addingModule = false;
    dnn.controlBar.addModuleDataVar = null;
    dnn.controlBar.isMouseDown = false;
    dnn.controlBar.hideModuleLocationMenu = true;
    dnn.controlBar.showSelectedModule = false;
    dnn.controlBar.status = null;
    dnn.controlBar.scrollBarFixedSize = 980;
    
    //Lazy loading settings
    dnn.controlBar.moduleLoadingPageIndex = 0;
    dnn.controlBar.moduleLoadingInitialPageSize = 15;
    dnn.controlBar.moduleLoadingSize = 7;
    dnn.controlBar.allModulesLoaded = false;
    dnn.controlBar.forceLoadingPane = false;
    dnn.controlBar.forceCategorySelection = false;
    dnn.controlBar.forceCategorySearch = false;
    dnn.controlBar.scrollTrigger = true;

    //Search Module settings
    dnn.controlBar.minInputLength = 2;
    dnn.controlBar.inputDelay = 400;    
    dnn.controlBar.lastVal = '';
    dnn.controlBar.searchTimeout = null;

    //Bookmark Modules settings
    dnn.controlBar.bookmarkModuleCategory = settings.bookmarkModuleCategory;
    dnn.controlBar.bookmarkedModuleKeys = settings.bookmarkedModuleKeys.split(',');
    
    //Scrolling when dragging
    dnn.controlBar.initialDragPosition = null;
    dnn.controlBar.initialScrollPosition = null;
    dnn.controlBar.arrowScrollingContainer = null;    
    dnn.controlBar.arrowScrollingButtonSpeed = 10;
    dnn.controlBar.mouseWheelScrollingContainer = null;
    
    dnn.controlBar.getService = function () {
        return $.dnnSF();
    };
    dnn.controlBar.getServiceUrl = function (service) {
        service = service || dnn.controlBar.getService();
        return service.getServiceRoot('internalservices') + 'controlbar/';
    };
    
    dnn.controlBar.getPageServiceUrl = function (service) {
        service = service || dnn.controlBar.getService();
        return service.getServiceRoot('internalservices') + 'PageService/';
    };

    dnn.controlBar.getSiteRoot = function () {
        return dnn.getVar("sf_siteRoot", "/");
    };
    dnn.controlBar.saveStatus = function () {
        var categoryComboVal = $find(settings.categoryComboId).get_value();
        var selectedPageId = dnn.controlBar.selectedPage ? dnn.controlBar.selectedPage.id : '';
        var selectedPageName = dnn.controlBar.selectedPage ? dnn.controlBar.selectedPage.name : '';
        var visibilityComboVal = $find(settings.visibilityComboId).get_value();
        var moduleLoadIndex = dnn.controlBar.moduleLoadingPageIndex;
        var forceScrollX = dnn.controlBar.getCurrentScrollPositionX($('#ControlBar_ModuleListHolder_NewModule').next().data('jsp'));
        var searchTerm = dnn.controlBar.getSearchTermValue();
        var persistValue = [
                                dnn.controlBar.addNewModule,
                                categoryComboVal,
                                selectedPageId,
                                selectedPageName,
                                visibilityComboVal,
                                moduleLoadIndex,
                                forceScrollX,
                                searchTerm
                            ].join('|');
        dnn.dom.setCookie('ControlBarInitStatus', persistValue, 0, dnn.controlBar.getSiteRoot());
        dnn.controlBar.status = null;
    };
    dnn.controlBar.loadStatus = function () {
        if (dnn.controlBar.status) {
            return;
        }

        var persistValue = dnn.dom.getCookie('ControlBarInitStatus');
        if (persistValue) {
            var persits = persistValue.split('|');
            dnn.controlBar.status = {
                addNewModule: persits[0] == 'true',
                category: persits[1],
                pageId: persits[2],
                pageName: persits[3],
                visibility: persits[4],
                moduleLoadIndex: persits[5],
                forceScrollX: persits[6],
                searchTerm: persits[7]
            };
        }
        else {
            dnn.controlBar.status = null;
        }
        dnn.dom.setCookie('ControlBarInitStatus', '', -1, dnn.controlBar.getSiteRoot());
    };
    dnn.controlBar.removeStatus = function () {
        dnn.controlBar.status = null;
        dnn.dom.setCookie('ControlBarInitStatus', '', -1, dnn.controlBar.getSiteRoot());
    };
    dnn.controlBar.responseError = function (xhr) {
        if (xhr) {
            if (xhr.status == '401') {
                dnnModal.show(settings.loginUrl + (settings.loginUrl.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 300, 650, true, '');
            }
        }
    };
    dnn.controlBar.getBookmarkItems = function (ul) {
        var items = [];
        $('li', ul).each(function () {
            var tabname = $(this).data('tabname');
            if (tabname)
                items.push(tabname);
        });
        return items.join(',');
    };

    dnn.controlBar.saveBookmarkModules = function (title, bookmarkModules, $bookmarkLink, removedBookmarkModuleId) {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'SaveBookmark',
            type: 'POST',
            data: { Title: title, Bookmark: bookmarkModules },
            beforeSend: service.setModuleHeaders,
            success: function () {
                $bookmarkLink.addClass("hideBookmark");
                if (dnn.controlBar.isBookmarkModuleCategorySelected()) {
                    var $moduleItem = $("#ControlBar_Module_AddNewModule ul.ControlBar_ModuleList .ControlBar_ModuleDiv[data-module=" + removedBookmarkModuleId + "]");
                    if ($moduleItem) {
                        $moduleItem.parent().hide();
                    }
                } else {
                    $find(settings.categoryComboId).findItemByValue(dnn.controlBar.bookmarkModuleCategory).select();
                }
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    };

    dnn.controlBar.saveBookmark = function (title, ul) {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        var bookmark = dnn.controlBar.getBookmarkItems(ul);
        $.ajax({
            url: serviceUrl + 'SaveBookmark',
            type: 'POST',
            data: { Title: title, Bookmark: bookmark },
            beforeSend: service.setModuleHeaders,
            success: function () {
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    };

    dnn.controlBar.showModuleListLoading = function(containerId, showPanel, showNoResultMessage) {
        var loadingContainer = $(containerId);
        var shouldShowMessage = showNoResultMessage;
        if (dnn.controlBar.isFirstModuleLoadingPage()) {
            loadingContainer.removeClass("NextElements");
        } else {
            loadingContainer.addClass("NextElements");
            shouldShowMessage = shouldShowMessage && false;
        }
        dnn.controlBar.setModuleListLoading(containerId, !showPanel, shouldShowMessage);
    };

    dnn.controlBar.setModuleListLoading = function (containerId, hideLoading, showNoResultMessage) {
        var loadingContainer = $(containerId);
        var messageContainer = loadingContainer.prev();
        var listContainer = loadingContainer.next();
        var scrollContainer = listContainer.next();
        if (hideLoading) {
            if (showNoResultMessage) {
                listContainer.hide();
                scrollContainer.hide();
                loadingContainer.hide();
                messageContainer.show();

                $('p.ControlBar_ModuleListMessage_InitialMessage', messageContainer).hide();
                $('p.ControlBar_ModuleListMessage_NoResultMessage', messageContainer).show();

            }
            else {
                listContainer.show();
                scrollContainer.show();
                loadingContainer.hide();
                messageContainer.hide();
            }
        } else {
            if (dnn.controlBar.isFirstModuleLoadingPage()) {
                listContainer.hide();
            } else {
                listContainer.show();
            }
            scrollContainer.hide();
            loadingContainer.show();
            messageContainer.hide();
        }
    };

    dnn.controlBar.getModuleLoadingCurrentIndex = function() {
        if (dnn.controlBar.isFirstModuleLoadingPage()) {
            return 0;
        } else {
            return dnn.controlBar.moduleLoadingInitialPageSize + (dnn.controlBar.moduleLoadingSize * (dnn.controlBar.moduleLoadingPageIndex -1));
        }
    };
    
    dnn.controlBar.getModuleLoadingCurrentPageSize = function () {
        if (dnn.controlBar.isFirstModuleLoadingPage()) {
            if (dnn.controlBar.forceModuleLoadIndex) {
                return dnn.controlBar.moduleLoadingInitialPageSize + dnn.controlBar.forceModuleLoadIndex * dnn.controlBar.moduleLoadingSize;
            }
            return dnn.controlBar.moduleLoadingInitialPageSize;            
        } else {
            return dnn.controlBar.moduleLoadingSize;
        }
    };

    dnn.controlBar.isFirstModuleLoadingPage = function() {
        return (dnn.controlBar.moduleLoadingPageIndex == 0);
    };

    dnn.controlBar.getWaitingTime = function(startDate) {
        var minimumTimeToLoading = 300;
        var elapsedTime = new Date() - startDate;
        var waitingTime = elapsedTime > minimumTimeToLoading ? 0 : minimumTimeToLoading - elapsedTime;

        return waitingTime;
    };

    dnn.controlBar.getDesktopModulesForNewModule = function (category, val) {
        if (dnn.controlBar.allModulesLoaded) {
            return;
        }
        if (!dnn.controlBar.isFirstModuleLoadingPage()) {
            dnn.controlBar.scrollTrigger = false;
        }
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);                
        dnn.controlBar.showModuleListLoading('#ControlBar_ModuleListWaiter_NewModule', true);
        var startDate = new Date();
        var currentIndex = dnn.controlBar.getModuleLoadingCurrentIndex();
        var pageSize = dnn.controlBar.getModuleLoadingCurrentPageSize();
        $.ajax({
            url: serviceUrl + 'GetPortalDesktopModules',
            type: 'GET',
            data: 'category=' + category + '&loadingStartIndex=' + currentIndex + '&loadingPageSize=' + pageSize + '&searchTerm=' +val,
            beforeSend: service.setModuleHeaders,
            success: function (d) {
                setTimeout(function() {
                    if (d && d.length) {                        
                        dnn.controlBar.showModuleListLoading('#ControlBar_ModuleListWaiter_NewModule', false);
                        dnn.controlBar.forceLoadingPane = false;
                        dnn.controlBar.setLoadingModulesMessage(settings.loadingModulesMessage);                        
                        var containerId = '#ControlBar_ModuleListHolder_NewModule';
                        if ((dnn.controlBar.isFirstModuleLoadingPage() && d.length < dnn.controlBar.moduleLoadingInitialPageSize) || (!dnn.controlBar.isFirstModuleLoadingPage() && d.length < dnn.controlBar.moduleLoadingSize)) {
                            dnn.controlBar.allModulesLoaded = true;
                        }

                        if (dnn.controlBar.isFirstModuleLoadingPage()
                                && d.length > dnn.controlBar.moduleLoadingInitialPageSize
                                && (d.length - dnn.controlBar.moduleLoadingInitialPageSize) % dnn.controlBar.moduleLoadingSize != 0) {
                                dnn.controlBar.allModulesLoaded = true;
                        }

                        dnn.controlBar.renderModuleList(containerId, d);
                        if (dnn.controlBar.isFirstModuleLoadingPage() && d.length > dnn.controlBar.moduleLoadingInitialPageSize) {
                            var pageCount = d.length - dnn.controlBar.moduleLoadingInitialPageSize;
                            if (pageCount % dnn.controlBar.moduleLoadingSize == 0) {
                                dnn.controlBar.moduleLoadingPageIndex = parseInt(pageCount / dnn.controlBar.moduleLoadingSize) + 1;
                            } else {
                                dnn.controlBar.moduleLoadingPageIndex = parseInt(pageCount / dnn.controlBar.moduleLoadingSize) + 2;
                            }
                            
                        } else {
                            dnn.controlBar.moduleLoadingPageIndex = dnn.controlBar.moduleLoadingPageIndex + 1;
                        }
                        
                    } else {
                        if (dnn.controlBar.getSelectedCategory() == settings.defaultCategoryValue) {
                            dnn.controlBar.showModuleListLoading('#ControlBar_ModuleListWaiter_NewModule', false, true);
                            dnn.controlBar.setLoadingModulesMessage(settings.loadingModulesMessage);
                            dnn.controlBar.allModulesLoaded = true;
                        } else {
                            dnn.controlBar.setLoadingModulesMessage(String.format(settings.loadingModulesOnNoDefaultCategoryMessage, dnn.controlBar.getSelectedCategory()));
                            dnn.controlBar.forceCategorySearch = true;
                            $find(settings.categoryComboId).findItemByValue(settings.defaultCategoryValue).select();
                        }
                        
                    }
                }, dnn.controlBar.getWaitingTime(startDate));

            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
                if (!dnn.controlBar.isFirstModuleLoadingPage()) {
                    dnn.controlBar.scrollTrigger = true;
                }
            }
        });
    };

    dnn.controlBar.setLoadingModulesMessage = function(message) {
        var $loadingMessageElement = $("#" + settings.loadingModulesId);
        $loadingMessageElement.text(message);
    };

    dnn.controlBar.getSearchTermValue = function() {
        var $searchInput = $("#" + settings.searchInputId);
        return $searchInput.val();
    };

    dnn.controlBar.initModuleSearch = function() {
        var $searchInput = $("#" + settings.searchInputId);

        $searchInput.mouseup(function () {
            return false;
        }).keypress(function (e) {
            if (e.keyCode == 13) {
                var val = $(this).val();
                clearTimeout(dnn.controlBar.searchTimeout);
                dnn.controlBar.searchModules(val);                
                return false;
            }
        }).keyup(function (e) {
            var val = $(this).val();
            if (dnn.controlBar.lastVal.length != val.length &&
                (val.length == 0 || val.length >= dnn.controlBar.minInputLength)) {
                clearTimeout(dnn.controlBar.searchTimeout);
                dnn.controlBar.searchTimeout = setTimeout(function () {
                    dnn.controlBar.searchModules(val);
                }, dnn.controlBar.inputDelay);
            }
            dnn.controlBar.lastVal = val;
        });

        var $searchButton = $("#ControlBar_Module_AddNewModule .search-container .search-button");
        $searchButton.click(function() {
            dnn.controlBar.searchModules(dnn.controlBar.getSearchTermValue());
        });

        var $clearButton = $("#ControlBar_Module_AddNewModule .search-container .clear-button");
        $clearButton.click(function() {
            dnn.controlBar.resetModuleSearch();
            dnn.controlBar.searchModules(dnn.controlBar.getSearchTermValue());
        });
    };
    dnn.controlBar.initModuleSearch();

    dnn.controlBar.searchModules = function (val) {
        dnn.controlBar.moduleLoadingPageIndex = 0;
        dnn.controlBar.allModulesLoaded = false;
        dnn.controlBar.getDesktopModulesForNewModule(dnn.controlBar.getSelectedCategory(), val);
    };
    
    dnn.controlBar.getTabModules = function (tab) {
        dnn.controlBar.resetModuleSearch();        
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);        
        dnn.controlBar.setModuleListLoading('#ControlBar_ModuleListWaiter_ExistingModule');
        $.ajax({
            url: serviceUrl + 'GetTabModules',
            type: 'GET',
            data: 'tab=' + tab,
            beforeSend: service.setModuleHeaders,
            success: function (d) {
                if (d && d.length) {
                    dnn.controlBar.setModuleListLoading('#ControlBar_ModuleListWaiter_ExistingModule', true);
                    var containerId = '#ControlBar_ModuleListHolder_ExistingModule';
                    dnn.controlBar.allModulesLoaded = true;
                    dnn.controlBar.renderModuleList(containerId, d);
                    dnn.controlBar.moduleLoadingPageIndex = dnn.controlBar.moduleLoadingPageIndex + 1;
                }
                else {
                    dnn.controlBar.setModuleListLoading('#ControlBar_ModuleListWaiter_ExistingModule', true, true);
                }
                dnn.controlBar.allModulesLoaded = true;
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    };
    dnn.controlBar.addModule = function (module, page, pane, position, sort, visibility, addExistingModule, copyModule) {
        var dataVar = {
            Module: module,
            Page: page,
            Pane: pane,
            Position: position, Sort: sort,
            Visibility: visibility,
            AddExistingModule: addExistingModule,
            CopyModule: copyModule
        };
        var sharing = (dnn.getVar('moduleSharing') || 'false') == 'true';

        if (sharing && !dnn.controlBar.addNewModule) {
            var selectedTabId = page;
            var selectedModuleId = module;

            var parameters = {
                ModuleId: selectedModuleId,
                TabId: selectedTabId
            };

            var moduleShareableUrl = $.dnnSF().getServiceRoot('internalservices') + 'ModuleService/GetModuleShareable';

            $.ajax({
                url: moduleShareableUrl,
                type: 'GET',
                async: false,
                data: parameters,
                success: function (m) {
                    if (!m) {
                        dnn.controlBar.addingModule = false;
                        return;
                    }

                    if (m.RequiresWarning) {
                        dnn.controlBar.popupShareableWarning();
                        dnn.controlBar.addModuleDataVar = dataVar;
                    } else {
                        dnn.controlBar.doAddModule(dataVar);
                    }
                },
                error: function (xhr) {
                    dnn.controlBar.addingModule = false;
                    dnn.controlBar.responseError(xhr);
                }
            });
        }
        else
            dnn.controlBar.doAddModule(dataVar);
    };

    dnn.controlBar.doAddModule = function (dataVar) {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'AddModule',
            type: 'POST',
            data: dataVar,
            beforeSend: service.setModuleHeaders,
            success: function (d) {
                dnn.controlBar.addingModule = false;
                dnn.dom.setCookie('FadeModuleID', d.TabModuleID, 0, dnn.controlBar.getSiteRoot());
                // save status to restore add module panel when page refresh
                dnn.controlBar.saveStatus();
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.addingModule = false;
                dnn.controlBar.responseError(xhr);
            }
        });
    };

    dnn.controlBar.clearHostCache = function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'ClearHostCache',
            type: 'POST',
            beforeSend: service.setModuleHeaders,
            success: function () {
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    };

    dnn.controlBar.copyPermissionsToChildren = function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'CopyPermissionsToChildren',
            type: 'POST',
            beforeSend: service.setModuleHeaders,
            success: function () {
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });

    };

    dnn.controlBar.publishPage = function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getPageServiceUrl(service);
        var requestData = { Publish: settings.publishedPage != "true" };
        $.ajax({
            url: serviceUrl + 'PublishPage',
            type: 'POST',
            data: requestData,
            beforeSend: service.setModuleHeaders,
            success: function () {
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });

    };

    dnn.controlBar.recycleAppPool = function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'RecycleApplicationPool',
            type: 'POST',
            beforeSend: service.setModuleHeaders,
            success: function () {
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    };

    dnn.controlBar.switchSite = function (site) {
        if (site) {
            var dataVar = { Site: site };
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'SwitchSite',
                type: 'POST',
                data: dataVar,
                beforeSend: service.setModuleHeaders,
                success: function (d) {
                    if (d && d.RedirectURL)
                        window.location.href = d.RedirectURL;
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });
        }
    };

    dnn.controlBar.switchLanguage = function (language) {
        if (language) {
            var dataVar = { Language: language };
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'SwitchLanguage',
                type: 'POST',
                data: dataVar,
                beforeSend: service.setModuleHeaders,
                success: function () {
                    window.location.href = window.location.href.split('#')[0];
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });
        }
    };

    dnn.controlBar.popupShareableWarning = function () {
        $('#shareableWarning').dialog({
            autoOpen: true,
            resizable: false,
            modal: true,
            width: '500px',
            zIndex: 1000,
            stack: false,
            title: settings.moduleShareableTitle,
            dialogClass: 'dnnFormPopup dnnClear',
            open: function () {
            },
            close: function () {
            }
        });
    };

    dnn.controlBar.hideShareableWarning = function () {
        $('#shareableWarning').dialog('close');
    };

    dnn.controlBar.isActiveElementEmpty = function() {
        var $activeElement = $(document.activeElement);
        if ($activeElement && $activeElement.length > 0 && ($activeElement.get(0).tagName == "INPUT" || $activeElement.get(0).tagName == "TEXTAREA")) {
            if ($activeElement.val().length == 0) {
                return true;
            } else {
                return false;
            }
        }
        return true;
    };

    dnn.controlBar.initialiseArrowScrolling = function(container) {
        dnn.controlBar.arrowScrollingContainer = container;
    };

    dnn.controlBar.initialiseMouseWheelScrolling = function (container) {
        dnn.controlBar.mouseWheelScrollingContainer = container.parent();
    };

    dnn.controlBar.isValidArrowScrollingContainer = function() {
        if (dnn.controlBar.arrowScrollingContainer && dnn.controlBar.arrowScrollingContainer.is(":visible")) {
            return true;
        }
        return false;
    };

    dnn.controlBar.isValidMouseWheelScrollingContainer = function(targetElement) {
        if (dnn.controlBar.mouseWheelScrollingContainer && dnn.controlBar.mouseWheelScrollingContainer.has(targetElement).length > 0) {
            return true;
        }
        return false;
    }

    dnn.controlBar.getWheelEventDelta = function(event) {
        if (event.deltaY) {
            return event.deltaY;
        }
        if (event.wheelDelta) {
            return event.wheelDelta * (-1); //IE uses wheelDelta property
        }
        return event.detail;
    };

    dnn.controlBar.moveScrollingContainer = function (direction, speed) {
        if (!speed) {
            speed = 1;
        }
        if (!dnn.controlBar.scrollTrigger) {
            return;
        }
        var xOffset = direction == "left" ? dnn.controlBar.arrowScrollingButtonSpeed * (-1) * speed: (direction == "right" ? dnn.controlBar.arrowScrollingButtonSpeed * speed : 0);
        var scrollContainer = dnn.controlBar.arrowScrollingContainer.next();
        var jspapi = scrollContainer.data('jsp');
        if (jspapi && xOffset != 0) {
            jspapi.scrollByX(xOffset);
        }
    };

    dnn.controlBar.moveArrowScrollingContainer = function (keyCode) {        
        if (keyCode == 37) {
            dnn.controlBar.moveScrollingContainer("left", 1);
        }else if (keyCode == 39) {
            dnn.controlBar.moveScrollingContainer("right", 1);
        }        
    };

    dnn.controlBar.moveMouseWheelScrollingContainer = function(delta) {
        if (delta < 0) {
            dnn.controlBar.moveScrollingContainer("left", 3);
        } else {
            dnn.controlBar.moveScrollingContainer("right", 3);
        }
    };
    
    dnn.controlBar.addBookmarkModule = function(moduleId) {
        if (dnn.controlBar.bookmarkedModuleKeys.indexOf(moduleId) < 0) {
            dnn.controlBar.bookmarkedModuleKeys.push(moduleId);
        }
        return dnn.controlBar.bookmarkedModuleKeys.join(',');
    };

    dnn.controlBar.removeBookmarkModule = function (moduleId) {
        var index = dnn.controlBar.bookmarkedModuleKeys.indexOf(moduleId);
        if (index >= 0) {
            dnn.controlBar.bookmarkedModuleKeys.splice(index,1);
        }
        return dnn.controlBar.bookmarkedModuleKeys.join(',');
    };

    dnn.controlBar.isBookmarkModuleCategorySelected = function() {
        return (dnn.controlBar.getSelectedCategory() == dnn.controlBar.bookmarkModuleCategory);
    };

    dnn.controlBar.getBookmarkClass = function (bookmarked, existsInBookmarkCategory) {
        if (existsInBookmarkCategory) {
            return 'bookmark hideBookmark'; //Modules in Common/Bookmark category must not be bookmarked
        }

        if (!dnn.controlBar.isBookmarkModuleCategorySelected()) {
            return bookmarked ? 'bookmark hideBookmark' : 'bookmark';
        } else {
            return bookmarked ? 'removeBookmark' : 'bookmark hideBookmark';
        }
    };

    dnn.controlBar.getBookmarkTooltip = function(bookmarkClass) {
        if (bookmarkClass.indexOf('removeBookmark') >= 0) {
            return settings.removeBookmarksTip;
        }
        return settings.addToBookmarksTip;
    };

    dnn.controlBar.fillModuleList = function(ul, moduleList) {
        if (dnn.controlBar.isFirstModuleLoadingPage()) {
            ul.empty().css('left', 1000);
        }
        $('li.moreModules', ul).remove();
        for (var i = 0; i < moduleList.length; i++) {
            var bookmarkClass = dnn.controlBar.getBookmarkClass(moduleList[i].Bookmarked, moduleList[i].ExistsInBookmarkCategory);
            var bookmarkTooltip = dnn.controlBar.getBookmarkTooltip(bookmarkClass);
            ul.append('<li><div class="ControlBar_ModuleDiv" data-module=' + moduleList[i].ModuleID + '><div class="ModuleLocator_Menu"></div><a href="javascript:void(0)" class="' + bookmarkClass + '" title="' + bookmarkTooltip + '"/><img src="' + moduleList[i].ModuleImage + '" alt="" /><span>' + moduleList[i].ModuleName + '</span></div></li>');
        }
    };

    dnn.controlBar.cleanViewPort = function() {
        $(".viewportSeparator").each(function() {
            $(this).get(0).style.display = '';
        });            
    };

    dnn.controlBar.setViewPort = function(windowWidth, margin) {
        var viewportRelativeSize = Math.round(margin * 100 / windowWidth);
        $(".viewportSeparator").css("width", viewportRelativeSize + "%").show();
    };

    dnn.controlBar.getScrollBarMargin = function(windowWidth) {
        return Math.round((windowWidth - dnn.controlBar.scrollBarFixedSize) / 2);
    };

    dnn.controlBar.renderModuleList = function (containerId, moduleList) {

        var currentModuleLoadingPageIndex = dnn.controlBar.moduleLoadingPageIndex;
        var container = $(containerId);
        dnn.controlBar.initialiseArrowScrolling(container);
        dnn.controlBar.initialiseMouseWheelScrolling(container);

        var scrollContainer = container.next();        
        if (dnn.controlBar.isFirstModuleLoadingPage()) {
            var api = scrollContainer.data('jsp');
            if (api) {
                api.scrollToX(0, null);
                api.destroy();
            }
            scrollContainer = container.next(); // reinit because api destroy...    
        }
        
        var ul = $('ul.ControlBar_ModuleList', container);
        dnn.controlBar.fillModuleList(ul, moduleList);
        
        $('#ControlBar_Module_ModulePosition').hide();

        // some math here
        var windowWidth = $(window).width();
        var margin = dnn.controlBar.getScrollBarMargin(windowWidth);
        var ulWidth = (dnn.controlBar.getModuleLoadingCurrentIndex() * 160) + moduleList.length * 160 + (dnn.controlBar.allModulesLoaded ? margin : 0);
        ul.css('width', ulWidth + 'px');        
        dnn.controlBar.setViewPort(windowWidth, margin);
        var dummyScrollWidth = Math.round((dnn.controlBar.scrollBarFixedSize * (ulWidth + margin)) / windowWidth);
        var ulLeft = margin;        
        var oldX = 0;
        var reloading = false;
        var isFirstModuleLoadingPage = dnn.controlBar.isFirstModuleLoadingPage();
        var modulesInitFunc = function () {
            $('div.controlBar_ModuleListScrollDummy_Content', scrollContainer).css('width', dummyScrollWidth);
            if (currentModuleLoadingPageIndex == 0) {
                scrollContainer.jScrollPane({'arrowButtonSpeed': dnn.controlBar.arrowScrollingButtonSpeed});
            } else {
                var jspapi = scrollContainer.data('jsp');
                if (jspapi) {                    
                    dnn.controlBar.scrollTrigger = false;
                    jspapi.reinitialise();
                    dnn.controlBar.scrollTrigger = true;                    
                }
            }
            var $searchInput = $("#" + settings.searchInputId);
            $searchInput.focus();
            
            scrollContainer.unbind('jsp-scroll-x');
            scrollContainer.bind('jsp-scroll-x', function (e, x, isAtleft, isAtRight) {                
                var xOffset, leftOffset;
                if (!dnn.controlBar.scrollTrigger) {
                    return;
                }
                if (isAtleft) {
                    oldX = 0;
                    ulLeft = margin;
                    reloading = false;                    
                } else if (isAtRight) {
                    var justAtRight = (oldX == x);
                    oldX = Math.round((dnn.controlBar.scrollBarFixedSize * (ulWidth + margin)) / windowWidth) - dnn.controlBar.scrollBarFixedSize;                         
                    if (moduleList.length != 0 && justAtRight && !reloading) {                        
                        reloading = true;                        
                        dnn.controlBar.getDesktopModulesForNewModule(dnn.controlBar.getSelectedCategory(), dnn.controlBar.getSearchTermValue());
                    }
                    ulLeft = -(ulWidth - windowWidth);
                } else {
                    reloading = false;
                    if (x > oldX) {                        
                        // scroll to right
                        xOffset = x - oldX;
                        leftOffset = ((ulWidth + margin) / ((dnn.controlBar.scrollBarFixedSize * (ulWidth + margin)) / windowWidth)) * xOffset;
                        ulLeft -= Math.abs(leftOffset);
                    } else {                        
                        // scroll to left
                        xOffset = oldX - x;
                        leftOffset = ((ulWidth + margin) / ((dnn.controlBar.scrollBarFixedSize * (ulWidth + margin)) / windowWidth)) * xOffset;
                        ulLeft += Math.abs(leftOffset);
                    }
                    oldX = x;
                }                                
                ul.css('left', ulLeft+"px");                
            });
            
            $('#ControlBar_Module_AddNewModule .ControlBar_ModuleDiv a.bookmark').on('click', function () {
                var moduleId = $(this).parent().attr('data-module');
                var bookmarTitle = "module";
                var bookmarkModules = dnn.controlBar.addBookmarkModule(moduleId);
                dnn.controlBar.saveBookmarkModules(bookmarTitle, bookmarkModules, $(this));
            }).hover(function () {
                if ($(this).attr("class").indexOf("hideBookmark") > 0) {
                    return; // The button is hidden
                }
                $(this).addClass("highlight");
                var $self = $(this).parent();
                $self.dnnHelperTipDestroy();
            }, function() {
                $(this).removeClass("highlight");
                var holderId = $(this).parent().attr('id');;
                var $self = $(this).parent();
                $self.dnnHelperTip({
                    helpContent: settings.dragModuleToolTip,
                    holderId: holderId,
                    show: true
                });
            });


            $('#ControlBar_Module_AddNewModule .ControlBar_ModuleDiv a.removeBookmark').on('click', function () {
                var moduleId = $(this).parent().attr('data-module');
                var bookmarTitle = "module";
                var bookmarkModules = dnn.controlBar.removeBookmarkModule(moduleId);
                dnn.controlBar.saveBookmarkModules(bookmarTitle, bookmarkModules, $(this), moduleId);
            }).hover(function () {
                if ($(this).attr("class").indexOf("hideBookmark") > 0) {
                    return; // The button is hidden
                }
                $(this).addClass("highlight");
                var $self = $(this).parent();
                $self.dnnHelperTipDestroy();
            }, function () {
                $(this).removeClass("highlight");
                var holderId = $(this).parent().attr('id');;
                var $self = $(this).parent();
                $self.dnnHelperTip({
                    helpContent: settings.dragModuleToolTip,
                    holderId: holderId,
                    show: true
                });
            });;

            $('div.ControlBar_ModuleDiv', ul).each(function () {
                if (!this.id)
                    this.id = 'ControlBar_ModuleDiv_' + $(this).data('module');
            }).hover(function () {

                if (!dnn.controlBar.isMouseDown) {
                    var $this = $(this);
                    var dataModuleId = $this.data('module');
                    if (dnn.controlBar.selectedModule && dnn.controlBar.selectedModule.data('module') !== dataModuleId && !$('#ControlBar_Module_ModulePosition').is(":visible")) {
                        dnn.controlBar.unselectModule(dnn.controlBar.selectedModule);
                    }
                    $this.addClass('ControlBar_Module_Selected');
                    $this.mousedown(function() {
                        $(this).addClass("grabbing");
                    });
                    if (!$('#ControlBar_Module_ModulePosition').is(":visible")) {
                    	$this.find('div.ModuleLocator_Menu').removeClass('ModuleLocator_Hover');
                    }
                    dnn.controlBar.selectedModule = $this;
                    dnn.controlBar.showSelectedModule = true;

                    var holderId = this.id;
                    var $self = $(this);
                    $self.dnnHelperTip({
                        helpContent: settings.dragModuleToolTip,
                        holderId: holderId,
                        show: true
                    });

                    $this.find('a.bookmark').show();
                    $this.find('a.removeBookmark').show();
                }

            }, function () {
                if (!dnn.controlBar.isMouseDown) {
                    dnn.controlBar.showSelectedModule = false;
                    var $this = $(this);
                    setTimeout(function () {
                    	if (!dnn.controlBar.showSelectedModule && dnn.controlBar.selectedModule && !$('#ControlBar_Module_ModulePosition').is(":visible")) {                    	    
                    	    dnn.controlBar.unselectModule(dnn.controlBar.selectedModule);
                    	}
                    	
                    }, 600);
                }

                $(this).dnnHelperTipDestroy();
            })
            .mousedown(function () {
                $('.DNNEmptyPane').each(function () {
                    $(this).removeClass('DNNEmptyPane').addClass('dnnDropEmptyPanes');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
                dnn.controlBar.addGrabbingStyle($(this));
            })
            .mouseup(function () {
                $('.dnnDropEmptyPanes').each(function () {
                    $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
                });
                $('.contentPane').each(function () {
                    // this special code is for you -- IE8
                    this.className = this.className;
                });
                dnn.controlBar.removeGrabbingStyle($('div.ControlBar_ModuleDiv', ul));
            })
            .draggable({
                dropOnEmpty: true,
                cursor: 'move',
                placeholder: "dnnDropTarget",
                helper: function (event, ui) {
                    var dragTip = $('<div class="dnnDragdropTip helperGrabbing"></div>');
                    var title = $('span', this).html();
                    dragTip.html(title);
                    dragTip.mouseup(function () {
                        dnn.controlBar.removeGrabbingStyle($('div.ControlBar_ModuleDiv', ul));                        
                    });
                    $('body').append(dragTip);

                    // destroy tooltip
                    $(this).dnnHelperTipDestroy();

                    //set data
                    dnn.controlBar.dragdropModule = $(this).data('module');
                    dnn.controlBar.dragdropPage = dnn.controlBar.selectedPage? dnn.controlBar.selectedPage.id : null;
                    dnn.controlBar.dragdropVisibility = $find(settings.visibilityComboId).get_value();
                    dnn.controlBar.dragdropCopyModule = $('#ControlBar_Module_chkCopyModule').get(0).checked;
                    dnn.controlBar.dragdropAddExistingModule = !dnn.controlBar.addNewModule;
                    return dragTip;
                },
                drag: function (e, ui) {
                    if (dnn.controlBar.isCursorOutsideY(e, container)) {
                        $("div.helperGrabbing").removeClass("helperGrabbing");
                        if (navigator.userAgent.match(/MSIE/i)) {
                            $("div.dnnDragdropTip").css("cursor", "move");
                        }                        
                        return;
                    }                    
                    $("div.dnnDragdropTip").addClass("helperGrabbing");
                    if (navigator.userAgent.match(/MSIE/i)) {
                        $("div.dnnDragdropTip").css("cursor", "");
                    }
                    
                    var xOffset = dnn.controlBar.initialDragPosition.X - e.pageX;
                    var scrollNewX = dnn.controlBar.initialScrollPosition.X + (((dnn.controlBar.scrollBarFixedSize * (ulWidth + margin)) / windowWidth) * xOffset) / (ulWidth + margin);
                    
                    var jspapi = scrollContainer.data('jsp');                    
                    if (jspapi) {
                        jspapi.scrollToX(scrollNewX);
                    }
                },
                cursorAt: { left: 10, top: 30 },
                connectToSortable: '.dnnSortable',
                stop: function (event, ui) {

                    $('.dnnDropEmptyPanes').each(function () {
                        $(this).removeClass('dnnDropEmptyPanes').addClass('DNNEmptyPane');
                    });
                    $('.contentPane').each(function () {
                        // this special code is for you -- IE8
                        this.className = this.className;
                    });

                    $('div.actionMenu').show();
                },
                start: function (event, ui) {
                    dnn.controlBar.setInitialPositions(event, scrollContainer);
                    $('div.actionMenu').hide();                    
                }
            });

            $('div.ModuleLocator_Menu', ul).hover(function () {
                var $this = $(this);
                $this.addClass('ModuleLocator_Hover');
                var left = $this.offset().left;
                dnn.controlBar.hideModuleLocationMenu = false;
                dnn.controlBar.showSelectedModule = true;
                $('#ControlBar_Module_ModulePosition')
                    .css({ left: left - 177, top: 135 })
                    .slideDown('fast', function () {
                        $(this).jScrollPane();
                    })
                    .hover(function () {
                        dnn.controlBar.hideModuleLocationMenu = false;
                        dnn.controlBar.showSelectedModule = true;

                    }, function () {
                        dnn.controlBar.hideModuleLocationMenu = true;
                        dnn.controlBar.showSelectedModule = false;
                        setTimeout(function () {
                        	if (dnn.controlBar.hideModuleLocationMenu) {
                        		$this.removeClass('ModuleLocator_Hover');
                            	$('#ControlBar_Module_ModulePosition').slideUp('fast', function () {
                                	if (dnn.controlBar.selectedModule && dnn.controlBar.selectedModule.data("module") != $this.parent().data("module")) {                                	    
                                	    dnn.controlBar.unselectModule($this.parent());
                                	}
                                });
                            }

                            if (!dnn.controlBar.showSelectedModule && dnn.controlBar.selectedModule) {                                
                                dnn.controlBar.unselectModule(dnn.controlBar.selectedModule);
                            }
                        }, 200);
                    });

                // hide tooltip
                $this.parent().dnnHelperTipDestroy();

            }, function () {
                var $this = $(this);
                dnn.controlBar.hideModuleLocationMenu = true;
                setTimeout(function () {
                    if (dnn.controlBar.hideModuleLocationMenu) {
                        $this.removeClass('ModuleLocator_Hover');
                        $('#ControlBar_Module_ModulePosition').slideUp('fast', function() {
                        	if (dnn.controlBar.selectedModule && dnn.controlBar.selectedModule.data("module") != $this.parent().data("module")) {
                        	    dnn.controlBar.unselectModule($this.parent());
                        	}
                        });
                    }

                    if (!dnn.controlBar.showSelectedModule && dnn.controlBar.selectedModule) {
                        dnn.controlBar.unselectModule(dnn.controlBar.selectedModule);
                    }
                }, 200);
            });

            if (isFirstModuleLoadingPage && dnn.controlBar.forceScrollX) {
                var jspapi = scrollContainer.data('jsp');                    
                if (jspapi) {
                    jspapi.scrollToX(dnn.controlBar.forceScrollX, null);
                    dnn.controlBar.forceScrollX = null;
                }
            }
        };
        if (dnn.controlBar.isFirstModuleLoadingPage()
                && (!dnn.controlBar.forceScrollX || dnn.controlBar.forceScrollX == 0)) {
            ul.animate({ left: margin }, 300);
        }
        setTimeout(modulesInitFunc, 0);
    };

    dnn.controlBar.addGrabbingStyle = function($selector) {
        $selector.addClass("grabbing");
        if (navigator.userAgent.match(/MSIE/i) || navigator.userAgent.match(/Safari/i)) {
            $selector.css("cursor", "url('/images/icon_cursor_grabbing.cur'), move");
        }
    };

    dnn.controlBar.removeGrabbingStyle = function($selector) {
        $selector.removeClass("grabbing");
        if (navigator.userAgent.match(/MSIE/i) || navigator.userAgent.match(/Safari/i)) {
            $selector.css("cursor", "");
        }
    };

    dnn.controlBar.getCurrentScrollPositionX = function(jspapi) {
        var scrollX = 0;
        if (jspapi) {
            var scrollPosX = parseInt(jspapi.getContentPositionX()); //getContentPosition can return a NaN value
            if (scrollPosX) {
                scrollX = scrollPosX;
            }
        }
        return scrollX;
    };

    dnn.controlBar.setInitialPositions = function(dragEvent, $scrollContainer) {
        dnn.controlBar.initialDragPosition = { "X": dragEvent.pageX, "Y": dragEvent.pageY };
               
        var scrollX = dnn.controlBar.getCurrentScrollPositionX($scrollContainer.data("jsp"));
        dnn.controlBar.initialScrollPosition = { "X": scrollX, "Y": 0 };
    };

    dnn.controlBar.isCursorOutsideY = function(dragEvent, $container) {
        var containerOffSet = $container.offset();
        if (dragEvent.pageY > containerOffSet.top + $container.height()) {
            return true;
        }
        return false;
    };


    dnn.controlBar.unselectModule = function($selectedModule) {
        $selectedModule.removeClass('ControlBar_Module_Selected');

        if ($selectedModule.find('a.bookmark').length > 0) {
            $selectedModule.find('a.bookmark').get(0).style.display = '';
        }
        if ($selectedModule.find('a.removeBookmark').length > 0) {
            $selectedModule.find('a.removeBookmark').get(0).style.display = '';
        }
    };

    dnn.controlBar.resetModuleSearch = function () {
        var $searchInput = $("#" + settings.searchInputId);
        $searchInput.val('').focus();
        if (dnn.controlBar.status && dnn.controlBar.status.searchTerm) {
            $searchInput.val(dnn.controlBar.status.searchTerm);
        }
        dnn.controlBar.resetModuleLoadingSettings();
    };

    dnn.controlBar.resetModuleLoadingSettings = function() {
        dnn.controlBar.moduleLoadingPageIndex = 0;
        dnn.controlBar.allModulesLoaded = false;
        dnn.controlBar.mousedown = false;
    }

    dnn.controlBar.getSelectedCategory = function () {
        var category = null;
        if (dnn.controlBar.status && dnn.controlBar.status.addNewModule) {
            var selectedCategory = dnn.controlBar.status.category;
            if (selectedCategory) {
                category = selectedCategory;
                dnn.controlBar.removeStatus();
                if ($find(settings.categoryComboId).get_value() != selectedCategory) {
                    dnn.controlBar.forceCategorySelection = true;    
                    $find(settings.categoryComboId).findItemByValue(selectedCategory).select();
                }
            }
        } else {
            var $categoryCombo = $find(settings.categoryComboId);
            if ($categoryCombo) {
                category = $categoryCombo.get_value();
            } else {
                category = settings.defaultCategoryValue;
            }
        }

        return category;
    };

    dnn.controlBar.ControlBar_Module_CategoryList_Changed = function (sender, e) {
        var item = e.get_item();
        if (item && !dnn.controlBar.forceCategorySelection) {
            if (!dnn.controlBar.forceCategorySearch) {
                //Reset module search
                dnn.controlBar.resetModuleSearch();
            } else {
                dnn.controlBar.forceCategorySearch = false;
                dnn.controlBar.resetModuleLoadingSettings();
            }          
            dnn.controlBar.getDesktopModulesForNewModule(item.get_value(), dnn.controlBar.getSearchTermValue());
        } else if (dnn.controlBar.forceCategorySelection) {
            dnn.controlBar.forceCategorySelection = false;
        }
    };
    
    dnn.controlBar.ControlBar_Module_PageList_Changed = function (selectedNode) {
        if (!selectedNode.key)
            dnn.controlBar.selectedPage = null;
        else
            dnn.controlBar.selectedPage = { id: parseInt(selectedNode.key, 10), name: selectedNode.value };
        
        var visibilityCombo = $find(settings.visibilityComboId);
        var makeCopyCheckbox = $("#" + settings.makeCopyCheckboxId);
        
        if (dnn.controlBar.selectedPage) {
                dnn.controlBar.getTabModules(dnn.controlBar.selectedPage.id);
                visibilityCombo.enable();
	            makeCopyCheckbox.attr("disabled", false).parent().removeClass("disabled");
                if (dnn.controlBar.status && !dnn.controlBar.status.addNewModule) {
                    visibilityCombo.findItemByValue(dnn.controlBar.status.visibility).select();
                }
        }
        else {
            visibilityCombo.disable();
            makeCopyCheckbox.attr("disabled", true).parent().addClass("disabled");
        }
    };

    //attach mouse move to detect mouse button
    $(document).mousedown(function () {
        dnn.controlBar.isMouseDown = true;    
    });
    $(document).mouseup(function () {
        dnn.controlBar.isMouseDown = false;
    });

    //attach resize to fix viewPort
    $(window).resize(function () {
        if (dnn.controlBar.addNewModule && !$("#ControlBar_ModuleListHolder_NewModule").is(":visible")) {
            return;
        }
        if (!dnn.controlBar.addNewModule && !$("#ControlBar_ModuleListHolder_ExistingModule").is(":visible")) {
            return;
        }

        var windowWidth = $(window).width();
        var margin = dnn.controlBar.getScrollBarMargin(windowWidth);
        dnn.controlBar.setViewPort(windowWidth, margin);
    });
    var currentUserMode = settings.currentUserMode;

    $('div.subNav').hide();
    $("#ControlNav > li").hoverIntent({
        over: function () {
    		// hide all the sub menu which has already shown.
    		$("#ControlNav > li").each(function() {
    			var subNav = $(this).find('div.subNav');
    			if (subNav.is(":visible")) {
    				//hide the drop down in subnav
    				$("div[class*=RadComboBox]", subNav).each(function() {
    					var combo = $find($(this).attr("id"));
    					if (combo != null) {
    						combo.hideDropDown();
    					}
    				});
    				$(this).prop("hoverIntent_s", 1).trigger("mouseleave");
    			}
    		});
            $('.onActionMenu').removeClass('onActionMenu');
            toggleModulePane($('.ControlModulePanel'), false);
            $(this).addClass("hover");
            var subNav = $(this).find('div.subNav');
	        subNav.slideDown(300, function() {
	            dnn.addIframeMask(subNav[0]);
	        });
        },
        out: function () {
        	if (!dnn.controlBar.focused) {
        	    $(this).removeClass("hover");
        	    var subNav = $(this).find('div.subNav');
        		subNav.slideUp(200, function() {
        		    dnn.removeIframeMask(subNav[0]);
        		});
	        }
        },
        timeout: 300,
        interval: 150
    });

    $('#ControlActionMenu > li').hoverIntent({
        over: function () {
            $('.onActionMenu').removeClass('onActionMenu');
            toggleModulePane($('.ControlModulePanel'), false);
            var subNav = $(this).find('ul');
            subNav.slideDown(200, function() {
                dnn.addIframeMask(subNav[0]);
            });
        },
        out: function () {
            var subNav = $(this).find('ul');
            subNav.slideUp(150, function() {
                dnn.removeIframeMask(subNav[0]);
            });
        },
        timeout: 300,
        interval: 150
    });

    $('ul#ControlEditPageMenu > li').hoverIntent({
        over: function () {
            $('.onActionMenu').removeClass('onActionMenu');
            toggleModulePane($('.ControlModulePanel'), false);
            var subNav = $(this).find('ul');
            subNav.slideDown(400, function () { dnn.addIframeMask(subNav[0]); });
        },
        out: function () {
            var subNav = $(this).find('ul');
            subNav.slideUp(300, function () { dnn.removeIframeMask(subNav[0]); });
        },
        timeout: 300,
        interval: 150
    });

    //Handling the Advanced Subnav Toggling
    $(".subNavToggle li a").click(function (event) {
        var ul = $(this).closest('ul');
        var divSubNav = ul.parent();
        if (!($(this).parent('li').hasClass('active'))) {

            // Handling the toggle states
            $('li', ul).removeClass('active');
            $(this).parent('li').addClass('active');

            // Handling the respective subnavs
            var anchorTarget = $(this).attr('href');
            $('dl', divSubNav).hide();
            $(anchorTarget).show();
        }
        return false;
    });

    $('#controlBar_AddNewModule').click(addModule);
    $('#controlBar_CreateModule').click(addCreateModule);

    function addCreateModule() {
        if (currentUserMode !== 'EDIT') {
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'ToggleUserMode',
                type: 'POST',
                data: { UserMode: 'EDIT' },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    dnn.dom.setCookie('ControlBarInit', 'CreateModule', 0, dnn.controlBar.getSiteRoot());
                    window.location.href = window.location.href.split('#')[0];
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });

            return false;
        }

        dnn.controlBar.forceCategorySelection = true;
        $find(settings.categoryComboId).findItemByValue("Developer").select();
        addModule();
    }

    function addModule () {
        if (currentUserMode !== 'EDIT') {
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'ToggleUserMode',
                type: 'POST',
                data: { UserMode: 'EDIT' },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    dnn.dom.setCookie('ControlBarInit', 'AddNewModule', 0, dnn.controlBar.getSiteRoot());
                    window.location.href = window.location.href.split('#')[0];
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });

            return false;
        }        
        dnn.controlBar.resetModuleSearch();

        if (dnn.controlBar.status && dnn.controlBar.status.moduleLoadIndex) {
            dnn.controlBar.forceModuleLoadIndex = dnn.controlBar.status.moduleLoadIndex;
        }
        if (dnn.controlBar.status && dnn.controlBar.status.forceScrollX) {
            dnn.controlBar.forceScrollX = dnn.controlBar.status.forceScrollX;
        }

        dnn.controlBar.getDesktopModulesForNewModule(dnn.controlBar.getSelectedCategory(), dnn.controlBar.getSearchTermValue());
        dnn.controlBar.cleanViewPort();
        dnn.controlBar.addNewModule = true;
        toggleModulePane($('#ControlBar_Module_AddNewModule'), true);
        $('#ControlBar_Action_Menu').addClass('onActionMenu');
        $('#ControlBar_ModuleListMessage_NewModule').hide();

        dnn.controlBar.forceModuleLoadIndex = null;
        return false;
    }
    $('#controlBar_AddExistingModule').click(function () {
        if (currentUserMode !== 'EDIT') {
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'ToggleUserMode',
                type: 'POST',
                data: { UserMode: 'EDIT' },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    dnn.dom.setCookie('ControlBarInit', 'AddExistingModule', 0, dnn.controlBar.getSiteRoot());
                    window.location.href = window.location.href.split('#')[0];
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });

            return false;
        }
        
        if (dnn.controlBar.status && !dnn.controlBar.status.addNewModule) {
            var selectedPageId = dnn.controlBar.status.pageId;
            if (selectedPageId) {
                dnn.controlBar.selectedPage = { id: parseInt(selectedPageId, 10), name: dnn.controlBar.status.pageName };
                dnn[settings.pagePickerId].selectedItem({ key: selectedPageId, value: dnn.controlBar.status.pageName });
                var visibilityCombo = $find(settings.visibilityComboId);
                var makeCopyCheckbox = $("#" + settings.makeCopyCheckboxId);                
                dnn.controlBar.getTabModules(selectedPageId);
                visibilityCombo.enable();
                makeCopyCheckbox.attr("disabled", false).parent().removeClass("disabled");
                if (dnn.controlBar.status && !dnn.controlBar.status.addNewModule) {
                    visibilityCombo.findItemByValue(dnn.controlBar.status.visibility).select();
                }
            }
        }

        dnn.controlBar.cleanViewPort();
        dnn.controlBar.addNewModule = false;
        toggleModulePane($('#ControlBar_Module_AddExistingModule'), true);
        $('#ControlBar_Action_Menu').addClass('onActionMenu');

        var messageContainer = $('#ControlBar_ModuleListMessage_ExistingModule');
        var loadingContainer = messageContainer.next();
        var listContainer = loadingContainer.next();
        var scrollContainer = listContainer.next();

        messageContainer.show();
        loadingContainer.hide();
        listContainer.hide();
        scrollContainer.hide();

        $('p.ControlBar_ModuleListMessage_InitialMessage', messageContainer).show();
        $('p.ControlBar_ModuleListMessage_NoResultMessage', messageContainer).hide();

        return false;
    });

    $('#ControlBar_Module_ModulePosition > li').click(function () {
        if (dnn.controlBar.addingModule) return false;
        dnn.controlBar.addingModule = true;

        var module = dnn.controlBar.selectedModule;
        var page = dnn.controlBar.selectedPage ? dnn.controlBar.selectedPage.id : -1;
        var pane = $(this).data('pane');
        var position = $(this).data('position');
        var visibility = $find(settings.visibilityComboId).get_value();
        var copyModule = $('#ControlBar_Module_chkCopyModule').get(0).checked;
        var addExistingModule = !dnn.controlBar.addNewModule;
        dnn.controlBar.addModule(module.data('module') + '', page, pane, position, '-1', visibility, addExistingModule + '', copyModule + '');
        return false;
    });

    $('#controlBar_ClearCache').click(function () {
        dnn.controlBar.clearHostCache();
        return false;
    });

    $('#controlBar_RecycleAppPool').click(function () {
        dnn.controlBar.recycleAppPool();
        return false;
    });

    $('a#controlBar_CopyPermissionsToChildren').dnnConfirm({
        text: settings.copyPermissionsToChildrenText,
        yesText: settings.yesText,
        noText: settings.noText,
        title: settings.titleText,
        callbackTrue: function () {
            dnn.controlBar.copyPermissionsToChildren();
        }
    });

	$('#controlBar_SwitchSite_DropDown, #controlBar_SwitchLanguage_DropDown').mouseenter(function (e) {
	    dnn.controlBar.focused = true;
	}).mouseleave(function(e) {
		dnn.controlBar.focused = false;
    }).click(function (e) {
		dnn.controlBar.focused = false;
    });

    $('#controlBar_SwitchSiteButton').click(function () {
        var site = $find('controlBar_SwitchSite').get_value();
        dnn.controlBar.switchSite(site);
        return false;
    });

    $('#controlBar_SwitchLanguageButton').click(function () {
        var language = $find('controlBar_SwitchLanguage').get_value();
        dnn.controlBar.switchLanguage(language);
        return false;
    });

    var toggleModulePane = function (pane, show) {
        var paneVisible = pane.is(':visible');
        if (show) {
            if (!paneVisible) {
                pane.animate({ height: 'show' }, 100, function() {
                    $('#Form').addClass("showModulePane");
                    $(window).resize();
                });
            }

        } else {
            if (paneVisible) {
                pane.animate({ height: 'hide' }, 100, function() {
                    $('#Form').removeClass("showModulePane");
                    $(window).resize();
                });
            }
        }
    };

    // generate url and popup
    $('a.ControlBar_PopupLink').click(function () {
        var href = $(this).attr('href');
        if (href) {
            dnnModal.show(href + (href.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 550, 950, true, '');
        }
        return false;
    });

    $('a.ControlBar_PopupLink_EditMode').click(function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        var that = this;

        if (currentUserMode !== 'EDIT') {
            var mode = 'EDIT';
            $.ajax({
                url: serviceUrl + 'ToggleUserMode',
                type: 'POST',
                data: { UserMode: mode },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    // then popup
                    var href = $(that).attr('href');
                    if (href) {
                        dnnModal.show(href + (href.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 550, 950, true, '');
                    }
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });
        } else {
            var href = $(that).attr('href');
            if (href) {
                dnnModal.show(href + (href.indexOf('?') == -1 ? '?' : '&') + 'popUp=true', true, 550, 950, true, '');
            }
        }
        return false;
    });

    $('a#ControlBar_DeletePage').dnnConfirm({
        text: settings.deleteText,
        yesText: settings.yesText,
        noText: settings.noText,
        title: settings.titleText
    });


    $('a#ControlBar_PublishPage').dnnConfirm({
        text: settings.publishConfirmText,
        yesText: settings.yesText,
        noText: settings.noText,
        title: settings.publishConfirmHeader,
        callbackTrue: function() {
            dnn.controlBar.publishPage();
        },
        width: '500px'
    });

    $('a#shareableWarning_cmdConfirm').click(function () {
        dnn.controlBar.hideShareableWarning();
        if (dnn.controlBar.addModuleDataVar) {
            dnn.controlBar.doAddModule(dnn.controlBar.addModuleDataVar);
        }
        dnn.controlBar.addModuleDataVar = null;
        dnn.controlBar.addingModule = false;
        return false;
    });

    $('a#shareableWarning_cmdCancel').click(function () {
        dnn.controlBar.hideShareableWarning();
        dnn.controlBar.addModuleDataVar = null;
        dnn.controlBar.addingModule = false;
        return false;
    });

    $('a#ControlBar_EditPage').click(function () {
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        var mode = currentUserMode === 'EDIT' ? 'VIEW' : 'EDIT';
        $.ajax({
            url: serviceUrl + 'ToggleUserMode',
            type: 'POST',
            data: { UserMode: mode },
            beforeSend: service.setModuleHeaders,
            success: function () {
                if( mode === 'VIEW') dnn.dom.setCookie('StayInEditMode', 'NO', '', dnn.controlBar.getSiteRoot());
                window.location.href = window.location.href.split('#')[0];
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
        return false;
    });

    $('#ControlBar_StayInEditMode').change(function () {
        var mode = this.checked ? "YES" : "NO";
        if (this.checked) {
            // disable view in layout mode
            $('#ControlBar_ViewInLayout').attr('disabled', 'disabled');
        } else {
            $('#ControlBar_ViewInLayout').removeAttr('disabled');
        }
        dnn.dom.setCookie('StayInEditMode', mode, '', dnn.controlBar.getSiteRoot());
        
        if (this.checked && currentUserMode !== 'EDIT') {
            $('a#ControlBar_EditPage').click();
        }
    }).change();

    $('#ControlBar_ViewInLayout').change(function () {
        if (this.disabled) return;
        var mode = this.checked ? "LAYOUT" : "VIEW";
        var service = dnn.controlBar.getService();
        var serviceUrl = dnn.controlBar.getServiceUrl(service);
        $.ajax({
            url: serviceUrl + 'ToggleUserMode',
            type: 'POST',
            data: { UserMode: mode },
            beforeSend: service.setModuleHeaders,
            success: function () {
                window.location.href = window.location.href;
            },
            error: function (xhr) {
                dnn.controlBar.responseError(xhr);
            }
        });
    });

    $(document.body).on('click', 'div.subNav a.bookmark', function () {
        var $this = $(this);
        if ($this.hasClass('hideBookmark')) return false;

        var wrapper = $this.closest('dl');
        var title = wrapper.attr('id').indexOf('host') > 0 ? 'host' : 'admin';
        var outerWrapper = wrapper.parent();
        var bookmarkWrapper = $('dl', outerWrapper).last();
        var ul = $('ul', bookmarkWrapper);
        var bookmarkList = $('ul > li > a', bookmarkWrapper).not('.bookmark');

        // add to bookmark
        var bookmarkUrl = $this.prev();
        var bookmarkTabname = $this.parent().data('tabname');
        var bookmarkHtml = bookmarkUrl.html();
        // check conflict or not
        var isConflict = false;
        bookmarkList.each(function (n, v) {
            var html = $(v).html();
            if (bookmarkHtml === html) {
                isConflict = true;
                return false;
            }
            return true;
        });

        if (!isConflict) {
            // add url to bookmark
            var li = $('<li data-tabname="' + bookmarkTabname + '"></li>');
            li.append(bookmarkUrl.clone());
            li.append($("<a href='javascript:void(0)' class='removeBookmark' title='" + settings.removeBookmarksTip + "'><span></span></a>"));
            ul.append(li);
            // hide this bookmark
            var bookmarkTitle = $this.attr('title');
            $this.addClass('hideBookmark').removeAttr('title').attr('data-title', bookmarkTitle);
            // save bookmark to server
            dnn.controlBar.saveBookmark(title, ul);
            // focus on bookmark tab
            $('li.BookmarkToggle > a', outerWrapper).click();
        }
    });
        
    $(document.body).on('keydown', function (e) {
        if (!dnn.controlBar.isActiveElementEmpty()) {
            return;
        }
        if ((e.keyCode == 37 || e.keyCode == 39) && dnn.controlBar.isValidArrowScrollingContainer())
        {        
            dnn.controlBar.moveArrowScrollingContainer(e.keyCode);
        }
    });

    $(document.body).mousewheel(function (event) {
        var wheelEvent = event.originalEvent;
        if (!wheelEvent) {
            return;
        }        
        if (!dnn.controlBar.isValidMouseWheelScrollingContainer(wheelEvent.target)) {
            return;
        }
        dnn.controlBar.moveMouseWheelScrollingContainer(dnn.controlBar.getWheelEventDelta(wheelEvent));
        event.preventDefault();
    });

    $(document.body).on('click', 'div.subNav a.removeBookmark', function () {
        var $this = $(this);
        var li = $this.parent();
        var tabname = li.attr('data-tabname');        
        var wrapper = $this.closest('dl');
        var title = wrapper.attr('id').indexOf('host') > 0 ? 'host' : 'admin';
        var outerWrapper = wrapper.parent();
        var bookmarkWrapper = $('dl', outerWrapper).last();
        var ul = $('ul', bookmarkWrapper);
        // toggle class for original menu item
        if (title === 'admin') {
            $('#controlbar_admin_basic li, #controlbar_admin_advanced li').each(function () {
                if ($(this).data('tabname') === tabname) {
                    var addbookmarkLink = $(this).find('a.bookmark');
                    var addbookmarktitle = addbookmarkLink.data('title');
                    addbookmarkLink.removeClass('hideBookmark').removeAttr('data-title').attr('title', addbookmarktitle);
                    return false;
                }
                return true;
            });
        }
        else {
            $('#controlbar_host_basic li, #controlbar_host_advanced li').each(function () {
                if ($(this).data('tabname') === tabname) {
                    var addbookmarkLink = $(this).find('a.bookmark');
                    var addbookmarktitle = addbookmarkLink.data('title');
                    addbookmarkLink.removeClass('hideBookmark').removeAttr('data-title').attr('title', addbookmarktitle);
                    return false;
                }
                return true;
            });
        }
        // already bookmarked, remove it
        li.remove();
        // save bookmark to server
        dnn.controlBar.saveBookmark(title, ul);
    });

    $('a.controlBar_CloseAddModules').click(function () {
        var modulePane = dnn.controlBar.addNewModule ? $('#ControlBar_Module_AddNewModule') : $('#ControlBar_Module_AddExistingModule');
        toggleModulePane(modulePane, false);
    });

    // push page down
	$('#Form').addClass("showControlBar");

    // initialize -- this action is between page mode toggle
    var initAction = dnn.dom.getCookie('ControlBarInit');
    if (initAction) {
        dnn.dom.setCookie('ControlBarInit', '', -1, dnn.controlBar.getSiteRoot());
        // load status
        dnn.controlBar.loadStatus();
        switch (initAction) {
            case 'AddNewModule':
                setTimeout(function () {
                    $('#controlBar_AddNewModule').click();
                }, 500);
                break;
            case 'AddExistingModule':
                setTimeout(function () {
                    $('#controlBar_AddExistingModule').click();
                }, 500);
                break;
            case 'CreateModule':
                setTimeout(function () {
                    $('#controlBar_CreateModule').click();
                }, 500);
                break;
        }

        return;
    }

    // fade module if needed
    var fadeModule = dnn.dom.getCookie('FadeModuleID');
    if (fadeModule) {
        dnn.dom.setCookie('FadeModuleID', '', -1, dnn.controlBar.getSiteRoot());
        var anchorLink = $('a[name="' + fadeModule + '"]');
        var module = anchorLink.parent();
        if (module && module.length > 0) {
            module.css('background-color', '#fffacd');
            setTimeout(function() {
                module.css('background', '#fffff0');
                setTimeout(function() {
                    module.css('background', 'transparent');
                }, 300);
            }, 2500);

            // scroll to new added module
            var moduleTop = (module.offset().top - 50);
            if (moduleTop > 0) {
                $('body').scrollTop(moduleTop);
            }

            // load status
            dnn.controlBar.loadStatus();
            if (dnn.controlBar.status) {
                if (dnn.controlBar.status.addNewModule) {
                    setTimeout(function() {
                        $('#controlBar_AddNewModule').click();
                    }, 500);
                } else {
                    setTimeout(function() {
                        $('#controlBar_AddExistingModule').click();
                    }, 500);
                }
            }
        }
    }
};

$(function () {
	if (dnn.controlBarSettings)
		dnn.controlBar.init(dnn.controlBarSettings);

	//extend dnnControlPanel to show or hide control panel
	if (typeof $.fn.dnnControlPanel == "undefined") {
		$.fn.dnnControlPanel = {};
		$.fn.dnnControlPanel.show = function() {
			$("#ControlBar").slideDown();
		};
		$.fn.dnnControlPanel.hide = function() {
			$("#ControlBar").slideUp();
		};
	}
	
	//hide drop down's item list when scroll the window.
	$(window).scroll(function (e) {
		if ($(e.target).hasClass("rcbScroll")) {
			return;
		}
		$("div[id^=ControlBar][id$=_DropDown][class*=RadComboBoxDropDown]").each(function() {
			var id = $(this).attr("id").replace("_DropDown", "");
			var combo = $find(id);
			if (combo != null && combo.get_dropDownVisible()) {
				combo.hideDropDown();
			}
        });
	});
    
    //Set the checkbox in control bar as DNN style.
    $('#ControlBar input[type="checkbox"]').dnnCheckbox();
});