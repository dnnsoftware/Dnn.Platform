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

    var suportAjaxUpload = (function () {
        var support;
        return function() {
            if (typeof support === "undefined") {
                var xhr = new XMLHttpRequest;
                support = !!(xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
            }
            return support;
        };
    })();

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
            this.options = $.extend({}, FileUpload.defaults(), this.options);

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
            this._folderPicker = new dnn.DropDownList(null, this.options.folderPicker);

            var dialog = $element('div', { tabindex: '-1', 'class': 'fu-container', role: 'dialog' }).append(
                $element('h5', { 'class': 'fu-dialog-header' }).text(this.options.dialogHeader),
                $element('div', { 'class': 'fu-dialog-content' }).append(
                    $element("div", { 'class': 'fu-dialog-content-header' }).append(
                        $element("div", { 'class': 'dnnLeft' }).append(
                            $element("ul", { 'class': 'dnnButtonGroup' }).append(
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);", 'class': 'active' }).text(this.options.uploadFileMethod).on("click", $.proxy(this._selectUpload, this, this._uploadMethods.local))),
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);" }).text(this.options.uploadFromWebMethod).on("click", $.proxy(this._selectUpload, this, this._uploadMethods.web)))),
                            $element("span").append(
                                $element("input", { type: 'checkbox', id: checkBoxId }),
                                $element("label", { 'for': checkBoxId, 'class': 'fu-decompress-label' }).text(this.options.decompressLabel))),
                        $element("div", { 'class': 'fu-folder-picker-container dnnRight' }).append(
                            $element("label").text(this.options.uploadToFolderLabel),
                            this._folderPicker.$element.addClass("dnnLeftComboBox"))),
                    $element("div", { 'class': 'fu-dialog-content-fileupload-local' }).append(
                        $element("div", { 'class': 'fu-dialog-drag-and-drop-area' }).append(
                            $element("div", { 'class': 'fu-dialog-drag-and-drop-area-message' }))),
                    $element("div", { style: 'display: none', 'class': 'fu-dialog-content-fileupload-web' }).append(
                        $element("table", { 'class': 'dnnFileUploadWebInput' }).append(
                            $element("tbody").append(
                                $element("tr").append(
                                    $element("td").append(
                                        $element("div", { 'class': 'txtWrapper' }).append(
                                            $element("input", { type: 'text' } ))),
                                $element("td").append(
                                    $element("a", { href: 'javascript:void(0);', 'class': 'dnnSecondaryAction' }).text(this.options.uploadFromWebButtonText)))))),
                    $element("div", { style: 'display: none', 'class': 'fu-fileupload-statuses-container' }).append(
                        $element("ul", { 'class': 'fu-fileupload-statuses' }))));

            return dialog;
        },

        _createFileUploadStatusElement: function (fileName) {
            var status = { fileName: fileName, overwrite: false, extract: false };
            var $status = $element("li").append(
                $element("div", { "class": "fu-fileupload-filename-container" }).append(
                    $element("span", { "class": "fu-fileupload-filename", title: fileName }).text(fileName)),
                $element("div", { "class": "fu-fileupload-progressbar-container" }).append(
                    $element("div", { "class": "fu-fileupload-progressbar ui-progressbar" }).append(
                        $element("div", { "class": "ui-progressbar-value" }).width(0)),
                    $element("a", { href: "javascript:void(0);", "class": "uploading" })))
                .data("status", status);
            return $status;
        },

        _showFileUploadStatus: function ($fileUploadStatus, state, data) {

            $fileUploadStatus.find(".fu-fileupload-filename-container .fu-file-already-exists-prompt").remove();

            var $prompt = $element("div", { "class": "fu-file-already-exists-prompt" });
            var $statusMessage = $element("span", { "class": "fu-status-message" });
            $prompt.append($statusMessage);
            $fileUploadStatus.find(".fu-fileupload-filename-container").append($prompt);

            var message;
            // file already exists scenario: show Keep/Replace prompt
            if (state.AlreadyExists) {

                var self = this;
                var $keepButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-keep" })
                    .text("Keep")
                    .click(function () {
                        $fileUploadStatus.removeClass().addClass(self.options.statusCancelledCss);
                        self._showFileUploadStatus($fileUploadStatus, { Message: "File upload stopped" }, data);
                    });

                var $replaceButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-replace" })
                    .text("Replace")
                    .on('click', function () {
                        var $this = $(this);
                        $fileUploadStatus.data("status").overwrite = true;
                        var data = $this.data();
                        data.uploadedBytes = 0;
                        data.data = null;
                        data.submit();
                        $prompt.remove();
                    })
                    .data(data);

                $prompt.append($keepButton, $replaceButton);
                message = "The file you want to upload already exists in this folder";
            }
            else {
                message = state.Message;
            }
            $statusMessage.text(message);

            $fileUploadStatus.find(".fu-fileupload-progressbar-container").attr("title", message);
        },

        _add: function (e, data) {
            if (!this._$fileUploadStatusesContainer.is(':visible')) {
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
            }

            var message;

            // Empty file upload does not be supported in IE10
            if (data.files[0].size == 0 && $.browser.msie && $.browser.version == "10.0") {
                message = "Empty file upload is not supported";
            }

            if (typeof message === "undefined" && this.options.maxFileSize && data.files[0].size > this.options.maxFileSize) {
                message = "File size is too large";
            }

            if (message) {
                var $fileUploadStatus = this._getInitializedStatusElement(data).addClass(this.options.statusErrorCss);
                this._showFileUploadStatus($fileUploadStatus, { Message: message }, data);
                return;
            }

            setTimeout(function () { data.submit(); }, 25);
        },

        _getInitializedStatusElement: function(data) {
            var fileName = data.files[0].name;
            var $fileUploadStatus = this._getFileUploadStatusElement(fileName);
            var cancelUpload;
            if (!$fileUploadStatus.length) {
                $fileUploadStatus = this._createFileUploadStatusElement(fileName);
                this._$fileUploadStatuses.append($fileUploadStatus);
                cancelUpload = function() {
                    var xhr = data && data.jqXHR;
                    if (xhr && xhr.readyState !== 4) {
                        xhr.abort();
                    }
                };
                $fileUploadStatus.find(".fu-fileupload-progressbar-container a").on("click", cancelUpload);
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
            }
            this._initProgressBar($fileUploadStatus);
            return $fileUploadStatus;
        },

        _submit: function (e, data) {
            var $fileUploadStatus = this._getInitializedStatusElement(data);
            var extract = this._extract();
            var fileName = data.files[0].name;
            var extension = fileName.substring(fileName.lastIndexOf('.') + 1);
            if (extension === 'zip' && extract) {
                $fileUploadStatus.data("status").extract = true;
            }

            data.formData = {
                folder: this._selectedPath(),
                filter: '',
                extract: extract,
                overwrite: $fileUploadStatus.data("status").overwrite
            };
            return true;
        },

        _progress: function(e, data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            if (data.formData.extract) {
                if ($fileUploadStatus.find('.fu-dialog-fileupload-extracting').length === 0) {
                    $fileUploadStatus.find('.fu-fileupload-filename').append(
                        $element("span", { "class": "fu-dialog-fileupload-extracting"}).text(" - " + this.options.decompressingFile));
                }
                return;
            }
            var progress = parseInt(data.loaded / data.total * 100, 10);
            if (progress < 100) {
                this._setProgressBar($fileUploadStatus, progress);
            }
        },

        _done: function(e, data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            var error = this._getFileUploadError(data);
            if (error) {
                if (!error.AlreadyExists) {
                    this._setProgressBar($fileUploadStatus, 100);
                    $fileUploadStatus.addClass(this.options.statusErrorCss);
                }
                this._showFileUploadStatus($fileUploadStatus, error, data);
                return;
            }
            $fileUploadStatus.data("status").overwrite = false;
            this._setProgressBar($fileUploadStatus, 100);
            this._showFileUploadStatus($fileUploadStatus, { Message: "File uploaded" }, data);
            $fileUploadStatus.addClass(this.options.statusUploadedCss);
        },

        _fail: function(e, data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            $fileUploadStatus.addClass(this.options.statusErrorCss);
            var message = data.errorThrown === "abort" ? "Upload cancelled" : "Upload failed";
            this._showFileUploadStatus($fileUploadStatus, { Message: message }, data);
        },

        _dragover: function () {
            this._$dragAndDropArea.addClass("dragover");
        },

        _drop: function () {
            this._$dragAndDropArea.removeClass("dragover");
        },

        _initFileUploadFromLocal: function () {

            this._$inputFileControl.fileupload({
                url: this._uploadUrl(),
                beforeSend: $.dnnSF().setModuleHeaders,
                dropZone: this._$dragAndDropArea,
                sequentialUpload: false,
                progressInterval: 20,
                autoUpload: false
            })
            .on("fileuploadadd", $.proxy(this._add, this))
            .on("fileuploadsubmit", $.proxy(this._submit, this))
            .on("fileuploadprogress", $.proxy(this._progress, this))
            .on("fileuploaddone", $.proxy(this._done, this))
            .on("fileuploadfail", $.proxy(this._fail, this))
            .on("fileuploaddragover", $.proxy(this._dragover, this))
            .on("fileuploaddrop", $.proxy(this._drop, this));
        },

        _extract: function() {
            return this._$extract.is(':checked');
        },

        _selectedPath: function() {
            var selectedPathArray = this._folderPicker.selectedPath();
            var selectedPath = "";
            if (selectedPathArray.length > 1) {
                for (var i = 1, size = selectedPathArray.length; i < size; i++) {
                    selectedPath += selectedPathArray[i].name + "/";
                }
            } 
            return selectedPath;
        },

        _getFileUploadError: function(data) {
            var error;
            try {
                if (!suportAjaxUpload()) {
                    error = JSON.parse($("pre", data.result).html());
                }
                else {
                    error = JSON.parse(data.result);
                }
            }
            catch (e) {
                return null;
            }
            if (!error.Message) {
                return null;
            }
            return error;
        },

        _getFileUploadStatusElement: function(fileName) {
            return this._$fileUploadStatuses.children().filter(function (index) { return $(this).data("status").fileName == fileName; });
        },

        _initProgressBar: function ($fileUploadStatus) {
            $fileUploadStatus.find('.fu-fileupload-progressbar > div').css('width', '0');
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
            this._$fileUploadStatusesContainer = this.$element.find('.fu-fileupload-statuses-container').hide().jScrollbar();
            this._$fileUploadStatuses = this._$fileUploadStatusesContainer.find('.fu-fileupload-statuses').empty();
            this._$dragAndDropArea = this.$element.find('.fu-dialog-drag-and-drop-area');
            this._$inputFileControl = $element("input", { type: 'file', name: 'postfile', multiple: true, "data-text": this.options.dragAndDropAreaTitle });
            this._$extract = this.$element.find("." + "fu-dialog-content-header").find("input");

            var stateElementId = this.options.internalStateFieldId;
            if (stateElementId) {
                this._stateElement = document.getElementById(stateElementId);
            }

            this._draggable = supportDragDrop();

            this._$inputFileControl.appendTo(this._$dragAndDropArea.find('.fu-dialog-drag-and-drop-area-message')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._initFileUploadFromLocal();
        },

        _uploadUrl: function() {
            var serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
            var url = serviceUrl + this.options.fileUploadMethod;
            if (!suportAjaxUpload()) {
                var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
                url += '?__RequestVerificationToken=' + antiForgeryToken;
            }
            return url;
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
                resizable: false,
                width: this.options.width,
                height: this.options.height,
                close: $.proxy(this._onCloseDialog, this),
                buttons: [ {
                    text: this.options.closeButtonText,
                    click: function () { $(this).dialog("close"); },
                    "class": "dnnSecondaryAction"
                    }
                ]
            });

        }

    };

    FileUpload._defaults = {
        dialogCss: "fu-dialog",
        statusErrorCss: "fu-status-error",
        statusUploadedCss: "fu-status-uploaded",
        statusCancelledCss: "fu-status-cancelled",
        width: 780,
        height: 630,
        serviceRoot: "InternalServices",
        fileUploadMethod: "fileupload/postfile"
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

