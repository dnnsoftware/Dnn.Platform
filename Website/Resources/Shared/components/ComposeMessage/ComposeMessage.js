(function ($) {
    $.fn.dnnComposeMessage = function (options) {
        var opts = $.extend({}, $.fn.dnnComposeMessage.defaultOptions, options),
            $wrap = $(opts.openTriggerScope),
            html,
            composeMessageDialog,
            canSend = false,
            users = [],
            roles = [],
            attachments = [];

        opts.serviceurlbase = opts.servicesFramework.getServiceRoot('InternalServices') + 'MessagingService/';

        //construct the form
        html = "<fieldset>";
        html += "<div class='dnnFormItem'><div class='dnnLabel'><label for='to'>" + opts.toText + "</label></div><input type='text' id='to' name='to'/></div>";
        html += "<div class='dnnFormItem'><div class='dnnLabel'><label for='subject'>" + opts.subjectText + "</label></div><input type='text' id='subject' name='subject' maxlength='400'/></div>";
        html += "<div class='dnnFormItem'><div class='dnnLabel'><label for='bodytext'>" + opts.messageText + "</label></div><textarea rows='2' cols='20' id='bodytext' name='bodytext'/></div>";

        if (opts.showAttachments) {
            html += "<div class='dnnFormItem'><div class='dnnLabel'<label>" + opts.attachmentsText + "</label></div><div><button type='button' id='fileFromSite' class='dnnTertiaryAction'><span>" + opts.browseText + "</span></button><div class='fileUploadArea'><input id='uploadFileId' type='file' name='files[]' data-text='" + opts.uploadText + "' /></div><div class='messageAttachments'><ul></ul></div></div></div>";
	        html += "<div class='itemUpload'><div class='fileupload-error dnnFormMessage dnnFormValidationSummary' style='display:none;'></div><div class='progress_bar_wrapper'><div class='progress_context' style='margin:10px 0px; display:none;'><div class='upload_file_name' style='margin:5px 0;'></div><div class='progress-bar green'><div style='width:0px;'><span></span> </div></div></div></div></div>";
            html += "<div id='userFileManager'></div>";

            opts.userFileManagerOptions.openTriggerSelector = '#fileFromSite';
	        opts.userFileManagerOptions.attachCallback = attachFile;
        }

        html += "</fieldset>";

        function getWaitTimeForNextMessage() {
            var returnValue = 0; // If the request fails, just return 0
            $.ajax({
                url: opts.serviceurlbase + "WaitTimeForNextMessage",
                async: false,
                cache: false // Important. IE is caching this call, so we need to explicitly set it to false
            }).done(function (data) {
                if (data.Result === "success") {
                    returnValue = data.Value;
                }
            });
            return returnValue;
        };
        function updateSendButtonStatus() {
            var sendButton = composeMessageDialog.dialog("widget").find('.ui-dialog-buttonpane button:first');
            if ((users.length > 0 || roles.length > 0) && composeMessageDialog.find('#subject').val().trim().length > 0 && canSend) {
                sendButton.removeAttr('disabled').removeClass('disabled');
            } else {
                sendButton.attr('disabled', 'disabled').addClass('disabled');
            }
        };
        function displayMessage(placeHolderElement, message) {
            var messageNode = $("<div/>")
                .addClass('dnnFormMessage dnnFormWarning')
                .text(message);

            placeHolderElement.prepend(messageNode);

            messageNode.fadeOut(3000, 'easeInExpo', function () {
                messageNode.remove();
            });
        };
	    function attachFile(file) {
            if ($.inArray(file.id, attachments) === -1) {
                attachments.push(file.id);
                composeMessageDialog.find('.messageAttachments ul').append('<li><a href="#" title="' + file.name + '">' + file.name + '</a><a href="#" class="removeAttachment" title="' + opts.removeText + '"></a></li>');
                composeMessageDialog.find('.messageAttachments li:last-child .removeAttachment').click(function () {
                    var index = $.inArray(file.id, attachments);
                    if (index !== -1) {
                        attachments.splice(index, 1);
                        $(this).parent().remove();
                    }
                    return false;
                });
            }
        };
        $wrap.delegate(opts.openTriggerSelector, 'click', function (e) {

            if (opts.canTrigger && !opts.canTrigger()) {
                return;
            }

            e.preventDefault();
            e.stopPropagation();

            var autoclose,
                messageId = -1;

            // Reset variable values
            canSend = false;
            users = [];
            roles = [];
            attachments = [];

            composeMessageDialog = $("<div class='composeMessageDialog dnnForm dnnClear'/>").html(html).dialog(opts);

	        if (opts.showAttachments && !$wrap.data('fileManagerInitialized')) {
                // we only need to initialize this plugin once, doing so more than once will lead to multiple dialogs.
                // this is because the #userFileManager element is never destroyed when the compose message dialog is closed.
	        	composeMessageDialog.find('#userFileManager').userFileManager(opts.userFileManagerOptions);
		        
                $wrap.data('fileManagerInitialized', true);
            }
	        
            composeMessageDialog.find('.fileUploadArea').dnnUserFileUpload({
				maxFileSize: opts.maxFileSize,
				serverErrorMessage: opts.serverErrorText,
				addImageServiceUrl: opts.servicesFramework.getServiceRoot('CoreMessaging') + 'FileUpload/UploadFile',
				beforeSend: opts.servicesFramework.setModuleHeaders,
				callback: attachFile,
				complete: function() {
					composeMessageDialog.find('.fileUploadArea input:file').data("wrapper").get(0).childNodes[0].nodeValue = opts.uploadText;
				}
			});

	        composeMessageDialog.find('#to').tokenInput(opts.serviceurlbase + "Search", {
				// We can set the tokenLimit here
				theme: "facebook",
				resultsFormatter: function (item) {
					if (item.id.startsWith("user-")) {
						return "<li class='user'><img src='" + item.iconfile + "' title='" + item.name + "' height='25px' width='25px' /><span>" + item.name + "</span></li>";
					} else if (item.id.startsWith("role-")) {
						return "<li class='role'><img src='" + item.iconfile + "' title='" + item.name + "' height='25px' width='25px' /><span>" + item.name + "</span></li>";
					}
					return "<li>" + item[this.propertyToSearch] + "</li>"; // Default formatter
				},
				minChars: 2,
				preventDuplicates: true,
				hintText: '',
				noResultsText: opts.noResultsText,
				searchingText: opts.searchingText,
				onAdd: function (item) {
					if (item.id.startsWith("user-")) {
						users.push(item.id.substring(5));
					} else if (item.id.startsWith("role-")) {
						roles.push(item.id.substring(5));
					}
					updateSendButtonStatus();
				},
				onDelete: function (item) {
					var array = item.id.startsWith("user-") ? users : roles,
						id = item.id.substring(5),
						index = $.inArray(id, array);

					if (index !== -1) {
						array.splice(index, 1);
					}
					updateSendButtonStatus();
				},
				onError: function (xhr, status) {
					displayMessage(composeMessageDialog, opts.autoSuggestErrorText + status);
				}
			});

			composeMessageDialog.find('#subject').keyup(function () {
				updateSendButtonStatus();
			});

            var prePopulatedRecipients = opts.onPrePopulate(this);

            if (prePopulatedRecipients != null) {
                var to = composeMessageDialog.find('#to');
                $.each(prePopulatedRecipients, function (index, value) {
                    to.tokenInput("add", value);
                });
            }

            composeMessageDialog.dialog({
                minWidth: 650,
                modal: true,
                resizable: false,
                open: function () {
                    composeMessageDialog.dialog("widget").find('.ui-dialog-buttonpane :button').removeClass().addClass('dnnTertiaryAction');
                    messageId = -1;

                    canSend = false;
                    var sendButton = composeMessageDialog.dialog("widget").find('.ui-dialog-buttonpane button:first');
                    var timeForNextMessage = getWaitTimeForNextMessage();
                    if (timeForNextMessage > 0) {
                        var throttlingMessage = $('<div/>')
                            .addClass('ThrottlingWarning dnnFormMessage dnnFormWarning')
                            .text(opts.throttlingText + ' ' + timeForNextMessage + ' sec');

                        composeMessageDialog.prepend(throttlingMessage);

                        sendButton.attr('disabled', 'disabled').addClass('disabled');
                        var countdown = setInterval(function () {
                            timeForNextMessage--;
                            if (timeForNextMessage == 0) {
                                canSend = true;
                                throttlingMessage.remove();
                                if (composeMessageDialog.find('#to').val().trim().length > 0 && composeMessageDialog.find('#subject').val().trim().length > 0) {
                                    sendButton.removeAttr('disabled').removeClass('disabled');
                                }
                                clearInterval(countdown);
                            } else {
                                throttlingMessage.text(opts.throttlingText + ' ' + timeForNextMessage + ' sec');
                            }
                        }, 1000);
                    } else {
                        canSend = true;
                        sendButton.attr('disabled', 'disabled').addClass('disabled');
                    }

                    if (prePopulatedRecipients != null) {
                        composeMessageDialog.find('#subject').focus();
                    }
	                
					composeMessageDialog.find('.fileUploadArea input').dnnFileInput();
                },
                close: function(event, ui) {
	                composeMessageDialog.destroy();
                },
                buttons: [
                    {
                        text: opts.sendText,
                        click: function () {
                            var params = {
                                subject: encodeURIComponent(composeMessageDialog.find('#subject').val()),
                                body: encodeURIComponent(composeMessageDialog.find('#bodytext').val()),
                                roleIds: (roles.length > 0 ? JSON.stringify(roles) : {}),
                                userIds: (users.length > 0 ? JSON.stringify(users) : {}),
                                fileIds: (attachments.length > 0 ? JSON.stringify(attachments) : {})
                            };
                            $.ajax(
                                {   
                                    url: opts.serviceurlbase + "Create",
                                    type: "POST", 
                                    data: JSON.stringify(params),
                                    contentType: "application/json", 
                                    dataType: "json",
                                    beforeSend: opts.servicesFramework.setModuleHeaders
                                }).done(function (data) {
                                if (data.Result === "success") {
                                    composeMessageDialog.dialog("option", "title", opts.messageSentTitle);
                                    var dismissThis = $('<a href="javascript:void(0)"/>') // DO NOT USE href="#" IN ORDER TO PREVENT ISSUES WITH IE
                                        .text(' ' + opts.dismissThisText)
                                        .click(function () {
                                            composeMessageDialog.dialog("close");
                                        });
                                    var messageSent = $('<div/>')
                                        .addClass('MessageSent dnnFormMessage dnnFormSuccess')
                                        .text(opts.messageSentText)
                                        .append(dismissThis);
                                    composeMessageDialog.html(messageSent);
                                    composeMessageDialog.dialog("widget").find('.ui-dialog-buttonpane button').remove();

                                    messageId = data.Value;
                                    autoclose = setInterval(function () {
                                        composeMessageDialog.dialog("close");
                                    }, opts.msgSentAutoCloseTimeout);
                                } else {
                                    displayMessage(composeMessageDialog, opts.createMessageErrorText);
                                }
                            }).fail(function (xhr, status) {
                                displayMessage(composeMessageDialog, opts.createMessageErrorWithDescriptionText + eval("(" + xhr.responseText + ")").ExceptionMessage);
                            });
                        }
                    },
                    {
                        text: opts.cancelText,
                        click: function () {
                            $(this).dialog("close");
                        }
                    }
                ],
                close: function () {
	                composeMessageDialog.remove();
                    if (autoclose != null) {
                        clearInterval(autoclose);
                    }
                    if (messageId != -1 && opts.onMessageSent != null) {
                        opts.onMessageSent(messageId);
                    }
                }
            });

            composeMessageDialog.dialog('open');
        });
    };

    $.fn.dnnComposeMessage.defaultOptions = {
        openTriggerScope: 'body', // defines parent scope for openTriggerSelector, allows for event delegation
        openTriggerSelector: '.ComposeMessage', // opens dialog
        onMessageSent: function (messageId) {
            // messageId is the identifier of the newly created message
        },
        title: 'Compose Message',
        toText: 'Send to',
        subjectText: 'Subject',
        messageText: 'Your Message',
        attachmentsText: 'Attachment(s)',
        browseText: 'Browse',
        uploadText: 'Choose File',
        removeText: 'Remove attachment',
        sendText: 'Send',
        cancelText: 'Cancel',
        messageSentTitle: 'Message Sent',
        messageSentText: 'Your message has been sent successfully.',
        dismissThisText: 'Dismiss this',
        dismissAllText: 'Dismiss all',
        throttlingText: 'Please wait before sending a new message.',
        noResultsText: 'No results',
        searchingText: 'Searching...',
        createMessageErrorText: 'An error occurred while creating the message. Please try again later.',
        createMessageErrorWithDescriptionText: 'An error occurred while creating the message: ',
        autoSuggestErrorText: 'An error occurred while getting suggestions: ',
        dialogClass: 'dnnFormPopup dnnClear',
        autoOpen: false,
        showAttachments: false,
        onPrePopulate: function (target) {
            // params
            //     target: Is the element which raised the click event that opens the dialog
            // returns
            //     An array of objects with 2 properties:
            //         id: an string with a prefix based on the type of recipient ("user-" for users and "role-" for roles) and a suffix with the user/role identifier
            //         name: the displayname in case of users and rolename in case of roles
            // example
            //     var context = ko.contextFor(this);
            //     var prePopulatedRecipients = [{ id: "user-" + context.$data.UserId(), name: context.$data.DisplayName() }];
            //     return prePopulatedRecipients;

            return null;
        },
        msgSentAutoCloseTimeout: 3000,
        userFileManagerOptions: {}
    };

} (jQuery));