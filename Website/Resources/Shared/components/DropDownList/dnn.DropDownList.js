; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    //
    // DropDownList
    //
    var DropDownList = this.DropDownList = function (element, options) {
        this.element = element;
        this.options = options;

        this.init();
    };

    DropDownList.prototype = {

        constructor: DropDownList,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, DropDownList.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);

            this._$selectedItemContainer = this.$element.find(this.options.selectedItemSelector);
            this._$selectedItemCaption = this._$selectedItemContainer.find(".selected-value");

            var stateElementId = this.options.internalStateFieldId;
            if (stateElementId) {
                this._stateElement = document.getElementById(stateElementId);
            }
            this._$itemListContainer = this.$element.find(this.options.itemListContainerSelector);
            this._onItemListShowHandler = $.proxy(this._onItemListShow, this);
            this._onItemListHideHandler = $.proxy(this._onItemListHide, this);
            this._onSelectedItemCaptionClickHandler = $.proxy(this._onSelectedItemCaptionClick, this);

            this._onDocumentClickHandler = $.proxy(this._onDocumentClick, this);

            this._$selectedItemCaption.on("click", this._onSelectedItemCaptionClickHandler);

            this.disabled(this.options.disabled);

        },

        selectedItem: function (item) {
            if (typeof item === "undefined") {
                // getter:
                var state = this._internalState();
                return state ? state.selectedItem : null;
            }
            // setter:
            this._internalState({ selectedItem: item });
            this._setSelectedItemCaption(item);
            if (this._treeView) {
                this._treeView.selectedId(item ? item.key : null);
            }
            return item;
        },

        disabled: function (disable) {
            if (typeof disable !== "undefined") {
                if (disable) {
                    this._selectedItemTitle = this._$selectedItemCaption.prop("title");
                    this._$selectedItemCaption.removeAttr("href").removeAttr("title");
                }
                else {
                    this._$selectedItemCaption.prop("href", "javascript:void(0);").prop("title", this._selectedItemTitle || "");
                }
                this._disabled = disable;
            }
            return this._disabled;
        },

        _setSelectedItemCaption: function (item) {
            var caption = item ? item.value : "";
            this._$selectedItemCaption.text(caption || this.options.selectItemDefaultText);
        },

        _onItemListShow: function () {
            this._$selectedItemCaption.addClass("opened");
            $(document).on("click." + this.element.id, this._onDocumentClickHandler);
            if (!this._treeView) {
                this._treeView = this._createTreeView();
                var item = this.selectedItem();
                this._treeView.selectedId(item ? item.key : null);
                this._treeView.show();
            }
        },

        _onItemListHide: function () {
            this._$selectedItemCaption.removeClass("opened");
            $(document).off("click." + this.element.id, this._onDocumentClickHandler);
        },

        _onSelectedItemCaptionClick: function (eventObject) {
            if (this._disabled) {
                return;
            }
            if (this._$itemListContainer.is(':visible')) {
                this._$itemListContainer.slideUp('fast', this._onItemListHideHandler);
            }
            else {
                this._$itemListContainer.slideDown('fast', this._onItemListShowHandler);
            }
        },

        _closeItemList: function () {
            if (this._$itemListContainer.is(':visible')) {
                this._$itemListContainer.slideUp('fast', this._onItemListHideHandler);
            }
        },

        _internalState: function (stateObject) {
            if (typeof stateObject === "undefined") {
                // getter:
                if (typeof this._state === "undefined") {
                    try {
                        this._state = (this._stateElement && this._stateElement.value) ?
                            JSON.parse(this._stateElement.value) : null;
                    }
                    catch (ex) {
                    }
                }
                return this._state;
            }
            else {
                // setter:
                if (this._stateElement) {
                    var stateAsJson = stateObject ? JSON.stringify(stateObject) : "";
                    this._stateElement.value = stateAsJson;
                }
                this._state = stateObject;
            }
            return this._state;
        },

        _onDocumentClick: function (eventObject) {
            // clicked outside of the dropdown list
            // when click on the element, the onMouseClick handler is executed
            var clickedElement = eventObject.target;
            if (this._$itemListContainer && !$.inContainer(this.$element, clickedElement)) {
                this._closeItemList();
            }
        },

        _createTreeView: function () {
            var controller = new DynamicTreeViewController(this.options.services);
            var treeView = new DynamicTreeView(this._$itemListContainer[0], this.options, controller);
            var onChangeNodeHandler = $.proxy(this._onChangeNode, this);
            $(treeView).on("onchangenode", onChangeNodeHandler);
            var onSelectNodeHandler = $.proxy(this._onSelectNode, this);
            $(treeView).on("onselectnode", onSelectNodeHandler);
            return treeView;
        },

        _onSelectNode: function(eventObject, node) {
            this._closeItemList();
        },

        _onChangeNode: function(eventObject, node) {
            var item = node ? TreeNodeConverter.toKeyValue(node.data) : null;
            this.selectedItem(item);

            if (this.options.onSelectionChanged && this.options.onSelectionChanged.length) {
                for (var i = 0, size = this.options.onSelectionChanged.length; i < size; i++) {
                    dnn.executeFunctionByName(this.options.onSelectionChanged[i], window, item);
                }
            }
            if (typeof this.options.onSelectionChangedBackScript === "function") {
                this.options.onSelectionChangedBackScript.apply(this);
            }
        }

    };

    DropDownList._defaults = {};

    DropDownList.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DropDownList._defaults, settings);
        }
        return DropDownList._defaults;
    };

    //
    // DynamicTreeView
    //
    var DynamicTreeView = this.DynamicTreeView = function (element, options, controller) {
        this.element = element;
        this.options = options;
        this.controller = controller;

        this.init();
    };

    DynamicTreeView.prototype = {

        constructor: DynamicTreeView,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, DynamicTreeView.defaults(), this.options);

            this.$this = $(this);
            this.$element = $(this.element);

            this._$itemListHeaderElement = this.$element.find(".dt-header");
            this._$clearButton = this._$itemListHeaderElement.find(".clear-button");
            this._$clearButton.on("click.drop-down-list", $.proxy(this._onClearClick, this));
            this._$searchButton = this._$itemListHeaderElement.find(".search-button");
            this._$searchButton.on("click.drop-down-list", $.proxy(this._onSearchClick, this));
            var onSortHandler = $.proxy(this._onSortClick, this);
            this._$searchContainer = this._$itemListHeaderElement.find(".search-container");
            this._$searchInput = this._$itemListHeaderElement.find(".search-input").onEnter(onSortHandler).on("keyup.drop-down-list", $.proxy(this._displayClearButton, this));
            this._displayClearButton();
            this._$sortButton = this._$itemListHeaderElement.find(".sort-button");
            this._$sortButton.on("click.drop-down-list", onSortHandler);
            this._itemListHeaderHeight = this._$itemListHeaderElement.outerHeight(true);

            this._$itemListFooterElement = this.$element.find(".dt-footer");
            this._$resultValue = this._$itemListFooterElement.find(".result").children(":first");
            this._itemListFooterHeight = this._$itemListFooterElement.outerHeight(true);

            this._$itemListContentElement = this.$element.find(".dt-content");
            this._$itemListContentElement.height(this.$element.height() - this._itemListHeaderHeight - this._itemListFooterHeight);
            this._$itemListContentElement.jScrollbar();

            this._$resizerElement = this.$element.find(".resizer");
            this._resizer = new dnn.Resizer(this._$resizerElement[0], { container: this.$element });

            $(this._resizer).bind("resized", $.proxy(this._onResize, this));

            this._numberOfItems = 0;

            var $tree = this._$itemListContentElement.find(".dt-tree");
            this._tree = new dnn.TreeView($tree[0], { showRoot: true });

            var onTreeRedrawHandler = $.proxy(this._onTreeRedraw, this);
            $(this._tree).on("onredraw", onTreeRedrawHandler);

            var onCreateNodeElementHandler = $.proxy(this._onCreateNodeElement, this);
            $(this._tree).on("oncreatenode", onCreateNodeElementHandler);

            this._onNodeIconClickHandler = $.proxy(this._onNodeIconClick, this);
            this._onNodeTextClickHandler = $.proxy(this._onNodeTextClick, this);

            this._onItemsLoadedHandler = $.proxy(this._onItemsLoaded, this);

            this._firstNode = null;
            if (this.options.firstItem) {
                this._firstNode = TreeNodeConverter.createNodeFromModel(this.options.firstItem);
            }

            this._sortOrder = dnn.SortOrder.unspecified;

        },

        _onCreateNodeElement: function (eventObject, nodeElement, nodeContext) {
            if (!nodeContext) {
                return;
            }
            var $nodeElement = $(nodeElement);
            var data = nodeContext.data || { name: "", selectable: false };
            var textNode = $("<a>" + $.htmlEncode(data.name) + "</a>").addClass(this.options.textCss);
            if (data.selectable) {
                textNode.prop("href", "javascript:void(0);").bind("click", this._onNodeTextClickHandler);
            }
            else {
                textNode.addClass(this.options.unselectableCss);
            }
            $nodeElement
                .append($("<a href='javascript:;'/>").addClass(this.options.iconCss).on("click", this._onNodeIconClickHandler))
                .append(textNode);
            this._initializeNodeIcon(nodeContext, $nodeElement);
            $nodeElement.children().eq(1).prop("title", data.name);
        },

        _initializeNodeIcon: function (nodeContext, $nodeElement) {
            var iconClass;
            var iconTooltip = "";
            if (nodeContext.hasChildren()) {
                iconClass = this.options.expandedCss;
                iconTooltip = this.options.folderCollapseTooltip;
            }
            else if (nodeContext.data.hasChildren) {
                iconClass = this.options.collapsedCss;
                iconTooltip = this.options.folderExpandTooltip;
            }
            else {
                iconClass = this.options.emptyCss;
            }
            $nodeElement = $nodeElement || this._tree.getNodeElement(nodeContext);
            var $iconElement = $nodeElement.children(":first");
            $iconElement.removeClass(this.options.expandingCss).addClass(iconClass);
            $iconElement.prop("title", iconTooltip);
        },

        _onNodeTextClick: function (eventObject) {
            eventObject.preventDefault();
            eventObject.stopPropagation();
            var clickedElement = eventObject.currentTarget;
            var nodeContext = this._tree.nodeContext(clickedElement);
            this._onChangeNode(nodeContext);
            $(this).trigger($.Event("onselectnode"), [nodeContext]);
        },

        _onNodeIconClick: function (eventObject) {
            eventObject.preventDefault();
            eventObject.stopPropagation();
            var clickedElement = eventObject.currentTarget;
            var $clickedElement = $(clickedElement);
            var $node = $clickedElement.parent();
            var nodeContext = this._tree.nodeContext(clickedElement);

            var tooltip;
            if ($clickedElement.hasClass(this.options.collapsedCss)) {
                // expand
                $clickedElement.removeClass(this.options.collapsedCss).addClass(this.options.expandingCss);
                var onLoadChildNodesHandler = $.proxy(this._onLoadChildren, this, nodeContext);
                var searchText = this._$searchInput.val().trim();
                this.controller.getChildren(nodeContext.data.id, this._sortOrder, searchText, onLoadChildNodesHandler);
                tooltip = this.options.folderLoadingTooltip;
            }
            else {
                // collapse: slide up and remove
                this._tree.removeChildren($node[0]);
                $clickedElement.removeClass(this.options.expandedCss).addClass(this.options.collapsedCss);
                this._onNodeCollapse(nodeContext);
                tooltip = this.options.folderExpandTooltip;
            }
            $clickedElement.prop("title", tooltip);
        },

        _onLoadChildren: function (nodeContext, children) {
            nodeContext.data.hasChildren = children.length > 0;
            nodeContext.removeChildren().addChildNodes(children);
            this._tree.showChildren(nodeContext);
            this._initializeNodeIcon(nodeContext, this._tree.getNodeElement(nodeContext));
            this._numberOfItems += children.length;
            this._$resultValue.text(this._numberOfItems);
            this._ensureSelectedNode();
        },

        _onNodeCollapse: function (nodeContext) {
            this._numberOfItems -= nodeContext.getNumberOfChildren(true);
            this._$resultValue.text(this._numberOfItems);
            if (this._selectedNode !== nodeContext && nodeContext.hasNode(this._selectedNode)) {
                this._selectedNode = null;
            }
            nodeContext.removeChildren();
        },

        _selectNodeElement: function (nodeContext, selected) {
            if (this._tree) {
                this._tree.addNodeClass(nodeContext, selected, this.options.selectedCss);
            }
        },

        _displayClearButton: function () {
            var input = this._$searchInput.val();
            if (input && input.length > 0) {
                this._$clearButton.show();
            }
            else {
                this._$clearButton.hide();
            }
        },

        _onTreeRedraw: function () {
            this._ensureScrollBar();
        },

        _ensureScrollBar: function () {
            this._$itemListContentElement.jScrollbar("update");
        },

        _ensureSelectedNode: function () {
            this.selectedId(this._selectedNodeId);
        },

        _getNodeById: function (itemId) {
            if (!this._rootNode) {
                return null;
            }
            var node = null;
            var callback = function (traversedNode, stack) {
                if (traversedNode.data && traversedNode.data.id === itemId) {
                    node = traversedNode;
                    return true;
                }
                return false;
            };
            this._rootNode.inOrderTraverse(callback);
            return node;
        },

        selectedId: function (id) {
            if (typeof id === "undefined") {
                return this._selectedNodeId;
            }
            if (id !== this._selectedNodeId || !this.selectedNode()) {
                var node = this._getNodeById(id);
                this.selectedNode(node);
            }
            return this._selectedNodeId = id;
        },

        selectedNode: function (node) {
            if (typeof node === "undefined") {
                return this._selectedNode;
            }
            this._selectNodeElement(this._selectedNode, false);
            this._selectNodeElement(node, true);
            this._selectedNodeId = node && node.data ? node.data.id : null;
            return this._selectedNode = node;
        },

        _onChangeNode: function (node) {
            if (!dnn.NTree.equals(node, this._selectedNode)) {
                this.selectedNode(node);
                $(this).trigger($.Event('onchangenode'), [node]);
            }
        },

        _onResize: function (eventObject, containerSize) {
            this._$itemListContentElement.height(containerSize.height - this._itemListHeaderHeight - this._itemListFooterHeight);
            this._ensureScrollBar();
        },

        _onItemsLoaded: function (rootNode) {
            this._tree.collapseTree($.proxy(this._showTree, this, rootNode));
        },

        _showTree: function (rootNode) {
            this._rootNode = rootNode;
            if (this._firstNode) {
                this._rootNode.children.insertAt(0, this._firstNode);
            }
            this._loading(false);
            this._tree.showTree(this._rootNode.children);
            this._selectedNode = null;
            this._ensureSelectedNode();
            this._tree.addNodeClass(this._firstNode, true, "first-item");
            this._numberOfItems = this._rootNode.getNumberOfChildren(true) - (this._firstNode ? 1 : 0);
            this._$resultValue.text(this._numberOfItems);
        },

        show: function () {
            this._getTree(this._selectedNodeId);
        },

        _onClearClick: function () {
            this._$searchInput.val("");
            this._$clearButton.hide();
            if (this._searchHasBeenApplied) {
                this._search();
            }
        },

        _loading: function (loading) {
            if (loading) {
                this._$itemListContentElement.addClass("loading-items");
            }
            else {
                this._$itemListContentElement.removeClass("loading-items");
            }
        },

        _getTree: function (selectedItemId) {
            this._loading(true);
            if (selectedItemId && (!this._firstNode || (this._firstNode && this._firstNode.data.id !== selectedItemId))) {
                this._$searchInput.val("");
                this.controller.getTreeWithItem(selectedItemId, this._sortOrder, this._onItemsLoadedHandler);
            }
            else {
                this.controller.getTree(this._sortOrder, this._onItemsLoadedHandler);
            }
            this._$searchContainer.removeClass(this.options.searchOnCss);
            this._searchHasBeenApplied = false;
        },

        _onSearchClick: function () {
            this._search();
        },

        _onSortClick: function () {
            this._sort();
        },

        _search: function() {
            this._loading(true);
            var searchText = this._$searchInput.val().trim();
            if (searchText) {
                this._$searchContainer.addClass(this.options.searchOnCss);
                this.controller.search(searchText, this._sortOrder, this._onItemsLoadedHandler);
                this._searchHasBeenApplied = true;
            }
            else {
                this._$searchContainer.removeClass(this.options.searchOnCss);
                this.controller.getTree(this._sortOrder, this._onItemsLoadedHandler);
                this._searchHasBeenApplied = false;
            }
        },

        _sort: function() {
            this._loading(true);
            if (this._sortOrder === dnn.SortOrder.unspecified) {
                this._sortOrder = dnn.SortOrder.ascending;
                this._$sortButton.addClass("asc").prop("title", this.options.sortDescendingButtonTooltip);
            }
            else if (this._sortOrder === dnn.SortOrder.ascending) {
                this._sortOrder = dnn.SortOrder.descending;
                this._$sortButton.removeClass("asc").addClass("desc").prop("title", this.options.unsortedOrderButtonTooltip);
            }
            else {
                this._sortOrder = dnn.SortOrder.unspecified;
                this._$sortButton.removeClass("desc").prop("title", this.options.sortAscendingButtonTooltip);
            }
            var searchText = "";
            if (this._searchHasBeenApplied) {
                searchText = this._$searchInput.val().trim();
                if (searchText === "") {
                    this._$searchContainer.removeClass(this.options.searchOnCss);
                    this._searchHasBeenApplied = false;
                }
            }
            this.controller.sortTree(this._sortOrder, this._rootNode, searchText, this._onItemsLoadedHandler);
        }

    };

    DynamicTreeView._defaults = {
        selectedCss: "selected",
        unselectableCss: "unselectable",
        expandedCss: "expanded",
        expandingCss: "expanding",
        collapsedCss: "collapsed",
        emptyCss: "empty",
        iconCss: "icon",
        textCss: "text",
        searchOnCss: "searchOn"
    };

    DynamicTreeView.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DynamicTreeView._defaults, settings);
        }
        return DynamicTreeView._defaults;
    };


    var TreeNodeConverter = this.TreeNodeConverter = function () {
    };

    TreeNodeConverter.castNodes = function (nodes) {
        var castedNodes = [];
        for (var i = 0, count = nodes.length; i < count; i++) {
            castedNodes.push(TreeNodeConverter.castNode(nodes[i]));
        }
        return castedNodes;
    };

    TreeNodeConverter.castNode = function(model) {
        if (!model) {
            return null;
        }
        var castedNode = TreeNodeConverter.createNodeFromModel(model.data);
        for (var i = 0, count = model.children.length; i < count; i++) {
            castedNode.children.push(TreeNodeConverter.castNode(model.children[i]));
        }
        return castedNode;
    };

    TreeNodeConverter.createNodesFromData = function (model) {
        var castedNodes = [];
        if (model) {
            for (var i = 0, count = model.length; i < count; i++) {
                castedNodes.push(TreeNodeConverter.createNodeFromModel(model[i]));
            }
        }
        return castedNodes;
    };

    TreeNodeConverter.createNodeFromModel = function(data) {
        var node = new dnn.NTree();
        node.data = TreeNodeConverter.castModelData(data);
        return node;
    };

    TreeNodeConverter.castModelData = function(model) {
        return model ? { hasChildren: model.hasChildren, name: model.value, id: model.key, selectable: typeof (model.selectable) === "undefined" || model.selectable } : null;
    };

    TreeNodeConverter.toKeyValue = function (data) {
        return data ? { key: data.id, value: data.name } : null;
    };

    TreeNodeConverter.toTreeOfIds = function (tree) {
        if (!tree) {
            return null;
        }
        var node = new dnn.NTree({ id: tree.data.id });
        for (var i = 0, count = tree.children.length; i < count; i++) {
            if (tree.children[i].hasChildren()) {
                node.children.push(TreeNodeConverter.toTreeOfIds(tree.children[i]));
            }
        }
        return node;
    };

    var KeyValueConverter = this.KeyValueConverter = function () {
    };

    KeyValueConverter.arrayToDictionary = function(pairs, keyProp, valProp) {
        var dictionary = new dnn.Dictionary();
        if (pairs && pairs.length > 0) {
            for (var i = 0, size = pairs.length; i < size; i++) {
                dictionary.set(pairs[i][keyProp], pairs[i][valProp]);
            }
        }
        return dictionary;
    };

    KeyValueConverter.dictionaryToArray = function (dictionary, keyProp, valProp) {
        var pairs = [];
        if (dictionary) {
            for (var key in dictionary) {
                var pair = {};
                pair[keyProp] = key;
                pair[valProp] = dictionary[key];
                pairs.push(pair);
            }
        }
        return pairs;
    };

    var DynamicTreeViewController = this.DynamicTreeViewController = function (options) {
        this.options = options;
        this.init();
    };

    DynamicTreeViewController.prototype = {
        constructor: DynamicTreeViewController,

        init: function() {
            this.options = $.extend({}, DynamicTreeViewController.defaults(), this.options);
            this._serviceUrl = $.dnnSF().getServiceRoot(this.options.serviceRoot);
            this.parameters = KeyValueConverter.arrayToDictionary(this.options.parameters, "Key", "Value");
        },

        _callGet: function (data, onLoadHandler, method) {
            $.extend(data, this.parameters.entries());
            var serviceSettings = {
                url: this._serviceUrl + method,
                beforeSend: $.dnnSF().setModuleHeaders,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: data,
                type: "GET",
                async: true,
                success: onLoadHandler,
                error: $.onAjaxError
            };
            $.ajax(serviceSettings);
        },

        getTreeWithItem: function (itemId, sortOrder, onGetTreeCallback) {
            var onGetTreeHandler = $.proxy(this._onGetTree, this, onGetTreeCallback);
            this._callGet({ itemId: itemId, sortOrder: sortOrder }, onGetTreeHandler, this.options.getTreeWithNodeMethod);
        },

        _onGetTree: function (onGetTreeCallback, data, textStatus, jqXhr) {
            var rootNode = null;
            var castedRootNode = TreeNodeConverter.castNode(data.Tree);
            if (data.IgnoreRoot) {
                if (dnn.NTree.isEmpty(castedRootNode)) {
                    castedRootNode.data = { hasChildren: false, name: "", id: this.options.rootId };
                }
                rootNode = castedRootNode;
            }
            else {
                rootNode = new dnn.NTree({ hasChildren: true, name: "", id: this.options.rootId });
                if (!dnn.NTree.isEmpty(castedRootNode)) {
                    rootNode.children.push(castedRootNode);
                }
            }
            onGetTreeCallback.apply(this, [rootNode]);
        },

        getChildren: function (parentId, sortOrder, searchText, onGetChildrenCallback) {
            var onGetChildrenHandler = $.proxy(this._onGetChildren, this, onGetChildrenCallback);
            this._callGet({ parentId: parentId, sortOrder: sortOrder, searchText: searchText }, onGetChildrenHandler, this.options.getNodeDescendantsMethod);
        },

        _onGetChildren: function(onGetChildrenCallback, data, textStatus, jqXhr) {
            var nodes = TreeNodeConverter.createNodesFromData(data.Items);
            onGetChildrenCallback.apply(this, [nodes]);
        },

        search: function(searchText, sortOrder, onSearchCallback) {
            var onSearchHandler = $.proxy(this._onGetTree, this, onSearchCallback);
            this._callGet({ searchText: searchText, sortOrder: sortOrder }, onSearchHandler, this.options.searchTreeMethod);
        },

        getTree: function (sortOrder, onGetFirstLevelItemsCallback) {
            var onGetFirstLevelItemsHandler = $.proxy(this._onGetTree, this, onGetFirstLevelItemsCallback);
            this._callGet({ sortOrder: sortOrder }, onGetFirstLevelItemsHandler, this.options.getTreeMethod);
        },

        sortTree: function (sortOrder, rootNode, searchText, onSortTreeCallback) {
            var onSortTreeHandler = $.proxy(this._onGetTree, this, onSortTreeCallback);
            this._callGet({ treeAsJson: JSON.stringify(TreeNodeConverter.toTreeOfIds(rootNode)), sortOrder: sortOrder, searchText: searchText }, onSortTreeHandler, this.options.sortTreeMethod);
        }

    };

    DynamicTreeViewController._defaults = {
        serviceRoot: "InternalServices",
        sortTreeMethod: "ItemListService/SortPages",
        getTreeMethod: "ItemListService/GetPages",
        searchTreeMethod: "ItemListService/SearchPages",
        getNodeDescendantsMethod: "ItemListService/GetPageDescendants",
        getTreeWithNodeMethod: "ItemListService/GetTreePathForPage",
        rootId: "Root"
    };

    DynamicTreeViewController.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DynamicTreeViewController._defaults, settings);
        }
        return DynamicTreeViewController._defaults;
    };

}).apply(dnn, [jQuery, window, document]);


dnn.createDropDownList = function (selector, options, methods) {
    $(document).ready(function () {
        var $element = $(selector);
        // in case when control is excluded from rendering by server tags logic,
        // a check on the html element existence is performed here:
        if ($element.length === 0) {
            return;
        }
        $.extend(options, methods);
        var element = $element[0];
        var instance = new dnn.DropDownList(element, options);
        if (element.id) {
            dnn[element.id] = instance;
        }
    });
};

