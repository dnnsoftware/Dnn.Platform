// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    $.fn.dnnModuleManager = function (options) {
        if (!this.data("dnnModuleManager")) {
            this.data("dnnModuleManager", new dnnModuleManager(this, options));
        }

        return this;
    };

    var dnnModuleDialogInstance, dnnExsitingModuleDialogInstance;
    var getModuleDialog = dnn.ContentEditorManager.getModuleDialog = function () {
        if (!dnnModuleDialogInstance) {
            dnnModuleDialogInstance = new dnn.dnnModuleDialog();
        }

        return dnnModuleDialogInstance;
    }

    var getExsitingModuleDialog = dnn.ContentEditorManager.getExsitingModuleDialog = function () {
        if (!dnnExsitingModuleDialogInstance) {
            dnnExsitingModuleDialogInstance = new dnn.dnnExistingModuleDialog();
        }

        return dnnExsitingModuleDialogInstance;
    }

    ///dnnModuleManager Plugin
    var dnnModuleManager = function (container, options) {
        this.options = options;
        this.container = container;
        this.init();
    };

    dnnModuleManager.prototype = {
        constructor: dnnModuleManager,

        init: function () {
            this.options = $.extend({}, $.fn.dnnModuleManager.defaultOptions, this.options);
            var $this = this.$pane = this.container;

            $this.addClass('dnnModuleManager');
            $this.append(this._generateLayout());

            this._initPaneName();
            this._injectVisualEffects();

            this._handleEvents();
        },

        getHandler: function () {
            this._handler = this.getPane().find('> .addModuleHandler');

            return this._handler;
        },

        getExistingModuleHandler: function () {
            this._existingModuleHandler = this.getPane().find('> .addExistingModuleHandler');

            return this._existingModuleHandler;
        },

        getPane: function () {
            return $('#' + this.$pane.attr('id'));
        },

        _generateLayout: function () {
            return $('<a href="#" class="addModuleHandler" aria-label="Add Module"><span></span></a><a href="#" class="addExistingModuleHandler" aria-label="Add Existing Module"><span></span></a>');
        },

        _initPaneName: function() {
            if (!this.$pane.attr('data-name')) {
                var paneName = this.$pane.attr('id').replace('dnn_', '');
                this.$pane.attr('data-name', paneName);
            }
        },
        
        _injectVisualEffects: function () {
            var actionMenus = [];
            var handler = this;

            this.container.find('div.DnnModule').each(function() {
                var module = $(this);
                var moduleId = handler._findModuleId(module);
                actionMenus.push('#moduleActions-' + moduleId);

                if (module.data('effectsInjected')) {
                    return;
                }

                module.data('effectsInjected', true);
                module.mouseover(function () {
                    if (window['cem_dragging']) {
                        return; //do nothing when dragging module.
                    }

                    if (!module.hasClass('active-module') && !module.hasClass('floating')) {

                        module.parent().find('> div.DnnModule').removeClass('active-module');
                        module.addClass('active-module');
                        $(menusSelector).not('[class~="floating"]').stop(true, true).fadeTo('fast', 0.5, function () {
                            $('#moduleActions-' + moduleId).not('[class~="floating"]').stop(true, true).fadeTo(0).show().fadeTo('fast', 1);
                        });
                    }
                });
            });

            if (this.container.data('effectsInjected')) {
                return;
            }

            this.container.data('effectsInjected', true);
            var menusSelector = actionMenus.join(',');
            this.container.mouseover(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                if (!$(this).hasClass('active-pane')) {
                    $('.actionMenu').stop(true, true).hide();
                    $(menusSelector).not('[class~="floating"]').show();
                    $('.dnnSortable[id]').removeClass('active-pane');
                    $(this).addClass('active-pane');
                }
                return false;
            }).mouseout(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                var target = $(e.relatedTarget);
                if (target.parents('.active-pane').length > 0 || target.hasClass('actionMenu') || target.parents('.actionMenu').length > 0) {
                    if (target.hasClass('actionMenu') && !target.hasClass('floating')) {
                        $('.actionMenu:visible').stop(true, true).css({ opacity: 0.5 });
                        target.stop(true, true).css({ opacity: 1 });
                    } else if (target.parents('.actionMenu').length > 0 && !target.parents('.actionMenu').hasClass('floating')) {
                        $('.actionMenu:visible').stop(true, true).css({ opacity: 0.5 });
                        target.parents('.actionMenu').stop(true, true).css({ opacity: 1 });
                    }
                    return false;
                }

                $(menusSelector).stop(true, true).hide();
                $(this).removeClass('active-pane');
                return false;
            });

            $(document).mouseover(function (e) {
                if (window['cem_dragging']) {
                    return false; //do nothing when dragging module.
                }

                if ($(e.target).parents('.actionMenu').length > 0 || $('.actionMenu:visible').length == 0) {
                    return;
                }

                $('.actionMenu').stop(true, true).hide();
                $('.dnnSortable[id]').removeClass('active-pane');
                $('.dnnSortable[id] > div.DnnModule').removeClass('active-module');
            });

            $('.actionMenu').mouseover(function () {
                return false;
            });

            setTimeout(function () {
                $('.dnnSortable[id]').trigger('mouseout');

                $(menusSelector).find('li[id$="-Delete"] a').each(function () {
                    var $deleteButton = $(this);
                    $deleteButton.off('click').dnnConfirm({
                        text: dnn.ContentEditorManagerResources.deleteModuleConfirm,
                        yesText: dnn.ContentEditorManagerResources.confirmYes,
                        noText: dnn.ContentEditorManagerResources.confirmNo,
                        title: dnn.ContentEditorManagerResources.confirmTitle,
                        isButton: true,
                        callbackTrue: function () {
                            dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
                            location.href = $deleteButton.attr('href');
                        }
                    });
                });
            }, 1000);
        },

        _findModuleId: function (module) {
            return parseInt(module.find("a").first().attr("name"));
        },

        _handleEvents: function () {
            this.getHandler().click($.proxy(this._addModuleHandlerClick, this));
            this.getExistingModuleHandler().click($.proxy(this._addExisingModuleHandlerClick, this));
        },

        _addModuleHandlerClick: function (e) {
            var dialog = getModuleDialog();
            if (!this._handler.hasClass('active')) {
                dialog.apply(this).open();
                this._handler.addClass('active');
            } else {
                dialog.close();
                this._handler.removeClass('active');
            }
            return false;
        },
        _addExisingModuleHandlerClick: function (e) {
            var dialog = getExsitingModuleDialog();
            if (!this._existingModuleHandler.hasClass('active')) {
                dialog.apply(this).open();
                this._existingModuleHandler.addClass('active');
            } else {
                dialog.close();
                this._existingModuleHandler.removeClass('active');
            }
            return false;
        }
    };

    $.fn.dnnModuleManager.defaultOptions = {};
    ///dnnModuleManager Plugin END
}(jQuery));
