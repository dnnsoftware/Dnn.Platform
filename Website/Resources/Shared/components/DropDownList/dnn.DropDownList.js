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
            var controller = new dnn.DynamicTreeViewController(this.options.services);
            var treeView = new dnn.SortableTreeView(this._$itemListContainer[0], this.options.itemList, controller);
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
            var item = node ? dnn.TreeNodeConverter.toKeyValue(node.data) : null;
            this.selectedItem(item);

            if (this.options.onSelectionChanged && this.options.onSelectionChanged.length) {
                for (var i = 0, size = this.options.onSelectionChanged.length; i < size; i++) {
                    dnn.executeFunctionByName(this.options.onSelectionChanged[i], window, item);
                }
            }
            if (typeof this.options.onSelectionChangedBackScript === "function") {
                this.options.onSelectionChangedBackScript.apply(this);
            }
            var e = $.Event('onchangenode');
            this.$this.trigger(e, [item]);
        }

    };

    DropDownList._defaults = {};

    DropDownList.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(DropDownList._defaults, settings);
        }
        return DropDownList._defaults;
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

