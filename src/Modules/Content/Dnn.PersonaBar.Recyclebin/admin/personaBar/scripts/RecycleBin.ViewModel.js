// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

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
    'text!templatePath/RecycleBin.html',
    '../scripts/koBindingHandlers/toggle',
    '../scripts/koBindingHandlers/jScrollPane',
    '../scripts/koBindingHandlers/tabs',
    '../scripts/koBindingHandlers/setWhenHover',
    '../scripts/koBindingHandlers/setWidthFromParentScrollPaneWhen'], function ($, ko, template) {
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

        var PageOrTemplateInfo = function (requestData) {
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
            self.thumbnail = requestData.thumbnail;
            self.largeThumbnail = requestData.largeThumbnail;
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

            // thumbnail attributes
            self.thumbnailVisible = ko.observable(false);
            self.thumbnailTop = ko.observable("0px");
            self.thumbnailLeft = ko.observable("0px");

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
                var thumbnailIconPadding = 10;
                var thumbnailUsedWidth = 24; //The thumbnail icon width is 48, however we only have to keep in mind the half of it since the nested row starts at that position
                var labelWidth = 160;
                return (self.numberOfNestedChildren() * (separatorWidth + thumbnailIconPadding + thumbnailUsedWidth)) + labelWidth;
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

            self.thumbnailVisible.subscribe(function (newVal) {
                if (!newVal) {
                    return;
                }

                var pageId = self.id;
                var $preview = $('.row[data-page-id="' + pageId + '"]').find('.pages-preview');
                if ($preview.parent().is('.pageDataContainer')) {
                    $(document.body).append($preview);
                }
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
            var IMAGE_THUMBNAIL_HEIGTH = 480;

            // var _privateVar;
            var _options, _resx, _serviceController, _utility, _viewModel;

            // var privateMethod
            var getSelectedElements, markAllElementsAs, markAllModulesAsIfPossible, getTreesOfPages, setOddEvenOrderInRoots, calculateDepthOfPages,
                changeElementSelectedStatus, restoreSelectedPagesHandler, removeSelectedPagesHandler,
                pageRestoreRevomeOperationsCallback, canBeDeleted,
                restoreSelectedModulesHandler, removeSelectedModulesHandler, restoreSelectedTemplatesHandler, removeSelectedTemplatesHandler,
                restorePageHandler, removePageHandler, restoreModuleHandler, removeModuleHandler, restoreTemplateHandler, removeTemplateHandler, emptyRecycleBinHandler,
                getDeletedPageList, getDeletedModuleList,
                getDeletedTemplateList, showPreview, hidePreview, getService, getViewModel, tabActivated;

            /* Class properties */
            DnnPageRecycleBin.class = 'DnnPageRecycleBin';
            DnnPageRecycleBin.type = 'Class'; // If class only has Class Methods put Static Class here

            /* Private Constants and Properties */

            /* Constructor */
            function DnnPageRecycleBin(resx, serviceController, utility, options) {
                _resx = resx;
                _serviceController = serviceController;
                _utility = utility;
                _options = options;
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
                    _utility.notify(_resx.recyclebin_UnableToSelectAllModules);
            }

            restoreSelectedPagesHandler = function () {
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
                            _utility.notify(_resx.Service_RemoveTabErrorHeader + result.errors);
                            return;
                        }
                        getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                    });
                }
            };

            restoreSelectedModulesHandler = function () {
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
            };

            removeSelectedModulesHandler = function () {
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
            };

            restoreSelectedTemplatesHandler = function () {
                var templatesList, viewModel;

                viewModel = getViewModel();
                templatesList = getSelectedElements(viewModel.deletedtemplatesList());

                if (templatesList.length > 0) {
                    var confirmText, yesText, noText;
                    confirmText = templatesList.length > 1 ? _resx.recyclebin_RestoreTemplatesConfirm : _resx.recyclebin_RestoreTemplateConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        getService().post('RestorePage', templatesList, function () {
                            for (var i = 0; i < templatesList.length; i++) {
                                viewModel.deletedtemplatesList.remove(templatesList[i]);
                            }
                        });
                    });
                }
            };

            removeSelectedTemplatesHandler = function () {
                var templatesList, viewModel;

                viewModel = getViewModel();
                templatesList = getSelectedElements(viewModel.deletedtemplatesList());

                if (templatesList.length > 0) {
                    var confirmText, yesText, noText;
                    confirmText = templatesList.length > 1 ? _resx.recyclebin_RemoveTemplatesConfirm : _resx.recyclebin_RemoveTemplateConfirm;
                    yesText = _resx.recyclebin_YesConfirm;
                    noText = _resx.recyclebin_NoConfirm;

                    _utility.confirm(confirmText, yesText, noText, function () {
                        getService().post('RemovePage', templatesList, function () {
                            for (var i = 0; i < templatesList.length; i++) {
                                viewModel.deletedtemplatesList.remove(templatesList[i]);
                            }
                        });
                    });
                }
            };

            pageRestoreRevomeOperationsCallback = function (data) {
                if (data.Status > 0) {
                    // Error: inform
                    _utility.notify(data.Message);
                }
                getDeletedPageList();
                //CONTENT-4010 - call refresh in person bar
                $("#showsite").data('need-refresh', true);
            };

            restorePageHandler = function (pageData) {
                var confirmText, yesText, noText;

                confirmText = _resx.recyclebin_RestorePageConfirm;
                yesText = _resx.recyclebin_YesConfirm;
                noText = _resx.recyclebin_NoConfirm;

                _utility.confirm(confirmText, yesText, noText, function () {
                    var pagesList = [];
                    pagesList.push({ id: pageData.id });
                    getService().post('RestorePage', pagesList, pageRestoreRevomeOperationsCallback);
                });
            };

            removePageHandler = function (pageData) {
                var confirmText, yesText, noText;

                confirmText = _resx.recyclebin_RemovePageConfirm;
                yesText = _resx.recyclebin_YesConfirm;
                noText = _resx.recyclebin_NoConfirm;

                _utility.confirm(confirmText, yesText, noText, function () {
                    if (pageData.children().length > 0) {
                        _utility.notify(_resx.Service_RemoveTabError.replace(/\{0\}/g, pageData.name));
                        return;
                    }
                    var pagesList = [];
                    pagesList.push({ id: pageData.id });
                    getService().post('RemovePage', pagesList, pageRestoreRevomeOperationsCallback);
                });
            };

            restoreModuleHandler = function (moduleData, e) {
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
            };

            removeModuleHandler = function (moduleData, e) {
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
            };

            restoreTemplateHandler = function (templateData, e) {
                var viewModel, confirmText, yesText, noText;
                viewModel = getViewModel();
                confirmText = _resx.recyclebin_RestoreTemplateConfirm;
                yesText = _resx.recyclebin_YesConfirm;
                noText = _resx.recyclebin_NoConfirm;

                _utility.confirm(confirmText, yesText, noText, function () {
                    var templateList = [];
                    templateList.push({ id: templateData.id });
                    getService().post('RestorePage', templateList, function () {
                        viewModel.deletedtemplatesList.remove(templateData);
                    });
                });
            };

            removeTemplateHandler = function (templateData, e) {
                var viewModel, confirmText, yesText, noText;
                viewModel = getViewModel();
                confirmText = _resx.recyclebin_RemoveTemplateConfirm;
                yesText = _resx.recyclebin_YesConfirm;
                noText = _resx.recyclebin_NoConfirm;

                _utility.confirm(confirmText, yesText, noText, function () {
                    var templateList = [];
                    templateList.push({ id: templateData.id });
                    getService().post('RemovePage', templateList, function () {
                        viewModel.deletedtemplatesList.remove(templateData);
                    });
                });
            };

            emptyRecycleBinHandler = function () {
                var viewModel = getViewModel();
                var confirmText = _resx.recyclebin_EmptyRecycleBinConfirm;
                var deleteText = _resx.recyclebin_DeleteConfirm;
                var cancelText = _resx.recyclebin_CancelConfirm;

                _utility.confirm(confirmText, deleteText, cancelText, function () {
                    getService().get('EmptyRecycleBin', { t: (new Date).getTime() }, function () {
                        viewModel.deletedpagesList.removeAll();
                        viewModel.deletedmodulesList.removeAll();
                        viewModel.deletedtemplatesList.removeAll();
                    });
                });
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

            getDeletedPageList = function () {
                var listOfPages, viewModel;

                listOfPages = [];
                viewModel = getViewModel();
                viewModel.deletedPagesReady(false);

                viewModel.deletedpagesList.removeAll();

                getService().get('GetDeletedPageList', {}, function (data) {
                    for (var i = 0; i < data.length; i++) {
                        var page = new PageOrTemplateInfo(data[i]);

                        listOfPages.push(page);
                    }
                    viewModel.selectAllPages(false);
                    viewModel.deletedpagesList(getTreesOfPages(listOfPages));
                    viewModel.deletedPagesReady(true);
                });
            };

            getDeletedModuleList = function () {
                var viewModel = getViewModel();
                viewModel.deletedModulesReady(false);
                viewModel.deletedmodulesList.removeAll();

                getService().get('GetDeletedModuleList', {}, function (data) {
                    for (var i = 0; i < data.length; i++) {
                        var module = new ModuleInfo(data[i]);
                        viewModel.deletedmodulesList.push(module);
                    }
                    viewModel.deletedModulesReady(true);
                });
            };

            getDeletedTemplateList = function () {
                var viewModel = getViewModel();
                viewModel.deletedTemplatesReady(false);
                viewModel.deletedtemplatesList.removeAll();

                getService().get('GetDeletedTemplates', {}, function (data) {
                    for (var i = 0; i < data.length; i++) {
                        var template = new PageOrTemplateInfo(data[i]);
                        viewModel.deletedtemplatesList.push(template);
                    }
                    viewModel.selectAllTemplates(false);
                    viewModel.deletedTemplatesReady(true);
                });
            },

            showPreview = function (page, event) {
                var $image, offset, left, top;
                $image = $(event.target);

                offset = $image.offset();
                left = (offset.left + $image.width() + 5);
                top = (offset.top + $image.height() / 2 - IMAGE_THUMBNAIL_HEIGTH / 2);

                page.thumbnailLeft(left + "px");
                page.thumbnailTop(top + "px");

                page.thumbnailVisible(true);
            };

            hidePreview = function (page, event) {
                page.thumbnailVisible(false);
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
                    case 'templates':
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

                        activeTab: ko.observable(0),

                        deletedPagesReady: ko.observable(false),
                        deletedModulesReady: ko.observable(false),
                        deletedTemplatesReady: ko.observable(false),

                        deletedpagesList: ko.observableArray([]),
                        deletedmodulesList: ko.observableArray([]),
                        deletedtemplatesList: ko.observableArray([]),

                        selectAllPages: ko.observable(false),
                        selectAllTemplates: ko.observable(false),
                        selectAllModules: ko.observable(false),


                        changeElementSelectedStatus: changeElementSelectedStatus,

                        removePage: removePageHandler,
                        removeModule: removeModuleHandler,
                        removeTemplate: removeTemplateHandler,

                        restorePage: restorePageHandler,
                        restoreModule: restoreModuleHandler,
                        restoreTemplate: restoreTemplateHandler,

                        restoreSelectedPages: restoreSelectedPagesHandler,
                        restoreSelectedModules: restoreSelectedModulesHandler,
                        restoreSelectedTemplates: restoreSelectedTemplatesHandler,

                        removeSelectedPages: removeSelectedPagesHandler,
                        removeSelectedModules: removeSelectedModulesHandler,
                        removeSelectedTemplates: removeSelectedTemplatesHandler,

                        refreshPages: getDeletedPageList,
                        refreshModules: getDeletedModuleList,
                        refreshTemplates: getDeletedTemplateList,

                        emptyRecycleBin: emptyRecycleBinHandler,
                        showPreview: showPreview,
                        hidePreview: hidePreview,

                        tabActivated: tabActivated
                    };

                    _viewModel.selectAllPages.subscribe(function (newValue) {
                        markAllElementsAs(_viewModel.deletedpagesList(), newValue);
                    });

                    _viewModel.selectAllTemplates.subscribe(function (newValue) {
                        markAllElementsAs(_viewModel.deletedtemplatesList(), newValue);
                    });
                    _viewModel.selectAllModules.subscribe(function (newValue) {
                        markAllModulesAsIfPossible(_viewModel.deletedmodulesList(), newValue);
                    });

                }

                return _viewModel;
            };

            getService = function () {
                _utility.sf.moduleRoot = "personaBar";
                _utility.sf.controller = "PageManagement";

                return _utility.sf;
            };

            DnnPageRecycleBin.prototype.init = function (wrapper) {
                _options = $.extend({}, RECYCLE_BIN_DEFAULT_OPTIONS, _options);

                var viewModel = getViewModel();
                ko.applyBindings(viewModel, wrapper[0]);
            };

            DnnPageRecycleBin.prototype.show = function () {
                getDeletedPageList();
                getDeletedModuleList();
                getDeletedTemplateList();
            };

            return DnnPageRecycleBin;
        })();

        return DnnPageRecycleBin;
    });