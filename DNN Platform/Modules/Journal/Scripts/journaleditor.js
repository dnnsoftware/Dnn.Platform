(function ($, window) {
    $.fn.journalEditor = function (options) {
        $.fn.journalEditor.defaultOptions = {
            buttonSelector: 'button',
            closeSelector: '.close',
            triggers: [50, 51],
            minLength: 1,
            sources: [null, null],
            tokens: [null, null]
        };
        var opts = $.extend({}, $.fn.journalEditor.defaultOptions, options),
            $wrap = this,
            $triggers = opts.triggers,
            $minLength = opts.minLength,
            $sources = opts.sources,
            $triggered = false,
            $search = '',
            $triggerIndex = -1,
            $menu = $("<ul class='jacmenu'></ul>"),
            $tokens = opts.tokens,
            $id = $(this).attr('id');

        var savedRange = null;
        var triggerPos = 0;
        var isInFocus = false;
        var searchIndex = -1;
        var KEY = {
            BACKSPACE: 8,
            TAB: 9,
            RETURN: 13,
            ESC: 27,
            LEFT: 37,
            UP: 38,
            RIGHT: 39,
            DOWN: 40,
            COMMA: 188
        };
        $wrap.on("mouseup", function () {
            saveSelection();
        })
        .on("keypress", function (event) {
            switch (event.keyCode) {
                case KEY.TAB:
                    event.preventDefault();
                    if ($triggered && searchIndex > -1) {
                        return selectItem();
                    };
                    break;
                case KEY.DOWN:
                    if ($triggered) {
                        event.preventDefault();
                        move(1);
                    }
                    break;
                case KEY.UP:

                    if ($triggered && searchIndex > 0) {
                        event.preventDefault();
                        move(-1);
                    }
                    break;
                case KEY.ESC:
                    event.preventDefault();
                    dispose();
                    break;
            }

        })
        .on("mousedown", function (event) {
            return cancelEvent(event);
        })
        .on("click", function (event) {
            return cancelEvent(event);
        })
        .on("blur", function () {
            isInFocus = false;
            console.log(savedRange);
        })
        .on("focus", function () {
            restoreSelection();
        })
        .on("keyup", function (event) {
            switch (event.keyCode) {
                case KEY.DOWN:
                    event.preventDefault();
                    break;
                case KEY.TAB:
                    event.preventDefault();
                    break;
            }

            if (event.keyCode == 13 && $triggered && searchIndex > -1) {
                return;
            };

            saveSelection();

            if (event.keyCode == $.ui.keyCode.UP || event.keyCode == $.ui.keyCode.DOWN) {
                return;
            }
            if (event.keyCode == $.ui.keyCode.ESCAPE) {
                $triggered = false;
                return;
            }

            if (event.keyCode == 8) {
                var node = savedRange.startContainer.parentElement;
                if (node.className == 'juser' || node.className == 'jtag') {
                    var p = node.parentNode;
                    p.removeChild(node);
                    $triggered = false;
                    triggerPos = 0;
                    return;
                }
            };
            if (event.keyCode == 32) {
                $triggered = false;
                $search = '';
            }
            if (event.shiftKey && $.inArray(event.keyCode, $triggers) > -1) {
                triggerPos = savedRange.endOffset;
                $triggerIndex = $.inArray(event.keyCode, $triggers);
                $triggered = true;

            }
            if ($triggered) {
                $search = getText(triggerPos, savedRange.endOffset);
                if ($search.length >= $minLength) {
                    var callback = $sources[$triggerIndex];
                    callback($search, render);
                }
            };
            return;
        });
        $wrap.parent().after($menu);
        function dispose() {
            $menu.empty();
            $menu.hide();
            searchIndex = -1;
            $triggerIndex = -1;
            triggerPos = 0;
            $search = '';
            $triggered = false;
            savedRange = null;
            $wrap.find('br').remove();
        };
        function render(search, data) {
            data = filter(data, search);
            $menu.empty();
            $menu.show();
            searchIndex = -1;
            $.each(data, function (index, item) {
                if (typeof (item) == 'string') {
                    var value = item;
                    item = {};
                    item.value = value;
                    item.label = value;
                }
                var li = $('<li></li>').attr('id', item.value).append(item.label).click(function (event) {
                    $wrap.focus();
                    event.preventDefault();
                    var label = $(this).text();
                    var value = $(this).attr('id');
                    var token = $tokens[$triggerIndex].replace('value', value).replace('label', label);
                    var end = savedRange.endOffset;
                    createSelection(triggerPos - 1, end);
                    insertHtmlAtCursor(token);
                    dispose();
                    moveCursorToEnd();
                });
                $menu.append(li);
            });

        }
        function selectItem() {
            $wrap.focus();
            var item = {};
            $menu.children('li').each(function (index) {
                if (index == searchIndex) {
                    item.value = $(this).attr('id');
                    item.label = $(this).text();
                    return false;
                }
            });
            $wrap.data($triggerIndex, item);
            var token = $tokens[$triggerIndex].replace('value', item.value).replace('label', item.label);
            var end = savedRange.endOffset;
            createSelection(triggerPos - 1, end);
            insertHtmlAtCursor(token);
            dispose();
            moveCursorToEnd();
            console.log($wrap.data());
        }

        function escapeRegex(value) {
            return value.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
        }
        function filter(array, term) {
            var matcher = new RegExp(escapeRegex(term), "i");
            return $.grep(array, function (value) {
                return matcher.test(value.label || value.value || value);
            });
        }
        function insertHtmlAtCursor(html) {
            var range, node;
            if (window.getSelection && window.getSelection().getRangeAt) {
                range = window.getSelection().getRangeAt(0);
                range.deleteContents();
                if (range.createContextualFragment) {
                    node = range.createContextualFragment(html);
                    range.insertNode(node);
                } else {
                    var range = document.selection.createRange();
                    range.pasteHTML(html);
                }
            } else if (document.selection && document.selection.createRange) {
                document.selection.createRange().pasteHTML(html);
            }
        }
        function getCursorPosition() {
            var cursorPos;
            function findNode(list, node) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i] == node) {
                        return i;
                    };
                };
                return -1;
            }
            if (window.getSelection) {
                var selObj = window.getSelection();
                var selRange = selObj.getRangeAt(0);
                cursorPos = findNode(selObj.anchorNode.parentNode.childNodes, selObj.anchorNode) + selObj.anchorOffset;
                /* FIXME the following works wrong in Opera when the document is longer than 32767 chars */
                return cursorPos;
            } else if (document.selection) {
                var range = document.selection.createRange();
                var bookmark = range.getBookmark();
                /* FIXME the following works wrong when the document is longer than 65535 chars */
                cursorPos = bookmark.charCodeAt(2) - 11; /* Undocumented function [3] */
                return cursorPos;
            }
        }
        function saveSelection() {
            if (window.getSelection) {
                savedRange = window.getSelection().getRangeAt(0);
            } else if (document.selection) {
                savedRange = document.selection.createRange();
            }
        }
        function restoreSelection() {

            isInFocus = true;
            document.getElementById($id).focus();
            if (savedRange != null) {
                if (window.getSelection) {
                    var s = window.getSelection();
                    if (s.rangeCount > 0)
                        s.removeAllRanges();
                    s.addRange(savedRange);
                } else if (document.createRange) {
                    window.getSelection().addRange(savedRange);
                } else if (document.selection) {
                    savedRange.select();
                }
            }
        }
        function cancelEvent(e) {
            if (isInFocus == false && savedRange != null) {
                if (e && e.preventDefault) {
                    //alert("FF");
                    e.stopPropagation(); // DOM style (return false doesn't always work in FF)
                    e.preventDefault();
                } else {
                    window.event.cancelBubble = true; //IE stopPropagation
                }
                restoreSelection();
                return false; // false = IE style
            }
        }
        function createSelection(start, end) {
            var field = document.getElementById($id);
            var range = document.createRange();
            range.setStart(savedRange.startContainer, start);
            range.setEnd(savedRange.endContainer, end);
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        }

        function getText(start, end) {

            var range = document.createRange();
            range.setStart(savedRange.startContainer, start);
            range.setEnd(savedRange.endContainer, end);
            return range.toString();

        }
        function moveCursorToEnd() {
            var range, selection;
            var div = document.getElementById($id);
            if (document.createRange) {
                range = document.createRange();
                range.selectNodeContents(div);
                range.collapse(false);
                selection = window.getSelection();
                selection.removeAllRanges();
                selection.addRange(range);
            } else if (document.selection) {
                range = document.body.createTextRange();
                range.moveToElementText(div);
                range.collapse(false);
                range.select();
            }
            div.focus();
            saveSelection();
        }
        function move(index) {
            $menu.children('li').removeClass('liselected');
            if (searchIndex == -1) {
                searchIndex = 0;
                $menu.children('li').first().addClass('liselected');
            } else {
                searchIndex += index;
                if (searchIndex < 0) {
                    searchIndex = -1;
                }
                $menu.children('li').each(function (index) {
                    if (index == searchIndex) {
                        $(this).addClass('liselected');
                        return false;
                    }
                });
            }
        }

    }
} (jQuery, window));