/*
 * DNN Rich Text Editor - editor-neutral wrapper around Jodit.
 *
 * Modules should use this API instead of calling Jodit directly, so they keep working if the underlying editor changes:
 *   dnn.richTextEditor.onReady(function (instance) { ... })   called for every editor, including ones already created
 *   dnn.richTextEditor.get(id)                                 the instance for a text area id, or null
 *   instance.getValue() / instance.setValue(html) / instance.resize(width, height) / instance.focus()
 *   dnn.richTextEditor.configure(function (config, options) { ... })   adjusts the Jodit config before each editor is created
 */
(function (window, document) {
    'use strict';

    var dnn = window.dnn = window.dnn || {};
    if (dnn.richTextEditor) {
        return;
    }

    var toolbars = {
        basic: ['bold', 'italic', '|', 'ul', 'ol', '|', 'link', 'image'],
        standard: [
            'undo', 'redo', '|', 'link', 'image', 'hr', '|', 'fullsize', 'source', '\n',
            'bold', 'italic', 'strikethrough', 'eraser', '|', 'ul', 'ol', 'outdent', 'indent', '|', 'paragraph',
        ],
        // null keeps Jodit's own default (complete) toolbar
        full: null,
    };

    var instances = {};
    var readyCallbacks = [];
    var configureCallbacks = [];

    function ensureStylesheet(href) {
        var links = document.getElementsByTagName('link');
        for (var i = 0; i < links.length; i++) {
            if (links[i].getAttribute('href') === href) {
                return;
            }
        }

        var link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = href;
        document.head.appendChild(link);
    }

    function resolveLanguage(culture) {
        var available = (window.Jodit && window.Jodit.lang) || {};
        var name = (culture || '').toLowerCase().replace('-', '_');
        if (available[name]) {
            return name;
        }

        var neutral = name.split('_')[0];
        return available[neutral] ? neutral : 'en';
    }

    function getAntiForgeryHeaders() {
        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? { RequestVerificationToken: token.value } : {};
    }

    // Points Jodit's file browser and uploader at the DNN FileBrowser service, which enforces folder permissions.
    function configureFileBrowser(config, fileBrowser) {
        var headers = getAntiForgeryHeaders();
        var connectorUrl = fileBrowser.url + 'Connector';

        config.filebrowser = {
            ajax: {
                url: connectorUrl,
                headers: headers,
            },
            // Jodit's own permissions defaults would send this one as a GET, so the request settings are repeated here.
            permissions: {
                url: connectorUrl,
                method: 'POST',
                headers: headers,
            },
            // Not supported by the service; hide them regardless of what it reports.
            permissionsPresets: {
                allowFileMove: false,
                allowFileRename: false,
                allowFileUploadRemote: false,
                allowFolderMove: false,
                allowFolderRemove: false,
                allowFolderRename: false,
                allowGeneratePdf: false,
                allowImageCrop: false,
                allowImageResize: false,
            },
            editImage: false,
            moveFile: false,
            moveFolder: false,
            renameFolder: false,
            deleteFolder: false,
        };

        config.uploader = {
            url: fileBrowser.url + 'Upload',
            headers: headers,
            insertImageAsBase64URI: false,
        };
    }

    function createInstance(id, element, editor) {
        return {
            id: id,
            element: element,
            editor: editor,
            getValue: function () {
                return editor.value;
            },
            setValue: function (html) {
                editor.value = html;
            },
            resize: function (width, height) {
                if (width !== undefined && width !== null) {
                    editor.e.fire('setWidth', width);
                }

                if (height !== undefined && height !== null) {
                    editor.e.fire('setHeight', height);
                }
            },
            focus: function () {
                editor.s.focus();
            },
        };
    }

    function notifyReady(instance) {
        instance.isReady = true;
        readyCallbacks.forEach(function (callback) {
            callback(instance);
        });
    }

    function destroy(id) {
        var instance = instances[id];
        if (!instance) {
            return;
        }

        delete instances[id];
        try {
            instance.editor.destruct();
        } catch (e) {
            // The editor's markup may already have been removed by an UpdatePanel refresh.
        }
    }

    function create(id, options) {
        var element = document.getElementById(id);
        if (!element || !window.Jodit) {
            return null;
        }

        var existing = instances[id];
        if (existing) {
            if (existing.element === element) {
                return existing;
            }

            // An UpdatePanel replaced the markup; the old editor is orphaned.
            destroy(id);
        }

        options = options || {};
        (options.stylesheets || []).forEach(ensureStylesheet);

        var instance;
        var config = {
            language: resolveLanguage(options.language),
            toolbarAdaptive: false,
            hidePoweredByJodit: !!options.hidePoweredBy,
            askBeforePasteHTML: false,
            askBeforePasteFromWord: false,
            defaultActionOnPasteFromWord: 'insert_clear_html',
            // Keep existing content intact: only strip what the CKEditor provider also stripped.
            cleanHTML: {
                denyTags: 'script,object,embed',
                removeEmptyElements: false,
                // Icon markup such as <i class="fa ..."> must not become <em>.
                replaceOldTags: false,
                sandboxIframesInContent: false,
            },
            events: {
                afterInit: function () {
                    if (instance) {
                        notifyReady(instance);
                    }
                },
            },
        };

        var buttons = toolbars[(options.toolbar || '').toLowerCase()];
        if (buttons) {
            config.buttons = buttons;
        }

        if (options.width) {
            config.width = options.width;
        }

        if (options.height) {
            config.height = options.height;
        }

        if (options.fileBrowser && options.fileBrowser.url) {
            configureFileBrowser(config, options.fileBrowser);
        }

        configureCallbacks.forEach(function (callback) {
            callback(config, options);
        });

        var editor = window.Jodit.make(element, config);
        instance = createInstance(id, element, editor);
        instances[id] = instance;

        // afterInit may already have fired synchronously inside Jodit.make
        if (editor.isReady && !instance.isReady) {
            notifyReady(instance);
        }

        return instance;
    }

    function sync(id) {
        var instance = instances[id];
        if (!instance) {
            return;
        }

        var editor = instance.editor;

        // Source view applies edits after a short delay; switching back to WYSIWYG applies any pending changes.
        if (editor.getRealMode() !== window.Jodit.constants.MODE_WYSIWYG) {
            editor.setMode(window.Jodit.constants.MODE_WYSIWYG);
        }

        instance.element.value = editor.value;
    }

    dnn.richTextEditor = {
        create: create,
        destroy: destroy,
        sync: sync,
        syncAll: function () {
            Object.keys(instances).forEach(sync);
        },
        get: function (id) {
            return instances[id] || null;
        },
        getAll: function () {
            return Object.keys(instances).map(function (id) {
                return instances[id];
            });
        },
        onReady: function (callback) {
            readyCallbacks.push(callback);
            Object.keys(instances).forEach(function (id) {
                if (instances[id].isReady) {
                    callback(instances[id]);
                }
            });
        },
        configure: function (callback) {
            configureCallbacks.push(callback);
        },
    };
}(window, document));
