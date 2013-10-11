; if (typeof window.dnn === "undefined" || window.dnn === null) { window.dnn = {}; }; //var dnn = dnn || {};

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved

(function ($, window, document, undefined) {
    "use strict";

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


            var $parent = $(this.element).parent();
            $parent.empty();
            this.$element = this._createLayout();
            $parent.append(this.$element);


            //this.$element = this.element ? $(this.element) : this._createLayout();

            this._$buttonGroup = this.$element.find(".dnnFileUploadHead  ul.dnnButtonGroup");
            this._$uploadResultPanel = this.$element.find('.dnnFileUploadExternalResultZone');
            this._$dialogCloseBtn = this.$element.find('.dnnFileUploadDialogClose');
            this._$dropFileZone = this.$element.find('.dnnFileUploadDropZone');
            this._$inputFileControl = $("<input type='file' name='postfile' multiple data-text='DRAG FILES HERE OR CLICK TO BROWSE' />");
            this._$decompressZipCheckbox = this.$element.find('input.normalCheckbox');
            
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

            this._$uploadResultPanel.hide().find('.dnnFileUploadResultZone').empty();
            this._$inputFileControl.appendTo(this._$dropFileZone.find('.dnnDropFileMessage')).dnnFileInput(
                {
                    buttonClass: 'normalClass',
                    showSelectedFileNameAsButtonText: false
                });

            this._$buttonGroup.find('a').on('click', function () {
                self._$buttonGroup.find('li').removeClass('active');
                $(this).parent().addClass('active');
                self._$uploadResultPanel.hide();
                var uploadMethod = $(this).attr('href').replace('#', '');
                self.$element.find('.dnnFileUploadContainer').hide();
                self.$element.find('.' + uploadMethod).show();
                return false;
            });

            this._initUploadFileFromLocal();
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
            var dialog = $("<div id='fu-dialog' tabindex='-1' class='fu-container' role='dialog'/>")
                .append($("<p class='dnnFileUploadFileInfo'/>")
                    .append($("<span>Use one of the methods below to upload files</span>")))
                .append($("<div class='dnnFileUploadPanel'/>")
                    .append($("<div class='dnnFileUploadHead'/>")
                        .append($("<div class='dnnLeft'/>")
                            .append($("<ul class='dnnButtonGroup'/>")
                                .append($("<li class='active'/>")
                                    .append($("<a href='javascript:void(0);'>Upload File</a>")))
                                .append($("<li/>")
                                    .append($("<a href='javascript:void(0);'>From Web</a>"))))
                            .append($("<span/>")
                                .append($("<input type='checkbox' id='" + checkBoxId + "' class='normalCheckbox'/>"))
                                .append($("<label for='" + checkBoxId + "' class='dnnFileUploadDecompressZipInfo'>Decompress Zip Files</label>"))))
                        .append($("<div class='fu-folder-picker-container dnnRight'/>")
                            .append($("<label>Upload To:</label>"))
                            .append(this._folderPicker.$element.addClass("dnnLeftComboBox")))
                        .append($("<div class='dnnClear'/>")))

                    .append($("<div class='dnnFileUploadFromLocal dnnFileUploadContainer'/>")
                        .append($("<div class='dnnFileUploadDropZone'/>")
                            .append($("<div class='dnnDropFileMessage'/>"))))

                    .append($("<div style='display: none' class='dnnFileUploadFromWeb dnnFileUploadContainer'/>")
                        .append($("<table class='dnnFileUploadWebInput'/>")
                            .append($("<tbody/>")
                                .append($("<tr/>")
                                    .append($("<td/>")
                                        .append($("<div class='txtWrapper'/>")
                                            .append($("<input type='text'/>"))))
                                    .append($("<td/>")
                                        .append($("<a href='javascript:void(0);' class='dnnSecondaryAction'>Load</a>")))))))

                    .append($("<div style='display: none' class='dnnFileUploadExternalResultZone'/>")
                        .append($("<div class='dnnFileUploadResultZone'/>"))));

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
                        self._$uploadResultPanel.show().jScrollPane();
                    }
                    //TODO: do some check
                    data.submit();
                },
                submit: function (e, data) {
                    var fileResultZone = self._getUploadFileResultZone(data.files[0].name);
                    if (!fileResultZone.length) {
                        fileResultZone = self._getNewUploadFileResultZone(data.files[0].name);
                        self._$uploadResultPanel.find('.dnnFileUploadResultZone').append(fileResultZone);
                        fileResultZone.find('.dnnFileUploadFileStatusIcon.uploading').on('click', function() {
                            if (data.jqXHR) data.jqXHR.abort();
                        });

                        self._$uploadResultPanel.show().jScrollPane();
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
                            fileResultZone.find('.dnnFileUploadFileName')
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
        
        _getNewUploadFileResultZone: function(fileName) {
            return $('<div class="dnnFileUploadFile" data-filename="' + fileName + '">' +
                        '<div class="dnnFileUploadFileName">' + fileName + '</div>' +
                        '<div class="dnnFileUploadFileProgress">' +
                            '<div class="dnnFileUploadFileProgressBar ui-progressbar">' + 
                                '<div class="ui-progressbar-value" style="width: 0%;"></div>' +
                            '</div>' +
                            '<div class="dnnFileUploadFileStatusIcon uploading"></div>' +
                        '</div>' +
                    '</div>');
        },

        _getUploadFileResultZone: function(fileName) {
            return this._$uploadResultPanel.find('div[data-filename="' + fileName + '"]');
        },
        
        _initProgressBar: function(fileResultZone) {
            fileResultZone.find('.dnnFileUploadFileProgressBar > div').css('width', '0%');
            fileResultZone.find('.dnnFileUploadFileStatusIcon').removeClass('finished').addClass('uploading');
            fileResultZone.find('.dnnFileUploadFileProgress').show();
        },
        
        _setProgressBar: function(fileResultZone, progress) {
            fileResultZone.find(".dnnFileUploadFileProgress").show();

            if (!progress) {
                fileResultZone.find('.dnnFileUploadFileProgressBar').addClass('indeterminate-progress');
                fileResultZone.find('.dnnFileUploadFileProgressBar > div').css('width', '100%');
                return;
            }

            if (progress < 100) {
                fileResultZone.find(".dnnFileUploadFileProgressBar > div").css('width', progress + '%');
                return;
            }

            fileResultZone.find('.dnnFileUploadFileStatusIcon').removeClass('uploading').addClass('finished');
            fileResultZone.find('.dnnFileUploadFileProgressBar.indeterminate-progress').removeClass('indeterminate-progress');
            fileResultZone.find('.dnnFileUploadExtracting').remove();
            fileResultZone.find('.dnnFileUploadFileProgressBar > div').css('width', '100%');
        }

    };

    FileUpload._defaults = {};

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

