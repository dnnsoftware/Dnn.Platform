// IE8 doesn't like using var dnnModule = dnnModule || {}
if (typeof dnnModule === "undefined" || dnnModule === null) { dnnModule = {}; };

dnnModule.DigitalAssetsFileUpload = function ($, servicesFramework, settings, resources, refreshFolder, getCurrentFolderPath) {
    
    function supportAjaxUpload() {
        var xhr = new XMLHttpRequest;
        return !!(xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
    }

    function zipConfirmation($element, data) {

        var dialogText = resources.zipConfirmationText.replace('[FILE]', data.files[0].name);

        $("<div class='dnnDialog'></div>").html(dialogText).data(data).dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            width: 400,
            height: 220,
            resizable: false,
            title: resources.uploadFilesTitle,
            buttons:
                [
                    {
                        id: "keepCompressed_button",
                        text: resources.keepCompressedText,
                        "class": "dnnSecondaryAction",
                        click: function () {
                            $element.attr("data-extract", "false");
                            $(this).data().submit();
                            $(this).dialog("close");
                        }
                    },
                    {
                        id: "expandFile_button",
                        text: resources.expandFileText,
                        "class": "dnnSecondaryAction",
                        click: function () {
                            $element.attr("data-extract", "true");
                            $(this).data().submit();
                            $(this).dialog("close");
                        }
                    }
                ]
        });
    }

    function doesBrowserSupportDragDrop() {
        return ('draggable' in document.createElement('span'));
    }

    function disposeFileUpload() {
        $('#dnnModuleDigitalAssetsUploadFileDialogInput').fileupload("destroy");
        $('#dnnModuleDigitalAssetsUploadFileDialogInput').parent().remove();
    }

    function initFileUpload() {

        $('#dnnModuleDigitalAssetsUploadFileResultZone').empty();
        $("#dnnModuleDigitalAssetsUploadFileExternalResultZone").hide();

        if (!doesBrowserSupportDragDrop()) {
            $("#dnnModuleDigitalAssetsUploadFileDropZone").remove();
            $("#dnnModuleDigitalAssetsUploadFileDragDropInfo").remove();
        }

        var $closeDialog = $("#dnnModuleDigitalAssetsUploadFileDialogClose").unbind("click").click(function () {
            $("#dnnModuleDigitalAssetsUploadFileModal").dialog('close');
        });

        var $inputFile = $("<input id='dnnModuleDigitalAssetsUploadFileDialogInput' type='file' name='postfile' multiple data-text='" + resources.chooseFileText + "' />");

        $inputFile.insertBefore($closeDialog);
        $inputFile.dnnFileInput(
            {
                buttonClass: 'dnnPrimaryAction',
                showSelectedFileNameAsButtonText: false
            });

        var url = servicesFramework.getServiceRoot('internalservices') + 'fileupload/postfile';
        if (!supportAjaxUpload()) {
            var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
            url += '?__RequestVerificationToken=' + antiForgeryToken;
        }

        var $dropZone = $('#dnnModuleDigitalAssetsUploadFileDropZone');

        $inputFile.fileupload({
            url: url,
            beforeSend: servicesFramework.setModuleHeaders,
            dropZone: $dropZone,
            sequentialUpload: false,
            progressInterval: 20,
            add: function (e, data) {
                if (!$("#dnnModuleDigitalAssetsUploadFileExternalResultZone").is(":visible")) {
                    $("#dnnModuleDigitalAssetsUploadFileModal").dialog({ height: 580 });
                    $("#dnnModuleDigitalAssetsUploadFileExternalResultZone").show().jScrollPane();
                }

                // Empty file upload does not be supported in IE10
                if (data.files[0].size == 0 && $.browser.msie && $.browser.version == "10.0") {
                    showErrorMessage(data.files[0].name, resources.fileUploadEmptyFileUploadIsNotSupported);
                    return;
                }

                if (data.files[0].size > settings.maxFileUploadSize) {
                    var message = resources.maxFileUploadSizeErrorText.replace("[MAXFILESIZE]", settings.maxFileUploadSizeHumanReadable);
                    showErrorMessage(data.files[0].name, message);
                    return;
                }

                data.submit();
            },
            submit: function (e, data) {

                var overwrite = 'false';

                var $element = getUploadFileResultZone(data.files[0].name);
                if ($element.length == 0) {

                    $element = getNewUploadFileResultZone(data.files[0].name);
                    $("#dnnModuleDigitalAssetsUploadFileResultZone").append($element);

                    $element.find('.dnnModuleDigitalAssetsUploadFileStatusIcon.uploading').click(function () {
                        if (data.jqXHR) data.jqXHR.abort();
                    });

                    $("#dnnModuleDigitalAssetsUploadFileExternalResultZone").jScrollPane();
                } else {
                    $element.find('.dnnModuleDigitalAssetsUploadFileNotification').hide();
                    $element.find('.dnnModuleDigitalAssetsUploadFileNotification').empty();
                    initProgressBar($element);

                    overwrite = $element.attr("data-fileoverwrite");
                }

                var extract = $element.attr("data-extract");
                var extension = data.files[0].name.substring(data.files[0].name.lastIndexOf('.') + 1);
                if (extension == 'zip' && extract == null) {
                    zipConfirmation($element, data);
                    return false;
                }

                data.formData = {
                    folder: getCurrentFolderPath(),
                    filter: '',
                    overwrite: overwrite,
                    isHostMenu: settings.isHostMenu,
                    extract: extract
                };

                return true;
            },
            progress: function (e, data) {
                var $element = getUploadFileResultZone(data.files[0].name);

                if (data.formData.extract == "true") {
                    if ($element.find('.dnnModuleDigitalAssetsUploadingExtracting').length == 0) {
                        $element.find('.dnnModuleDigitalAssetsUploadFileFileName')
                            .append("<span class='dnnModuleDigitalAssetsUploadingExtracting'> - "
                                + resources.uploadingExtracting + "</span>");
                    }
                    setProgressBarProgress($element);
                    return;
                }

                var progress = parseInt(data.loaded / data.total * 100, 10);
                if (progress < 100) {
                    setProgressBarProgress($element, progress);
                }
            },
            done: function (e, data) {
                var $element = getUploadFileResultZone(data.files[0].name);

                var error = getFileUploadError(data);
                if (error) {
                    handleFileUploadError($element, error, data);
                    return;
                }

                setProgressBarProgress($element, 100);
                $element.attr("data-fileoverwrite", "false");
            },
            fail: function (e, data) {
                var $element = getUploadFileResultZone(data.files[0].name);

                if (data.errorThrown === 'abort') {
                    setNotification($element, resources.fileUploadStoppedText);
                    showFileNotification($element);
                    return;
                }

                setNotification($element, "<span class='dnnModuleDigitalAssetsErrorMessage'>" + resources.fileUploadErrorOccurredText + "</span>");
                showFileNotification($element);
                return;
            },
            dragover: function () {
                $dropZone.addClass("dragover");
            },
            drop: function () {
                $dropZone.removeClass("dragover");
            }
        });

        $dropZone.on('dragleave', function () {
            $(this).removeClass("dragover");
        });
    }

    function getFileUploadError(data) {
        var error;
        try {
            if (!supportAjaxUpload()) {
                error = JSON.parse($("pre", data.result).html());
            } else {
                error = JSON.parse(data.result);
            }
        } catch (e) {
            return null;
        }

        if (!error.Message) return null;

        return error;
    }

    function showErrorMessage(filename, message) {
        var $element = getUploadFileResultZone(filename);
        if ($element.length == 0) {
            $element = getNewUploadFileResultZone(filename);
            $("#dnnModuleDigitalAssetsUploadFileResultZone").append($element);
            $("#dnnModuleDigitalAssetsUploadFileExternalResultZone").jScrollPane();
        }
        setNotification($element, "<span class='dnnModuleDigitalAssetsErrorMessage'>" + message + "</span>");
        showFileNotification($element);
    }

    function handleFileUploadError($element, error, data) {

        // File already Exists Scenario
        if (error.AlreadyExists) {
            var replaceButton = $('<a class="dnnModuleDigitalAssetsUploadFileReplaceFile dnnModuleDigitalAssetsUploadFileAction">' + resources.replaceText + '</a>')
                .on('click', function () {
                    $(this).closest('.dnnModuleDigitalAssetsUploadFileFile').attr("data-fileoverwrite", "true");
                    $(this).data().uploadedBytes = 0;
                    $(this).data().data = null;
                    $(this).data().submit();
                });

            setNotification($element, resources.fileUploadAlreadyExistsText +
                "<span class='dnnModuleDigitalAssetsUploadFileActions'><a class='dnnModuleDigitalAssetsUploadFileKeepFile dnnModuleDigitalAssetsUploadFileAction'>" +
                resources.keepText + "</a></span>");

            $element.find('.dnnModuleDigitalAssetsUploadFileActions').prepend(replaceButton.clone(true).data(data));

            $element.find('.dnnModuleDigitalAssetsUploadFileNotification .dnnModuleDigitalAssetsUploadFileKeepFile').click(function () {
                $element.find('.dnnModuleDigitalAssetsUploadFileActions').remove();
                setNotification($element, resources.fileUploadStoppedText);
            });

        } else {
            setNotification($element, "<span class='dnnModuleDigitalAssetsErrorMessage'>" + error.Message + "</span>");
        }

        showFileNotification($element);

        $("#dnnModuleDigitalAssetsUploadFileExternalResultZone").jScrollPane();
    }

    function getNewUploadFileResultZone(id) {
        return $("<div class='dnnModuleDigitalAssetsUploadFileFile' data-fileoverwrite='false' data-filename='" + id + "'>" +
                "<span class='dnnModuleDigitalAssetsUploadFileFileName'>" + id + "</span>" +
                "<div class='dnnModuleDigitalAssetsUploadFileProgress'>" +
                "<div class='dnnModuleDigitalAssetsUploadFileProgressBar ui-progressbar'><div class='ui-progressbar-value' style='width: 0%;' /></div>" +
                "<div class='dnnModuleDigitalAssetsUploadFileStatusIcon uploading'/></div>" +
                "<div class='dnnModuleDigitalAssetsUploadFileNotification'/>");
    }

    function getUploadFileResultZone(id) {
        return $("#dnnModuleDigitalAssetsUploadFileResultZone div[data-filename='" + id + "']");
    }

    function initProgressBar($element) {
        $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar>div').css('width', '0%');
        $element.find('.dnnModuleDigitalAssetsUploadFileStatusIcon').removeClass('finished').addClass('uploading');
        $element.find('.dnnModuleDigitalAssetsUploadFileProgress').show();
    }

    function setProgressBarProgress($element, progress) {
        $element.find(".dnnModuleDigitalAssetsUploadFileProgress").show();

        if (!progress) {
            $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar').addClass('indeterminate-progress');
            $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar>div').css('width', '100%');
            return;
        }

        if (progress < 100) {
            $element.find(".dnnModuleDigitalAssetsUploadFileProgressBar>div").css('width', progress + '%');
            return;
        }

        $element.find('.dnnModuleDigitalAssetsUploadFileStatusIcon').removeClass('uploading').addClass('finished');
        $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar.indeterminate-progress').removeClass('indeterminate-progress');
        $element.find('.dnnModuleDigitalAssetsUploadingExtracting').remove();
        $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar>div').css('width', '100%');
    }

    function setNotification($element, content) {
        var notification = $element.find('.dnnModuleDigitalAssetsUploadFileNotification');
        notification.empty();
        notification.html(content);
    }

    function showFileNotification($element) {

        $element.find('.dnnModuleDigitalAssetsUploadFileNotification').show();
        $element.find('.dnnModuleDigitalAssetsUploadFileProgressBar>div').css('width', '0%');
        $element.find('.dnnModuleDigitalAssetsUploadFileProgress').hide();
    }

    function uploadFiles() {
        $("#dnnModuleDigitalAssetsUploadFileModal").dialog({
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            title: resources.uploadFilesTitle,
            resizable: false,
            width: 680,
            height: 320,
            close: function () {
                refreshFolder();
                disposeFileUpload();
            }
        });

        initFileUpload();
    }

    return {
        uploadFiles: uploadFiles
    };
}