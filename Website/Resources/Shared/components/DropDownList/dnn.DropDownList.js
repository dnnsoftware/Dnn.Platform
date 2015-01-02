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
    // DropDownList
    //
    var DropDownList = this.DropDownList = function (element, options, controller) {
        this.element = element;
        this.options = options;
        this._controller = controller;

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

            this.$element = this.element ? $(this.element) : this._createLayout();

            this._$selectedItemContainer = this.$element.find("." + this.options.selectedItemCss);
            this._$selectedItemCaption = this._$selectedItemContainer.find("." + this.options.selectedValueCss);

            this._onOpenItemListHandler = $.proxy(this._onOpenItemList, this);
            this._onCloseItemListHandler = $.proxy(this._onCloseItemList, this);
            this._onSelectedItemCaptionClickHandler = $.proxy(this._onSelectedItemCaptionClick, this);

            this._onDocumentClickHandler = $.proxy(this._onDocumentClick, this);

            this._$selectedItemCaption.on("click", this._onSelectedItemCaptionClickHandler);

            this._state = new StateField(this.options.initialState, this.options.internalStateFieldId);

            var stateValue = this._state.val();
            this._setSelectedItemCaption(stateValue ? stateValue.selectedItem : null);

            this.disabled(this.options.disabled);

            this._id = dnn.guid();
        },

        _createLayout: function () {
            var layout = $("<div id='" + (this.options.containerId || dnn.uid()) + "' class='" + this.options.containerCss + "'/>")
                    .append($("<div class='" + this.options.selectedItemCss + "'/>")
                        .append($("<a href='javascript:void(0);' class='" + this.options.selectedValueCss + "'/>")));
            return layout;
        },
        
        id: function() {
            return this.$element.attr('id');
        },

        selectedItem: function (item) {
            if (typeof item === "undefined") {
                // getter:
                var state = this._state.val();
                return state ? state.selectedItem : null;
            }
            // setter:
            this._state.val({ selectedItem: item });
            this._setSelectedItemCaption(item);
            if (this._treeView) {
                this._treeView.selectedId(item ? item.key : null);
            }
            this.$element.trigger("selectedItemChanged", item);
            
            return item;
        },

        selectedPath: function () {
            return this._treeView ? this._treeView.selectedPath() : [];
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
        
        refresh: function (parentId) {
            this.options.services.parameters.parentId = parentId;
            if (this._treeView) {
                this._treeView._showTree();
                this._treeView._dynamicTree.updateLayout();
            }
        },

        _setSelectedItemCaption: function (item) {
            var caption = item ? item.value : "";
            this._$selectedItemCaption.text(caption || this.options.selectItemDefaultText);
        },

        _onOpenItemList: function () {
            this._$selectedItemCaption.addClass(this.options.openedItemListCss);
            $(document).on("click." + this._id, this._onDocumentClickHandler);

            this._treeView._dynamicTree.scrollToSelectedNode();
        },

        _onCloseItemList: function () {
            this._$selectedItemCaption.removeClass(this.options.openedItemListCss);
            $(document).off("click." + this._id, this._onDocumentClickHandler);
            this._$treeViewLayout.detach(); // keeps all jQuery data associated with the removed element
        },

        _openItemList: function () {
            var showTreeView = false;
            if (!this._treeView) {
                this._treeView = this._createTreeView();
                this._$treeViewLayout = this._treeView.$element;
                showTreeView = true;
            }
            // append treeView layout to DOM before calling show
            if (!$.inContainer(this.$element, this._$treeViewLayout[0])) {
                this.$element[0].appendChild(this._$treeViewLayout[0]);
            }
            if (showTreeView) {
                this._treeView.show();
            }
            this._$treeViewLayout.slideDown(this.options.openItemListSpeed, this._onOpenItemListHandler);
        },

        _isItemListOpen: function() {
            return this._$treeViewLayout && this._$treeViewLayout.is(':visible');
        },

        _closeItemList: function () {
            this._$treeViewLayout.slideUp(this.options.closeItemListSpeed, this._onCloseItemListHandler);
        },

        _onSelectedItemCaptionClick: function (eventObject) {
            if (this._disabled) {
                return;
            }
            this._isItemListOpen() ? this._closeItemList() : this._openItemList();
        },

        _onDocumentClick: function (eventObject) {
            // clicked outside of the dropdown list
            // when click on the element, the onMouseClick handler is executed
            var clickedElement = eventObject.target;
            if (!$.inContainer(this.$element, clickedElement) && this._isItemListOpen()) {
                this._closeItemList();
            }
        },

        _createTreeView: function () {
            var controller = this._controller ? this._controller : new dnn.DynamicTreeViewController(this.options.services);
            var treeView = new dnn.SortableTreeView(null, this.options.itemList, controller);
            var onChangeNodeHandler = $.proxy(this._onChangeNode, this);
            var onSelectNodeHandler = $.proxy(this._onSelectNode, this);
            var onTreeLoadedHandler = $.proxy(this._onTreeLoaded, this);
            var onShowChildrenHandler = $.proxy(this._onShowChildren, this);
            var onRequestExpandHandler = $.proxy(this._onRequestExpand, this);
            $(treeView).on("onchangenode", onChangeNodeHandler)
                       .on("onselectnode", onSelectNodeHandler)
                       .on("ontreeloaded", onTreeLoadedHandler)
                       .on("onloadchildren", onTreeLoadedHandler)
                       .on("onshowchildren", onShowChildrenHandler)
                       .on("onrequestexpand", onRequestExpandHandler);
            var item = this.selectedItem();
            treeView.selectedId(item ? item.key : null);
            return treeView;
        },
        
        _onTreeLoaded: function () {
            var id = this.$element[0].id;
            if (!id) {
                return;
            }

            var expandPath = dnn.getVar(id + "_expandPath");

            if (expandPath == null) {
                $(this._treeView).off("onshowchildren");
            }

            if (expandPath && expandPath.length > 0) {
                var position = expandPath.indexOf(',') > -1 ? expandPath.indexOf(',') : expandPath.length;
                var nodeId = expandPath.substr(0, position);
                var leftPath = expandPath.substr(position);
                if (leftPath.length > 1 && leftPath[0] == ',') {
                    leftPath = leftPath.substr(1);
                }

                dnn.setVar(id + "_expandPath", leftPath);
                var dynamicTree = this._treeView._dynamicTree;
                var node = dynamicTree._getNodeById(nodeId);
                if (node) {
                    var expandIcon = dynamicTree._tree.getNodeElement(node).children(":first");
                    if (!expandIcon.hasClass(dynamicTree.options.expandedCss)) {
                        expandIcon.trigger('click');
                    } else {
                        this._onTreeLoaded();
                    }
                }
            }
        },
        
        _onShowChildren: function () {
            this._treeView._dynamicTree.scrollToSelectedNode();
            var id = this.$element[0].id;
            if (id) {
                var expandPath = dnn.getVar(id + "_expandPath");
                if (!expandPath) {
                    $(this._treeView).off("onshowchildren");
                }
            }
        },
        
        _onRequestExpand: function() {
            this._onTreeLoaded();
        },

        _onSelectNode: function(eventObject, node) {
            this._closeItemList();
        },

        _onChangeNode: function(eventObject, node) {
            var item = node ? dnn.TreeNodeConverter.toKeyValue(node.data) : null;
            this.selectedItem(item);

            if (this.options.onSelectionChanged && this.options.onSelectionChanged.length) {
                for (var i = 0, size = this.options.onSelectionChanged.length; i < size; i++) {
                    dnn.executeFunctionByName(this.options.onSelectionChanged[i], window, item, eventObject.target.$element);
                }
            }
            if (typeof this.options.onSelectionChangedBackScript === "function") {
                this.options.onSelectionChangedBackScript.apply(this);
            }
            this.$this.trigger($.Event("onchangenode"), [item]);
        }

    };

    DropDownList._defaults = {
        containerCss: "dnnDropDownList",
        selectedItemCss: "selected-item",
        selectedValueCss: "selected-value",
        openedItemListCss: "opened",
        openItemListSpeed: "fast",
        closeItemListSpeed: "fast"
    };

    DropDownList.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DropDownList._defaults, settings);
        }
        return DropDownList._defaults;
    };

    var StateField = this.StateField = function (initialState, stateElementId) {
        if (stateElementId) {
            this._stateElement = document.getElementById(stateElementId);
        }
        this.val(initialState);
    };

    StateField.prototype = {
        constructor: StateField,

        val: function (stateObject) {
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
        }

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

