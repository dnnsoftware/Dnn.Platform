// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

﻿(function ($) {
    var dnnImagesUploader = function (options) {
        this.options = options;
        this.container = null;
        this.init();
    };

    dnnImagesUploader.prototype = {
        constructor: dnnImagesUploader,
        init: function () {
            this.options = $.extend({}, dnn.ImagesUploaderResources, this.options);
            if (typeof FileReader == "undefined") {
                return;
            }

            this._uploadedFiles = [];
            this._attachEventListeners();
        },
        _attachEventListeners: function() {
            $(document.body).on('dragenter', $.proxy(this._onDragEnterHandler, this))
                            .on('dragover', $.proxy(this._onDragOverHandler, this))
                            .on('dragleave', $.proxy(this._onDragLeaveHandler, this))
                            .on('drop', $.proxy(this._onDragDropHandler, this));
        },
        _findContainer: function (element) {
            //ignore on redactor
            if ($(element).closest('.redactor-editor').length > 0) {
                return [];
            }
            return $(element).closest(this.options.selector);
        },
        _onDragEnterHandler: function (e) {
            e.stopPropagation();
            e.preventDefault();

            if (this._findContainer(e.target).length == 0) {
                if (this.container != null) {
                    this.container.removeClass('images-dragover');
                    this.container = null;
                }

                return;
            }

            if (this.container != null && this._findContainer(e.target)[0] != this.container[0]) {
                this.container.removeClass('images-dragover');
            }

            this.container = this._findContainer(e.target);
            this.container.addClass('images-dragover');
        },
        _onDragOverHandler: function (e) {
            e.stopPropagation();
            e.preventDefault();
        },
        _onDragLeaveHandler: function (e) {          
            e.stopImmediatePropagation();
            e.preventDefault();           
        },
        _onDragDropHandler: function (e) {
            e.stopPropagation();
            e.preventDefault();

            var files = typeof e.originalEvent.dataTransfer != 'undefined' ? e.originalEvent.dataTransfer.files : null;
            if (this._findContainer(e.target).length == 0 || !files || files.length == 0) {
                if (this.container != null) {
                    this.container.removeClass('images-dragover');
                    this.container = null;
                }

                return;
            }

            if (this._processing) {
                $.dnnAlert({
                    title: this.options.imageProgressTitle,
                    text: this.options.imageProgress
                });
                return;
            }

            this.container = this._findContainer(e.target);
            
            var badFiles = this._isValidFiles(files);
            var errMessage = '';
            if (badFiles.hasOwnProperty("extensions"))
                errMessage = badFiles.extensions + ' - ' + this.options.invalidFileMessage + '\n';  
            if (badFiles.hasOwnProperty("bigsize"))
                errMessage = errMessage + badFiles.bigsize + ' - '+ this.options.fileTooLarge + '\n';
            if (errMessage.length > 0) {

                if (this.goodFiles.length > 0) {
                    var handler = this;
                    var confirmQ = this.options.confirmQuestion.replace('{0}', this.goodFiles.length);
                    var confirmT = this.options.fileAlertConfirmTitle.replace('{0}', files.length - this.goodFiles.length);
                    var _opts = new Object();
                    _opts.title = confirmT;
                    _opts.text = errMessage + '<br/><p/><b>' + confirmQ + '</b>';
                    _opts.callbackTrue = function(data) {
                        handler._saveFiles(handler.goodFiles);
                    };
                    _opts.callbackFalse = function (data) {
                        handler.container.removeClass('images-dragover');
                        handler.container = null;
                    };
                    
                    $.dnnConfirm(_opts);
                    
                } else {
                    $.dnnAlert({
                        title: this.options.fileAlertConfirmTitle.replace('{0}', files.length),
                        text: errMessage
                    });
                    this.container.removeClass('images-dragover');
                    this.container = null;
                    return;
                }
            }
            else
                this._saveFiles(this.goodFiles);
        },
        
        _isValidFiles: function(files) {
            
            var badFiles = new Object();
            this.goodFiles = [];
            var loops = files.length - 1;
            for (var i = loops; i >= 0; i--) {
                var thisfile = true;
                var extension = files[i].name.split('.').pop().toLowerCase();
                if (this.options.imageExtensionsAllowed.indexOf(extension) == -1) {
                    if (!badFiles.hasOwnProperty("extensions")) {
                        badFiles.extensions = new Array(files[i].name);
                    } else badFiles.extensions.push(files[i].name);
                    thisfile = false;
                }
                if (files[i].size > this.options.maxFileSize) {
                    if (!badFiles.hasOwnProperty("bigsize")) {
                        badFiles.bigsize = new Array(files[i].name);
                    }else badFiles.bigsize.push(files[i].name);
                    thisfile = false;
                }
                if (thisfile)
                    this.goodFiles.push(files[i]);
            }
            return badFiles;
        },
        
        _saveFiles: function (files) {
            this._fileQueue = [];
            this._processing = true;
            this._uploadedFiles = [];

            for (var i = 0; i < files.length; i++) {
                this._fileQueue.push(files[i]);
            }

            if (this._fileQueue.length > 0) {
                this._postFile();
            }
        },
        _postFile: function () {
            var handler = this;
            var file = this._fileQueue[0];
            var data = new FormData();
            data.append('file', file);

            var serviceUrl = this._getUploadServiceUrl();
            $.ajax({
                url: serviceUrl,
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                beforeSend: this._service.setModuleHeaders,
                type: 'POST',
                success: function (returnData) {
                    setTimeout(function () {
                        handler._saveFileComplete(eval(returnData)[0].filelink); //run after complete event executed
                    }, 0);
                },
                error: function (xhr) {
                    setTimeout(function() {
                        $.dnnAlert({
                            title: handler.options.fileUploadFailed,
                            text: file.name
                        });
                        handler._saveFileComplete('');
                    }, 0);
                }
            });
        },
        _saveFileComplete: function (filePath) {
            if (filePath != '') {
                this._uploadedFiles.push(filePath);
            }

            this._fileQueue.splice(0, 1);
            if (this._fileQueue.length > 0) {
                this._postFile();
                return;
            }
            
            if (this._fileQueue.length == 0) {
                var imageContent = '';
                for (var i = 0; i < this._uploadedFiles.length; i++) {
                    imageContent += '<p><img src="' + this._uploadedFiles[i] + '" style="max-width: 100%;" /></p>';
                }
                this.container.removeClass('images-dragover');
                this._appendContent(imageContent);
            }

        },
        _appendContent: function (content) {
            if (this._isPane()) {
                this._addNewModule(content);
            } else {
                var moduleId = this._findModuleId();
                this._updateModule(moduleId, content);
            }
        },
        _addNewModule: function (content) {
            var handler = this;
            var paneName = this._findPaneName();
            var serviceUrl = this._getHtmlServiceUrl('CreateNewModule');
            var data = {
                PaneName: paneName,
                Content: content
            };

            $.ajax({
                url: serviceUrl,
                data: data,
                cache: false,
                beforeSend: this._service.setModuleHeaders,
                type: 'POST',
                success: function (returnData) {
                    handler._updateModuleComplete(paneName, returnData.ModuleId);
                }
            });
        },
        _updateModule: function(moduleId, content) {
            var handler = this;
            var paneName = this._findPaneName();
            var serviceUrl = this._getHtmlServiceUrl('UpdateModuleContent');
            var data = {
                ModuleId: moduleId,
                Content: content
            };

            $.ajax({
                url: serviceUrl,
                data: data,
                cache: false,
                beforeSend: this._service.setModuleHeaders,
                type: 'POST',
                success: function (returnData) {
                    handler._updateModuleComplete(paneName, moduleId);
                }
            });
        },
        _updateModuleComplete: function (paneName, moduleId) {
            if (typeof dnn.ContentEditorManager == "undefined") {
                location.reload();
            }
            var handler = this;
            var dialog = dnn.ContentEditorManager.getModuleDialog();
            dialog.setModuleId(moduleId);
            dialog.refreshPane(paneName, '', function() {
                var module = $('div.DnnModule-' + moduleId);
                module.trigger('editmodule');
                handler._processing = false;
                handler.container = null;
                dialog.setModuleId(-1);
                dnn.ContentEditorManager.triggerChangeOnPageContentEvent();
            });
        },
        _isPane: function() {
            return this.container.hasClass('EvoqEmptyPane');
        },
        _findModuleId: function() {
            return this.container.children('a').eq(0).attr('name');
        },
        _findPaneName: function () {
            if (this._isPane()) {
                return this.container.attr('id').replace('dnn_', '');
            } else {
                return this.container.parent().attr('id').replace('dnn_', '');
            }
        },
        _getServiceUrl: function (module, service, method) {
            if (typeof this._service == "undefined") {
                this._service = $.dnnSF().ServicesFramework();
            }
            return this._service.getServiceRoot(module) + service + "/" + method;
        },
        _getUploadServiceUrl: function () {
            return this._getServiceUrl("DNNCorp/EvoqLibrary", "Redactor", "PostImages");
        },
        _getHtmlServiceUrl: function (method) {
            return this._getServiceUrl("HtmlPro", "HtmlTextPro", method);
        }
    };

    var initImagesUploader = function() {
        var uploader = new dnnImagesUploader({
            selector: '.EvoqEmptyPane, .DnnModule-DNN_HTML'
        });
    };

    $(document).ready(function() {
        initImagesUploader();
    });
}(jQuery));