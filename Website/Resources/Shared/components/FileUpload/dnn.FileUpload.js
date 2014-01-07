; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
            var isLocal = uploadMethod === this._uploadMethods.local;
            this.$element.find(".fu-dialog-content-fileupload-local").toggle(isLocal);
            this.$element.find(".fu-dialog-content-fileupload-web").toggle(!isLocal);
            this._uploadMethod = uploadMethod;

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
                $element('h5', { 'class': 'fu-dialog-header' }).text(this.options.resources.dialogHeader),
                $element('div', { 'class': 'fu-dialog-content' }).append(
                    $element("div", { 'class': 'fu-dialog-content-header' }).append(
                        $element("div", { 'class': 'dnnLeft' }).append(
                            $element("ul", { 'class': 'dnnButtonGroup' }).append(
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);", 'class': 'active' }).text(this.options.resources.uploadFileMethod).on("click", $.proxy(this._selectUpload, this, this._uploadMethods.local))
                                ),
                                $element("li").append(
                                    $element("a", { href: "javascript:void(0);" }).text(this.options.resources.uploadFromWebMethod).on("click", $.proxy(this._selectUpload, this, this._uploadMethods.web))
                                )
                            ),
                            this._$decompressOption = $element("span").append(
                                $element("input", { type: 'checkbox', id: checkBoxId }),
                                $element("label", { 'for': checkBoxId, 'class': 'fu-decompress-label' }).text(this.options.resources.decompressLabel)
                            )
                        ),
                        $element("div", { 'class': 'fu-folder-picker-container dnnRight' }).append(
                            $element("label").text(this.options.resources.uploadToFolderLabel),
                            this._folderPicker.$element.addClass("dnnLeftComboBox")
                        )
                    ),
                    $element("div", { 'class': 'fu-dialog-content-fileupload-local' }).append(
                        $element("div", { 'class': 'fu-dialog-drag-and-drop-area' }).append(
                            $element("div", { 'class': 'fu-dialog-drag-and-drop-area-message' })
                        )
                    ),
                    $element("div", { style: 'display: none', 'class': 'fu-dialog-content-fileupload-web' }).append(
                        $element("table", { 'class': 'fu-dialog-url-upload-area' }).append(
                            $element("tbody").append(
                                $element("tr").append(
                                    $element("td").append(
                                        $element("div").append(
                                            this._$url = $element("input", { type: 'text', title: this.options.resources.urlTooltip, placeholder: this.options.resources.urlTooltip })
                                                .onEnter($.proxy(this._uploadByUrl, this))
                                                .on("change keyup paste input propertychange", $.proxy(this._validateUrl, this))
                                                .prefixInput({ prefix: "http" })
                                        )
                                    ),
                                    $element("td").append(
                                        $element("a", { href: 'javascript:void(0);', 'class': 'dnnSecondaryAction' }).text(this.options.resources.uploadFromWebButtonText).on("click", $.proxy(this._uploadByUrl, this))
                                    )
                                )
                            )
                        )
                    ),
                    $element("div", { style: 'display: none', 'class': 'fu-fileupload-statuses-container' }).append(
                        $element("ul", { 'class': 'fu-fileupload-statuses' })
                    )
                )
            );

            return dialog;
        },

        _validateUrl: function() {
            var url = this._$url.val();
            if (dnn.isUrl(url)) {
                this._$url.removeClass("fu-dialog-url-error");
            }
            else {
                this._$url.addClass("fu-dialog-url-error");
            }
        },

        _getFileExtension: function(filename) {
            var parts = filename.split(".");
            if (parts.length === 1 || (parts[0] === "" && parts.length === 2)) {
                // If a.length is one, it's a visible file with no extension;
                // If a[0] === "" and a.length === 2 it's a hidden file with no extension ie. .htaccess;
                return "";
            }
            return parts.pop();
        },

        _isValidExtension: function (fileName, acceptableExtensions) {
            if (typeof acceptableExtensions === "undefined" || acceptableExtensions === null || acceptableExtensions.length === 0) {
                return true;
            }
            for (var i = 0; i < acceptableExtensions.length; i++) {
                if (fileName.toLowerCase().endsWith(acceptableExtensions[i].toLowerCase())) {
                    return true;
                }
            }
            return false;
        },

        _uploadByUrl: function(eventObject) {
            var url = this._$url.val();
            if (!dnn.isUrl(url)) {
                this._$url.addClass("fu-dialog-url-error");
                return;
            }

            if (!this._isValidExtension(url, this.options.extensions)) {
                this._$url.addClass("fu-dialog-url-error");
                $.dnnAlert({ title: this.options.resources.errorDialogTitle || "Error", text: this.options.resources.invalidFileExtensions });
                return;
            }

            this._$url.removeClass("fu-dialog-url-error");
            this._$url.val("");

            var status = this._getInitializedStatusElement({ fileName: url }).data("status");
            this._submitUrl(status);
        },

        _submitUrl: function (status) {
            var serviceUrl = $.dnnSF().getServiceRoot("InternalServices");
            var serviceSettings = {
                beforeSend: $.dnnSF().setModuleHeaders,
                url: serviceUrl + "FileUpload/UploadFromUrl",
                type: "POST",
                async: true,
                success: $.proxy(this._onUploadByUrl, this, [status.fileName]),
                error: $.onAjaxError
            };
            serviceSettings.data = { Url: status.fileName, Folder: this._selectedPath(), Overwrite: status.overwrite, Unzip: this._extract(), Filter: "", IsHostMenu: false };
            $.ajax(serviceSettings);
        },

        _onUploadByUrl: function (url, data, textStatus, jqXhr) {
            this._processResponse(url[0], data);
        },

        _createFileUploadStatusElement: function (status) {
            status.overwrite = false;
            var fileName = status.fileName;
            var cancelUpload = status.data ? function () {
                var xhr = status.data.jqXHR;
                if (xhr && xhr.readyState !== 4) {
                    xhr.abort();
                }
            } : function() {};
            var $status = $element("li").append(
                $element("a", { href: "javascript:void(0);", "class": "fu-fileupload-thumbnail" }).append(
                    $element("div").append(
                        $element("img", { "class": "pt", src: "/Icons/Sigma/ExtFile_32X32_Standard.png" })
                    )
                ),
                $element("div", { "class": "fu-fileupload-filename-container" }).append(
                    $element("span", { "class": "fu-fileupload-filename", title: fileName }).text(fileName)
                ),
                $element("div", { "class": "fu-fileupload-progressbar-container" }).append(
                    $element("div", { "class": "fu-fileupload-progressbar ui-progressbar" }).append(
                        $element("div", { "class": "ui-progressbar-value" }).width(0)
                    ),
                    $element("a", { href: "javascript:void(0);", "class": "uploading" }).on("click", cancelUpload)
                )
            )
            .data("status", status);
            return $status;
        },

        _showFileUploadStatus: function ($fileUploadStatus, state) {

            $fileUploadStatus.find(".fu-fileupload-filename-container .fu-file-already-exists-prompt").remove();

            var $prompt = $element("div", { "class": "fu-file-already-exists-prompt" });
            var $statusMessage = $element("span", { "class": "fu-status-message" });
            $prompt.append($statusMessage);
            $fileUploadStatus.find(".fu-fileupload-filename-container").append($prompt);

            var message;
            // file already exists scenario: show Keep/Replace prompt
            if (state.alreadyExists) {

                this._showThumbnail($fileUploadStatus, state);
                var self = this;
                var $keepButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-keep" })
                    .text(this.options.resources.keepButtonText)
                    .click(function () {
                        $fileUploadStatus.removeClass().addClass(self.options.statusCancelledCss);
                        self._showFileUploadStatus($fileUploadStatus, { message: self.options.resources.uploadStopped });
                    });

                var $replaceButton = $element("a", { href: "javascript:void(0);", "class": "fu-file-already-exists-prompt-button-replace" })
                    .text(this.options.resources.replaceButtonText)
                    .on('click', function () {
                        var status = $fileUploadStatus.data("status");
                        status.overwrite = true;
                        self._submitResource(status);
                        $prompt.remove();
                    });

                $prompt.append($keepButton, $replaceButton);
                message = this.options.resources.fileAlreadyExists;
            }
            else {
                message = state.message;
            }
            $statusMessage.text(message);

            $fileUploadStatus.find(".fu-fileupload-progressbar-container").attr("title", message);
        },

        _add: function (e, data) {
            if (!this._$fileUploadStatusesContainer.is(':visible')) {
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
            }

            var count = data.originalFiles.length;
            if (!(typeof this.options.maxFiles === "undefined" || this.options.maxFiles === 0) && this.options.maxFiles < count) {
                if (data.originalFiles[0] === data.files[0]) {
                    alert(this.options.resources.tooManyFiles);
                }
                return;
            }

            if (data.originalFiles.slice(-1)[0] === data.files[0]) {
                // last file in the list
                for (var i = 0; i < count; i++) {
                    if (!this._isValidExtension(data.originalFiles[i].name, this.options.extensions)) {
                        $.dnnAlert({ title: this.options.resources.errorDialogTitle || "Error", text: this.options.resources.invalidFileExtensions });
                        break;
                    }
                }
            }

            if (!this._isValidExtension(data.files[0].name, this.options.extensions)) {
                return;
            }

            var message;

            // Empty file upload does not be supported in IE10
            if (data.files[0].size == 0 && $.browser.msie && $.browser.version == "10.0") {
                message = this.options.resources.emptyFileUpload;
            }

            if (typeof message === "undefined" && this.options.maxFileSize && data.files[0].size > this.options.maxFileSize) {
                message = this.options.resources.fileIsTooLarge;
            }

            var statusData = { fileName: data.files[0].name, data: data };
            var $fileUploadStatus = this._getInitializedStatusElement(statusData);

            if (message) {
                this._showFileUploadStatus($fileUploadStatus, { message: message });
                $fileUploadStatus.removeClass().addClass(this.options.statusErrorCss);
                return;
            }

            setTimeout(function () { data.submit(); }, 25);
        },

        _getInitializedStatusElement: function(data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.fileName);
            if (!$fileUploadStatus.length) {
                $fileUploadStatus = this._createFileUploadStatusElement(data);
                this._$fileUploadStatuses.prepend($fileUploadStatus);
                this._$fileUploadStatusesContainer.show().jScrollbar("update");
            }
            this._setProgress($fileUploadStatus, 0);
            return $fileUploadStatus;
        },

        _submitResource: function(status) {
            if (status.data) {
                this._submitFile(status);
            }
            else {
                this._submitUrl(status);
            }
        },

        _submitFile: function(status) {
            var data = status.data;
            data.uploadedBytes = 0;
            data.data = null;
            data.submit();
        },

        _submit: function (e, data) {
            var statusData = this._getFileUploadStatusElement(data.files[0].name).data("status");
            data.formData = {
                folder: this._selectedPath(),
                filter: "",
                extract: this._extract(),
                overwrite: statusData.overwrite
            };
            return true;
        },

        _progress: function(e, data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            if (data.formData.extract) {
                if ($fileUploadStatus.find('.fu-dialog-fileupload-extracting').length === 0) {
                    $fileUploadStatus.find('.fu-fileupload-filename').append(
                        $element("span", { "class": "fu-dialog-fileupload-extracting" }).text(" - " + this.options.resources.decompressingFile));
                }
                return;
            }
            var progress = parseInt(data.loaded / data.total * 100, 10);
            if (progress < 100) {
                this._setProgress($fileUploadStatus, progress);
            }
        },

        _done: function (e, data) {
            this._processResponse(data.files[0].name, data.result);
        },

        _processResponse: function (fileName, response) {
            var $fileUploadStatus = this._getFileUploadStatusElement(fileName);
            var result = this._getFileUploadResult(response);
            if (result.message) {
                if (!result.alreadyExists) {
                    $fileUploadStatus.removeClass().addClass(this.options.statusErrorCss);
                }
                this._showFileUploadStatus($fileUploadStatus, result);
                return;
            }
            this._showThumbnail($fileUploadStatus, result);
            $fileUploadStatus.data("status").overwrite = false;
            this._setProgress($fileUploadStatus, 100);
            this._showFileUploadStatus($fileUploadStatus, { message: this.options.resources.fileUploaded });
            $fileUploadStatus.removeClass().addClass(this.options.statusUploadedCss);
        },

        _showThumbnail: function ($fileUploadStatus, result) {
            if (this._isValidExtension(result.path, [".bmp", ".gif", ".png", ".jpg", ".jpeg", ".tiff"])) {
                var img = $fileUploadStatus[0].firstChild.firstChild.firstChild;
                $(img).prop("src", result.path).removeClass().addClass(result.orientation === 1 ? "pt" : "ls");
            }
        },

        _fail: function(e, data) {
            var $fileUploadStatus = this._getFileUploadStatusElement(data.files[0].name);
            $fileUploadStatus.removeClass().addClass(this.options.statusErrorCss);
            var message = data.errorThrown === "abort" ? this.options.resources.fileUploadCancelled : this.options.resources.fileUploadFailed;
            this._showFileUploadStatus($fileUploadStatus, { message: message });
        },

        _dragover: function (e, data) {
            this._$dragAndDropArea.addClass("dragover");
        },

        _drop: function (e, data) {
            this._$dragAndDropArea.removeClass("dragover");
        },

        _initFileUploadFromLocal: function () {

            this._$inputFileControl.fileupload({
                url: this._uploadFromLocalUrl(),
                beforeSend: $.dnnSF().setModuleHeaders,
                dropZone: this._$dragAndDropArea,
                sequentialUpload: true,
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

        _getFileUploadResult: function(response) {
            var result;
            try {
                if (!suportAjaxUpload()) {
                    result = JSON.parse($("pre", response).html());
                }
                else {
                    result = JSON.parse(response);
                }
            }
            catch (e) {
                return null;
            }
            return result;
        },

        _getFileUploadStatusElement: function (fileName) {
            return this._$fileUploadStatuses.children().filter(function (index) { return $(this).data("status").fileName == fileName; });
        },

        _setProgress: function ($fileUploadStatus, percent) {

            $fileUploadStatus.find(".fu-fileupload-progressbar > div").css('width', percent + '%');

            if (percent === 0) {
                $fileUploadStatus.find('.fu-fileupload-progressbar').addClass('indeterminate-progress');
            }

            if (percent === 100) {
                $fileUploadStatus.find('.fu-fileupload-progressbar').removeClass('indeterminate-progress');
                $fileUploadStatus.find('.fu-dialog-fileupload-extracting').remove();
            }
        },

        _onCloseDialog: function () {
            this._isShown = false;
        },

        _ensureDialog: function () {
            if (this.$element) {
                return;
            }

            this.$element = this._createLayout();

            if (this._isValidExtension(".zip", this.options.extensions)) {
                this._$decompressOption.show();
            }
            else {
                this._$decompressOption.hide();
            }

            this._$buttonGroup = this.$element.find(".fu-dialog-content-header ul.dnnButtonGroup");
            this._$fileUploadStatusesContainer = this.$element.find('.fu-fileupload-statuses-container').hide().jScrollbar();
            this._$fileUploadStatuses = this._$fileUploadStatusesContainer.find('.fu-fileupload-statuses').empty();
            this._$dragAndDropArea = this.$element.find('.fu-dialog-drag-and-drop-area');
            this._$inputFileControl = $element("input", { type: 'file', name: 'postfile', multiple: typeof this.options.maxFiles === "undefined" || this.options.maxFiles === 0 || this.options.maxFiles > 1, "data-text": this.options.resources.dragAndDropAreaTitle });
            this._$extract = this.$element.find("." + "fu-dialog-content-header").find("input");

            this._$inputFileControl.appendTo(this._$dragAndDropArea.find('.fu-dialog-drag-and-drop-area-message')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._initFileUploadFromLocal();
        },

        _uploadFromLocalUrl: function() {
            var serviceUrl = $.dnnSF(this.options.moduleId).getServiceRoot(this.options.serviceRoot);
            var url = serviceUrl + this.options.fileUploadMethod;
            var webResource = new dnn.WebResourceUrl(url);
            if (!suportAjaxUpload()) {
                var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
                webResource.parameters().set("__RequestVerificationToken", antiForgeryToken);
            }
            return webResource.toPathAndQuery();
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
                title: this.options.resources.title,
                resizable: false,
                width: this.options.width,
                height: this.options.height,
                close: $.proxy(this._onCloseDialog, this),
                buttons: [ {
                    text: this.options.resources.closeButtonText,
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
        fileUploadMethod: "FileUpload/UploadFromLocal",
        maxFiles: 1,
        extensions: [".png", ".jpeg", ".jpg", ".bmp"]
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

