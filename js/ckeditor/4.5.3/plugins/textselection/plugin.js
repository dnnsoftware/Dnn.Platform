/* 
Copyright (c) 2003-2009, CKSource - Frederico Knabben. All rights reserved. 
For licensing, see LICENSE.html or http://ckeditor.com/license 
*/
    /** 
     * Represent plain text selection range. 
     */
    CKEDITOR.plugins.add('textselection',
    {
        version: 1.04,
        init: function (editor) {
            // Corresponding text range of wysiwyg bookmark.
            var wysiwygBookmark;

            // Auto sync text selection with 'WYSIWYG' mode selection range.
            if (editor.config.syncSelection
                    && CKEDITOR.plugins.sourcearea) {
                editor.on('beforeModeUnload', function (evt) {
                    if (editor.mode === 'source') {
                        if (editor.mode === 'source' && !editor.plugins.codemirror) {
                            var range = editor.getTextSelection();

                            // Fly the range when create bookmark. 
                            delete range.element;
                            range.createBookmark(editor);
                            sourceBookmark = true;
                            evt.data = range.content;
                        }
                    }
                });
                editor.on('mode', function () {
                    if (editor.mode === 'wysiwyg' && sourceBookmark) {

                        editor.focus();
                        var doc = editor.document,
                            range = new CKEDITOR.dom.range(editor.document),
                            walker,
                            startNode,
                            endNode,
                            isTextNode = false;

                        range.setStartAt(doc.getBody(), CKEDITOR.POSITION_AFTER_START);
                        range.setEndAt(doc.getBody(), CKEDITOR.POSITION_BEFORE_END);
                        walker = new CKEDITOR.dom.walker(range);
                        // walker.type = CKEDITOR.NODE_COMMENT;
                        walker.evaluator = function (node) {
                            //
                            var match = /cke_bookmark_\d+(\w)/.exec(node.$.nodeValue);
                            if (match) {
                                if(decodeURIComponent(node.$.nodeValue)
                                    .match(/<!--cke_bookmark_[0-9]+S-->.*<!--cke_bookmark_[0-9]+E-->/)){
                                    isTextNode = true;
                                    startNode = endNode = node;
                                    return false;
                                } else {
                                    if (match[1] === 'S') {
                                        startNode = node;
                                    } else if (match[1] === 'E') {
                                        endNode = node;
                                        return false;
                                    }
                                }
                            }
                        };
                        walker.lastForward();
                        range.setStartAfter(startNode);
                        range.setEndBefore(endNode);
                        range.select();
                        // Scroll into view for non-IE.
                        // Scroll into view for non-IE.
                        if (!CKEDITOR.env.ie || (CKEDITOR.env.ie && CKEDITOR.env.version === 9)) {
                            editor.getSelection().getStartElement().scrollIntoView(true);
                        } // Remove the comments node which are out of range.
                        if(isTextNode){
                            //remove all of our bookmarks from the text node
                            //then remove all of the cke_protected bits that added because we had a comment
                            //whatever code is supposed to clean these cke_protected up doesn't work
                            //when there's two comments in a row like: <!--{cke_protected}{C}--><!--{cke_protected}{C}-->
                            startNode.$.nodeValue = decodeURIComponent(startNode.$.nodeValue).
                                replace(/<!--cke_bookmark_[0-9]+[SE]-->/g,'').
                                replace(/<!--[\s]*\{cke_protected}[\s]*\{C}[\s]*-->/g,'');
                        } else {
                            //just remove the comment nodes
                            startNode.remove();
                            endNode.remove();
                        }
                    }
                }, null, null, 10);

                editor.on('beforeGetModeData', function () {
                    if (editor.mode === 'wysiwyg' && editor.getData()) {
                        if (CKEDITOR.env.gecko && !editor.focusManager.hasFocus) {
                            return;
                        }
                        var sel = editor.getSelection(), range;
                        if (sel && (range = sel.getRanges()[0])) {
                            wysiwygBookmark = range.createBookmark(editor);
                        }
                    }
                });
                // Build text range right after wysiwyg has unloaded. 
                editor.on('afterModeUnload', function (evt) {
                    if (editor.mode === 'wysiwyg' && wysiwygBookmark) {
                        textRange = new CKEDITOR.dom.textRange(evt.data);
                        textRange.moveToBookmark(wysiwygBookmark, editor);

                        evt.data = textRange.content;
                    }
                });
                editor.on('mode', function () {
                    if (editor.mode === 'source' && textRange && !editor.plugins.codemirror) {
                        textRange.element = new CKEDITOR.dom.element(editor._.editable.$);
                        textRange.select();
                    }
                });

                editor.on('destroy', function () {
                    textRange = null;
                    sourceBookmark = null;
                });
            }
        }
    });

    /** 
     * Gets the current text selection from the editing area when in Source mode. 
     * @returns {CKEDITOR.dom.textRange} Text range represent the caret positoins. 
     * @example 
     * var textSelection = CKEDITOR.instances.editor1.<b>getTextSelection()</b>; 
     * alert( textSelection.startOffset ); 
     * alert( textSelection.endOffset ); 
     */
    CKEDITOR.editor.prototype.getTextSelection = function () {
        return this._.editable && getTextSelection(this._.editable.$) || null;
    };

    /** 
     * Returns the caret position of the specified textfield/textarea. 
     * @param {HTMLTextArea|HTMLTextInput} element 
     */
    function getTextSelection(element) {
        var startOffset, endOffset;

        if (!CKEDITOR.env.ie) {
            startOffset = element.selectionStart;
            endOffset = element.selectionEnd;
        } else {
            element.focus();
            
            // The current selection 
            if (window.getSelection) {
                startOffset = element.selectionStart;
                endOffset = element.selectionEnd;
            } else {
                var range = document.selection.createRange(),
                    textLength = range.text.length;
                
                // Create a 'measuring' range to help calculate the start offset by 
                // stretching it from start to current position. 
                var measureRange = range.duplicate();
                measureRange.moveToElementText(element);
                measureRange.setEndPoint('EndToEnd', range);

                endOffset = measureRange.text.length;
                startOffset = endOffset - textLength;
            }
        }
        return new CKEDITOR.dom.textRange(
            new CKEDITOR.dom.element(element), startOffset, endOffset);
    }

    /** 
     * Represent the selection range within a html textfield/textarea element, 
     * or even a flyweight string content represent the text content. 
     * @constructor 
     * @param {CKEDITOR.dom.element|String} element 
     * @param {Number} start 
     * @param {Number} end 
     */
    CKEDITOR.dom.textRange = function (element, start, end) {
        if (element instanceof CKEDITOR.dom.element
            && (element.is('textarea')
                || element.is('input') && element.getAttribute('type') == 'text')) {
            this.element = element;
            this.content = element.$.value;
        } else if (typeof element == 'string')
            this.content = element;
        else
            throw 'Unkown "element" type.';
        this.startOffset = start || 0;
        this.endOffset = end || 0;
    };
    
     /**
     * Changes the editing mode of this editor instance. (Override of the original function)
     *
     * **Note:** The mode switch could be asynchronous depending on the mode provider.
     * Use the `callback` to hook subsequent code.
     *
     *                // Switch to "source" view.
     *                CKEDITOR.instances.editor1.setMode( 'source' );
     *                // Switch to "wysiwyg" view and be notified on completion.
     *                CKEDITOR.instances.editor1.setMode( 'wysiwyg', function() { alert( 'wysiwyg mode loaded!' ); } );
     *
     * @param {String} [newMode] If not specified, the {@link CKEDITOR.config#startupMode} will be used.
     * @param {Function} [callback] Optional callback function which is invoked once the mode switch has succeeded.
     */
    CKEDITOR.editor.prototype.setMode = function (newMode, callback) {
        var editor = this;

        var modes = this._.modes;

        // Mode loading quickly fails.
        if (newMode == editor.mode || !modes || !modes[newMode])
            return;

        editor.fire('beforeSetMode', newMode);

        if (editor.mode) {
            var isDirty = editor.checkDirty();

            editor._.previousMode = editor.mode;

            editor.fire('beforeModeUnload');

            editor.fire('beforeGetModeData');
            var data = editor.getData();

            data = editor.fire('beforeModeUnload', data);
            data = editor.fire('afterModeUnload', data);

            // Detach the current editable.
            editor.editable(0);

            editor._.data = data;

            // Clear up the mode space.
            editor.ui.space('contents').setHtml('');

            editor.mode = '';
        }

        // Fire the mode handler.
        this._.modes[newMode](function () {
            // Set the current mode.
            editor.mode = newMode;

            if (isDirty !== undefined) {
                !isDirty && editor.resetDirty();
            }

            // Delay to avoid race conditions (setMode inside setMode).
            setTimeout(function () {
                editor.fire('mode');
                callback && callback.call(editor);
            }, 0);
        });
    };

    CKEDITOR.dom.textRange.prototype =
    {
        /** 
         * Sets the text selection of the specified textfield/textarea. 
         * @param {HTMLTextArea|HTMLTextInput} element 
         * @param {CKEDITOR.dom.textRange} range 
         */
        select: function() {

            var startOffset = this.startOffset,
                endOffset = this.endOffset,
                element = this.element.$;
            if (endOffset == undefined) {
                endOffset = startOffset;
            }

            if (CKEDITOR.env.ie && CKEDITOR.env.version == 9) {
                element.focus();
                element.selectionStart = startOffset;
                element.selectionEnd = startOffset;
                setTimeout(function() {
                    element.selectionStart = startOffset;
                    element.selectionEnd = endOffset;
                }, 20);

            } else {
                if (element.setSelectionRange) {
                    if (CKEDITOR.env.ie) {
                        element.focus();
                    }
                    element.setSelectionRange(startOffset, endOffset);
                    if (!CKEDITOR.env.ie) {
                        element.focus();
                    }
                } else if (element.createTextRange) {
                    element.focus();
                    var range = element.createTextRange();
                    range.collapse(true);
                    range.moveStart('character', startOffset);
                    range.moveEnd('character', endOffset - startOffset);
                    range.select();
                }
            }
        },

        /** 
         * Select the range included within the bookmark text with the bookmark 
         * text removed. 
         * @param {Object} bookmark Exactly the one created by CKEDITOR.dom.range.createBookmark( true ). 
         */
        moveToBookmark: function(bookmark, editor) {
            var content = this.content;
            function removeBookmarkText(bookmarkId) {

                var bookmarkRegex = new RegExp('<span[^<]*?' + bookmarkId + '.*?/span>'),
                    offset;
                content = content.replace(bookmarkRegex, function(str, index) {
                    offset = index;
                    return '';
                });
                return offset;
            }

            this.startOffset = removeBookmarkText(bookmark.startNode);
            this.endOffset = removeBookmarkText(bookmark.endNode);
            this.content = content;
            this.updateElement();

            if (editor.undoManager) {
                editor.undoManager.unlock();
            }
        },

        /** 
         * If startOffset/endOffset anchor inside element tag, start the range before/after the element 
         */
        enlarge: function() {
            var htmlTagRegexp = /<[^>]+>/g;
            var content = this.content,
                start = this.startOffset,
                end = this.endOffset,
                match,
                tagStartIndex,
                tagEndIndex;

            // Adjust offset position on parsing result. 
            while (match = htmlTagRegexp.exec(content)) {
                tagStartIndex = match.index;
                tagEndIndex = tagStartIndex + match[0].length;
                if (start > tagStartIndex && start < tagEndIndex)
                    start = tagStartIndex;
                if (end > tagStartIndex && end < tagEndIndex) {
                    end = tagEndIndex;
                    break;
                }
            }

            this.startOffset = start;
            this.endOffset = end;
        },

        createBookmark: function(editor) {
            // Enlarge the range to avoid tag partial selection. 
            this.enlarge();
            var content = this.content,
                start = this.startOffset,
                end = this.endOffset,
                id = CKEDITOR.tools.getNextNumber(),
                bookmarkTemplate = '<!--cke_bookmark_%1-->';

            content = content.substring(0, start) + bookmarkTemplate.replace('%1', id + 'S')
                + content.substring(start, end) + bookmarkTemplate.replace('%1', id + 'E')
                + content.substring(end);

            if (editor.undoManager) {
                editor.undoManager.lock();
            }

            this.content = content;
            this.updateElement();
        },

        updateElement: function() {
            if (this.element)
                this.element.$.value = this.content;
        }
    };

    var Browser = {
        Version: function() {
            var version = 999;
            if (navigator.appVersion.indexOf("MSIE") != -1)
                version = parseFloat(navigator.appVersion.split("MSIE")[1]);
            return version;
        }
    };

    // Seamless selection range across different modes. 
    CKEDITOR.config.syncSelection = true;

    var textRange,sourceBookmark;
