; if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

(function ($, window, document, undefined) {
    "use strict";

    var NTree = this.NTree = function (data) {
        this.data = (typeof data === "undefined") ? null : data;
        this.children = [];
    };

    NTree.prototype = {

        constructor: NTree,

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

    NTree.equals = function (one, another, nodeComparer) {
        if (one === another) {
            return true;
        }
        return typeof nodeComparer === "function" && one && another && nodeComparer.call(this, one.data, another.data);
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
                if (typeof callback === "function" && callback.call(this, node, stack)) {
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

    NTree.getPath = function (root, predicate) {
        var path = [];
        var callback = function (traversedNode, stack) {
            if (typeof predicate === "function" && predicate.call(this, traversedNode.data)) {
                path = stack.slice(0); // clones array
                path.push(traversedNode); // adds the current node
                return true;
            }
            return false;
        };
        NTree.inOrderTraverse(root, callback);
        return path;
    };

    var Dictionary = this.Dictionary = function (elements) {
        this._elements = elements || {};
        this.hasSpecialProto = false; // has "__proto__" key
        this.specialProto = undefined; // "__proto__" element
    };

    Dictionary.prototype = {

        constructor: Dictionary,

        has: function (key) {
            if (key === "__proto__") {
                return this.hasSpecialProto;
            }
            return {}.hasOwnProperty.call(this._elements, key);
        },

        get: function (key) {
            if (key === "__proto__") {
                return this.specialProto;
            }
            return this.has(key) ? this._elements[key] : undefined;
        },

        set: function (key, value) {
            if (Object.isNullOrUndefined(key)) {
                throw "InvalidArgumentException {" + key + "},{" + value + "}";
            }
            if (key === "__proto__") {
                this.hasSpecialProto = true;
                this.specialProto = value;
            }
            else {
                this._elements[key] = value;
            }
        },

        remove: function(key) {
            if (key === "__proto__") {
                this.hasSpecialProto = false;
                this.specialProto = undefined;
            }
            else {
                delete this._elements[key];
            }
        },

        clear: function() {
            this._elements = [];
        },

        keys: function() {
            var names = Object.getOwnPropertyNames(this._elements);
            if (this.hasSpecialProto) {
                names.push("__proto__");
            }
            return names;
        },

        entries: function() {
            return this._elements;
        }

    };


    var MinHeap = this.MinHeap = function(array, comparator) {

        this.heap = array || [];

        /**
         * Default comparator used if an override is not provided.
         * @private
         */
        this.compare = comparator || function(one, another) {
            return one == another ? 0 : one < another ? -1 : 1;
        };

        /**
         * Retrieve the index of the left child of the node at index i.
         */
        this._left = function(index) {
            return 2 * index + 1;
        };

        /**
         * Retrieve the index of the right child of the node at index i.
         */
        this._right = function(index) {
            return 2 * index + 2;
        };

        /**
         * Retrieve the index of the parent of the node at index i.
         */
        this._parent = function (index) {
            return index > 0 ? (index - 1) >> 1 : -1; // (i - 1) / 2
        };

        /**
         * Ensure that the contents of the heap don't violate the constraint.
         */
        this._heapify = function(i) {
            var lIdx = this._left(i);
            var rIdx = this._right(i);
            var smallest;
            if (lIdx < this.heap.length && this.compare(this.heap[lIdx], this.heap[i]) < 0) {
                smallest = lIdx;
            }
            else {
                smallest = i;
            }
            if (rIdx < this.heap.length && this.compare(this.heap[rIdx], this.heap[smallest]) < 0) {
                smallest = rIdx;
            }
            if (i != smallest) {
                var temp = this.heap[smallest];
                this.heap[smallest] = this.heap[i];
                this.heap[i] = temp;
                this._heapify(smallest);
            }
        };

        /**
         * Starting with the node at index i, move up the heap until parent value
         * is less than the node.
         */
        this.siftUp = function(i) {
            var p = this._parent(i);
            if (p >= 0 && this.compare(this.heap[p], this.heap[i]) > 0) {
                var temp = this.heap[p];
                this.heap[p] = this.heap[i];
                this.heap[i] = temp;
                this.siftUp(p);
            }
        };

        /**
         * Heapify the contents of an array.
         * This function is called when an array is provided.
         * @private
         */
        this.heapifyArray = function() {
            // for loop starting from floor size/2 going up and heapify each.
            var i = Math.floor(this.heap.length / 2) - 1;
            for (; i >= 0; i--) {
                //	jstestdriver.console.log("i: ", i);
                this._heapify(i);
            }
        };

        // If an initial array was provided, then heapify the array.
        if (array != null) {
            this.heapifyArray();
        };
    };

    /**
     * Place an item in the heap.
     */
    MinHeap.prototype.push = function (item) {
        this.heap.push(item);
        this.siftUp(this.heap.length - 1);
    };

    /**
     * Insert an item into the heap.
     */
    MinHeap.prototype.insert = function (item) {
        this.push(item);
    };

    /**
     * Pop the minimum valued item off of the heap. The heap is then updated 
     * to float the next smallest item to the top of the heap.
     * @returns the minimum value contained within the heap.
     * @function
     */
    MinHeap.prototype.pop = function () {
        var value;
        if (this.heap.length > 1) {
            value = this.heap[0];
            // Put the bottom element at the top and let it drift down.
            this.heap[0] = this.heap.pop();
            this._heapify(0);
        }
        else {
            value = this.heap.pop();
        }
        return value;
    };

    /**
     * Remove the minimum item from the heap.
     * @returns the minimum value contained within the heap.
     */
    MinHeap.prototype.remove = function () {
        return this.pop();
    };

    /**
     * Returns the minimum value contained within the heap.  This will
     * not remove the value from the heap.
     * @returns the minimum value within the heap.
     * @function
     */
    MinHeap.prototype.getMin = function () {
        return this.heap[0];
    };

    /**
     * Return the current number of elements within the heap.
     * @returns size of the heap.
     * @function
     */
    MinHeap.prototype.size = function () {
        return this.heap.length;
    };

}).apply(dnn, [jQuery, window, document]);
