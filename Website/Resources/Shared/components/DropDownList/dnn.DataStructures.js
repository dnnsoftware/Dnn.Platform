; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

(function ($, window, document, undefined) {
    "use strict";

    var NTree = this.NTree = function (data) {
        this.data = (typeof data === "undefined") ? null : data;
        this.children = [];
    };

    NTree.prototype = {

        addChildNode: function (node) {
            this.children.push(node);
            return this;
        },

        addChildNodes: function (nodes) {
            this.children = this.children.concat(nodes);
            return this;
        },

        hasNode: function (node) {
            return NTree.hasNode(this, node);
        },

        removeChildren: function () {
            for (var i = 0, count = this.children.length; i < count; i++) {
                var child = this.children[i];
                if (child) {
                    child.removeChildren();
                }
            }
            this.children = [];
            return this;
        },

        getNumberOfChildren: function (recursive) {
            var n = this.children.length;
            if (recursive) {
                for (var i = 0, count = this.children.length; i < count; i++) {
                    var child = this.children[i];
                    if (child) {
                        n += child.getNumberOfChildren(recursive);
                    }
                }
            }
            return n;
        },

        hasChildren: function () {
            return (this.children && this.children.length > 0);
        },

        inOrderTraverse: function (callback) {
            return NTree.inOrderTraverse(this, callback);
        }

    };

    NTree.equals = function (one, another) {
        if (one === another) {
            return true;
        }
        if (one && another && one.data && another.data) {
            return one.data.id === another.data.id;
        }
        return false;
    };

    NTree.sort = function (root, comparer) {
        if (!(root && comparer)) {
            return;
        }
        root.children.sort(comparer);
        for (var i = 0, count = root.children.length; i < count; i++) {
            NTree.sort(root.children[i], comparer);
        }
    };

    NTree.hasNode = function (root, node) {
        if (!(root && node)) {
            return false;
        }
        if (root === node) {
            return true;
        }
        for (var i = 0, count = root.children.length; i < count; i++) {
            if (NTree.hasNode(root.children[i], node)) {
                return true;
            }
        }
        return false;
    };

    NTree.isEmpty = function (node) {
        if (!node) {
            return true;
        }
        return (!node.data && (!node.children || node.children.length === 0));
    };

    NTree.inOrderTraverse = function (root, callback) {
        if (!root) {
            return false;
        }
        var node = root;
        var stack = [];
        var indexStack = [];
        var index = 0;
        while (true) {
            while (node) {
                if (callback && callback(node, stack)) {
                    stack = [];
                    indexStack = [];
                    break;
                }
                if (node.children.length > 0) {
                    stack.push(node);
                    indexStack.push(0);
                    node = node.children[0];
                }
                else {
                    node = null;
                }
            }
            if (stack.length === 0) {
                break;
            }
            node = stack.pop();
            index = indexStack.pop();
            if (node.children.length - 1 > index) {
                index += 1;
                stack.push(node);
                indexStack.push(index);
                node = node.children[index];
            }
            else {
                node = null;
            }
        }
        return true;
    };

    var Dictionary = this.Dictionary = function (elements) {
        this.elements = elements || {};
        this.hasSpecialProto = false; // has "__proto__" key
        this.specialProto = undefined; // "__proto__" element
    };

    Dictionary.prototype = {

        has: function (key) {
            if (key === "__proto__") {
                return this.hasSpecialProto;
            }
            return {}.hasOwnProperty.call(this.elements, key);
        },

        get: function (key) {
            if (key === "__proto__") {
                return this.specialProto;
            }
            return this.has(key) ? this.elements[key] : undefined;
        },

        set: function(key, value) {
            if (key === "__proto__") {
                this.hasSpecialProto = true;
                this.specialProto = value;
            }
            else {
                this.elements[key] = value;
            }
        },

        remove: function(key) {
            if (key === "__proto__") {
                this.hasSpecialProto = false;
                this.specialProto = undefined;
            }
            else {
                delete this.elements[key];
            }
        },

        clear: function(key) {
            this.elements = [];
        },

        entries: function() {
            return this.elements;
        }

    };

}).apply(dnn, [jQuery, window, document]);
