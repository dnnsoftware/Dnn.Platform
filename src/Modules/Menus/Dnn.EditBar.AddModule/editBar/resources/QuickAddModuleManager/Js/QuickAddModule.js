// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

﻿dnn = dnn || {};

dnn.QuickAddModuleManager = {
    init: function (options) {
        if (options.type == "QuickAddModuleManager" && options.panes) {
            var panesClientIds = options.panesClientIds.split(';');
            var panes = options.panes.split(',');
            for (var i = 0; i < panes.length; i++) {
                (function (paneName, paneClientIds) {
                    var parentPaneClientIds = paneClientIds.split(',');
                    for (var j = 0; j < parentPaneClientIds.length; j++) {
                        (function(paneClientId) {
                            var pane = paneClientId.length > 0 ? $('[id=' + paneClientId + '][class~="EvoqEmptyPane"]') : null;                            
                            if (pane && pane.length > 0) {
                                pane.dnnQuickAddModuleManager({
                                    pane: paneName,
                                    page: options.page,
                                    textModule: options.textModule,
                                    imgModule: options.imgModule,
                                    addModuleTitle: options.addModuleTitle,
                                    addTextModuleTitle: options.addTextModuleTitle,
                                    addImageModuleTitle: options.addImageModuleTitle
                                });
                            }
                        })(parentPaneClientIds[j]);
                    }
                })(panes[i], panesClientIds[i]);
            }
        }
    }
};

(function ($) {
    $.fn.dnnQuickAddModuleManager = function (options) {

        if (!this.data("dnnQuickAddModuleManager")) {
            this.data("dnnQuickAddModuleManager", new dnnQuickAddModuleManager(this, options));
        }
        return this;
    };

    var dnnQuickAddModuleManager = function (container, options) {
        this.options = options;
        this.container = container;
        this.init();
    };

    dnnQuickAddModuleManager.prototype = {
        constructor: dnnQuickAddModuleManager,
        init: function () {
            this.options = $.extend({}, $.fn.dnnQuickAddModuleManager.defaultOptions, this.options);
            var $this = this.$this = this.container;
            $this.addClass('dnnQuickAddModuleManager');
            var handlerContainer = $('<div class="handlerContainer" />');
            $this.append(handlerContainer);

            handlerContainer.append(this._generateLayoutText());
            handlerContainer.append(this._generateLayoutImage());
            handlerContainer.append(this._generateLayoutModule());
        },

        _generateLayoutModule: function () {
            var handler = $('<div class="QuickAddHandler QuickAddModuleHandler"></div>');
            handler.attr('title', this.options.addModuleTitle);
            handler.click($.proxy(this._addQuickAddModuleHandlerClick, this));
            return handler;
        },
        _generateLayoutImage: function () {
            var handler = $('<div class="QuickAddHandler QuickAddImageHandler"></div>');
            handler.attr('title', this.options.addImageModuleTitle);
            handler.click($.proxy(this._addQuickAddImageHandlerClick, this));
            return handler;
        },
        _generateLayoutText: function () {
            var handler = $('<div class="QuickAddHandler QuickAddTextHandler"></div>');
            handler.attr('title', this.options.addTextModuleTitle);
            handler.click($.proxy(this._addQuickAddTextHandlerClick, this));
            return handler;
        },

        _getModuleManager: function() {
            return this.container.data('dnnModuleManager');
        },

        _getModuleDialog: function() {
            var moduleManager = this._getModuleManager();
            var dialog = dnn.ContentEditorManager.getModuleDialog();
            dialog.apply(moduleManager);

            return dialog;
        },

        _addQuickAddModuleHandlerClick: function () {
            this._getModuleManager().getHandler().click();
            var moduleDialog = this._getModuleDialog();
            moduleDialog.noFloat();
        },
        _addQuickAddImageHandlerClick: function () {
            var moduleDialog = this._getModuleDialog();
            moduleDialog.noFloat();
            moduleDialog.addModule(this.options.imgModule, function (reload) {
                if (reload) {
                    moduleDialog._setCookie('ImageModulePane', this.attr('id'));
                } else {
                    var handler = this;
                    setTimeout(function () {
                        if (typeof window.dnnLoadScriptsInAjaxMode === "undefined" || window.dnnLoadScriptsInAjaxMode.length == 0) {
                            handler.find('div.DnnModule').find('.redactor-box .re-dnnImageUpload').click();
                        } else {
                            $(window).one('dnnScriptLoadComplete', function() {
                                handler.find('div.DnnModule').find('.redactor-box .re-dnnImageUpload').click();
                            });
                        }
                    }, 1000);

                }
            });
        },
        _addQuickAddTextHandlerClick: function () {
            var moduleDialog = this._getModuleDialog();
            moduleDialog.noFloat();
            moduleDialog.addModule(this.options.textModule);
        },

        _removeWatermark: function (element) {
            $(element).find(".QuickAddModuleHandler").remove();
            $(element).find(".QuickAddImageHandler").remove();
            $(element).find(".QuickAddTextHandler").remove();
            $(element).removeClass("dnnQuickAddModuleManager");
        }
    };

    $(window).load(function handleAddNewImageInHtmlModuleFromCookie() {
        //handle the new image module from cookie
        var handleImageModule = function () {
            var moduleDialog = dnn.ContentEditorManager.getModuleDialog();

            var imageModulePaneId = moduleDialog._getCookie('ImageModulePane');
            if (imageModulePaneId) {
                var $pane = $('#' + imageModulePaneId);
                $pane.find('.redactor-box .re-dnnImageUpload').click();
                moduleDialog._removeCookie('ImageModulePane');
            }
        };

        setTimeout(handleImageModule, 600);
    });
}(jQuery));
