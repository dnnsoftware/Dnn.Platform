; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($, window, document, undefined) {
    "use strict";

    var $element = function (element, props) {
        var $e = $(document.createElement(element));
        props && $e.attr(props);
        return $e;
    };

    var suportAjaxUpload = function() {
        var xhr = new XMLHttpRequest;
        return !!(xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
    };

    var supportDragDrop = function() {
        return ('draggable' in document.createElement('span'));
    };

    var FileUpload = this.FileUpload = function (options) {
        this.options = options;
        this.init();
    };

    FileUpload.prototype = {

        constructor: FileUpload,

        init: function () {
            var self = this;
            this.options = $.extend({}, FileUpload.defaults(), this.options);
            this.serviceFramework = $.dnnSF();

            this.$this = $(this);

            this._uploadMethods = new dnn.Enum([{ key: 0, value: "undefined" }, { key: 1, value: "local" }, { key: 2, value: "web" }]);
            this._uploadMethod = this._uploadMethods.local;

            this.show();

        },

        _selectUpload: function (uploadMethod, eventObject) {
            eventObject.preventDefault();
            eventObject.stopPropagation();
            var isLocal = uploadMethod === this._uploadMethods.local;
            this.$element.find(".fu-dialog-content-fileupload-local").toggle(isLocal);
            this.$element.find(".fu-dialog-content-fileupload-web").toggle(!isLocal);

            var clickedElement = eventObject.currentTarget;
            this._$buttonGroup.find('a').each(function(i, element) {
                if (element !== clickedElement) {
                    $(element).removeClass("active");
                }
            });
            $(clickedElement).addClass('active');
        },

        _createLayout: function () {

            var checkBoxId = dnn.uid("fu_");

            var folderPickerOptions = {
                itemList: {
                    clearButtonTooltip: "Clear",
                    firstItem: null,
                    loadingResultText: "...Loading Results",
                    resultsText: "Results",
                    searchButtonTooltip: "Search",
                    searchInputPlaceHolder: "Search...",
                    selectedItemCollapseTooltip: "Click to collapse",
                    selectedItemExpandTooltip: "Click to expand",
                    sortAscendingButtonTitle: "A-Z",
                    sortAscendingButtonTooltip: "Sort in ascending order",
                    sortDescendingButtonTooltip: "Sort in descending order",
                    unsortedOrderButtonTooltip: "Remove sorting"
                },
                onSelectionChanged: [],
                selectItemDefaultText: "Select A Folder",
                selectedItemCss: "selected-item",
                services: {
                    getNodeDescendantsMethod: "ItemListService/GetFolderDescendants",
                    getTreeMethod: "ItemListService/GetFolders",
                    getTreeWithNodeMethod: "ItemListService/GetTreePathForFolder",
                    parameters: [],
                    rootId: "Root",
                    searchTreeMethod: "ItemListService/SearchFolders",
                    serviceRoot: "InternalServices",
                    sortTreeMethod: "ItemListService/SortFolders"
                }
            };

            this._folderPicker = new dnn.DropDownList(null, this.options.folderPicker);
            var dialog = $element('div', { tabindex: '-1', 'class': 'fu-container', role: 'dialog' }).append(
                $element('h5', { 'class': 'fu-dialog-header' }).text("Use one of the methods below to upload files"),
                $element('div', { 'class': 'fu-dialog-content' }).append(
                    $element("div", { 'class': 'fu-dialog-content-header' }).append(
                        $element("div", { 'class': 'dnnLeft' }).append(
                            $element("ul", { 'class': 'dnnButtonGroup' }).append(
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);", 'class': 'active' }).text("Upload File").on("click", $.proxy(this._selectUpload, this, this._uploadMethods.local))),
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);" }).text("From Web").on("click", $.proxy(this._selectUpload, this, this._uploadMethods.web)))),
                            $element("span").append(
                                $element("input", { type: 'checkbox', id: checkBoxId }),
                                $element("label", { 'for': checkBoxId, 'class': 'fu-decompress-label' }).text("Decompress Zip Files"))),
                        $element("div", { 'class': 'fu-folder-picker-container dnnRight' }).append(
                            $element("label").text("Upload To:"),
                            this._folderPicker.$element.addClass("dnnLeftComboBox"))),
                    $element("div", { 'class': 'fu-dialog-content-fileupload-local' }).append(
                        $element("div", { 'class': 'fu-dialog-drag-and-drop-area' }).append(
                            $element("div", { 'class': 'dnnDropFileMessage' }))),
                    $element("div", { style: 'display: none', 'class': 'fu-dialog-content-fileupload-web' }).append(
                        $element("table", { 'class': 'dnnFileUploadWebInput' }).append(
                            $element("tbody").append(
                                $element("tr").append(
                                    $element("td").append(
                                        $element("div", { 'class': 'txtWrapper' }).append(
                                            $element("input", { type: 'text' } ))),
                                $element("td").append(
                                    $element("a", { href: 'javascript:void(0);', 'class': 'dnnSecondaryAction' }).text("Load")))))),
                    $element("div", { style: 'display: none', 'class': 'fu-fileupload-statuses-container' }).append(
                        $element("ul", { 'class': 'fu-fileupload-statuses' }))));

            return dialog;
        },

        _initUploadFileFromLocal: function () {
            var self = this;
            this._$inputFileControl.fileupload({
                url: this._uploadUrl,
                beforeSend: this.serviceFramework.setModuleHeaders,
                dropZone: this._$dropFileZone,
                sequentialUpload: false,
                progressInterval: 20,
                add: function (e, data) {
                    if (!self._$fileUploadStatuses.is(':visible')) {
                        self._$fileUploadStatuses.show().jScrollbar("update");
                    }
                    //TODO: do some check
                    data.submit();
                },
                submit: function (e, data) {
                    var $fileUploadStatus = self._getFileUploadStatusElement(data.files[0].name);
                    if (!$fileUploadStatus.length) {
                        $fileUploadStatus = self._createFileUploadStatusElement(data.files[0].name);
                        self._$fileUploadStatuses.find('.fu-fileupload-statuses').append($fileUploadStatus);
                        $fileUploadStatus.find('.fu-fileupload-progressbar-check.uploading').on('click', function() {
                            if (data.jqXHR) data.jqXHR.abort();
                        });
                        self._$fileUploadStatuses.show().jScrollbar("update");
                    }
                    else {
                        self._initProgressBar($fileUploadStatus);
                    }

                    var extract = self._extract();
                    var extension = data.files[0].name.substring(data.files[0].name.lastIndexOf('.') + 1);
                    if (extension === 'zip' && extract == 'true') {
                        $fileUploadStatus.attr('data-extract', 'true');
                    }

                    data.formData = {
                        folder: self._getSelectedFolder(),
                        filter: '',
                        extract : extract,
                        overwrite: 'true'
                    };
                    return true;
                },
                progress: function(e, data) {
                    var $fileUploadStatus = self._getFileUploadStatusElement(data.files[0].name);
                    if (data.formData.extract == "true") {
                        if ($fileUploadStatus.find('.fu-dialog-fileupload-extracting').length == 0) {
                            $fileUploadStatus.find('.fu-fileupload-filename')
                                .append("<span class='fu-dialog-fileupload-extracting'> - Decompressing File</span>");
                        }
                        return;
                    }
                    var progress = parseInt(data.loaded / data.total * 100, 10);
                    if (progress < 100) {
                        self._setProgressBar($fileUploadStatus, progress);
                    }
                },
                done: function(e, data) {
                    var fileResultZone = self._getFileUploadStatusElement(data.files[0].name);
                    var fileError = self._getFileUploadError(data);
                    if (fileError) {
                        return;
                    }
                    self._setProgressBar(fileResultZone, 100);
                },
                fail: function(e, data) {
                    //TODO: not implemented  
                },
                dragover: function () {
                    self._$dropFileZone.addClass("dragover");
                },
                drop: function () {
                    self._$dropFileZone.removeClass("dragover");
                }

            });
        },

        _extract: function() {
            return this._$extract.is(':checked') ? 'true' : 'false';
        },

        _getSelectedFolder: function() {
            var selectedPathArray = this._folderPicker.selectedPath();
            var selectedPath = '';
            if (selectedPathArray.length > 1) {
                for (var i = 1; i < selectedPathArray.length; i++) {
                    selectedPath += selectedPathArray[i].name + '/';
                }
            } 
            return selectedPath;
        },

        _getFileUploadError: function(data) {
            var error;
            try {
                if (!this._ajaxUploadable) {
                    error = JSON.parse($("pre", data.result).html());
                }
                else {
                    error = JSON.parse(data.result);
                }
            } catch (e) {
                return null;
            }

            if (!error.Message) return null;
            return error;
        },

        _createFileUploadStatusElement: function (fileName) {
            var $status = $element("li", { "class": "fu-fileupload-status", "data-filename": fileName }).append(
                $element("span", { "class": "fu-fileupload-filename" }).text(fileName),
                $element("div", { "class": "fu-fileupload-progressbar-container" }).append(
                    $element("div", { "class": "fu-fileupload-progressbar ui-progressbar" }).append(
                        $element("div", { "class": "ui-progressbar-value" }).width(0)),
                    $element("div", { "class": "fu-fileupload-progressbar-check uploading" })));
            return $status;
        },

        _getFileUploadStatusElement: function(fileName) {
            return this._$fileUploadStatuses.find('li[data-filename="' + fileName + '"]');
        },

        _initProgressBar: function ($fileUploadStatus) {
            $fileUploadStatus.find('.fu-fileupload-progressbar > div').css('width', '0');
            $fileUploadStatus.find('.fu-fileupload-progressbar-check').removeClass('finished').addClass('uploading');
            $fileUploadStatus.find('.fu-fileupload-progressbar-container').show();
        },

        _setProgressBar: function ($fileUploadStatus, progress) {
            $fileUploadStatus.find(".fu-fileupload-progressbar-container").show();

            if (!progress) {
                $fileUploadStatus.find('.fu-fileupload-progressbar').addClass('indeterminate-progress');
                $fileUploadStatus.find('.fu-fileupload-progressbar > div').css('width', '100%');
                return;
            }

            if (progress < 100) {
                $fileUploadStatus.find(".fu-fileupload-progressbar > div").css('width', progress + '%');
                return;
            }

            $fileUploadStatus.find('.fu-fileupload-progressbar-check').removeClass('uploading').addClass('finished');
            $fileUploadStatus.find('.fu-fileupload-progressbar.indeterminate-progress').removeClass('indeterminate-progress');
            $fileUploadStatus.find('.fu-dialog-fileupload-extracting').remove();
            $fileUploadStatus.find('.fu-fileupload-progressbar > div').css('width', '100%');
        },

        _onCloseDialog: function () {
            this._isShown = false;
        },

        _ensureDialog: function () {
            if (this.$element) {
                return;
            }

            this.$element = this._createLayout();

            this._$buttonGroup = this.$element.find(".fu-dialog-content-header ul.dnnButtonGroup");
            this._$fileUploadStatuses = this.$element.find('.fu-fileupload-statuses-container').jScrollbar();

            this._$dialogCloseBtn = this.$element.find('.dnnFileUploadDialogClose');
            this._$dropFileZone = this.$element.find('.fu-dialog-drag-and-drop-area');
            this._$inputFileControl = $element("input", { type: 'file', name: 'postfile', multiple: true, "data-text": 'DRAG FILES HERE OR CLICK TO BROWSE' });
            this._$extract = this.$element.find("." + "fu-dialog-content-header").find("input");

            var stateElementId = this.options.internalStateFieldId;
            if (stateElementId) {
                this._stateElement = document.getElementById(stateElementId);
            }

            this._draggable = supportDragDrop();
            this._ajaxUploadable = suportAjaxUpload();
            this._uploadUrl = this.serviceFramework.getServiceRoot('internalservices') + 'fileupload/postfile';
            if (!this._ajaxUploadable) {
                var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
                this._uploadUrl += '?__RequestVerificationToken=' + antiForgeryToken;
            }

            this._$fileUploadStatuses.hide().find('.fu-fileupload-statuses').empty();
            this._$inputFileControl.appendTo(this._$dropFileZone.find('.dnnDropFileMessage')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._initUploadFileFromLocal();
        },

        show: function () {
            if (this._isShown) {
                return;
            }
            this._isShown = true;

            this._ensureDialog();

            this.$element.dialog({
                modal: true,
                autoOpen: true,
                dialogClass: "dnnFormPopup " + this.options.dialogCss,
                title: this.options.title,
                resizable: true,
                width: this.options.width,
                height: this.options.height,
                close: $.proxy(this._onCloseDialog, this)
            });

        }

    };

    FileUpload._defaults = {
        dialogCss: "fu-dialog",
        width: 780,
        height: 500
    };

    FileUpload.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(FileUpload._defaults, settings);
        }
        return FileUpload._defaults;
    };

    var FileUploadDialog = this.FileUploadDialog = dnn.singletonify(FileUpload);

}).apply(dnn, [jQuery, window, document]);


dnn.createFileUpload = function (options) {
    $(document).ready(function () {
        var instance = dnn.FileUploadDialog.getInstance(options);
    });
};

