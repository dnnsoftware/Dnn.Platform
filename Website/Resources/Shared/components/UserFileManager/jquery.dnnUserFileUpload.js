/*globals jQuery */
(function ($) {
    "use strict";

    var supportAjaxUpload = function () {
        var xhr = new XMLHttpRequest;
        return !!(xhr && ('upload' in xhr) && ('onprogress' in xhr.upload));
    };

    $.fn.dnnUserFileUpload = function (options) {
        var opts = $.extend({}, $.fn.dnnUserFileUpload.defaultOptions, options),
            $wrap = $(this),
            $fileUploadWrapperSelector = $(opts.fileUploadWrapperSelector);

        function displayError(message) {
            $(opts.progressContextSelector).hide('fade');
            var $message = $(opts.errorMessageSelector);
            $message.text(message).show();
            setTimeout((function () { $message.hide('fade'); }), 4000);
        }

        // error response 
        $fileUploadWrapperSelector.bind('fileuploadfail', function (e, data) {
            opts.complete(data);
            displayError(opts.serverErrorMessage);
        });

        // success response
        $fileUploadWrapperSelector.bind('fileuploaddone', function (e, data) {
            opts.complete(data);
            var result;
            if (data.result[0].body) {
                result = $.parseJSON(data.result[0].body.innerText)[0];
            }
            else {
                result = data.result[0];
            }

            if (result.success) {
                opts.callback(result);
            }
            else {
                displayError(result.message);
            }
        });

        var url = opts.addImageServiceUrl;
        if (!supportAjaxUpload()) {
            var antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
            url += '?__RequestVerificationToken=' + antiForgeryToken;
        }

        var $fileupload = $fileUploadWrapperSelector;
        if (!$fileupload.data('loaded')) {

            $fileupload.fileupload({
                dataType: 'json',
                url: url,
                maxFileSize: opts.maxFileSize,
                beforeSend: opts.beforeSend,
                add: function (e, data) {
                    data.context = $(opts.progressContextSelector);
                    data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                    data.context.show('fade');
                    data.submit();
                },
                progress: function (e, data) {
                    if (data.context) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                    }
                },
                done: function (e, data) {
                    if (data.context) {
                        data.context.find(opts.progressBarSelector).css('width', '100%').find('span').html('100%');
                        setTimeout((function () { data.context.hide('fade'); }), 2000);
                    }
                }
            }).data('loaded', true);

        }

        $wrap.show();

    };

    $.fn.dnnUserFileUpload.defaultOptions = {
        fileUploadWrapperSelector: '.fileUploadArea', // wrapper element for the main file upload content area
        addImageServiceUrl: '/DesktopModules/Journal/API/FileUpload/UploadFile', // post files here
        progressContextSelector: '.progress_context', // wrapper element for the progress area
        progressFileNameSelector: '.upload_file_name', // element to update file name text w/ during upload
        progressBarSelector: '.progress-bar div', // the actual progress bar element itself, its width will be expanded dynamically
        errorMessageSelector: '.fileupload-error', // the element to display the error message, its text will be updated dynamically
        serverErrorMessage: 'Unexpected error. This generally happens when the file is too large.', // error message when server returns a 500, 404 or the like
        beforeSend: null, //method to set the request headers should be an instance on servicesFramework.setModuleHeaders
        callback: function (result) {
            // function called after the upload is successful. Supplied with result object representing the file.
            // key properties: name, extension, type, size, url, message, file_id
            // e.g. console.log('file id: ' + result.file_id + ' path: ' + result.url);
        },
        complete: function (data) {
            // A function to be called when the request finishes (after success and error callbacks are executed)
        }
    };
} (jQuery));