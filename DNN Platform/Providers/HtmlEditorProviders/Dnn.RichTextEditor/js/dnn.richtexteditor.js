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

    var pageLinkButton = 'dnnPageLink';
    var pageLinkIcon = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M6 2h8l5 5v6h-2V8h-4V4H6v16h6v2H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2z"/>'
        + '<path d="M15.5 14a2.5 2.5 0 0 0 0 5h1v-1.5h-1a1 1 0 0 1 0-2h1V14zm3 0v1.5h1a1 1 0 0 1 0 2h-1V19h1a2.5 2.5 0 0 0 0-5zm-2 1.75v1.5h2v-1.5z"/></svg>';
    var pageLists = {};

    // The page list is the same for every editor on the page, so it is fetched once per service URL.
    function loadPages(url) {
        if (!pageLists[url]) {
            pageLists[url] = window.fetch(url + 'List', { credentials: 'same-origin', headers: getAntiForgeryHeaders() })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error(response.statusText);
                    }

                    return response.json();
                })
                .catch(function (error) {
                    delete pageLists[url];
                    throw error;
                });
        }

        return pageLists[url];
    }

    function insertPageLink(editor, range, link, page) {
        var Dom = window.Jodit.modules.Dom;
        if (range) {
            editor.s.selectRange(range);
        }

        if (link && Dom.isOrContains(editor.editor, link)) {
            link.setAttribute('href', page.url);
        } else if (range && !editor.s.isCollapsed()) {
            var node = editor.s.current();
            var links = Dom.isTag(node, 'img') ? [Dom.wrap(node, 'a', editor.createInside)] : editor.s.wrapInTag('a');
            links.forEach(function (a) {
                a.setAttribute('href', page.url);
            });
        } else {
            var a = editor.createInside.element('a');
            a.setAttribute('href', page.url);
            a.textContent = page.name;
            editor.s.insertNode(a, false, false);
            editor.s.setCursorAfter(a);
        }

        editor.synchronizeValues();
    }

    function createPageLinkPopup(editor, current, close, pageLinks) {
        var text = pageLinks.text || {};
        var link = current ? window.Jodit.modules.Dom.closest(current, 'a', editor.editor) : null;

        // The search box takes the focus, so remember where the link goes.
        var range = editor.s.isInsideArea ? editor.s.range.cloneRange() : null;

        var container = document.createElement('div');
        container.className = 'dnn-rte-pagelink';
        var search = document.createElement('input');
        search.type = 'search';
        search.className = 'jodit-input dnn-rte-pagelink__search';
        search.placeholder = text.search || '';
        search.setAttribute('aria-label', text.search || '');
        var list = document.createElement('div');
        list.className = 'dnn-rte-pagelink__list';
        var status = document.createElement('div');
        status.className = 'dnn-rte-pagelink__status';
        status.textContent = text.loading || '';
        container.appendChild(search);
        container.appendChild(list);
        container.appendChild(status);

        var items = [];
        var choose = function (page) {
            insertPageLink(editor, range, link, page);
            close();
        };

        var filter = function () {
            var query = search.value.trim().toLowerCase();
            var visible = 0;
            items.forEach(function (item) {
                var match = !query || item.page.name.toLowerCase().indexOf(query) !== -1;
                item.element.style.display = match ? '' : 'none';
                visible += match ? 1 : 0;
            });
            status.textContent = visible ? '' : (text.empty || '');
            status.style.display = visible ? 'none' : '';
        };

        search.addEventListener('input', filter);
        search.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') {
                return;
            }

            e.preventDefault();
            var first = items.filter(function (item) {
                return item.page.selectable && item.element.style.display !== 'none';
            })[0];
            if (first) {
                choose(first.page);
            }
        });

        loadPages(pageLinks.url).then(function (pages) {
            pages.forEach(function (page) {
                var element = document.createElement(page.selectable ? 'button' : 'span');
                element.className = 'dnn-rte-pagelink__item' + (page.selectable ? '' : ' dnn-rte-pagelink__item_disabled');
                element.style.paddingLeft = (8 + (page.level * 14)) + 'px';
                element.textContent = page.name;
                element.title = page.url;
                if (page.selectable) {
                    element.type = 'button';
                    element.addEventListener('click', function (e) {
                        e.preventDefault();
                        choose(page);
                    });
                }

                list.appendChild(element);
                items.push({ page: page, element: element });
            });
            filter();
            search.focus();
        }, function () {
            status.textContent = text.error || '';
        });

        return container;
    }

    // Adds the Insert Page Link button, which lists the site's pages through the DNN Pages service.
    function configurePageLinks(config, pageLinks) {
        if (window.Jodit.modules.Icon && !window.Jodit.modules.Icon.exists(pageLinkButton)) {
            window.Jodit.modules.Icon.set(pageLinkButton, pageLinkIcon);
        }

        config.controls = config.controls || {};
        config.controls[pageLinkButton] = {
            icon: pageLinkButton,
            tooltip: (pageLinks.text && pageLinks.text.button) || 'Insert page link',
            popup: function (editor, current, close) {
                return createPageLinkPopup(editor, current, close, pageLinks);
            },
        };

        if (config.buttons) {
            // Place it right after the regular link button.
            var linkIndex = config.buttons.indexOf('link');
            config.buttons = config.buttons.slice();
            config.buttons.splice(linkIndex < 0 ? config.buttons.length : linkIndex + 1, 0, pageLinkButton);
        } else {
            config.extraButtons = (config.extraButtons || []).concat([pageLinkButton]);
        }
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

        if (options.pageLinks && options.pageLinks.url) {
            configurePageLinks(config, options.pageLinks);
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
