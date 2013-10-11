; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

// the semi-colon before function invocation is a safety net against concatenated 
// scripts and/or other plugins which may not be closed properly.
(function ($, window, document, undefined) {
    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variables rather than globals
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).


    // The actual plugin constructor
    var TreeView = this.TreeView = function (element, options) {
        this.$this = $(this);
        this.$element = $(element);
        this.options = options;
        this.init();
    };

    TreeView.prototype = {

        init: function () {
            // Place initialization logic here
            // You already have access to the DOM element and the options via the instance, 
            // e.g., this.element and this.options
            this.options = $.extend(TreeView.defaults(), this.options);
            this._onRedrawHandler = $.proxy(this._onRedraw, this);
        },

        _createChildrenNodeElement: function (childrenContext, isRoot) {
            if (!childrenContext || childrenContext.length === 0) {
                return $([]);
            }
            var childContext;
            var $childNodeElement;
            var $childrenNodeElement = $("<ul/>");

            $childrenNodeElement.addClass(this.options.nodeListCss);
            if (isRoot) {
                $childrenNodeElement.addClass(this.options.rootCss);
            }
            for (var i = 0, count = childrenContext.length; i < count; i++) {
                childContext = childrenContext[i];
                if (childContext) {
                    $childNodeElement = this._createNodeElement(childContext);
                    $childNodeElement.append(this._createChildrenNodeElement(childContext.children));
                    $childrenNodeElement.append($childNodeElement);
                }
            }
            return $childrenNodeElement;
        },

        showTree: function (nodeContext) {
            if (!nodeContext) {
                return;
            }
            var nodes = Object.IsArray(nodeContext) ? nodeContext : [nodeContext];
            this._$treeElement = this._createChildrenNodeElement(nodes, true).hide();
            this.$element.empty().append(this._$treeElement);
            this._$treeElement.slideDown({ duration: 0, complete: this._onRedrawHandler });
        },

        collapseTree: function (onComplete) {
            var self = this;
            if (this._$treeElement && this._$treeElement.length) {
                this._$treeElement.slideUp({
                    duration: 0,
                    complete: function() {
                        $(this).remove();
                        self._onRedraw();
                        typeof onComplete === "function" && onComplete.apply(self);
                    }
                });
            }
            else {
                typeof onComplete === "function" && onComplete.apply(self);
            }
        },

        showChildren: function (nodeContext) {
            if (!nodeContext) {
                return null;
            }
            var $childrenNodeElement = this._createChildrenNodeElement(nodeContext.children).hide();
            var $nodeElement = this.getNodeElement(nodeContext);
            $nodeElement.children('ul').remove();
            $nodeElement.append($childrenNodeElement);
            $childrenNodeElement.slideDown({ duration: this.options.expandSpeed, easing: this.options.expandEasing, complete: this._onRedrawHandler });
            return $childrenNodeElement;
        },

        removeChildren: function (nodeElement) {
            var self = this;
            var nodeContext = this.nodeContext(nodeElement);
            $(nodeElement).find('ul').slideUp(this.options.collapseSpeed, this.options.collapseEasing, function () { $(this).remove(); self._onRedraw(nodeContext); });
        },

        _onRedraw: function (nodeContext) {
            var e = $.Event('onredrawtree');
            this.$this.trigger(e, [nodeContext]);
        },

        getNodeElement: function (nodeContext) {
            var rootElement = this.getRootElement();
            var self = this;
            var filterFunction = function (i) {
                var context = self.nodeContext(this);
                return context === nodeContext;
            };
            //var list = $(rootElement).find('li').andSelf().filter(filterFunction);
            var list = $(rootElement).find('li').filter(filterFunction);
            return list.first();
        },

        _createNodeElement: function (nodeContext) {
            if (!nodeContext) {
                return null;
            }
            var $nodeElement = $("<li/>").addClass(this.options.nodeCss);
            this.nodeContext($nodeElement[0], nodeContext);
            this.$this.trigger("oncreatenode", [$nodeElement[0], nodeContext ]);
            return $nodeElement;
        },

        /*_removeSelections: function() {
			var rootElement = this.getRootElement();
			$(rootElement).find('li').andSelf().each(function (index, element) { $(element).children().eq(1).removeClass("selected"); });
		},*/

        addNodeClass: function (nodeContext, add, className) {
            if (!nodeContext) {
                return;
            }
            var $nodeElement = this.getNodeElement(nodeContext);
            var addClass = typeof (add) === "undefined" || add;
            var element = $nodeElement.children().eq(1);
            if (addClass) {
                element.addClass(className);
            }
            else {
                element.removeClass(className);
            }
        },

        getRootElement: function () {
            var $node = this._$treeElement; //this.$element.find("ul li:first");
            return ($node && $node.length > 0) ? $node[0] : null;
        },

//        _getParentNodeElement: function (node) {
//            if (node && node !== this.getRootElement()) {
//                var $parent = $(node).parent().parent();
//                if ($parent && $parent.length > 0) {
//                    return $parent[0];
//                }
//            }
//            return null;
//        },

        nodeContext: function (element, context) {
            var nodeElement = (element) ? $(element).closest("li")[0] : this.getRootElement();
            if (!nodeElement) {
                return null;
            }
            if (typeof context === "undefined") {
                return $.data(nodeElement, "tree-node");
            }
            else {
                $.data(nodeElement, "tree-node", context);
            }
            return context;
        }

    };

    TreeView._defaults = {
        expandSpeed: 200, // default = 200 (ms); use -1 for no animation
        collapseSpeed: 200, // default = 200 (ms); use -1 for no animation
        expandEasing: null, // easing function to use on expand (optional)
        collapseEasing: null, //easing function to use on collapse (optional)
        nodeListCss: "tv-nodes",
        nodeCss: "tv-node",
        rootCss: "tv-root"
    };

    TreeView.defaults = function (settings, dataAndMethods) {
        if (typeof settings !== "undefined") {
            $.extend(TreeView._defaults, settings);
        }
        if (typeof dataAndMethods !== "undefined") {
            $.extend(TreeView._defaults, dataAndMethods);
        }
        return TreeView._defaults;
    };

}).apply(dnn, [jQuery, window, document]);

