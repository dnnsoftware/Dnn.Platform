// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
if (typeof dnn.ContentEditorManager === "undefined" || dnn.ContentEditorManager === null) { dnn.ContentEditorManager = {}; };

(function ($) {
    var moduleAlias = { 'Content Layout': 'Grids' };

    ///dnnExistingModuleDialog Plugin

    var dnnExistingModuleDialog = dnn.dnnExistingModuleDialog = function (options) {
        this.options = options;
        this.init();
    };

    dnnExistingModuleDialog.prototype = {
        constructor: dnnExistingModuleDialog,
        init: function () {
            this.options = $.extend({}, dnnExistingModuleDialog.defaultOptions, this.options);

            var layout = this._generateLayout();
            $(document.body).append(layout);
            layout.hide();

            this._moduleDialog = dnn.ContentEditorManager.getModuleDialog();
            this._addCloseButton();

            this._initUI();
            this._attachEvents();
        },

        apply: function (moduleManager) {
            this._moduleDialog.apply(moduleManager, true);
            return this;
        },

        open: function () {
            this._showDialog();
            this._isOpen = true;
            this._noFloat = false;
            return this;
        },

        close: function (callback) {
            this._isOpen = false;
            this._hideDialog(callback);
            return this;
        },

        getModuleDialog: function () {
            if (!this._moduleDialog) {
                this._moduleDialog = dnn.ContentEditorManager.getModuleDialog();
            }

            return this._moduleDialog;
        },

        getModuleManager: function () {
            return this.getModuleDialog().getModuleManager();
        },

        getDefaultPane: function () {
            return this.getModuleDialog().getDefaultPane.apply(this.getModuleDialog(), arguments);
        },

        getPaneById: function () {
            return this.getModuleDialog().getPaneById.apply(this.getModuleDialog(), arguments);
        },

        isOpen: function () {
            return this._isOpen;
        },

        refreshPane: function () {
            return this.getModuleDialog().refreshPane.apply(this.getModuleDialog(), arguments);
        },

        addModule: function (moduleId, callback, callbackBeforeRefresh) {
            if (this.getModuleDialog()._working) {
                return false;
            }

            this.getModuleDialog()._working = true;

            var params = {
                Visibility: 0,
                Position: -1,
                Page: this._selectedPageId,
                Module: moduleId,
                Pane: this.getModuleDialog().options.paneName,
                AddExistingModule: true,
                CopyModule: this._dialogLayout.find('input.copy-module').prop('checked'),
                Sort: -1
            };

            this._moduleDialog._addingDesktopModuleId = moduleId;
            var handler = this;

            this._moduleDialog.showProgressBar();

            this.getModuleDialog()._getService().request('AddModule', 'POST', params, function (data) {
                if (callbackBeforeRefresh && typeof callback === "function") {
                    callback.call(handler.getModuleManager().getPane(), [data.TabModuleID]);
                    callback = null;
                }

                handler.getModuleDialog()._addModuleComplete(data, callback);
            });

            return false;
        },

        _onSelectPageChanged: function (value) {
            this._selectedPageId = parseInt(value.key);
            this._loadModuleList();
        },

        _createDropdown: function (element, options) {
            var selectPageCallback = options.selectPageCallback;
            var id = "#{0}".replace(/\{0\}/g, element.id);

            var selectPageProxyCallback = function () {
                selectPageCallback(this.selectedItem());
            };

            var defaultOptions = {
                disabled: false,
                initialState: {
                    selectedItem: options.selectedFolder
                },
                services: {
                    moduleId: '',
                    serviceRoot: 'InternalServices',
                    rootId: 'Root',
                    parameters: { includeAllTypes: "true" },
                    includeDisabled: true
                },
                onSelectionChangedBackScript: selectPageProxyCallback
            };

            $.extend(true, defaultOptions, options);

            dnn.createDropDownList(id, defaultOptions, {});
        },

        _createPagesDropdown: function () {
            var handler = this;
            var resx = dnn.ContentEditorManagerResources;
            var pagePickerOptions = {
                selectPageCallback: $.proxy(handler._onSelectPageChanged, handler),
                selectedFolder: -1,
                selectedItemCss: 'selected-item',
                internalStateFieldId: 'AddExistingModule_FolderPicker_State',
                selectItemDefaultText: resx.pagePicker_selectItemDefaultText,
                itemList: {
                    sortAscendingButtonTitle: resx.pagePicker_sortAscendingButtonTitle,
                    unsortedOrderButtonTooltip: resx.pagePicker_unsortedOrderButtonTooltip,
                    sortAscendingButtonTooltip: resx.pagePicker_sortAscendingButtonTooltip,
                    sortDescendingButtonTooltip: resx.pagePicker_sortDescendingButtonTooltip,
                    selectedItemExpandTooltip: resx.pagePicker_selectedItemExpandTooltip,
                    selectedItemCollapseTooltip: resx.pagePicker_selectedItemCollapseTooltip,
                    searchInputPlaceHolder: resx.pagePicker_searchInputPlaceHolder,
                    clearButtonTooltip: resx.pagePicker_clearButtonTooltip,
                    searchButtonTooltip: resx.pagePicker_searchButtonTooltip,
                    loadingResultText: resx.pagePicker_loadingResultText,
                    resultsText: resx.pagePicker_resultsText,
                    firstItem: null,
                    disableUnspecifiedOrder: true
                },
                services: {
                    rootNodeName: resx.pagePicker_selectItemDefaultText
                }
            };

            handler._createDropdown($('#AddExistingModule_Pages')[0], pagePickerOptions);
        },

        _calcPosition: function (handler) {
            var left, top;
            var dialogWidth = this._dialogLayout.outerWidth();
            var dialogHeight = this._dialogLayout.outerHeight();
            var personaBarWidth = $('#personaBar-iframe').outerWidth();
            left = $(document).scrollLeft() + personaBarWidth / 2 + ($(window).width() - dialogWidth) / 2;
            top = $(document).scrollTop() + ($(window).height() - dialogHeight) / 2;
            this._dialogLayout.css({
                left: left,
                top: top
            });
        },

        _generateLayout: function () {
            var layout = this._dialogLayout = $('' +
                '<div class="dnnModuleDialog dnnExistingModuleDialog">' +
                    '<div class="dnnPageContainer">' +
                        '<div class="dnnPage">' +
                            '<div class="dnnDialogTitle">' +
                                '<span class="title">' + dnn.ContentEditorManagerResources.addExistingModule + '</span>' +
                            '</div>' +
                            '<div class="dnnDialogBody dnnModuleList">' +
                                '<div class="dnnModuleHeader">' +
                                    '<ul>' +
                                        '<li>' +
                                            '<label>' + dnn.ContentEditorManagerResources.page + '</label>' +
                                            '<div id="AddExistingModule_Pages" class="dnnDropDownList page">' +
                                                '<div class="selected-item">' +
                                                    '<a href="javascript:void(0);" title="Click to expand" class="selected-value">Select Page</a>' +
                                                '</div>' +
                                            '</div>' +
                                            '<input type="hidden" id="AddExistingModule_FolderPicker_State" />' +
                                        '</li>' +
                                        '<li>' +
                                            '<input type="checkbox" class="copy-module" checked="true" aria-label="Copy" /><label>' + dnn.ContentEditorManagerResources.makeCopy + '</label>' +
                                        '</li>' +
                                        '<div class="clear"></div>' +
                                    '</ul>' +
                                '</div>' +
                                '<div class="listContainer listAll"><ul></ul></div>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
					'<div class="dnnProgressBar"><span /></div>' +
                '</div>');

            return layout;
        },

        _addCloseButton: function () {
            var closeButton = this._closeButton = $('<span class="btn-close" />');
            $(".dnnExistingModuleDialog .dnnDialogTitle .title").after(closeButton);

            return closeButton;
        },

        _showDialog: function () {
            this._createMask();
            this._dialogLayout.show('fast', $.proxy(this._dialogOpened, this));
            $(document).on('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
        },

        _hideDialog: function (callback) {
            this._destroyMask();
            this._dialogLayout.hide('fast', function () {
                if (typeof callback == "function") {
                    callback.call(this);
                }
            });

            $(document).off('keyup', $.proxy(this._handleKeyEvent, this));
            $(window).resize();
        },

        _createMask: function () {
            var mask = $('<div class="dnnDialogMask"></div>');
            mask.css({
                left: 0,
                top: 0,
                width: '100%',
                height: $(document).outerHeight()
            });

            $(document.body).append(mask).css('overflow', 'hidden');

            this._calcPosition(this.getModuleManager().getExistingModuleHandler());
        },

        _destroyMask: function () {
            $(document.body).css('overflow', '').find('.dnnDialogMask').remove();
        },

        _dialogOpened: function () {
            this._calcPosition(this.getModuleManager().getExistingModuleHandler());

            //when dialog first open, raise dialog initial event
            if (!this._initialized) {
                this._dialogLayout.trigger('dialoginit');
                this._initialized = true;

                this._loadModuleList();
            }

            this._dialogLayout.trigger('dialogopen');
        },

        _loadModuleList: function () {
            this._dialogLayout.find('li.dnnModuleItem').remove();
            this._dialogLayout.find('div.dnnModuleDialog_ModuleListMessage').remove();

            if (!this._selectedPageId) {
                var container = this._dialogLayout.find(".dnnModuleList .listContainer.listAll ul");
                container.after(this._getNoResultTemplate());
                return;
            }

            if (!this.getModuleDialog()._getService().isLoading()) {
                this.getModuleDialog()._getService().request('GetTabModules', 'GET', {
                    tab: this._selectedPageId
                }, $.proxy(this._renderModuleList, this));
            }
        },

        _renderModuleList: function (data) {
            var container = this._dialogLayout.find(".dnnModuleList .listContainer.listAll ul");

            if (data.length > 0) {
                container.next().remove();
                //render rearranged list
                for (var i = 0; i < data.length; i++) {
                    var itemData = data[i];

                    var $item = $(this._renderItem(itemData));
                    container.append($item);
                }

                this._dialogLayout.trigger('moduleloaded');

                this._initScrollView();
            }
            else {
                if (container.has("li").length === 0) {
                    $(".jspVerticalBar").hide();
                    //scroll top to render no modules message
                    $(".jspDrag").css("top", "0px");
                    $(".jspPane").css("top", "0px");
                    container.after(this._getNoResultTemplate());
                }
            }
        },

        _getItemTemplate: function () {
            return '<li class="dnnModuleItem" data-moduleid="[$ModuleID$]">' +
                '<span class="icon [$ModuleName|css]"><img src="[$ModuleImage$]" alt="[$ModuleName$]" /></span>' +
                '<span class="title {0}">[$ModuleName$]</span>' +
                '</li>';
        },

        _getNoResultTemplate: function () {
            return '<div class="dnnModuleDialog_ModuleListMessage"><span>' + dnn.ContentEditorManagerResources.nomodules + '</span></div>';
        },

        _renderItem: function (item) {
            var extraclass;
            var template = this._getItemTemplate();
            var renderItem = this._cloneItem(item);
            if (moduleAlias[renderItem.ModuleName]) {
                renderItem.ModuleName = moduleAlias[renderItem.ModuleName];
            }

            for (var a in renderItem) {
                if (renderItem.hasOwnProperty(a)) {
                    var shortMatchRegex = new RegExp('\\[\\$' + a + '\\|(\\d+)\\$\\]', 'g');
                    var shortMatch = shortMatchRegex.exec(template);
                    while (shortMatch) {
                        var val = renderItem[a].toString();
                        var length = parseInt(shortMatch[1], 10);
                        if (val.length > length) {
                            val = val.substr(0, length) + "...";
                        }

                        template = template.replace(shortMatch[0], val);
                        shortMatch = shortMatchRegex.exec(template);

                    };

                    template = this._replaceAll(template, '[$' + a + '|css]', this._replaceAll(renderItem[a].toString().toLowerCase(), ' ', '-'));
                    template = this._replaceAll(template, '[$' + a + '$]', renderItem[a]);

                    if (a === 'ModuleName') {
                        extraclass = renderItem[a].length > 20 ? 'longTitle' : '';
                        template = template.replace('{0}', extraclass);
                    }
                }
            }

            return template;
        },

        _cloneItem: function () {
            return this.getModuleDialog()._cloneItem.apply(this.getModuleDialog(), arguments);
        },

        _initScrollView: function () {
            var container = this._dialogLayout.find(".dnnModuleList");
            if (container.data('jsp')) {
                container.data('jsp').reinitialise();
            } else {
                container.jScrollPane();
            }

            $(".jspVerticalBar").show();
        },

        _attachEvents: function () {
            this._dialogLayout.on('click', '.dnnModuleItem', $.proxy(this._doAddModule, this));
            $(window).on('resize', '', $.proxy(this._layoutResized, this));
            this._dialogLayout.on('click', 'span.btn-close', $.proxy(this._closeButtonHandler, this));
            $(window).on('beforeunload', $.proxy(this._windowBeforeUnload, this));
        },

        _initUI: function () {
            this._createPagesDropdown();
            this._dialogLayout.find('input[type="checkbox"]').dnnCheckbox();
        },

        _closeButtonHandler: function () {
            this.getModuleManager().getExistingModuleHandler().click();
        },

        _handleKeyEvent: function (e) {
            if (e.keyCode === 27) {
                this.getModuleManager().getExistingModuleHandler().click();
            }
        },

        _doAddModule: function (e) {
            var $item = $(e.target);
            $item = $item.hasClass('dnnModuleItem') ? $item : $item.parents('.dnnModuleItem');
            var handler = this;
            var moduleId = $item.data("moduleid");
            if (!moduleId || moduleId === -1) {
                return false;
            }
            this.addModule(moduleId, function () {
                handler._hideDialog();
            }, true);

            return false;
        },

        _layoutResized: function (e) {
            if (this.isOpen()) {
                this._calcPosition(this.getModuleManager().getExistingModuleHandler());
            }
        },

        _replaceAll: function () {
            return this.getModuleDialog()._replaceAll.apply(this.getModuleDialog(), arguments);
        }
    };

    dnnExistingModuleDialog.defaultOptions = {};

    ///dnnModuleDialog Plugin END
}(jQuery));