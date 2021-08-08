// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

/*
* Module responsible to Css Editor
*/
'use strict';

define([
        'jquery',
        'knockout',
        'knockout.mapping',
        'main/codeEditor',
        'main/config',
        'dnn.jquery',
        'main/koBindingHandlers/jScrollPane'
    ],
    function($, ko, koMapping, codeEditor, cf, jScrollPane) {
        var config = cf.init();

        var identifier, utility, $panel, viewModel, cssEditor, cssContent, curPortalId;

        var requestService = function(type, method, controller, params, callback, failure) {
            utility.sf.moduleRoot = "personaBar";
            utility.sf.controller = controller !== "" ? controller : "CssEditor";

            utility.sf[type].call(utility.sf, method, params, callback, failure);
        }

        var getPortals = function() {
            requestService('get', 'GetPortals', "Portals", {}, function(data) {
                viewModel.portals(data.Results);
                viewModel.portal(data.Results[0]);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
        }

        var getStyleSheet = function() {
            requestService('get', 'GetStyleSheet', "", { 'portalId': curPortalId }, function(data) {
                cssContent.setValue(data.Content);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
        }

        var saveStyleSheet = function() {
            requestService('post', 'UpdateStyleSheet', "", { 'portalId': curPortalId, 'styleSheetContent': cssContent.getValue() }, function(data) {
                utility.notify(utility.resx.CssEditor.StyleSheetSaved);
            }, function() {
                // failed
                utility.notifyError('Failed...');
            });
        }

        var restoreStyleSheet = function() {
            utility.confirm(utility.resx.CssEditor.ConfirmRestore, utility.resx.CssEditor.RestoreButton, utility.resx.CssEditor.CancelButton, function() {
                requestService('post', 'RestoreStyleSheet', "", { 'portalId': curPortalId }, function(data) {
                    cssContent.setValue(data.StyleSheetContent);
                    utility.notify(utility.resx.CssEditor.StyleSheetRestored);
                }, function() {
                    // failed
                    utility.notifyError('Failed...');
                });
            });
        }

        var initViewModel = function() {
            viewModel = {
                resx: utility.resx.CssEditor,
                stylesheet: ko.observable(''),
                saveStyleSheet: saveStyleSheet,
                restoreStyleSheet: restoreStyleSheet,
                portals: ko.observableArray([]),
                portal: ko.observable('')
            };
        }

        var portalChanged = function(data) {
            if (data != null && data.PortalID != curPortalId) {
                curPortalId = data.PortalID;
                getStyleSheet();
            }
        }

        var init = function(wrapper, util, params, callback) {
            identifier = params.identifier;
            utility = util;
            $panel = wrapper;

            initViewModel();

            ko.applyBindings(viewModel, $panel[0]);

            curPortalId = config.portalId;
            if (params.settings.isHost) {
                getPortals();
            }
            
            initCssEditor();

            var panelHeight = $('#CssEditor-panel').height();
            if (panelHeight > 400) {
                $('#monaco-editor').height(panelHeight - 400);
            }

            if (typeof callback === 'function') {
                callback();
            }

            $('#monaco-editor').on("blur", function(cm) {
                cm.save();
                return true;
            });

            viewModel.portal.subscribe(portalChanged);
        };

        var initCssEditor = function() {
            var monacoEditorLoaderScript = document.createElement('script');
            monacoEditorLoaderScript.type = 'text/javascript';
            monacoEditorLoaderScript.src = '/Resources/Shared/components/MonacoEditor/loader.js';
            document.body.appendChild(monacoEditorLoaderScript);

            require.config({ paths: { 'vs': '/Resources/Shared/components/MonacoEditor' }});
            require(['vs/editor/editor.main'], function(monaco) {
        
                self.MonacoEnvironment = {
                    getWorkerUrl: function (moduleId, label) {
                        if (label === 'typescript' || label === 'javascript') {
                        return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                            importScripts('${process.env.ASSET_PATH}/typescript.worker.js');`
                        )}`;
                        }
        
                    return `data:text/javascript;charset=utf-8,${encodeURIComponent(`
                        importScripts('${process.env.ASSET_PATH}/editor.worker.js');`
                    )}`;
                    }
                }

                cssEditor = monaco.editor;
                cssContent = cssEditor.createModel("", "css");
                initMonacoEditor();
                getStyleSheet();
            });

        }

        var initMonacoEditor = function() {
            var theme = "vs-light";
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                theme = "vs-dark";
            }
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                theme = e.matches ? "vs-dark" : "vs-light";
            });

            var monacoEditor = cssEditor.create(document.getElementById("monaco-editor"), {
                model: cssContent,
                language: "css",
                wordWrap: 'wordWrapColumn',
                wordWrapColumn: 80,
                wordWrapMinified: true,
                wrappingIndent: "indent",
                lineNumbers: "on",
                roundedSelection: false,
                scrollBeyondLastLine: false,
                readOnly: false,
                theme: theme,
                automaticLayout: true
            });
        }

        var load = function(params, callback) {
            if (typeof callback === 'function') {
                callback();
            }
        };

        return {
            init: init,
            load: load
        };
    });