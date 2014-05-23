; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

            if (this.options.scroll) {
                this.$element.jScrollbar();
            }

            this._numberOfNodes = 0;

            var $tree = this.$element.find("." + this.options.treeContainerCss);
            this._tree = new dnn.TreeView($tree[0], { showRoot: true });

            var onRedrawTreeHandler = $.proxy(this._onRedrawTree, this);
            $(this._tree).on("onredrawtree", onRedrawTreeHandler);

            var onCreateNodeElementHandler = $.proxy(this._onCreateNodeElement, this);
            $(this._tree).on("oncreatenode", onCreateNodeElementHandler);
            
            var onShowChildrenHandler = $.proxy(this._onShowChildren, this);
            $(this._tree).on("onshowchildren", onShowChildrenHandler);

            this._onNodeIconClickHandler = $.proxy(this._onNodeIconClick, this);
            this._onNodeTextClickHandler = $.proxy(this._onNodeTextClick, this);

            this._firstNode = this.options.firstNode;

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
                this._onExpandNode(nodeContext);
                tooltip = this.options.folderLoadingTooltip;
            }
            else {
                // collapse: slide up and remove
                this._tree.removeChildren($node[0]);
                $clickedElement.removeClass(this.options.expandedCss).addClass(this.options.collapsedCss);
                this._onCollapseNode(nodeContext);
                tooltip = this.options.folderExpandTooltip;
            }
            $clickedElement.prop("title", tooltip);
        },

        showChildren: function (nodeContext, children) {
            nodeContext.data.hasChildren = children.length > 0;
            nodeContext.removeChildren().addChildNodes(children);
            this._tree.showChildren(nodeContext);
            this._initializeNodeIcon(nodeContext, this._tree.getNodeElement(nodeContext));
            this._ensureSelectedNode();
            this._numberOfNodes += children.length;
        },

        _onCollapseNode: function (nodeContext) {
            this._numberOfNodes -= nodeContext.getNumberOfChildren(true);
            if (this._selectedNode !== nodeContext && nodeContext.hasNode(this._selectedNode)) {
                this._selectedNode = null;
            }
            nodeContext.removeChildren();
            $(this).trigger($.Event("oncollapsenode"), [nodeContext]);
        },

        _onExpandNode: function (nodeContext) {
            $(this).trigger($.Event("onexpandnode"), [nodeContext]);
        },

        count: function () {
            return this._numberOfNodes;
        },

        _selectNodeElement: function (nodeContext, selected) {
            if (this._tree) {
                this._tree.addNodeClass(nodeContext, selected, this.options.selectedCss);
            }
        },

        _onRedrawTree: function () {
            this.updateLayout();
            $(this).trigger($.Event("onredrawtree"));
        },
        
        _onShowChildren: function () {
            $(this).trigger($.Event("onshowchildren"));
        },

        updateLayout: function () {
            if (this.options.scroll) {
                this.$element.jScrollbar("update");
            }
        },

        _ensureSelectedNode: function () {
            this.selectedId(this._selectedNodeId);
        },

        _getNodeById: function (id) {
            var predicate = function(data) { return data && data.id === id; };
            var path = dnn.NTree.getPath(this._rootNode, predicate);
            return path.length > 0 ? path.pop() : null;
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
        
        scrollToSelectedNode: function() {               
            //if node selected, we need scoll tree to show the selected node.
            var node = this._getNodeById(this.selectedId());
            if (!node) {
                //if node is not exist, we should pop a node expand request to make sure its load by expand parents nodes.
                $(this).trigger($.Event("onrequestexpand"));
                return;
            }
            if (this.options.scroll && node) {
                var $nodeElement = this._tree.getNodeElement(node);
                var offset = $nodeElement.position().top - $nodeElement.parentsUntil('div', 'ul[class*="' + this._tree.options.nodeListCss + '"][class*="' + this._tree.options.rootCss + '"]').position().top;
                this.$element.scrollTop(offset);
                this.updateLayout();
            }
        },

        selectedPath: function () {
            var dataPath = [];
            var id = this.selectedId();
            var predicate = function(data) { return data && data.id === id; };
            var path = dnn.NTree.getPath(this._rootNode, predicate);
            if (path.length > 1) {
                path.shift(); // remove the first root node;
                for (var i = 0, size = path.length; i < size; i++) {
                    dataPath.push(path[i].data);
                }
            }
            return dataPath;
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

        rootNode: function (root) {
            if (typeof root !== "undefined") {
                this._tree.collapseTree($.proxy(this._showTree, this, root));
            }
            return this._rootNode;
        },

        _showTree: function (rootNode) {
            this._rootNode = rootNode;
            if (this._firstNode) {
                this._rootNode.children.insertAt(0, this._firstNode);
            }
            this._tree.showTree(this._rootNode.children);
            this._selectedNode = null;
            this._ensureSelectedNode();
            this._tree.addNodeClass(this._firstNode, true, this.options.firstItemCss);

            this._numberOfNodes = this._rootNode.getNumberOfChildren(true) - (this._firstNode ? 1 : 0);
        },

        _onChangeNode: function (node) {
            var nodeComparer = function(one, another) { return one && another && one.id === another.id; };
            if (!dnn.NTree.equals(node, this._selectedNode, nodeComparer)) {
                this.selectedNode(node);
                $(this).trigger($.Event("onchangenode"), [node]);
            }
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
        searchOnCss: "searchOn",
        firstItemCss: "first-item",
        treeContainerCss: "dt-tree",
        scroll: true
    };

    DynamicTreeView.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DynamicTreeView._defaults, settings);
        }
        return DynamicTreeView._defaults;
    };


    //
    // SortableTreeView
    //
    var SortableTreeView = this.SortableTreeView = function (element, options, controller) {
        this.element = element;
        this.options = options;
        this.controller = controller;

        this.init();
    };

    SortableTreeView.prototype = {

        constructor: SortableTreeView,

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend({}, SortableTreeView.defaults(), this.options);

            this.$this = $(this);

            this.$element = this.element ? $(this.element) : this._createLayout();

            this._$itemListHeaderElement = this.$element.find("." + this.options.headerCss);
            this._$clearButton = this._$itemListHeaderElement.find("." + this.options.clearButtonCss);
            this._$clearButton.on("click.drop-down-list", $.proxy(this._onClearClick, this));
            this._$searchButton = this._$itemListHeaderElement.find("." + this.options.searchButtonCss);
            this._$searchButton.on("click.drop-down-list", $.proxy(this._onSearchClick, this));
            var onSortHandler = $.proxy(this._onSortClick, this);
            var onSearchHandler = $.proxy(this._onSearchClick, this);
            this._$searchContainer = this._$itemListHeaderElement.find("." + this.options.searchContainerCss);
            this._$searchInput = this._$itemListHeaderElement.find("." + this.options.searchInputCss).onEnter(onSearchHandler).on("keyup.drop-down-list", $.proxy(this._displayClearButton, this));
            this._displayClearButton();
            this._$sortButton = this._$itemListHeaderElement.find("." + this.options.sortButtonCss);
            this._$sortButton.on("click.drop-down-list", onSortHandler);

            this._$itemListFooterElement = this.$element.find("." + this.options.footerCss);
            this._$resultValue = this._$itemListFooterElement.find("." + this.options.resultElementCss).children(":first");

            this._$itemListContentElement = this.$element.find("." + this.options.contentCss);

            this._onLoadTreeHandler = $.proxy(this._onLoadTree, this);

            this._firstNode = this.options.firstItem ? TreeNodeConverter.createNodeFromModel(this.options.firstItem) : null;
            this.options.firstNode = this._firstNode;

            this._dynamicTree = new DynamicTreeView(this._$itemListContentElement[0], this.options);
            var $dynamicTree = $(this._dynamicTree);
            $dynamicTree.on("onredrawtree", $.proxy(this._onRedrawTree, this));
            $dynamicTree.on("onshowchildren", $.proxy(this._onShowChildren, this));
            $dynamicTree.on("onexpandnode", $.proxy(this._onExpandNode, this));
            $dynamicTree.on("oncollapsenode", $.proxy(this._onCollapseNode, this));
            $dynamicTree.on("onselectnode", $.proxy(this._onSelectNode, this));
            $dynamicTree.on("onchangenode", $.proxy(this._onChangeNode, this));
            $dynamicTree.on("onrequestexpand", $.proxy(this._onRequestExpand, this));

            this._sortOrder(dnn.SortOrder.unspecified);

        },

        _createLayout: function () {
            var layout = $("<div class='" + this.options.containerCss + "'/>")
                    .append($("<div class='" + this.options.headerCss + "'/>")
                        .append($("<a href='javascript:void(0);' class='" + this.options.sortButtonCss + "'><span>" + this.options.sortAscendingButtonTitle + "</span></a>"))
                        .append($("<div class='" + this.options.searchContainerCss + "'/>")
                            .append($("<div class='" + this.options.searchInputContainerCss + "'/>")
                                .append($("<input type='text' placeholder='" + this.options.searchInputPlaceHolder + "' maxlength='200' autocomplete='off' class='" + this.options.searchInputCss + "'>")))
                            .append($("<a href='javascript:void(0);' title='" + this.options.clearButtonTooltip + "' style='display:none;' class='" + this.options.clearButtonCss + "'/>"))
                            .append($("<a href='javascript:void(0);' title='" + this.options.searchButtonTooltip + "' class='" + this.options.searchButtonCss + "'/>"))))
                    .append($("<div class='" + this.options.contentCss + "'/>")
                        .append($("<div class='" + this.options.treeContainerCss + "'/>")))
                    .append($("<div class='" + this.options.footerCss + "'/>")
                        .append($("<span class='" + this.options.resultElementCss + "'><b></b> " + this.options.resultsText + "</span>"))
                        .append($("<div class='" + this.options.resizerElementCss + "'/>")));
            return layout;
        },

        _onExpandNode: function(eventObject, nodeContext) {
            var onLoadChildNodesHandler = $.proxy(this._onLoadChildren, this, nodeContext);
            var searchText = this._searchInput();
            this.controller.getChildren(nodeContext.data.id, this._sortOrder(), searchText, onLoadChildNodesHandler);
        },

        _onCollapseNode: function (eventObject, nodeContext) {
            this._updateResult(this._dynamicTree.count());
        },

        _onLoadChildren: function (nodeContext, children) {
            this._dynamicTree.showChildren(nodeContext, children);
            this._updateResult(this._dynamicTree.count());
            
            $(this).trigger($.Event('onloadchildren'), [nodeContext]);
        },
        _onShowChildren: function(nodeContext) {
            $(this).trigger($.Event('onshowchildren'), [nodeContext]);
        },
        _onRequestExpand: function(nodeContext) {
            $(this).trigger($.Event('onrequestexpand'), [nodeContext]);
        },

        _updateResult: function (resultText) {
            this._$resultValue.text(resultText);
        },

        _searchInput: function(val) {
            if (typeof val === "undefined") {
                return this._$searchInput.val().trim();
            }
            this._$searchInput.val(val);
            return val;
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

        _onRedrawTree: function () {
        },

        selectedId: function (id) {
            return this._dynamicTree.selectedId(id);
        },

        selectedPath: function () {
            return this._dynamicTree.selectedPath();
        },

        selectedNode: function (node) {
            return this._dynamicTree.selectedNode(node);
        },

        _onChangeNode: function (eventObject, node) {
            $(this).trigger($.Event('onchangenode'), [node]);
        },

        _onSelectNode: function (eventObject, node) {
            $(this).trigger($.Event('onselectnode'), [node]);
        },

        _onResize: function (eventObject, containerSize) {
            this._$itemListContentElement.height(containerSize.height - this._itemListHeaderHeight - this._itemListFooterHeight);
            this._dynamicTree.updateLayout();
        },

        _onLoadTree: function (rootNode) {
            this._dynamicTree.rootNode(rootNode);
            this._updateResult(this._dynamicTree.count());
            this._loading(false);
            
            $(this).trigger($.Event('ontreeloaded'));
        },

        show: function () {
            /* the component is a part of DOM, the dimension is known at this point */
            this._itemListHeaderHeight = this._$itemListHeaderElement.outerHeight(true);
            this._itemListFooterHeight = this._$itemListFooterElement.outerHeight(true);
            this._$itemListContentElement.height(this.$element.height() - this._itemListHeaderHeight - this._itemListFooterHeight);

            if (!this._resizer) {
                var $resizerElement = this._$itemListFooterElement.find("." + this.options.resizerElementCss);
                this._resizer = new dnn.Resizer($resizerElement[0], { container: this.$element });
                $(this._resizer).bind("resized", $.proxy(this._onResize, this));
            }

            this._showTree(this._selectedNodeId);
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
                this._$itemListContentElement.addClass(this.options.loadingCss);
            }
            else {
                this._$itemListContentElement.removeClass(this.options.loadingCss);
            }
        },

        _showTree: function (selectedItemId) {
            this._loading(true);
            if (selectedItemId && (!this._firstNode || (this._firstNode && this._firstNode.data.id !== selectedItemId))) {
                this._searchInput("");
                this.controller.getTreeWithItem(selectedItemId, this._sortOrder(), this._onLoadTreeHandler);
            }
            else {
                this.controller.getTree(this._sortOrder(), this._onLoadTreeHandler);
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
            var searchText = this._searchInput();
            if (searchText) {
                this._$searchContainer.addClass(this.options.searchOnCss);
                this.controller.search(searchText, this._sortOrder(), this._onLoadTreeHandler);
                this._searchHasBeenApplied = true;
            }
            else {
                this._$searchContainer.removeClass(this.options.searchOnCss);
                this.controller.getTree(this._sortOrder(), this._onLoadTreeHandler);
                this._searchHasBeenApplied = false;
            }
        },

        _sortOrder: function (order) {
            if (typeof order === "undefined") {
                return this._sortOrderValue;
            }
            if (order === dnn.SortOrder.ascending) {
                this._$sortButton.removeClass("desc").addClass("asc").prop("title", this.options.sortDescendingButtonTooltip);
            }
            else if (order === dnn.SortOrder.descending) {
                this._$sortButton.removeClass("asc").addClass("desc").prop("title", this.options.unsortedOrderButtonTooltip);
            }
            else { // order === dnn.SortOrder.unspecified
                this._$sortButton.removeClass("desc").removeClass("asc").prop("title", this.options.sortAscendingButtonTooltip);
            }
            return this._sortOrderValue = order;
        },

        _getNextSortOrder: function() {
            var order = this._sortOrder();
            if (this.options.disableUnspecifiedOrder) {
                if (order === dnn.SortOrder.ascending) {
                    return dnn.SortOrder.descending;
                }
                
                return dnn.SortOrder.ascending;
            }
            if (order === dnn.SortOrder.unspecified) {
                return dnn.SortOrder.ascending;
            }
            if (order === dnn.SortOrder.ascending) {
                return dnn.SortOrder.descending;
            }
            return dnn.SortOrder.unspecified;
        },

        _sort: function () {
            this._loading(true);
            this._sortOrder(this._getNextSortOrder());
            var searchText = "";
            if (this._searchHasBeenApplied) {
                searchText = this._searchInput();
                if (searchText === "") {
                    this._$searchContainer.removeClass(this.options.searchOnCss);
                    this._searchHasBeenApplied = false;
                }
            }
            this.controller.sortTree(this._sortOrder(), this._dynamicTree.rootNode(), searchText, this._onLoadTreeHandler);
        }

    };

    SortableTreeView._defaults = {
        searchOnCss: "searchOn",
        containerCss: "dt-container",
        headerCss: "dt-header",
        footerCss: "dt-footer",
        contentCss: "dt-content",
        treeContainerCss: "dt-tree",
        clearButtonCss: "clear-button",
        searchButtonCss: "search-button",
        searchContainerCss: "search-container",
        searchInputContainerCss: "search-input-container",
        searchInputCss: "search-input",
        sortButtonCss: "sort-button",
        resultElementCss: "result",
        resizerElementCss: "resizer",
        loadingCss: "loading-items"
    };

    SortableTreeView.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(SortableTreeView._defaults, settings);
        }
        return SortableTreeView._defaults;
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

    var DynamicTreeViewController = this.DynamicTreeViewController = function (options) {
        this.options = options;
        this.init();
    };

    DynamicTreeViewController.prototype = {
        constructor: DynamicTreeViewController,

        init: function() {
            this.options = $.extend({}, DynamicTreeViewController.defaults(), this.options);
            this._serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
            this.parameters = this.options.parameters;
        },

        _callGet: function (data, onLoadHandler, method) {
            $.extend(data, this.parameters);
            var serviceSettings = {
                url: this._serviceUrl + method,
                beforeSend: $.dnnSF(this.options.moduleId).setModuleHeaders,
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
