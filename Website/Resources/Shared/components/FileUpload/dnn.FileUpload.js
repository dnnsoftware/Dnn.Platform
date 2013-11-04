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

    var FileUpload = this.FileUpload = function (element, options) {
        this.element = element;
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

            var $parent = $(this.element).parent();
            $parent.empty();
            this.$element = this._createLayout();
            $parent.append(this.$element);

            //this.$element = this.element ? $(this.element) : this._createLayout();

            this._$buttonGroup = this.$element.find(".fu-dialog-content-header ul.dnnButtonGroup");
            this._$uploadResultPanel = this.$element.find('.fu-fileupload-statuses-container');
            this._$uploadResultPanel.jScrollbar();

            this._$dialogCloseBtn = this.$element.find('.dnnFileUploadDialogClose');
            this._$dropFileZone = this.$element.find('.dnnFileUploadDropZone');
            this._$inputFileControl = $("<input type='file' name='postfile' multiple data-text='DRAG FILES HERE OR CLICK TO BROWSE' />");
            this._$decompressZipCheckbox = this.$element.find("." + "fu-dialog-content-header").find("input");
            
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

            this._$uploadResultPanel.hide().find('.fu-fileupload-statuses').empty();
            this._$inputFileControl.appendTo(this._$dropFileZone.find('.dnnDropFileMessage')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._initUploadFileFromLocal();
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

            this._folderPicker = new dnn.DropDownList(null, folderPickerOptions);
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
                        $element("div", { 'class': 'dnnFileUploadDropZone' }).append(
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
                        $element("div", { 'class': 'ps-container' }).append(
                            $element("ul", { 'class': 'fu-fileupload-statuses' })))));

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
                    if (!self._$uploadResultPanel.is(':visible')) {
                        self._$uploadResultPanel.show().jScrollbar("update");
                    }
                    //TODO: do some check
                    data.submit();
                },
                submit: function (e, data) {
                    var fileResultZone = self._getUploadFileResultZone(data.files[0].name);
                    if (!fileResultZone.length) {
                        fileResultZone = self._createFileUploadStatusElement(data.files[0].name);
                        self._$uploadResultPanel.find('.fu-fileupload-statuses').append(fileResultZone);
                        fileResultZone.find('.fu-fileupload-progressbar-check.uploading').on('click', function() {
                            if (data.jqXHR) data.jqXHR.abort();
                        });

                        self._$uploadResultPanel.show().jScrollbar("update");
                    } else {
                        self._initProgressBar(fileResultZone);
                    }

                    var extract = self._getDecompressZipFileOption();
                    var extension = data.files[0].name.substring(data.files[0].name.lastIndexOf('.') + 1);
                    if (extension === 'zip' && extract == 'true') {
                        fileResultZone.attr('data-extract', 'true');
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
                    var fileResultZone = self._getUploadFileResultZone(data.files[0].name);
                    if (data.formData.extract == "true") {
                        if (fileResultZone.find('.dnnFileUploadExtracting').length == 0) {
                            fileResultZone.find('.fu-fileupload-filename')
                                .append("<span class='dnnFileUploadExtracting'> - Decompressing File</span>");
                        }
                        return;
                    }
                    var progress = parseInt(data.loaded / data.total * 100, 10);
                    if (progress < 100) {
                        self._setProgressBar(fileResultZone, progress);
                    }
                },
                done: function(e, data) {
                    var fileResultZone = self._getUploadFileResultZone(data.files[0].name);
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

        _getDecompressZipFileOption: function() {
            return this._$decompressZipCheckbox.is(':checked') ? 'true' : 'false';
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
                } else {
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
                $element("div", { "class": "fu-fileupload-filename" }).text(fileName),
                $element("div", { "class": "fu-fileupload-progressbar-container" }).append(
                    $element("div", { "class": "fu-fileupload-progressbar ui-progressbar" }).append(
                        $element("div", { "class": "ui-progressbar-value" }).width(0)),
                    $element("div", { "class": "fu-fileupload-progressbar-check uploading" })));
            return $status;
        },

        _getUploadFileResultZone: function(fileName) {
            return this._$uploadResultPanel.find('li[data-filename="' + fileName + '"]');
        },

        _initProgressBar: function(fileResultZone) {
            fileResultZone.find('.fu-fileupload-progressbar > div').css('width', '0%');
            fileResultZone.find('.fu-fileupload-progressbar-check').removeClass('finished').addClass('uploading');
            fileResultZone.find('.fu-fileupload-progressbar-container').show();
        },

        _setProgressBar: function(fileResultZone, progress) {
            fileResultZone.find(".fu-fileupload-progressbar-container").show();

            if (!progress) {
                fileResultZone.find('.fu-fileupload-progressbar').addClass('indeterminate-progress');
                fileResultZone.find('.fu-fileupload-progressbar > div').css('width', '100%');
                return;
            }

            if (progress < 100) {
                fileResultZone.find(".fu-fileupload-progressbar > div").css('width', progress + '%');
                return;
            }

            fileResultZone.find('.fu-fileupload-progressbar-check').removeClass('uploading').addClass('finished');
            fileResultZone.find('.fu-fileupload-progressbar.indeterminate-progress').removeClass('indeterminate-progress');
            fileResultZone.find('.dnnFileUploadExtracting').remove();
            fileResultZone.find('.fu-fileupload-progressbar > div').css('width', '100%');
        }

    };

    FileUpload._defaults = {

    };

    FileUpload.defaults = function (settings) {
        if (typeof settings !== "undefined") {
            $.extend(FileUpload._defaults, settings);
        }
        return FileUpload._defaults;
    };

}).apply(dnn, [jQuery, window, document]);


dnn.createFileUploadControl = function (selector, options, methods) {
    $(document).ready(function () {
        var $element = $(selector);
        if ($element.length === 0) {
            return;
        }
        $.extend(options, methods);
        var element = $element[0];
        var instance = new dnn.FileUpload(element, options);
        if (element.id) {
            dnn[element.id] = instance;
        }
    });
};

