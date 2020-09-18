'use strict';
define(['jquery',
        'codemirror',
        'codemirror/addon/scroll/simplescrollbars',
        'codemirror/mode/sql/sql',
		'codemirror/mode/css/css',
		'codemirror/mode/xml/xml',
        'css!../../../../Resources/Shared/components/CodeEditor/lib/codemirror.css',
        'css!../../../../Resources/Shared/components/CodeEditor/addon/scroll/simplescrollbars.css'],
        function ($, codemirror) {
            var defaultOptions = {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                scrollbarStyle: "simple"
            };

            var modeMapping = {
                'mssql': 'text/x-mssql',
                'css': 'text/css',
                'xml': 'application/xml'
            }

            function init(element, options) {
                if (element.length) {
                    element = element[0];
                }

                var editorOptions = $.extend({}, defaultOptions, options);
                editorOptions.mode = modeMapping[options.mode] || options.mode;
                var editor = codemirror.fromTextArea(element, editorOptions);
                $(editor.display.input.textarea).attr('aria-label', 'Editor');
                return editor;
            }
            return {
                init: init
            };
        });