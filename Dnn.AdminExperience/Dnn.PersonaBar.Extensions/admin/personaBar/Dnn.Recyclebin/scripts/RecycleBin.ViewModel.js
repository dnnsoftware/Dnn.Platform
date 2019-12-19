// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/***
 * @class DnnPageRecycleBin
 *
 * This class bla bla bla
 *
 * @depends jQuery
 * @depends knockout
 *
 * @param {type} contructorParam (default value), explanation
 *
 */

define(['jquery', 'knockout',
    'main/koBindingHandlers/toggle',
    'main/koBindingHandlers/jScrollPane',
    'main/koBindingHandlers/tabs',
    'main/koBindingHandlers/setWhenHover',
    'main/koBindingHandlers/setWidthFromParentScrollPaneWhen'], function ($, ko) {
        var DnnPageRecycleBin;

        var ModuleInfo = function (requestData) {
            var self = this;

            // request data
            self.Id = requestData.Id;
            self.TabModuleId = requestData.TabModuleId;
            self.Title = requestData.Title;
            self.PortalId = requestData.PortalId;
            self.TabName = requestData.TabName;
            self.TabID = requestData.TabID;
            self.TabDeleted = requestData.TabDeleted;
            self.LastModifiedOnDate = requestData.LastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.selected = ko.observable(false);
            self.mouseOver = ko.observable(false);
        };

        var UserInfo = function (requestData) {
            var self = this;

            // request data
            self.Id = requestData.Id;
            self.Username = requestData.Username;
            self.PortalId = requestData.PortalId;
            self.DisplayName = requestData.DisplayName;
            self.Email = requestData.Email;
            self.LastModifiedOnDate = requestData.LastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.selected = ko.observable(false);
            self.mouseOver = ko.observable(false);
        };

        var PageInfo = function (requestData) {
            var PAGES_TREE_LEFT_PADDING = 50;

            var self = this;
            // private methods declaration 
            var calculateDepthOfNode, calculateNumberOfAllChildrenTree,
                selectAllChildren, isItsRootElementEven, setOddEvenInChildren,
                hasUnselectedChildren;

            // private methods implementation
            calculateDepthOfNode = function (node) {
                if (node.parent) {
                    self.depth(1 + calculateDepthOfNode(node.parent));
                } else {
                    self.depth(0);
                }
                return self.depth();
            };

            calculateNumberOfAllChildrenTree = function (children) {
                var numberOfNestedChildren = 0;

                if (!children || children.length === 0) {
                    return 0;
                }

                children.forEach(function (child) {
                    numberOfNestedChildren += calculateNumberOfAllChildrenTree(child.children());
                });

                return children.length + numberOfNestedChildren;
            };

            hasUnselectedChildren = function (children) {
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    if (!child.selected()) {
                        return true;
                    }

                    if (hasUnselectedChildren(child.children())) {
                        return true;
                    }
                }
                return false;
            };

            selectAllChildren = function (children, newValue) {
                if (!self.children) {
                    return;
                }

                children().forEach(function (child) {
                    child.selected(newValue);
                });
            };

            setOddEvenInChildren = function (children, newValue) {
                if (!self.children) {
                    return;
                }

                children().forEach(function (child) {
                    child.even(newValue);
                });
            };

            isItsRootElementEven = function (node) {
                if (node.parent) {
                    return isItsRootElementEven(node.parent);
                }
                return node.even();
            };

            // request data
            self.id = requestData.id;
            self.name = requestData.name;
            self.childCount = requestData.childCount;
            self.url = requestData.url;
            self.publishDate = requestData.publishDate;
            self.status = requestData.status;
            self.parentId = requestData.parentId;
            self.level = requestData.level;
            self.tabpath = requestData.tabpath;
            self.isspecial = requestData.isspecial;
            self.useDefaultSkin = requestData.useDefaultSkin;
            self.lastModifiedOnDate = requestData.lastModifiedOnDate;
            self.friendlyLastModifiedOnDate = requestData.friendlyLastModifiedOnDate;

            // other attributes
            self.even = ko.observable(false);
            self.even.extend({ notify: 'always' });

            self.selected = ko.observable(false);
            self.selected.extend({ notify: 'always' });

            self.mouseOver = ko.observable(false);
            self.depth = ko.observable(0);

            self.parent = null;
            self.children = ko.observableArray([]);

            // computed attributes
            self.numberOfNestedChildren = ko.computed(function () {
                return 1 + calculateNumberOfAllChildrenTree(self.children());
            });

            self.cellWidth = ko.computed(function () {
                var separatorWidth = 19;
                var labelWidth = 160;
                return (self.numberOfNestedChildren() * separatorWidth) + labelWidth;
            });

            self.totalHeight = ko.computed(function () {

                var childrenWithEdge, childrenHeight;

                childrenWithEdge = [];
                for (var i = 0; i < self.children().length - 1; i++) {
                    childrenWithEdge.push(self.children()[i]);
                }

                childrenHeight = calculateNumberOfAllChildrenTree(childrenWithEdge);

                if (childrenHeight === 0) {
                    return 0;
                }

                return (childrenHeight * 64) + "px";
            });

            self.isItsRootElementEven = ko.computed(function () {
                return isItsRootElementEven(self);
            });

            self.depthInPixels = ko.computed(function () {
                return (self.depth() * PAGES_TREE_LEFT_PADDING) + "px";
            });

            // subscribe methods
            self.selected.subscribe(function (newValue) {
                selectAllChildren(self.children, newValue);
            });

            self.even.subscribe(function (newValue) {
                setOddEvenInChildren(self.children, newValue);
            });

            // API methods
            self.calculateDepth = function () {
                calculateDepthOfNode(self);
            };
            self.hasUnselectedChildren = function () {
                return hasUnselectedChildren(self.children());
            };
        };

        DnnPageRecycleBin = (function () {
            'use strict';

            // var CONSTANT;
            var RECYCLE_BIN_DEFAULT_OPTIONS = {

            };

            // var _privateVar;
            var _options, _resx, _serviceController, _utility, _viewModel, _settings, _listOfPages;

            // var privateMethod
            var getSelectedElements, markAllElementsAs, markAllModulesAsIfPossible, getTreesOfPages, setOddEvenOrderInRoots, calculateDepthOfPages,
                changeElementSelectedStatus, restoreSelectedPagesHandler, removeSelectedPagesHandler,
                pageRestoreRevomeOperationsCallback, canBeDeleted,
                restoreSelectedModulesHandler, removeSelectedModulesHandler,
                restorePageHandler, removePageHandler, restoreModuleHandler, removeModuleHandler, emptyRecycleBinHandler,
                restoreUserHandler, removeUserHandler, restoreSelectedUsersHandler, removeSelectedUsersHandler, userRestoreRevomeOperationsCallback,
                Paginate, getDeletedPageList, getDeletedModuleList, getDeletedUserList,
                getService, getViewModel, tabActivated;

            /* Class properties */
            DnnPageRecycleBin.class = 'DnnPageRecycleBin';
            DnnPageRecycleBin.type = 'Class'; // If class only has Class Methods put Static Class here

            /* Private Constants and Properties */

            /* Constructor */
            function DnnPageRecycleBin(resx, serviceController, utility, options, settings) {
                _resx = resx;
                _serviceController = serviceController;
                _utility = utility;
                _options = options;
                _settings = {
                    canViewPages: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_PAGES_VIEW,
                    canViewModules: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_MODULES_VIEW,
                    canViewUsers: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_USERS_VIEW,
                    canManagePages: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_PAGES_EDIT,
                    canManageModules: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_MODULES_EDIT,
                    canManageUsers: settings.isHost || settings.isAdmin || settings.permissions.RECYCLEBIN_USERS_EDIT,
                    canEmptyRecycleBin: settings.isHost || settings.isAdmin ||
                        (settings.permissions.RECYCLEBIN_PAGES_VIEW && settings.permissions.RECYCLEBIN_PAGES_EDIT && settings.permissions.RECYCLEBIN_MODULES_VIEW && settings.permissions.RECYCLEBIN_MODULES_EDIT && settings.permissions.RECYCLEBIN_USERS_VIEW && settings.permissions.RECYCLEBIN_USERS_EDIT)
                };
            }


            /* Private Methods */
            // ex: method = function () {};
            changeElementSelectedStatus = function (page) {
                page.selected(!page.selected());
            };

            getSelectedElements = function (collection) {
                var selectedElements = [];

                if (!collection) {
                    return selectedElements;
                }

                collection.forEach(function (element) {
                    if (element.selected()) {
                        selectedElements.push(element);
                    }
                    if (element.children) {
                        selectedElements = selectedElements.concat(getSelectedElements(element.children()));
                    }
                });

                return selectedElements;
            };

            markAllElementsAs = function (collection, value) {
                if (!collection) {
                    return;
                }

                collection.forEach(function (element) {
                    element.selected(value);
                });
            };

            markAllModulesAsIfPossible = function (modules, newValue) {
                if (!modules) {
                    return;
                }
                var isAnyModuleInDeletedPage = false;
                var viewModel = getViewModel();
                modules.forEach(function (module) {
                    if (!module.TabDeleted) {
                        module.selected(newValue);
                    } else {
                        isAnyModuleInDeletedPage = true;
                    }
                });
                if (isAnyModuleInDeletedPage && viewModel.selectAllModules())
                    _utility.notifyError(_resx.recyclebin_UnableToSelectAllModules);
            }

            restoreSelectedPagesHandler = function () {
                if (_settings.canViewPages && _settings.canManagePages) {
                    var pagesList, viewModel;

                    viewModel = getViewModel();
                    pagesList = getSelectedElements(viewModel.deletedpagesList());

                    if (pagesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = pagesList.length > 1 ? _resx.recyclebin_RestorePagesConfirm : _resx.recyclebin_RestorePageConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestorePage', pagesList, pageRestoreRevomeOperationsCallback);
                        });
                    }
                }
            };

            canBeDeleted = function (selectedPages) {
                var result = {};
                result.value = true;
                result.errors = "";

                selectedPages.forEach(function (page) {
                    if (page.hasUnselectedChildren()) {

                        result.value = false;
                        result.errors += _resx.Service_RemoveTabError.replace(/\{0\}/g, page.name);
                    }
                });

                return result;
            };

            removeSelectedPagesHandler = function () {
                if (_settings.canViewPages && _settings.canManagePages) {
                    var pagesList, viewModel;

                    viewModel = getViewModel();
                    pagesList = getSelectedElements(viewModel.deletedpagesList());

                    if (pagesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = pagesList.length > 1 ? _resx.recyclebin_RemovePagesConfirm : _resx.recyclebin_RemovePageConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            var result = canBeDeleted(pagesList);
                            if (!result.value) {
                                _utility.notifyError(_resx.Service_RemoveTabErrorHeader + result.errors);
                                return;
                            }
                            getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                        });
                    }
                }
            };

            restoreSelectedModulesHandler = function () {
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, modulesList;

                    viewModel = getViewModel();
                    modulesList = getSelectedElements(viewModel.deletedmodulesList());

                    if (modulesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = modulesList.length > 1 ? _resx.recyclebin_RestoreModulesConfirm : _resx.recyclebin_RestoreModuleConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestoreModule', modulesList, function () {
                                for (var i = 0; i < modulesList.length; i++) {
                                    viewModel.deletedmodulesList.remove(modulesList[i]);
                                }
                                viewModel.selectAllModules(false);
                            });
                        });
                    }
                }
            };

            removeSelectedModulesHandler = function () {
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, modulesList;

                    viewModel = getViewModel();
                    modulesList = getSelectedElements(viewModel.deletedmodulesList());

                    if (modulesList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = modulesList.length > 1 ? _resx.recyclebin_RemoveModulesConfirm : _resx.recyclebin_RemoveModuleConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RemoveModule', modulesList, function () {
                                for (var i = 0; i < modulesList.length; i++) {
                                    viewModel.deletedmodulesList.remove(modulesList[i]);
                                }
                                viewModel.selectAllModules(false);
                            });
                        });
                    }
                }
            };

            restoreSelectedUsersHandler = function () {
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var usersList, viewModel;

                    viewModel = getViewModel();
                    usersList = getSelectedElements(viewModel.deletedusersList());

                    if (usersList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = usersList.length > 1 ? _resx.recyclebin_RestoreUsersConfirm : _resx.recyclebin_RestoreUserConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RestoreUser', usersList, userRestoreRevomeOperationsCallback);
                        });
                    }
                }
            };

            removeSelectedUsersHandler = function () {
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var viewModel, usersList;

                    viewModel = getViewModel();
                    usersList = getSelectedElements(viewModel.deletedusersList());

                    if (usersList.length > 0) {
                        var confirmText, yesText, noText;
                        confirmText = usersList.length > 1 ? _resx.recyclebin_RemoveUsersConfirm : _resx.recyclebin_RemoveUserConfirm;
                        yesText = _resx.recyclebin_YesConfirm;
                        noText = _resx.recyclebin_NoConfirm;

                        _utility.confirm(confirmText, yesText, noText, function () {
                            getService().post('RemoveUser', usersList, function () {
                                for (var i = 0; i < usersList.length; i++) {
                                    viewModel.deletedusersList.remove(usersList[i]);
                                }
                                viewModel.selectAllUsers(false);
                            });
                        });
                    }
                }
            };

            userRestoreRevomeOperationsCallback = function (data) {
                if (data.Status > 0) {
                    // Error: inform
                    _utility.notifyError(data.Message);
                }
                getDeletedUserList();
                //CONTENT-4010 - call refresh in person bar
                $("#showsite").data('need-refresh', true);
            };

            pageRestoreRevomeOperationsCallback = function (data) {
                if (data.Status > 0) {
                    // Error: inform
                    _utility.notifyError(data.Message);
                }
                getDeletedPageList();
                //CONTENT-4010 - call refresh in person bar
                $("#showsite").data('need-refresh', true);
            };

            restorePageHandler = function (pageData) {
                if (_settings.canViewPages && _settings.canManagePages) {
                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RestorePageConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var pagesList = [];
                        pagesList.push({ id: pageData.id });
                        getService().post('RestorePage', pagesList, pageRestoreRevomeOperationsCallback);
                    });
                }
            };

            restoreUserHandler = function (userData) {
                if (_settings.canViewUsers && _settings.canManageUsers) {

                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RestoreUserConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var usersList = [];
                        usersList.push({ id: userData.Id });
                        getService().post('RestoreUser', usersList, userRestoreRevomeOperationsCallback);
                    });
                }
            };

            removePageHandler = function (pageData) {
                if (_settings.canViewPages && _settings.canManagePages) {
                    var confirmText, yesText, noText;

                    confirmText = _resx.recyclebin_RemovePageConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        if (pageData.children().length > 0) {
                            _utility.notifyError(_resx.Service_RemoveTabParentTabError.replace(/\{0\}/g, pageData.name));
                            return;
                        }
                        var pagesList = [];
                        pagesList.push({ id: pageData.id });
                        getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                    });
                }
            };

            removeUserHandler = function (userData) {
                if (_settings.canViewUsers && _settings.canManageUsers) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
                    confirmText = _resx.recyclebin_RemoveUserConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var userList = [];
                        userList.push({
                            Id: userData.Id
                        });
                        getService().post('RemoveUser', userList, function () {
                            viewModel.deletedusersList.remove(userData);
                        });
                    });
                }
            };

            restoreModuleHandler = function (moduleData, e) {
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
                    confirmText = _resx.recyclebin_RestoreModuleConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var moduleList = [];
                        moduleList.push({
                            Id: moduleData.Id,
                            TabModuleId: moduleData.TabModuleId,
                            TabID: moduleData.TabID
                        });
                        getService().post('RestoreModule', moduleList, function () {
                            viewModel.deletedmodulesList.remove(moduleData);
                        });
                    });
                }
            };

            removeModuleHandler = function (moduleData, e) {
                if (_settings.canViewModules && _settings.canManageModules) {
                    var viewModel, confirmText, yesText, noText;
                    viewModel = getViewModel();
                    confirmText = _resx.recyclebin_RemoveModuleConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        var moduleList = [];
                        moduleList.push({
                            Id: moduleData.Id,
                            TabModuleId: moduleData.TabModuleId,
                            TabID: moduleData.TabID
                        });
                        getService().post('RemoveModule', moduleList, function () {
                            viewModel.deletedmodulesList.remove(moduleData);
                        });
                    });
                }
            };

            emptyRecycleBinHandler = function () {
                if (_settings.canEmptyRecycleBin) {
                    var viewModel = getViewModel();
                    var confirmText = _resx.recyclebin_EmptyRecycleBinConfirm;
                    var deleteText = _resx.recyclebin_DeleteConfirm;
                    var cancelText = _resx.recyclebin_CancelConfirm;

                    _utility.confirm(confirmText, deleteText, cancelText, function () {
                        getService().get('EmptyRecycleBin', { t: (new Date).getTime() }, function () {
                            viewModel.deletedpagesList.removeAll();
                            viewModel.deletedmodulesList.removeAll();
                            viewModel.deletedusersList.removeAll();
                        });
                    });
                }
            };

            setOddEvenOrderInRoots = function (roots) {
                var isPreviousElementEven = false;
                roots.forEach(function (root) {
                    root.even(isPreviousElementEven);
                    isPreviousElementEven = !isPreviousElementEven;
                });
            };

            calculateDepthOfPages = function (pages) {
                if (typeof pages === "undefined") {
                    return;
                }

                pages.forEach(function (page) {
                    page.calculateDepth();
                });
            };

            getTreesOfPages = function (pages) {
                var roots, idToNodeMap, parentNode;
                roots = [];
                idToNodeMap = {};

                pages.forEach(function (page) {
                    idToNodeMap[page.id] = page;

                    if (typeof page.parentId === "undefined") {
                        roots.push(page);
                        return;
                    }

                    parentNode = idToNodeMap[page.parentId];
                    if (!parentNode) {
                        roots.push(page);
                    } else {
                        parentNode.children.push(page);
                        page.parent = parentNode;
                    }
                });

                calculateDepthOfPages(pages);

                setOddEvenOrderInRoots(roots);

                return roots;
            };

            var TotalResults = {}
            var timeout = null;
            var pageSize=15;

            Paginate = function(API_METHOD, viewModelProp, elementId){
                var element = $(elementId).jScrollPane();
                var api = element.data("jsp");
                var viewModel = getViewModel();

                var compare = TotalResults.hasOwnProperty(viewModelProp)==true

                TotalResults[viewModelProp] = (compare) ? TotalResults[viewModelProp] : {modelName:viewModelProp, paginationRequestCount:1};

                var currentResultsLessThanTotal = function(){
                    var comparator = TotalResults[viewModelProp].remainingPages > 0;
                    return comparator;
                };


                element.bind('jsp-scroll-y', function(event, scrollPositionY, isAtTop, isAtBottom) {
                    if(!timeout){
                        if(scrollPositionY > 0 && isAtBottom && !timeout && currentResultsLessThanTotal() ) {
                            timeout = setTimeout(function(){
                                timeout=null;
                            },2000);

                            TotalResults[viewModelProp].paginationRequestCount = TotalResults[viewModelProp].paginationRequestCount || 1;

                            getService().get(API_METHOD, {pageIndex: TotalResults[viewModelProp].paginationRequestCount++, pageSize: pageSize }, function (data) {

                                var results = data.Results;
                                var conditional = TotalResults[viewModelProp].remainingPages-results.length > 0;
                                var temp = [];
                                TotalResults[viewModelProp].remainingPages = (conditional) ?  TotalResults[viewModelProp].remainingPages-results.length : 0;

                                switch(viewModelProp) {
                                    case 'deletedpagesList':
                                        results.forEach(function(temppage){
                                            var page = new PageInfo(temppage);
                                            temp.push(page);
                                        });
                                        var treeOfPages = getTreesOfPages(temp);
                                        treeOfPages.forEach(function(pageTemp) {
                                            viewModel[viewModelProp].push(pageTemp);
                                         });
                                    break;

                                    case 'deletedusersList':
                                        results.forEach(function(tempUser){
                                            var user = new UserInfo(tempUser);
                                            viewModel[viewModelProp].push(user)
                                        });
                                    break;

                                    case 'deletedtemplatesList':
                                        results.forEach(function(tempTemplate){
                                            var template = new TemplateInfo(tempTemplate);
                                            viewModel[viewModelProp].push(template);
                                        });
                                    break;

                                    case 'deletedmodulesList':
                                        results.forEach(function(tempModule){
                                            var module = new ModuleInfo(tempModule);
                                            viewModel[viewModelProp].push(module);
                                        });
                                    break;

                                }
                                api.reinitialise();
                                api.scrollToPercentY(.95);
                            });
                        }
                    }
                });
            };

            var initPagination = function() {
                var tabs = [
                    {apiMethod: "GetDeletedPageList", viewModelProp: "deletedpagesList", elementId:"#pageList" },
                    {apiMethod: "GetDeletedUserList", viewModelProp: "deletedusersList", elementId: "#userList"},
                    {apiMethod: "GetDeletedModuleList", viewModelProp: "deletedmodulesList", elementId:"#moduleList"},
                    {apiMethod: "GetDeletedTemplates", viewModelProp: "deletedtemplatesList", elementId:"#templateList"}
                ];

                tabs.forEach(function(tab){
                    Paginate(tab.apiMethod, tab.viewModelProp, tab.elementId );
                });
            };

            getDeletedPageList = function () {
                if (_settings.canViewPages) {
                    var listOfPages, viewModel;
                    var element = $('#pageList').jScrollPane();
                    var api = element.data("jsp");

                    api.scrollToPercentY(0);
                    listOfPages = [];
                    viewModel = getViewModel();
                    viewModel.deletedPagesReady(false);
                    viewModel.deletedpagesList.removeAll();


                    getService().get('GetDeletedPageList', {pageIndex:0, pageSize:pageSize}, function (data) {
                        var results = data.Results;
                        var viewModelProp = "deletedpagesList";
                        TotalResults[viewModelProp]={};
                        TotalResults[viewModelProp].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelProp].moduleName = viewModelProp;

                        for (var i = 0; i < results.length; i++) {
                            var page = new PageInfo(results[i]);
                            listOfPages.push(page);
                        }

                        _listOfPages = listOfPages.concat();
                        viewModel.selectAllPages(false);
                        viewModel.deletedpagesList(getTreesOfPages(_listOfPages));
                        viewModel.deletedPagesReady(true);
                    });
                }

            };

            getDeletedModuleList = function (isInitCall) {
                if (_settings.canViewModules) {
                    var viewModel = getViewModel();
                    if ((typeof (isInitCall) !== "boolean" || typeof (isInitCall) === "boolean" && !isInitCall) && !viewModel.deletedModulesReady()) {
                        return;
                    }
                    var element = $('#moduleList').jScrollPane();
                    var api = element.data("jsp");

                    api.scrollToPercentY(0);
                    viewModel.deletedModulesReady(false);
                    viewModel.deletedmodulesList.removeAll();


                    getService().get('GetDeletedModuleList', {pageIndex:0, pageSize:pageSize}, function (data) {
                        var results = data.Results;
                        var viewModelPropName = 'deletedmodulesList';

                        TotalResults[viewModelPropName]={};
                        TotalResults[viewModelPropName].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelPropName].paginationRequestCount=1;
                        TotalResults[viewModelPropName].moduleName = viewModelPropName;

                        for (var i = 0; i < results.length; i++) {
                            var module = new ModuleInfo(results[i]);
                            viewModel.deletedmodulesList.push(module);
                        }
                        viewModel.deletedModulesReady(true);

                    });
                }
            };


            getDeletedUserList = function (isInitCall) {
                if (_settings.canViewUsers) {
                    var viewModel = getViewModel();
                    if ((typeof (isInitCall) !== "boolean" || typeof (isInitCall) === "boolean" && !isInitCall) && !viewModel.deletedUsersReady()) {
                        return;
                    }
                    var element = $('#userList').jScrollPane();
                    var api = element.data("jsp");

                    api.scrollToPercentY(0);

                    viewModel.deletedUsersReady(false);
                    viewModel.deletedusersList.removeAll();

                    getService().get('GetDeletedUserList', {pageIndex:0, pageSize:pageSize}, function (data) {
                        var results = data.Results;
                        var viewModelPropName = "deletedusersList";
                        TotalResults[viewModelPropName]={};
                        TotalResults[viewModelPropName].remainingPages = data.TotalResults-pageSize;
                        TotalResults[viewModelPropName].moduleName = viewModelPropName;

                        for (var i = 0; i < results.length; i++) {
                            var user = new UserInfo(results[i]);
                            viewModel.deletedusersList.push(user);
                        }
                        viewModel.selectAllUsers(false);
                        viewModel.deletedUsersReady(true);

                    });
                }
            };

            tabActivated = function (event, ui) {
                var panelId = ui.newPanel.attr('id');
                var activeTab = 0;
                switch (panelId) {
                    case 'pages':
                        activeTab = 0;
                        break;
                    case 'modules':
                        activeTab = 1;
                        break;
                    case 'users':
                        activeTab = 2;
                        break;
                    default:
                        activeTab = 0;
                        break;
                }
                getViewModel().activeTab(activeTab);
            };

            getViewModel = function () {
                if (!_viewModel) {
                    _viewModel = {
                        resx: _resx,
                        settings: _settings,

                        activeTab: ko.observable(0),
                        deletedPagesReady: ko.observable(false),
                        deletedModulesReady: ko.observable(false),
                        deletedUsersReady: ko.observable(false),

                        deletedpagesList: ko.observableArray([]),
                        deletedmodulesList: ko.observableArray([]),
                        deletedusersList: ko.observableArray([]),

                        selectAllPages: ko.observable(false),
                        selectAllModules: ko.observable(false),
                        selectAllUsers: ko.observable(false),

                        changeElementSelectedStatus: changeElementSelectedStatus,

                        removePage: removePageHandler,
                        removeModule: removeModuleHandler,
                        removeUser: removeUserHandler,

                        restorePage: restorePageHandler,
                        restoreModule: restoreModuleHandler,
                        restoreUser: restoreUserHandler,

                        restoreSelectedPages: restoreSelectedPagesHandler,
                        restoreSelectedModules: restoreSelectedModulesHandler,
                        restoreSelectedUsers: restoreSelectedUsersHandler,

                        removeSelectedPages: removeSelectedPagesHandler,
                        removeSelectedModules: removeSelectedModulesHandler,
                        removeSelectedUsers: removeSelectedUsersHandler,

                        refreshPages: getDeletedPageList,
                        refreshModules: getDeletedModuleList,
                        refreshUsers: getDeletedUserList,

                        emptyRecycleBin: emptyRecycleBinHandler,

                        tabActivated: tabActivated
                    };
                    if (_settings.canViewPages && _settings.canManagePages) {
                        _viewModel.selectAllPages.subscribe(function (newValue) {
                            markAllElementsAs(_viewModel.deletedpagesList(), newValue);
                        });
                    }
                    if (_settings.canViewModules && _settings.canManageModules) {
                        _viewModel.selectAllModules.subscribe(function (newValue) {
                            markAllModulesAsIfPossible(_viewModel.deletedmodulesList(), newValue);
                        });
                    }
                    if (_settings.canViewUsers && _settings.canManageUsers) {
                        _viewModel.selectAllUsers.subscribe(function (newValue) {
                            markAllElementsAs(_viewModel.deletedusersList(), newValue);
                        });
                    }
                }

                return _viewModel;
            };

            getService = function () {
                _utility.sf.moduleRoot = "personaBar";
                _utility.sf.controller = "Recyclebin";

                return _utility.sf;
            };

            DnnPageRecycleBin.prototype.init = function (wrapper) {
                _options = $.extend({}, RECYCLE_BIN_DEFAULT_OPTIONS, _options);
                var viewModel = getViewModel();
                ko.applyBindings(viewModel, wrapper[0]);
                 initPagination();
            };

            DnnPageRecycleBin.prototype.show = function () {
                getDeletedPageList();
                getDeletedModuleList(true);
                getDeletedUserList(true);

            };

            return DnnPageRecycleBin;
        })();

        return DnnPageRecycleBin;
    });